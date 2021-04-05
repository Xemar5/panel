using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class RotatableModifier : Modifier
{
    public event Action<RotatableModifier, Quaternion> OnActionRegistered;
    
    [HideInInspector]
    [SerializeField]
    private Vector3 offset = default;
    [SerializeField]
    private bool isClockwise = true;
    [SerializeField]
    private int colorIndex = default;
    [SerializeField]
    private Quaternion rotationStep = Quaternion.Euler(0, 90, 0);

    private RotatableModifierRotator rotator;

    public Vector3 LocalPosition
    {
        get => offset;
#if UNITY_EDITOR
        set => offset = value;
#endif
    }
    public bool IsClockwise
    {
        get => isClockwise;
#if UNITY_EDITOR
        set => isClockwise = value;
#endif
    }
    public int ColorIndex => colorIndex;
    public Quaternion RotationStep => rotationStep;

    protected override void Initialize()
    {
        rotator = Instantiate(ModifierComponents.Instance.rotatableModifierRotatorPrefab, Owner.transform, false);
        rotator.Initialize(Owner, this);
        rotator.transform.localPosition = offset;
        rotator.OnClicked += Rotator_OnClicked;
    }
    protected override void Restart(InteractablePieceData previousPieceData, InteractablePieceData restartedPieceData, Modifier previousModifierData)
    {
        int length = previousPieceData.occupiedSpaceIndices.Length;
        for (int i = 0; i < length; i++)
        {
            Owner.Master.Spaces[previousPieceData.occupiedSpaceIndices[i]].Unoccupy(Owner);
        }
        for (int i = 0; i < length; i++)
        {
            Owner.Master.Spaces[restartedPieceData.occupiedSpaceIndices[i]].Occupy(Owner);
        }
    }

    private void Rotator_OnClicked(RotatableModifierRotator obj)
    {
        if (TryRotate(rotationStep))
        {
            OnActionRegistered?.Invoke(this, RotationStep);
        }
    }

    public bool TryRotate(Quaternion rotationStep)
    {
        Rotate(rotationStep);

        InteractablePieceData data = Owner.Data as InteractablePieceData;
        float sqrRadius = Owner.Master.CellAbsoluteRadius * Owner.Master.CellAbsoluteRadius;
        int length = data.occupiedSpaceIndices.Length;
        int[] newSpaces = new int[length];
        bool allBodiesValid = true;
        for (int i = 0; i < length; i++)
        {
            Vector3 bodyPosition = Owner.GetBodyPositionInPuzzle(i);
            int foundSpace = -1;
            for (int j = 0; j < Owner.Master.Spaces.Count; j++)
            {
                float sqrDistance = Utils.SqrDistance(Owner.Master.Spaces[j].transform.localPosition, bodyPosition);
                if (sqrDistance >= sqrRadius)
                {
                    continue;
                }

                bool isSpaceOccupied = Owner.Master.Spaces[j].IsOccupied();
                foreach (InteractablePiece piece in Owner.Master.Spaces[j].OccupyingPieces)
                {
                    if (piece == Owner)
                    {
                        /// Space occupied by the owner
                        isSpaceOccupied = false;
                        break;
                    }
                }

                if (!isSpaceOccupied)
                {
                    foundSpace = j;
                }
                else
                {
                    Debug.Log($"Found space at index {j} for body {i} is already occupied by another piece.");
                }
                break;
            }

            if (foundSpace != -1)
            {
                newSpaces[i] = foundSpace;
            }
            else
            {
                Debug.Log($"Space for body {i} not found.");
                allBodiesValid = false;
                break;
            }
        }
        if (allBodiesValid)
        {
            Debug.Log($"Rotating {(isClockwise ? "clockwise" : "counter clockwise")}.");
            Commit(data, newSpaces);
            return true;
        }
        else
        {
            Debug.Log($"Couldn't rotate piece {(isClockwise ? "clockwise" : "counter clockwise")}.");
            Rotate(Quaternion.Inverse(rotationStep));
            return false;
        }
    }

    private void Commit(InteractablePieceData data, int[] newSpaces)
    {
        int length = newSpaces.Length;
        for (int i = 0; i < length; i++)
        {
            Owner.Master.Spaces[data.occupiedSpaceIndices[i]].Unoccupy(Owner);
        }
        for (int i = 0; i < length; i++)
        {
            Owner.Master.Spaces[newSpaces[i]].Occupy(Owner);
        }
        data.occupiedSpaceIndices = newSpaces;

        Owner.Master.RegisterMove();
    }

    private void Rotate(Quaternion rotationStep)
    {
        Owner.transform.localPosition += Owner.transform.localRotation * offset;
        Owner.transform.localRotation *= rotationStep;
        Owner.transform.localPosition -= Owner.transform.localRotation * offset;
    }
}
