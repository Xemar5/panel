#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace EventReferencer
{
    public static class EventReferencer
    {
        private class EventData
        {
            public long fileId;
            public string name;
            public string file;
            public string fileGuid;
            public string scriptPath;
            public string methodName;

            public long gameObjectFileId;
            public string gameObjectGuid;

            public long targetFileId;
            public string targetGuid;
            public string typeName;


            public bool IsCompleted()
            {
                return
                    name != null &&
                    methodName != null &&
                    file != null &&
                    fileGuid != null &&
                    gameObjectGuid != null &&
                    targetGuid != null &&
                    typeName != null &&
                    scriptPath != null
                    ;
            }
            public EventData Clone()
            {
                return new EventData()
                {
                    fileId = this.fileId,
                    name = this.name,
                    file = this.file,
                    fileGuid = this.file,
                    gameObjectFileId = this.gameObjectFileId,
                    gameObjectGuid = this.gameObjectGuid,
                    scriptPath = this.scriptPath,
                    methodName = this.methodName,
                    typeName = this.typeName,
                    targetFileId = this.targetFileId,
                    targetGuid = this.targetGuid,
                };
            }
        }

        private class ComponentData
        {
            public string file;
            public long fileId;
            public int fileLine;
            public ComponentType componentType;
            public string fileGuid;
            public string scriptPath;

            public long scriptGameObjectPrefabFileId;
            public string scriptGameObjectPrefabGuid;

            public long scriptGameObjectFileId;
            public string scriptGameObjectGuid;

            public string objectName;
        }

        private struct ComponentReference : IEquatable<ComponentReference>
        {
            public string guid;
            public long fileId;

            public static bool operator ==(ComponentReference first, ComponentReference second) => first.Equals(second);
            public static bool operator !=(ComponentReference first, ComponentReference second) => !first.Equals(second);
            public bool Equals(ComponentReference reference)
            {
                return guid == reference.guid &&
                       fileId == reference.fileId;
            }
            public override bool Equals(object obj)
            {
                return obj is ComponentReference reference &&
                       guid == reference.guid &&
                       fileId == reference.fileId;
            }
            public override int GetHashCode()
            {
                int hashCode = -1094780902;
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(guid);
                hashCode = hashCode * -1521134295 + fileId.GetHashCode();
                return hashCode;
            }
        }

        private enum ComponentType
        {
            Other,
            GameObject,
            MonoBehaviour,
        }


        [MenuItem("Tools/Find event references")]
        private async static void GetAllEvents()
        {
            Debug.Log($"Gathering Yaml files...");
            List<string> files = await Task.Run(GatherEvents);

            Debug.Log($"Yaml files gathered: { files.Count }. Gathering components data...");
            Dictionary<ComponentReference, ComponentData> components = new Dictionary<ComponentReference, ComponentData>();
            foreach (string file in files)
            {
                GatherComponents(components, file);
            }

            Debug.Log($"Components data gathered: { components.Count }. Fixing component references...");
            FixComponentReferences(components);

            Debug.Log($"Components references fixed. Gathering event references...");
            List<EventData> datas = new List<EventData>();
            foreach (string file in files)
            {
                datas.AddRange(GetEventReferences(components, file));
            }
            if (datas.Count == 0)
            {
                Debug.Log($"No methods referenced!");
                return;
            }

            Debug.Log($"Event references gathered: { datas.Count }. Building and saving file...");
            string referencer = BuildFile(datas);

            foreach (string path in Directory.EnumerateFiles("Assets\\", nameof(EventReferencer) + ".*", SearchOption.AllDirectories))
            {
                string directory = Path.GetDirectoryName(path);
                string generatedPath = $"{directory}\\EventReferencerGenerated.cs";
                File.WriteAllText(generatedPath, referencer);

                Debug.Log($"Event references file generated and saved to file at: {generatedPath}!");
                AssetDatabase.ImportAsset(generatedPath);
                break;
            }
        }

        private static void GatherComponents(Dictionary<ComponentReference, ComponentData> components, string file)
        {
            const string idKey = "--- !u!";
            const string monoKey = "MonoBehaviour:";
            const string gameObjectKey = "GameObject:";
            const string prefabInstanceKey = "PrefabInstance:";
            const string nameKey = "m_Name: ";
            const string scriptKey = "m_Script: ";
            const string gameObjectRefKey = "m_GameObject: ";
            const string sourcePrefabRefKey = "m_SourcePrefab: ";
            const string gameObjectPrefabRefKey = "m_CorrespondingSourceObject: ";
            const string guidKey = "guid: ";

            string fileGuid = AssetDatabase.AssetPathToGUID(file);

            int index;
            ComponentData data = null;
            bool componentFound = false;

            int lineIndex = -1;
            foreach (string line in File.ReadLines(file))
            {
                string trimmedLine = line.Trim();
                lineIndex += 1;
                if (trimmedLine.StartsWith(idKey))
                {
                    if (data != null)
                    {
                        components.Add(new ComponentReference()
                        {
                            fileId = data.fileId,
                            guid = fileGuid,
                        }, data);
                        data = null;
                    }
                    data = new ComponentData();
                    data.file = file;
                    data.fileLine = lineIndex;
                    data.fileGuid = fileGuid;

                    trimmedLine = ExtractFileIdFromComponentHeader(trimmedLine);
                    if (long.TryParse(trimmedLine, out long fileId))
                    {
                        data.fileId = fileId;
                    }
                    else
                    {
                        Debug.LogError($"Error: {trimmedLine}");
                    }
                    componentFound = true;
                    continue;
                }

                if (componentFound)
                {
                    if (trimmedLine.StartsWith(monoKey))
                    {
                        data.componentType = ComponentType.MonoBehaviour;
                        continue;
                    }
                    else if (trimmedLine.StartsWith(gameObjectKey))
                    {
                        data.componentType = ComponentType.GameObject;
                        continue;
                    }
                    else if (data.componentType == ComponentType.Other)
                    {
                        data.componentType = ComponentType.Other;
                        continue;
                    }
                    if (data == null)
                    {
                        continue;
                    }

                    if (data.componentType == ComponentType.GameObject)
                    {
                        index = trimmedLine.IndexOf(nameKey);
                        if (index != -1)
                        {
                            data.objectName = trimmedLine.Substring(index + nameKey.Length);
                            continue;
                        }
                    }

                    index = trimmedLine.IndexOf(gameObjectRefKey);
                    if (index != -1)
                    {
                        data.scriptGameObjectFileId = GetFileId(trimmedLine);
                        data.scriptGameObjectGuid = GetGuid(trimmedLine, fileGuid);
                        continue;
                    }

                    index = trimmedLine.IndexOf(gameObjectPrefabRefKey);
                    if (index != -1)
                    {
                        data.scriptGameObjectPrefabFileId = GetFileId(trimmedLine);
                        data.scriptGameObjectPrefabGuid = GetGuid(trimmedLine, fileGuid);
                        continue;
                    }

                    if (data.componentType == ComponentType.MonoBehaviour)
                    {
                        index = trimmedLine.IndexOf(scriptKey);
                        if (index != -1)
                        {
                            string guid = fileGuid;
                            index = trimmedLine.IndexOf(guidKey);
                            if (index != -1) guid = trimmedLine.Substring(index + guidKey.Length, 32);
                            data.scriptPath = AssetDatabase.GUIDToAssetPath(guid);
                            continue;
                        }
                    }
                }
                else
                {
                    continue;
                }
            }

            /// Add last component
            if (data != null)
            {
                components.Add(new ComponentReference()
                {
                    fileId = data.fileId,
                    guid = fileGuid,
                }, data);
                data = null;
            }
        }

        private static string ExtractFileIdFromComponentHeader(string trimmedLine)
        {
            int index = trimmedLine.IndexOf('&') + 1;
            trimmedLine = trimmedLine.Substring(index);
            trimmedLine = new string(trimmedLine.TakeWhile(x => char.IsDigit(x) || x == '-').ToArray());
            return trimmedLine;
        }

        private static void FixComponentReferences(Dictionary<ComponentReference, ComponentData> components)
        {
            foreach (ComponentData data in components.Values)
            {
                FixComponentReference(components, data);
            }
        }

        private static void FixComponentReference(Dictionary<ComponentReference, ComponentData> components, ComponentData component)
        {
            long fieldId;
            string guid;
            if (component.scriptGameObjectFileId != 0)
            {
                fieldId = component.scriptGameObjectFileId;
                guid = component.scriptGameObjectGuid;
            }
            else if (component.scriptGameObjectPrefabFileId != 0)
            {
                fieldId = component.scriptGameObjectPrefabFileId;
                guid = component.scriptGameObjectPrefabGuid;
            }
            else
            {
                return;
            }

            ComponentReference key = new ComponentReference()
            {
                fileId = fieldId,
                guid = guid,
            };
            if (components.TryGetValue(key, out ComponentData referencedComponent))
            {
                if (referencedComponent.objectName == null)
                {
                    FixComponentReference(components, referencedComponent);
                }
                component.objectName = referencedComponent.objectName;
            }
            else
            {
                Debug.Log($"Couldn't find component referenced as fieldId: {fieldId}, guid: {guid}.");
            }
        }

        private static List<string> GatherEvents()
        {
            List<string> yamlFiles = new List<string>();
            foreach (string file in Directory.EnumerateFiles("Assets\\", "*.*", SearchOption.AllDirectories))
            {
                string firstLine = File.ReadLines(file).First();
                if (firstLine.StartsWith("%YAML"))
                {
                    yamlFiles.Add(file);
                }
            }
            return yamlFiles;
        }

        private static List<EventData> GetEventReferences(Dictionary<ComponentReference, ComponentData> components, string file)
        {
            const string methodNameKey = "m_MethodName: ";
            const string targetIdKey = "m_Target: ";
            const string gameObjectNameKey = "m_GameObject: ";
            const string scriptKey = "m_Script: ";
            const string idKey = "--- !u!";
            const string guidKey = "guid: ";

            string fileGuid = AssetDatabase.AssetPathToGUID(file);

            List<EventData> eventReferences = new List<EventData>();
            EventData data = new EventData();
            data.file = file;
            int index;
            foreach (string line in File.ReadLines(file))
            {
                string trimmedLine = line.Trim();

                index = trimmedLine.IndexOf(idKey);
                if (index != -1)
                {
                    /// Another object is parsed and the previous one is not complete; discard.
                    data = UpdateData(eventReferences, data, file, fileGuid, true);


                    string fileIdString = ExtractFileIdFromComponentHeader(trimmedLine);
                    if (int.TryParse(fileIdString, out int fileId))
                    {
                        data.fileId = fileId;
                        data = UpdateData(eventReferences, data, file, fileGuid, false);
                    }
                    continue;
                }

                index = trimmedLine.IndexOf(scriptKey);
                if (index != -1)
                {
                    trimmedLine = trimmedLine.Substring(index + scriptKey.Length);
                    string scriptGuid = GetGuid(trimmedLine, fileGuid);
                    data.scriptPath = AssetDatabase.GUIDToAssetPath(scriptGuid);

                    data = UpdateData(eventReferences, data, file, fileGuid, false);
                    continue;
                }

                index = trimmedLine.IndexOf(gameObjectNameKey);
                if (index != -1)
                {
                    data.gameObjectFileId = GetFileId(trimmedLine);
                    data.gameObjectGuid = GetGuid(trimmedLine, fileGuid);
                    ComponentReference reference = new ComponentReference()
                    {
                        fileId = data.gameObjectFileId,
                        guid = data.gameObjectGuid,
                    };

                    if (components.TryGetValue(reference, out ComponentData component))
                    {
                        data.name = component.objectName;
                        data = UpdateData(eventReferences, data, file, fileGuid, false);
                    }
                    continue;
                }

                index = trimmedLine.IndexOf(targetIdKey);
                if (index != -1)
                {
                    data.targetFileId = GetFileId(trimmedLine);
                    data.targetGuid = GetGuid(trimmedLine, fileGuid);
                    ComponentReference reference = new ComponentReference()
                    {
                        fileId = data.targetFileId,
                        guid = data.targetGuid,
                    };

                    if (components.TryGetValue(reference, out ComponentData component))
                    {
                        data.typeName = Path.GetFileNameWithoutExtension(component.scriptPath);
                        data = UpdateData(eventReferences, data, file, fileGuid, false);
                    }
                    else
                    {
                        Debug.LogError($"Couldn't find target.");
                    }
                    continue;
                }

                index = trimmedLine.IndexOf(methodNameKey);
                if (index != -1)
                {
                    data.methodName = trimmedLine.Substring(index + methodNameKey.Length);

                    data = UpdateData(eventReferences, data, file, fileGuid, false);
                    continue;
                }
            }
            return eventReferences;
        }

        private static long GetFileId(string trimmedLine, long defaultValue = -1)
        {
            const string fileIdKey = "fileID: ";

            int index = trimmedLine.IndexOf(fileIdKey);
            if (index == -1)
            {
                return defaultValue;
            }
            trimmedLine = trimmedLine.Substring(index + fileIdKey.Length);
            int endIndex = trimmedLine.IndexOfAny(new char[] { '}', ',', ' ' });
            if (endIndex != -1)
            {
                trimmedLine = trimmedLine.Substring(0, endIndex);
            }

            if (long.TryParse(trimmedLine, out long fileId))
            {
                return fileId;
            }
            return defaultValue;
        }
        private static string GetGuid(string trimmedLine, string defaultGuid = null)
        {
            const string guidKey = "guid: ";

            int index = trimmedLine.IndexOf(guidKey);
            if (index == -1)
            {
                return defaultGuid;
            }

            trimmedLine = trimmedLine.Substring(index + guidKey.Length);
            int endIndexBracket = trimmedLine.IndexOf('}');
            int endIndexComma = trimmedLine.IndexOf(',');
            if (endIndexBracket == -1) endIndexBracket = int.MaxValue;
            if (endIndexComma == -1) endIndexComma = int.MaxValue;
            trimmedLine = trimmedLine.Substring(0, Mathf.Min(endIndexBracket, endIndexComma));
            return trimmedLine;
        }

        private static EventData UpdateData(List<EventData> eventReferences, EventData data, string file, string fileGuid, bool newObject)
        {
            bool isCompleted = data.IsCompleted();
            if (isCompleted)
            {
                eventReferences.Add(data);
            }
            if (newObject)
            {
                data = new EventData();
                data.file = file;
                data.fileGuid = fileGuid;
            }
            else if (isCompleted)
            {
                data = data.Clone();
                data.typeName = null;
                data.methodName = null;
                data.targetFileId = 0;
                data.targetGuid = null;
            }
            return data;
        }

        private static string BuildFile(List<EventData> events)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"/// This file is automatically generated by { nameof(EventReferencer) } script. Made by sir.woody{ Environment.NewLine }{ Environment.NewLine }");
            foreach (EventData evnt in events)
            {
                if (!AppendReference(builder, evnt))
                {
                    Debug.LogError($"Couldn't find type");
                }
            }
            return builder.ToString();
        }

        private static bool AppendReference(StringBuilder builder, EventData evnt)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types = assembly.GetTypes();
                foreach (Type type in types)
                {
                    if (type.Name != evnt.typeName)
                    {
                        continue;
                    }

                    MethodInfo[] methods = type.GetMethods(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    MethodInfo method = null;
                    foreach (MethodInfo foundMethod in methods)
                    {
                        if (foundMethod.Name == evnt.methodName)
                        {
                            method = foundMethod;
                            break;
                        }
                    }
                    if (method == null)
                    {
                        continue;
                    }
                    Type declaringType = method.DeclaringType;

                    builder.AppendLine(
                        $"/// Method <see cref=\"{ declaringType.FullName }.{ method.Name }\"/>{ Environment.NewLine }" +
                        $"/// Referenced by game object named { evnt.name } (fileID: { evnt.gameObjectFileId }, guid: { evnt.gameObjectGuid }){ Environment.NewLine }" +
                        $"/// By component { Path.GetFileName(evnt.scriptPath) } (path: {evnt.scriptPath}, component fileID: { evnt.fileId }){ Environment.NewLine }" +
                        $"/// In file { evnt.file } (guid: { evnt.fileGuid }).{ Environment.NewLine }{ Environment.NewLine }");
                    return true;
                }
            }
            return false;
        }
    }
}
#endif