using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.Serialization;
using System.Linq;

[Serializable]
public abstract class PieceData : ScriptableObject
{
    [PropertyOrder(1), DisableIf("@true")]
    public string pieceIdentifier;
    [PropertyOrder(2), DisableIf("@true")]
    public int pieceIndex;
    [PropertyOrder(3), DisableIf("@true")]
    public Vector3 localPosition;
    [PropertyOrder(4), DisableIf("@true")]
    public Quaternion localRotation;


    //private void OnPositionChanged()
    //{
    //    Puzzle puzzle = Puzzle.GetSelectedPuzzle();
    //    if (puzzle == null) return;
    //    for (int i = 0; i < puzzle.Pieces.Count; i++)
    //    {
    //        if (puzzle.Pieces[i].Data == this)
    //        {
    //            puzzle.Pieces[i].transform.localPosition = localPosition;
    //            UnityEditor.EditorUtility.SetDirty(puzzle.Pieces[i]);
    //            puzzle.RebuildConnections();
    //            return;
    //        }
    //    }
    //}
    //private void OnRotationChanged()
    //{
    //    Puzzle puzzle = Puzzle.GetSelectedPuzzle();
    //    if (puzzle == null) return;
    //    for (int i = 0; i < puzzle.Pieces.Count; i++)
    //    {
    //        if (puzzle.Pieces[i].Data == this)
    //        {
    //            puzzle.Pieces[i].transform.localRotation = localRotation;
    //            UnityEditor.EditorUtility.SetDirty(puzzle.Pieces[i]);
    //            puzzle.RebuildConnections();
    //            return;
    //        }
    //    }
    //}
}

