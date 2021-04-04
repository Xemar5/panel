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

    private InteractablePiece owner;
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

    public override void Initialize(InteractablePiece owner)
    {
        base.Initialize(owner);
        this.owner = owner;
        rotator = Instantiate(ModifierComponents.Instance.rotatableModifierRotatorPrefab, owner.transform, false);
        rotator.Initialize(owner, this);
        rotator.transform.localPosition = offset;
        rotator.OnClicked += Rotator_OnClicked;
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

        InteractablePieceData data = owner.Data as InteractablePieceData;
        float sqrRadius = owner.Master.CellAbsoluteRadius * owner.Master.CellAbsoluteRadius;
        int length = data.occupiedSpaceIndices.Length;
        int[] newSpaces = new int[length];
        bool allBodiesValid = true;
        for (int i = 0; i < length; i++)
        {
            Vector3 bodyPosition = owner.GetBodyPositionInPuzzle(i);
            int foundSpace = -1;
            for (int j = 0; j < owner.Master.Spaces.Count; j++)
            {
                float sqrDistance = Utils.SqrDistance(owner.Master.Spaces[j].transform.localPosition, bodyPosition);
                if (sqrDistance >= sqrRadius)
                {
                    continue;
                }

                bool isSpaceOccupied = owner.Master.Spaces[j].IsOccupied();
                foreach (InteractablePiece piece in owner.Master.Spaces[j].OccupyingPieces)
                {
                    if (piece == owner)
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
            owner.Master.Spaces[data.occupiedSpaceIndices[i]].Unoccupy(owner);
        }
        for (int i = 0; i < length; i++)
        {
            owner.Master.Spaces[newSpaces[i]].Occupy(owner);
        }
        data.occupiedSpaceIndices = newSpaces;

        owner.Master.RegisterMove(owner);
    }

    private void Rotate(Quaternion rotationStep)
    {
        owner.transform.localPosition += owner.transform.localRotation * offset;
        owner.transform.localRotation *= rotationStep;
        owner.transform.localPosition -= owner.transform.localRotation * offset;
    }
}
