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
    private class ColoredModifierSpecialization : ModifierEditor.Specialization<ColoredModifier>
    {

        protected override void Run()
        {
            float size = 0.02f;

            Vector3 piecePosition = Piece.transform.position;
            Vector3 pieceUp = Piece.transform.up;
            Quaternion pieceRotation = Piece.transform.rotation;
            Handles.color = Modifier.Color;
            Handles.DrawWireDisc(piecePosition, pieceUp, size * 1.0f);
            Handles.DrawWireDisc(piecePosition, pieceUp, size * 1.2f);
            Handles.DrawWireDisc(piecePosition, pieceUp, size * 1.4f);
        }

    }
}
#endif
