using UnityEngine;

public abstract class Modifier : ScriptableObject
{
    public virtual void Initialize(InteractablePiece owner) { }
}
