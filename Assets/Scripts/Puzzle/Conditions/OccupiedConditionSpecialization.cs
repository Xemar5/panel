#if UNITY_EDITOR
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

public partial class Puzzle
{

    private class OccupiedConditionSpecialization : ConditionEditor.Specialization<OccupiedCondition, OccupiedConditionData>
    {
        protected override void Run()
        {
            if (Puzzle.spaces.Count == 0)
            {
                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.red;
                Handles.Label(Puzzle.transform.position, "No available spaces", style);
                return;
            }
            if (ConditionData.spaceIndices == null)
            {
                ConditionData.spaceIndices = new List<int>();
                UnityEditor.EditorUtility.SetDirty(ConditionData);
            }

            ConditionAdd();
            ConditionRemove();
        }
        private void ConditionAdd()
        {
            const float size = 0.02f;
            for (int i = 0; i < Puzzle.spaces.Count; i++)
            {
                SpacePiece space = Puzzle.spaces[i];
                int index = ConditionData.spaceIndices.IndexOf(i);
                if (index != -1)
                {
                    continue;
                }

                Handles.color = Color.white;

                if (Handles.Button(space.transform.position, Puzzle.transform.rotation, size, size, Handles.SphereHandleCap))
                {
                    ConditionData.spaceIndices.Add(i);
                    Condition.Load(ConditionData);
                    EditorUtility.SetDirty(ConditionData);
                    EditorUtility.SetDirty(Condition);
                }
            }
        }
        private void ConditionRemove()
        {
            const float size = 0.02f;

            Color handleColor = ConditionData.colorIndex >= 0 && ConditionData.colorIndex < Puzzle.Palette.Count ?
                Puzzle.Palette[ConditionData.colorIndex].color : Color.magenta;
            Handles.color = handleColor;

            for (int i = 0; i < ConditionData.spaceIndices.Count; i++)
            {
                if (Handles.Button(Puzzle.spaces[ConditionData.spaceIndices[i]].transform.position, Puzzle.transform.rotation, size, size, Handles.CubeHandleCap))
                {
                    ConditionData.spaceIndices.RemoveAt(i);
                    Condition.Load(ConditionData);
                    EditorUtility.SetDirty(ConditionData);
                    EditorUtility.SetDirty(Condition);
                }
            }
        }

    }

}
#endif