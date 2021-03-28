#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class ConditionCreationPopup : EditorWindow
{
    public event Action<Condition, int> OnFinished;

    private int conditionGroupIndex = 0;
    private List<string> conditionIds = null;
    public Puzzle Puzzle { get; set; }

    private void OnGUI()
    {
        EditorGUI.BeginDisabledGroup(Puzzle.ConditionSets.ConditionGroups.Count == 0);
        conditionGroupIndex = EditorGUILayout.IntSlider("Condition Group Index", conditionGroupIndex, 0, Puzzle.ConditionSets.ConditionGroups.Count);
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space();

        if (conditionIds == null)
        {
            conditionIds = ConditionDatabase.Instance.ConditionDictionary.Keys.ToList();
            conditionIds.Sort();
        }
        for (int i = 0; i < conditionIds.Count; i++)
        {
            if (GUILayout.Button(conditionIds[i]))
            {
                AddCondition(conditionIds[i]);
            }
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Cancel"))
        {
            Close();
        }
    }

    private void AddCondition(string conditionId)
    {
        Condition conditionPrefab = ConditionDatabase.Instance.ConditionDictionary[conditionId];
        OnFinished?.Invoke(conditionPrefab, conditionGroupIndex);
        Close();
    }
}

#endif