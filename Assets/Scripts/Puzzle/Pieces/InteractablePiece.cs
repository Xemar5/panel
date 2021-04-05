using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class InteractablePiece : Piece, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    public event Action<InteractablePiece, PointerEventData> OnDragStart;
    public event Action<InteractablePiece, PointerEventData> OnDragContinue;
    public event Action<InteractablePiece, PointerEventData> OnDragEnd;

    [PropertyOrder(4), SerializeField]
    private List<Transform> bodies = default;

    public IReadOnlyList<Transform> Bodies => bodies;


    public override void Initialize(Puzzle master)
    {
        base.Initialize(master);
        InteractablePieceData data = Data as InteractablePieceData;
        for (int i = 0; i < data.occupiedSpaceIndices.Length; i++)
        {
            master.Spaces[data.occupiedSpaceIndices[i]].Occupy(this);
        }
        for (int i = 0; i < data.modifiers.Count; i++)
        {
            data.modifiers[i].OnInitialize(this);
        }
    }
    protected override void Restart(PieceData restarted)
    {
        InteractablePieceData previousPieceData = Data as InteractablePieceData;
        InteractablePieceData restartedPieceData = restarted as InteractablePieceData;
        for (int i = 0; i < previousPieceData.modifiers.Count; i++)
        {
            Modifier restartedModifierData = restartedPieceData.modifiers[i];
            Modifier currentModifier = previousPieceData.modifiers[i];
            currentModifier.OnRestart(previousPieceData, restartedPieceData, restartedModifierData);
        }
        previousPieceData.occupiedSpaceIndices = restartedPieceData.occupiedSpaceIndices;
    }

    public Vector3 GetBodyPositionInPuzzle(int bodyIndex)
    {
        return transform.localPosition + transform.localRotation * Vector3.Scale(transform.localScale, bodies[bodyIndex].localPosition);
    }

    public int GetClosestDirectionIndex(Vector3 delta, float maxAngle)
    {
        InteractablePieceData data = Data as InteractablePieceData;

        int length = data.occupiedSpaceIndices.Length;
        if (length == 0)
        {
            throw new NotImplementedException("Implement handling 0 occupied spaces (if needed).");
        }

        SpacePiece occupiedSpace = Master.Spaces[data.occupiedSpaceIndices[0]];
        int closestDirectionIndex = occupiedSpace.GetClosestDirectionIndex(delta, maxAngle);
        return closestDirectionIndex;
    }

    public void OnBeginDrag(PointerEventData eventData) => OnDragStart?.Invoke(this, eventData);
    public void OnDrag(PointerEventData eventData) => OnDragContinue?.Invoke(this, eventData);
    public void OnEndDrag(PointerEventData eventData) => OnDragEnd?.Invoke(this, eventData);

    public override PieceData CreateData() => ScriptableObject.CreateInstance<InteractablePieceData>();
    public override void Save(PieceData pieceData)
    {
        base.Save(pieceData);
        pieceData.pieceIdentifier = contextPath;
    }
}
