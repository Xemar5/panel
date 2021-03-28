#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class ModifierCreationPopup : EditorWindow
{
    public event Action<Modifier> OnFinished;

    private List<Type> modifierTypes = null;
    public InteractablePieceData PieceData { get; set; }

    private void OnGUI()
    {
        if (modifierTypes == null)
        {
            modifierTypes = Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .Where(x =>
                    typeof(Modifier).IsAssignableFrom(x) &&
                    x != typeof(Modifier) &&
                    !x.IsAbstract &&
                    !x.IsGenericType)
                .ToList();
            modifierTypes.Sort((x, y) => x.Name.CompareTo(y.Name));
        }
        for (int i = 0; i < modifierTypes.Count; i++)
        {
            int exists = PieceData.modifiers.FindIndex(x => x.GetType() == modifierTypes[i]);
            EditorGUI.BeginDisabledGroup(exists != -1);
            if (GUILayout.Button(exists == -1 ? modifierTypes[i].Name : $"{modifierTypes[i].Name} (already added)"))
            {
                AddModifier(modifierTypes[i]);
            }
            EditorGUI.EndDisabledGroup();
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Cancel"))
        {
            Close();
        }
    }

    private void AddModifier(Type modifierType)
    {
        Modifier modifier = (Modifier)ScriptableObject.CreateInstance(modifierType);
        OnFinished?.Invoke(modifier);
        Close();
    }
}

#endif