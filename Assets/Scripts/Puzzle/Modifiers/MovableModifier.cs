using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public class MovableModifier : Modifier
{
    public event Action<MovableModifier, int> OnActionRegistered;

    public float minDrag = 5;
    [ReadOnly, HideInInlineEditors]
    public List<int> directionIndexConstraints = new List<int>();

    private int closestDirectionIndex = -1;
    private Vector3 dragDistance;
    private InteractablePiece owner;


    public override void Initialize(InteractablePiece owner)
    {
        this.owner = owner;
        owner.OnDragStart += OnDragStart;
        owner.OnDragContinue += OnDragContinue;
        owner.OnDragEnd += OnDragEnd;
    }

    private void OnDragStart(InteractablePiece owner, PointerEventData eventData)
    {
        dragDistance = Vector3.zero;
    }
    private void OnDragContinue(InteractablePiece owner, PointerEventData eventData)
    {
        Vector3 drag = CameraManager.Instance.Camera.transform.TransformDirection(eventData.delta);
        dragDistance += drag;
        if (dragDistance.sqrMagnitude < minDrag * minDrag)
        {
            this.closestDirectionIndex = -1;
            return;
        }

        int directionIndex = owner.GetClosestDirectionIndex(dragDistance, 90);
        if (directionIndexConstraints.Contains(directionIndex))
        {
            this.closestDirectionIndex = -1;
            return;
        }

        if (directionIndex != this.closestDirectionIndex)
        {
            this.closestDirectionIndex = directionIndex;
            if (this.closestDirectionIndex != -1)
            {
                InteractablePieceData ownerData = owner.Data as InteractablePieceData;
                Debug.Log($"closest: {directionIndex}");
            }
            else
            {
                Debug.Log($"closest: none");
            }
        }

    }
    private void OnDragEnd(InteractablePiece owner, PointerEventData eventData)
    {
        if (closestDirectionIndex != -1)
        {
            Debug.Log($"released at: {this.closestDirectionIndex}");
            int closestDirectionIndex = this.closestDirectionIndex;
            this.closestDirectionIndex = -1;
            if (TryMove(closestDirectionIndex))
            {
                OnActionRegistered?.Invoke(this, closestDirectionIndex);
            }
        }
        else
        {
            Debug.Log($"released at: none");
        }
    }

    public bool TryMove(int directionIndex)
    {
        InteractablePieceData data = owner.Data as InteractablePieceData;
        int length = data.occupiedSpaceIndices.Length;
        int[] spacesToMove = new int[length];
        for (int i = 0; i < length; i++)
        {
            SpacePiece space = owner.Master.Spaces[data.occupiedSpaceIndices[i]];
            SpacePieceData spaceData = space.Data as SpacePieceData;
            int neighbourIndex = spaceData.adjacentSpaceIndices[directionIndex];
            if (neighbourIndex == -1)
            {
                Debug.Log($"Move unavailable: No available space at adjacent to {data.occupiedSpaceIndices[i]} in direction at index {directionIndex}.");
                return false;
            }

            SpacePiece neighbour = owner.Master.Spaces[neighbourIndex];
            if (neighbour.IsOccupied())
            {
                bool isOccupiedByOwner = false;
                foreach (InteractablePiece occupyingPiece in neighbour.OccupyingPieces)
                {
                    if (occupyingPiece == owner)
                    {
                        isOccupiedByOwner = true;
                        break;
                    }
                }
                if (!isOccupiedByOwner)
                {
                    Debug.Log($"Move unavailable: Space at adjacent to {data.occupiedSpaceIndices[i]} in direction at index {directionIndex} is already occupied.");
                    return false;
                }
            }

            spacesToMove[i] = neighbourIndex;
        }

        for (int i = 0; i < length; i++)
        {
            owner.Master.Spaces[data.occupiedSpaceIndices[i]].Unoccupy(owner);
        }
        for (int i = 0; i < length; i++)
        {
            owner.Master.Spaces[spacesToMove[i]].Occupy(owner);
        }
        owner.transform.localPosition = owner.Master.Spaces[spacesToMove[0]].transform.localPosition;
        data.occupiedSpaceIndices = spacesToMove;
        Debug.Log($"Movable piece {name} moved (pieces spaces: {spacesToMove.Length})");

        owner.Master.RegisterMove(owner);
        return true;
    }

}
