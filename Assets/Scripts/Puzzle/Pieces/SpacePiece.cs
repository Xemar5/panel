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
    [SerializeField]
    private Transform[] corners = default;

    private HashSet<InteractablePiece> occupyingPieces = new HashSet<InteractablePiece>();

    public Transform[] Connections => connections;
    public Transform[] Corners => corners;

    protected override void Restart(PieceData restarted) { }
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

    public Vector3 GetCornerLocalToSelf(int cornerIndex) => corners[cornerIndex].localPosition;
    public Vector3 GetCornerLocalToPuzzle(int cornerIndex) => transform.localPosition + transform.localRotation * Vector3.Scale(transform.localScale, corners[cornerIndex].localPosition);
    public Vector3 GetDirectionLocalToSelf(int directionIndex) => connections[directionIndex].localPosition;
    public Vector3 GetDirectionLocalToPuzzle(int directionIndex) => transform.localRotation * connections[directionIndex].localPosition;

    public Vector3 GetDirectionPositionLocalToPuzzle(int directionIndex)
    {
        return transform.localPosition + transform.localRotation * Vector3.Scale(transform.localScale, connections[directionIndex].localPosition);
    }

    public int GetClosestDirectionIndex(Vector3 delta, float maxAngle)
    {
        int closestDirectionIndex = -1;
        float closestAngle = 180;
        Quaternion localRotation = this.transform.localRotation;

        for (int i = 0; i < this.Connections.Length; i++)
        {
            float angle = Vector3.Angle(localRotation * this.Connections[i].localPosition, delta);

            if (angle < closestAngle && angle < maxAngle)
            {
                closestAngle = angle;
                closestDirectionIndex = i;
            }
        }

        return closestDirectionIndex;
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

