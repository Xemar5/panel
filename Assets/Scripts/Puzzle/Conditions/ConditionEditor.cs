#if UNITY_EDITOR
using ConditionGroups;
using NUnit.Framework.Constraints;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

public partial class Puzzle
{
    [PropertySpace, PropertyOrder(ConditionsOrder + 2), NonSerialized, HideLabel, ShowInInspector, ShowIf(nameof(IsConditionToolActive)), ValueDropdown(nameof(ConditionOfTypeCount)), OnValueChanged(nameof(SetConditionData)), DisableIf(nameof(IsDataNull))]
    [InfoBox("Select existing condition or add a new one.", InfoMessageType.Warning, VisibleIf = nameof(IsConditionDataNull))]
    private Condition condition = default;
    [BoxGroup("Conditions/Conditions", false, false, order: ConditionsOrder + 3), NonSerialized, ShowInInspector, InlineEditor(InlineEditorObjectFieldModes.CompletelyHidden), ShowIf(nameof(IsConditionToolActiveAndSelected)), DisableIf(nameof(IsDataNull))]
    private ConditionData conditionData;

    private bool IsConditionDataNull() => conditionData == null;
    private void SetConditionData(Condition condition) => conditionData = !condition ? null : condition.Data;
    private bool IsConditionSelected() => condition != null;
    private bool IsConditionToolActiveAndSelected() => IsConditionToolActive() && IsConditionSelected();
    public static bool IsConditionToolActive() => typeof(ConditionEditor) == ToolManager.activeToolType;
    private bool IsConditionToolDisabled() => IsDataNull() || IsConditionToolActive();
    private string GetConditionToolLabel() => IsConditionToolActive() ? "- Conditions -" : "Conditions";

    [ButtonGroup("Tools/Buttons", -2), PropertyOrder(2), LabelText("$" + nameof(GetConditionToolLabel)), DisableIf(nameof(IsConditionToolDisabled))]
    private void SetEditorConditions() => ToolManager.SetActiveTool<ConditionEditor>();



    [PropertyOrder(ConditionsOrder + 1), Button, ShowIf(nameof(IsConditionToolActive)), DisableIf(nameof(IsDataNull)), LabelText("New Condition")]
    private void AddCondition()
    {
        ConditionCreationPopup popup = ScriptableObject.CreateInstance<ConditionCreationPopup>();
        popup.OnFinished += OnConditionTypeSelected;
        popup.Puzzle = this;
        popup.position = new Rect(0, 0, 300, 30);
        popup.ShowAuxWindow();
    }
    private void OnConditionTypeSelected(Condition conditionPrefab, int groupIndex)
    {
        Condition condition = (Condition)PrefabUtility.InstantiatePrefab(conditionPrefab);
        ConditionData conditionData = condition.CreateData();
        conditionData.conditionIdentifier = condition.ConditionId;
        conditionData.name = conditionPrefab.name;
        condition.transform.SetParent(transform, false);
        condition.Data = conditionData;
        if (groupIndex == conditionSets.ConditionGroups.Count)
        {
            conditionSets.AddGroup();
            data.ConditionGroups.Add(new ConditionGroupData());
        }
        conditionSets.AddCondition(condition, groupIndex);
        data.ConditionGroups[groupIndex].conditionDatas.Add(conditionData);
        condition.Load(conditionData);
        this.condition = condition;
        AssetDatabase.AddObjectToAsset(conditionData, data);
        EditorUtility.SetDirty(conditionData);
        EditorUtility.SetDirty(condition);
        EditorUtility.SetDirty(data);
        EditorUtility.SetDirty(this);
        SetConditionData(condition);
    }

    [PropertyOrder(ConditionsOrder + 4), Button, ShowIf(nameof(IsConditionToolActiveAndSelected)), GUIColor(1, 0.85f, 0.85f)]
    private void RemoveSelectedCondition()
    {
        conditionSets.RemoveCondition(condition);
        AssetDatabase.RemoveObjectFromAsset(condition.Data);
        if (Application.isPlaying)
        {
            Destroy(condition.gameObject);
        }
        else
        {
            DestroyImmediate(condition.gameObject);
        }
        condition = null;
        conditionData = null;
        EditorUtility.SetDirty(this.data);
        EditorUtility.SetDirty(this);
        SetConditionData(null);
    }


    private IEnumerable ConditionOfTypeCount()
    {
        if (typeof(ConditionEditor) == ToolManager.activeToolType)
        {
            ValueDropdownList<Condition> list = new ValueDropdownList<Condition>();
            foreach (ConditionGroup group in conditionSets.ConditionGroups)
            {
                foreach (Condition condition in group.Conditions)
                {
                    int groupIndex = conditionSets.GetGroupIndex(condition);
                    list.Add($"{condition.ConditionId.ToString()} (group: {groupIndex}, index: {condition.Data.conditionIndex})", condition);
                }
            }
            return list;
        }
        return new ValueDropdownList<ConditionData>();
    }

    public static Color GetSelectedColor(int colorIndex)
    {

        Puzzle puzzle = GetSelectedPuzzle();
        if (puzzle == null) return Color.magenta;
        if (colorIndex < 0 || colorIndex >= puzzle.Palette.Count) return Color.magenta;
        return puzzle.Palette[colorIndex].color;
    }




    [EditorTool("Occupied Condition Editor")]
    private class ConditionEditor : EditorTool
    {
        public abstract class Specialization
        {
            protected Puzzle Puzzle { get; private set; }

            public bool Run(Puzzle puzzle)
            {
                this.Puzzle = puzzle;
                return Run();
            }
            protected abstract bool Run();
        }
        public abstract class Specialization<C, D> : Specialization
            where C : Condition
            where D : ConditionData
        {
            protected C Condition { get; private set; }
            protected D ConditionData { get; private set; }

            protected override bool Run()
            {
                if (Puzzle.condition is C condition)
                {
                    this.Condition = condition;
                    this.ConditionData = (D)condition.Data;
                    return true;
                }
                else
                {
                    this.Condition = null;
                    this.ConditionData = null;
                    return false;
                }

            }
        }


        private Specialization[] specializations = default;
        private Specialization activeSpecialization = null;

        public override void OnToolGUI(EditorWindow window)
        {
            if (specializations == null)
            {
                specializations = GatherConditionSpecializations();
            }

            activeSpecialization = null;
            GameObject selection = Selection.activeGameObject;
            if (selection == null)
            {
                return;
            }
            Puzzle puzzle = selection.GetComponent<Puzzle>();
            if (puzzle == null)
            {
                return;
            }

            if (puzzle.condition == null)
            {
                return;
            }

            foreach (Specialization specialization in specializations)
            {
                if (specialization.Run(puzzle))
                {
                    activeSpecialization = specialization;
                    return;
                }
            }
        }

        private Specialization[] GatherConditionSpecializations()
        {
            Type[] types = Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .Where(IsTypeConditionEditorSpecialization)
                .ToArray();

            Specialization[] specializations = new Specialization[types.Length];
            for (int i = 0; i < types.Length; i++)
            {
                Type type = (Type)types[i];
                specializations[i] = (Specialization)Activator.CreateInstance(type);
                Debug.Log($"Condition specialization {types[i].Name} added.");
            }
            Debug.Log($"Condition specializations count: {specializations.Length}.");
            return specializations;
        }

        private static bool IsTypeConditionEditorSpecialization(Type x)
        {
            if (!typeof(Specialization).IsAssignableFrom(x)) return false;
            while (x != typeof(Specialization) && x.BaseType != null && (!x.IsGenericType || x.GetGenericTypeDefinition() != typeof(Specialization<,>)))
                x = x.BaseType;
            if (!typeof(Specialization).IsAssignableFrom(x)) return false;
            if (!x.IsConstructedGenericType) return false;
            return true;
        }
    }

}
#endif