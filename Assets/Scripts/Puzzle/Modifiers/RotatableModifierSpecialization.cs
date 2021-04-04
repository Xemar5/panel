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
    private class RotatableModifierSpecialization : ModifierEditor.Specialization<RotatableModifier>
    {
        protected override void Run()
        {
            HashSet<Vector3> drawnPositions = new HashSet<Vector3>();
            foreach (int spaceIndex in base.PieceData.occupiedSpaceIndices)
            {
                SpacePiece space = Puzzle.spaces[spaceIndex];
                Vector3 offset = space.transform.localPosition;
                HandleButton(drawnPositions, offset);
                for (int i = 0; i < space.Corners.Length; i++)
                {
                    offset = space.GetCornerLocalToPuzzle(i);
                    HandleButton(drawnPositions, offset);
                }
            }
        }

        private void HandleButton(HashSet<Vector3> drawnPositions, Vector3 localOffset)
        {
            const float size = 0.02f;
            Vector3 position = Puzzle.transform.TransformPoint(localOffset).Round(5);
            Handles.color = Utils.SqrDistance(Piece.transform.localRotation * Modifier.LocalPosition + Piece.transform.localPosition, localOffset) < Puzzle.CellAbsoluteRadius * Puzzle.CellAbsoluteRadius
                ? Color.red
                : Color.green;
            if (drawnPositions.Add(position) && Handles.Button(position, Puzzle.transform.rotation, size, size, Handles.SphereHandleCap))
            {
                Modifier.LocalPosition = Quaternion.Inverse(Piece.transform.localRotation) * (localOffset - Piece.transform.localPosition);
            }
        }
    }
}
#endif