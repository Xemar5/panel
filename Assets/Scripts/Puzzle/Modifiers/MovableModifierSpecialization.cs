#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public partial class Puzzle
{
    private class MovableModifierSpecialization : ModifierEditor.Specialization<MovableModifier>
    {

        protected override void Run()
        {
            float size = 0.1f;
            int spaceIndex = PieceData.occupiedSpaceIndices[0];
            Quaternion puzzleRotation = Puzzle.transform.rotation;
            Vector3 piecePosition = Piece.transform.position;

            for (int i = 0; i < Puzzle.spaces[spaceIndex].Connections.Length; i++)
            {
                Transform connection = Puzzle.spaces[spaceIndex].Connections[i];
                Vector3 direction = connection.localPosition;
                int constraintIndex = Modifier.directionIndexConstraints.IndexOf(i);
                Handles.color = constraintIndex == -1 ? Color.white : Color.red;
                if (Handles.Button(piecePosition + puzzleRotation * direction.normalized * size * 0.5f, puzzleRotation * Quaternion.LookRotation(direction, Vector3.up), size, size, Handles.ArrowHandleCap))
                {
                    if (constraintIndex == -1)
                    {
                        Modifier.directionIndexConstraints.Add(i);
                    }
                    else
                    {
                        Modifier.directionIndexConstraints.RemoveAtSwapback(constraintIndex);
                    }
                }
            }
        }

    }
}
#endif
