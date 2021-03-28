using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;


public class SpacePiece : Piece
{
    [SerializeField]
    private float smallestDistanceEpsilon = 0.5f;
    [SerializeField]
    private Transform[] connections = default;

    private HashSet<InteractablePiece> occupyingPieces = new HashSet<InteractablePiece>();

    public Transform[] Connections => connections;

    public override PieceData CreateData() => PieceData.CreateInstance<SpacePieceData>();

    public int OccupyCount() => occupyingPieces.Count;
    public bool IsOccupied() => occupyingPieces.Count > 0;
    public void Occupy(InteractablePiece piece) => occupyingPieces.Add(piece);
    public void Unoccupy(InteractablePiece piece) => occupyingPieces.Remove(piece);
    public IEnumerable<InteractablePiece> OccupyingPieces => occupyingPieces;
    public IEnumerable<Vector3> Directions
    {
        get
        {
            int length = connections.Length;
            for (int i = 0; i < length; i++)
            {
                yield return connections[i].localPosition;
            }
        }
    }

    public Vector3 GetDirectionPositionLocalToPuzzle(int directionIndex)
    {
        return transform.localPosition + transform.localRotation * Vector3.Scale(transform.localScale, connections[directionIndex].localPosition);
    }

#if UNITY_EDITOR
    public void UpdateDataInPuzzle(Puzzle puzzle)
    {
        SpacePieceData data = Data as SpacePieceData;
        data.adjacentSpaceIndices = new int[connections.Length];

        for (int i = 0; i < data.adjacentSpaceIndices.Length; i++)
        {
            data.adjacentSpaceIndices[i] = -1;
        }

        float sqrEpsilon = smallestDistanceEpsilon * smallestDistanceEpsilon;
        for (int i = 0; i < puzzle.Spaces.Count; i++)
        {
            if (puzzle.Spaces[i] == this)
            {
                data.pieceIndex = i;
            }
        }
        for (int j = 0; j < connections.Length; j++)
        {
            for (int i = 0; i < puzzle.Spaces.Count; i++)
            {
                Vector3 directionPosition = GetDirectionPositionLocalToPuzzle(j);
                if (Utils.SqrDistance(puzzle.Spaces[i].transform.localPosition, directionPosition) < sqrEpsilon)
                {
                    data.adjacentSpaceIndices[j] = i;
                    puzzle.Spaces[i].OnNeighbourChanged(puzzle, data.pieceIndex);
                    break;
                }
            }
        }

        UnityEditor.EditorUtility.SetDirty(data);
    }

    private void OnNeighbourChanged(Puzzle puzzle, int neighbourIndex)
    {
        SpacePieceData data = Data as SpacePieceData;
        SpacePiece neighbour = puzzle.Spaces[neighbourIndex];
        float sqrEpsilon = smallestDistanceEpsilon * smallestDistanceEpsilon;
        for (int i = 0; i < connections.Length; i++)
        {
            Vector3 directionPosition = GetDirectionPositionLocalToPuzzle(i);
            if (Utils.SqrDistance(neighbour.transform.localPosition, directionPosition) < sqrEpsilon)
            {
                data.adjacentSpaceIndices[i] = neighbourIndex;
                break;
            }
        }
        UnityEditor.EditorUtility.SetDirty(data);
    }
#endif
}

