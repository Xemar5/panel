using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class OccupiedConditionData : ConditionData, ISpaceReferencer, IColored
{
    [ReadOnly]
    [HideInInlineEditors]
    public List<int> spaceIndices = default;
    [ReadOnly]
    [HideInInlineEditors]
    public int colorIndex = 0;

    [ShowInInspector]
    [HideLabel]
    [PropertyRange(0, nameof(ColorIndexMax))]
    [InfoBox("Color index out of range.", VisibleIf = nameof(IsInvalidColor))]
    public int ColorIndex
    {
        get => colorIndex;
        set => colorIndex = value;
    }

#if UNITY_EDITOR

    [ShowInInspector]
    [HideLabel]
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

    private float ColorIndexMax
    {
        get
        {
            Puzzle puzzle = Puzzle.GetSelectedPuzzle();
            if (puzzle == null) return 0;
            return puzzle.Palette.Count - 1;
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
