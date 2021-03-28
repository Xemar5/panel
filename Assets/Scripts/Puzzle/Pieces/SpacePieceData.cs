using Sirenix.OdinInspector;
using System;

[Serializable]
public class SpacePieceData : PieceData, ISpaceReferencer
{
    [ReadOnly]
    public int[] adjacentSpaceIndices;

#if UNITY_EDITOR
    public void OnSpaceRemoved(int removedSpaceIndex)
    {
        bool hasDataChanged = false;
        if (pieceIndex > removedSpaceIndex)
        {
            pieceIndex -= 1;
            hasDataChanged = true;
        }
        else if (pieceIndex == removedSpaceIndex)
        {
            pieceIndex = -1;
            hasDataChanged = true;
        }
        for (int j = 0; j < adjacentSpaceIndices.Length; j++)
        {
            if (adjacentSpaceIndices[j] > removedSpaceIndex)
            {
                adjacentSpaceIndices[j] -= 1;
                hasDataChanged = true;
            }
            else if (adjacentSpaceIndices[j] == removedSpaceIndex)
            {
                adjacentSpaceIndices[j] = -1;
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

