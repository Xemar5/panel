using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Puzzle.Modifiers
{
    public class SiblingModifier : Modifier
    {
        [Flags]
        private enum MirroredModifiers
        {
            None      = 0,
            Movable   = 1 << 0,
            Rotatable = 1 << 1,
            All       = ~0,
        }

        [SerializeField]
        private int siblingIndex = 0;
        [SerializeField]
        private MirroredModifiers mirroredModifiers = MirroredModifiers.All;

        [NonSerialized]
        private MovableModifier movableModifier;
        [NonSerialized]
        private RotatableModifier rotatableModifier;
        [NonSerialized]
        private List<MovableModifier> siblingMovables = new List<MovableModifier>();
        [NonSerialized]
        private List<RotatableModifier> siblingRotatables = new List<RotatableModifier>();

        protected override void Initialize()
        {
            InitializeMovable(Owner, OwnerData);
            InitializeRotatable(Owner, OwnerData);
            GatherSiblings();
        }
        protected override void Restart(InteractablePieceData previousPieceData, InteractablePieceData restartedPieceData, Modifier previousModifierData)
        {
        }

        private void GatherSiblings()
        {
            foreach (InteractablePiece otherPiece in Owner.Master.Pieces)
            {
                if (otherPiece == Owner)
                {
                    continue;
                }

                InteractablePieceData otherPieceData = otherPiece.Data as InteractablePieceData;
                int index = otherPieceData.modifiers.FindIndex(x => x is SiblingModifier);
                if (index == -1)
                {
                    continue;
                }

                SiblingModifier sibling = otherPieceData.modifiers[index] as SiblingModifier;
                if (sibling.siblingIndex != this.siblingIndex)
                {
                    continue;
                }

                foreach (Modifier otherPieceModifier in otherPieceData.modifiers)
                {
                    if (otherPieceModifier is MovableModifier movable)
                    {
                        siblingMovables.Add(movable);
                    }
                    else if (otherPieceModifier is RotatableModifier rotatable)
                    {
                        siblingRotatables.Add(rotatable);
                    }
                }
            }
        }

        private void InitializeMovable(InteractablePiece owner, InteractablePieceData data)
        {
            if ((mirroredModifiers & MirroredModifiers.Movable) == MirroredModifiers.Movable)
            {
                movableModifier = data.modifiers.Find(x => x is MovableModifier) as MovableModifier;
                if (movableModifier == null)
                {
                    Debug.LogError($"Movable modifier not found in {owner.name}.");
                }
                else
                {
                    movableModifier.OnActionRegistered += MirrorMove;
                }
            }
        }

        private void InitializeRotatable(InteractablePiece owner, InteractablePieceData data)
        {
            if ((mirroredModifiers & MirroredModifiers.Rotatable) == MirroredModifiers.Rotatable)
            {
                rotatableModifier = data.modifiers.Find(x => x is RotatableModifier) as RotatableModifier;
                if (rotatableModifier == null)
                {
                    Debug.LogError($"Rotatable modifier not found in {owner.name}.");
                }
                else
                {
                    rotatableModifier.OnActionRegistered += MirrorRotation;
                }
            }
        }

        private void MirrorMove(MovableModifier sender, int directionIndex)
        {
            foreach (MovableModifier movable in siblingMovables)
            {
                movable.TryMove(directionIndex);
            }
        }
        private void MirrorRotation(RotatableModifier sender, Quaternion rotationStep)
        {
            foreach (RotatableModifier rotatable in siblingRotatables)
            {
                rotatable.TryRotate(rotationStep);
            }
        }

    }
}
