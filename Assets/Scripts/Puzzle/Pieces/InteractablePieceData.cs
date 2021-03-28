using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InteractablePieceData : PieceData, ISpaceReferencer
{
    [ReadOnly]
    public List<Modifier> modifiers = new List<Modifier>();
    [ReadOnly]
    public int[] occupiedSpaceIndices;

#if UNITY_EDITOR
    public void OnSpaceRemoved(int removedSpaceIndex)
    {
        bool hasDataChanged = false;
        for (int j = 0; j < occupiedSpaceIndices.Length; j++)
        {
            if (occupiedSpaceIndices[j] > removedSpaceIndex)
            {
                occupiedSpaceIndices[j] -= 1;
                hasDataChanged = true;
            }
            else if (occupiedSpaceIndices[j] == removedSpaceIndex)
            {
                occupiedSpaceIndices[j] = -1;
                hasDataChanged = true;
            }
        }
        if (hasDataChanged)
        {
            UnityEditor.EditorUtility.SetDirty(this);
        }
        for (int i = 0; i < modifiers.Count; i++)
        {
            ISpaceReferencer data = modifiers[i] as ISpaceReferencer;
            if (data != null) data.OnSpaceRemoved(removedSpaceIndex);
        }
    }
    public void OnPieceMoved(InteractablePiece owner, Puzzle puzzle)
    {
        float sqrRadius = puzzle.CellAbsoluteRadius * puzzle.CellAbsoluteRadius;
        int count = owner.Bodies.Count;
        occupiedSpaceIndices = new int[count];
        for (int i = 0; i < count; i++)
        {
            occupiedSpaceIndices[i] = -1;
        }
        for (int i = 0; i < count; i++)
        {
            Vector3 bodyInPuzzle = owner.GetBodyPositionInPuzzle(i);
            for (int j = 0; j < puzzle.Spaces.Count; j++)
            {
                if (Utils.SqrDistance(puzzle.Spaces[j].transform.localPosition, bodyInPuzzle) < sqrRadius)
                {
                    occupiedSpaceIndices[i] = j;
                    break;
                }
            }
            if (occupiedSpaceIndices[i] == -1)
            {
                Debug.Log($"Body part [{i}] of piece {owner.name} is not placed on any space.");
            }
        }
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}
