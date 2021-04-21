using System;
using UnityEngine;

public abstract class Modifier : ScriptableObject
{
    public InteractablePiece Owner { get; private set; }
    public InteractablePieceData OwnerData { get; private set; }
    public Puzzle Master { get; private set; }

    public void OnInitialize(InteractablePiece owner)
    {
        Owner = owner;
        OwnerData = owner.Data as InteractablePieceData;
        Master = owner.Master;
        Initialize();
    }
    public void OnRestart(InteractablePieceData previousPieceData, InteractablePieceData restartedPieceData, Modifier restartedModifierData)
    {
        Restart(previousPieceData, restartedPieceData, restartedModifierData);
    }

    protected abstract void Initialize();
    protected abstract void Restart(InteractablePieceData previousPieceData, InteractablePieceData restartedPieceData, Modifier restartedModifierData);
}
