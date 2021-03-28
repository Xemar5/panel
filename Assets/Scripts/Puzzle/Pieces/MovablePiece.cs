//using Newtonsoft.Json.Linq;
//using System;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.EventSystems;


//[Serializable]
//public class MovableData : PieceData, IColored
//{
//    [ReadOnly, HideInInlineEditors]
//    public int colorIndex;

//    public int ColorIndex { get => colorIndex; set => colorIndex = value; }
//#if UNITY_EDITOR
//    [PropertyOrder(1.5f), ShowInInspector, HideLabel, InfoBox("Color can be set in Puzzle's palette by clicking on the Set Piece Color button.", VisibleIf = nameof(IsInvalidColor))]
//    private Color Color
//    {
//        get
//        {
//            Puzzle puzzle = Puzzle.GetSelectedPuzzle();
//            if (puzzle == null) return Color.magenta;
//            if (colorIndex < 0 || colorIndex >= puzzle.Palette.Count) return Color.magenta;
//            return puzzle.Palette[colorIndex].color;
//        }
//    }

//    private bool IsInvalidColor
//    {
//        get
//        {
//            Puzzle puzzle = Puzzle.GetSelectedPuzzle();
//            if (puzzle == null) return true;
//            return colorIndex < 0 || colorIndex >= puzzle.Palette.Count;
//        }
//    }
//#endif


//}

//public class MovablePiece : Piece, IBeginDragHandler, IEndDragHandler, IDragHandler
//{

//    private int draggedOutputIndex = -1;
//    private Vector2 dragDistance;



//    public void OnBeginDrag(PointerEventData eventData)
//    {
//        //Ray ray = CameraManager.Instance.Camera.ScreenPointToRay(UnityEngine.Input.mousePosition);
//        //draggedOutputIndex = this.GetOutputInDirection(ray, 90);
//        dragDistance = Vector2.zero;
//    }
//    public void OnDrag(PointerEventData eventData)
//    {
//        dragDistance += eventData.delta;
//        if (dragDistance.sqrMagnitude < Master.MinDragDelta * Master.MinDragDelta)
//        {
//            draggedOutputIndex = -1;
//            return;
//        }

//        int closestOutputIndex = this.GetOutputInDirection(dragDistance, 90);
//        if (closestOutputIndex != draggedOutputIndex)
//        {
//            draggedOutputIndex = closestOutputIndex;
//            if (draggedOutputIndex != -1)
//            {
//                Debug.Log($"closest: {Outputs[draggedOutputIndex].nodes[0].name}");
//                //TryMove(draggedOutputIndex);
//            }
//            else
//            {
//                Debug.Log($"closest: none");
//            }
//        }
//    }
//    public void OnEndDrag(PointerEventData eventData)
//    {
//        if (draggedOutputIndex != -1)
//        {
//            Debug.Log($"released at: {Outputs[draggedOutputIndex].nodes[0].name}");
//            TryMove(draggedOutputIndex);
//            draggedOutputIndex = -1;
//        }
//        else
//        {
//            Debug.Log($"released at: none");
//        }
//    }

//    private void TryMove(int outputIndex)
//    {
//        Output output = Outputs[outputIndex];

//        Connection connection = GetOutputConnection(outputIndex);
//        if (connection == null)
//        {
//            Debug.Log($"No connection found.");
//            return;
//        }

        
//        HashSet<SpacePiece> swappedPieces = new HashSet<SpacePiece>();
//        List<Connection.Input> movedSpaces = new List<Connection.Input>();

//        for (int i = 0; i < output.nodes.Count; i++)
//        {
//            Vector3 draggedOutputPosition = output.GetSocketPositionInPuzzle(i, transform);
//            foreach (Connection.Input inputPiece in connection.InputPieces)
//            {
//                if (Master.Pieces[inputPiece.pieceIndex] is SpacePiece spacePiece && swappedPieces.Add(spacePiece))
//                {
//                    movedSpaces.Add(inputPiece);
//                }
//            }
//        }

//        if (swappedPieces.Count != output.nodes.Count)
//        {
//            Debug.Log($"Not enoght spaces (required: {output.nodes.Count}, adjacent: {swappedPieces.Count}).");
//            return;
//        }

//        Vector3 translation = output.GetSocketPositionInPuzzle(movedSpaces[0].closestOutputIndex, transform) - Inputs.GetSocketPositionInPuzzle(output.closestInputs[movedSpaces[0].closestOutputIndex], transform);
//        transform.localPosition += translation;
//        //for (int i = 0; i < movedSpaces.Count; i++)
//        //{
//        //    Piece piece = Master.Pieces[movedSpaces[i].pieceIndex];
//        //    Vector3 swappedTranslation = output.GetSocketPositionInPuzzle(movedSpaces[i].closestOutputIndex, transform) - Inputs.GetSocketPositionInPuzzle(output.furthestInputs[movedSpaces[i].closestOutputIndex], transform);
//        //    piece.transform.localPosition -= swappedTranslation;
//        //}
//        Debug.Log($"Movable piece {name} moved (pieces swapped: {swappedPieces.Count})");

//        Master.RebuildConnections();
//        Master.RegisterMove(this);
//    }

//    private void OnDrawGizmos()
//    {
//        if (draggedOutputIndex != -1)
//        {
//            Gizmos.DrawLine(this.Inputs.GetAvreageGlobalPosition(transform), Outputs[draggedOutputIndex].GetAvreageGlobalPosition(transform));
//        }
//    }

//    public override PieceData CreateData() => MovableData.CreateInstance<MovableData>();
//    public override void Save(PieceData pieceData)
//    {
//        MovableData data = (MovableData)pieceData;
//        base.Save(data);
//    }
//    public override void Load(PieceData pieceData)
//    {
//        MovableData data = (MovableData)pieceData;
//        base.Load(data);
//    }
//}