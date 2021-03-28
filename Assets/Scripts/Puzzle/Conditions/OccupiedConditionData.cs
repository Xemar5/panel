using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class OccupiedConditionData : ConditionData, ISpaceReferencer
{
    [ReadOnly, HideInInlineEditors]
    public List<int> spaceIndices = default;
    [ReadOnly, HideInInlineEditors]
    public int colorIndex = 0;


#if UNITY_EDITOR
    [ShowInInspector, HideLabel, InfoBox("Color can be set in Puzzle's palette by clicking on the Set Condition Color button.", VisibleIf = nameof(IsInvalidColor))]
    private Color Color
    {
        get
        {
            Puzzle puzzle = Puzzle.GetSelectedPuzzle();
            if (puzzle == null) return Color.magenta;
            if (colorIndex < 0 || colorIndex >= puzzle.Palette.Count) return Color.magenta;
            return puzzle.Palette[colorIndex].color;
        }
    }

    private bool IsInvalidColor
    {
        get
        {
            Puzzle puzzle = Puzzle.GetSelectedPuzzle();
            if (puzzle == null) return true;
            return colorIndex < 0 || colorIndex >= puzzle.Palette.Count;
        }
    }

    public void OnSpaceRemoved(int removedSpaceIndex)
    {
        bool hasDataChanged = false;
        for (int j = 0; j < spaceIndices.Count; j++)
        {
            if (spaceIndices[j] > removedSpaceIndex)
            {
                spaceIndices[j] -= 1;
                hasDataChanged = true;
            }
            else if (spaceIndices[j] == removedSpaceIndex)
            {
                spaceIndices.RemoveAtSwapback(j);
                hasDataChanged = true;
            }
        }
        if (hasDataChanged)
        {
            UnityEditor.EditorUtility.SetDirty(this);
        }

    }

#endif

}
