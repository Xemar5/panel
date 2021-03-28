#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;

public class PieceAdjustPopup : EditorWindow
{
    public event Action OnClose;

    public Puzzle Puzzle { get; set; }
    public Piece Piece { get; set; }
    public int ColorIndex { get; set; }
    public Vector3 Rotation { get; set; }

    private void OnGUI()
    {
        ColorIndex = EditorGUILayout.IntSlider("Color Index", ColorIndex, -1, Puzzle.Palette.Count - 1);
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.ColorField(ColorIndex == -1 ? new Color(0, 0, 0, 0) : Puzzle.Palette[ColorIndex].color);
        EditorGUI.EndDisabledGroup();

        Rotation = EditorGUILayout.Vector3Field("Rotation", Rotation);
        if (GUILayout.Button("Save"))
        {
            IColored colored = Piece.Data as IColored;
            colored.colorIndex = ColorIndex;
            Piece.transform.localRotation = Quaternion.Euler(Rotation);
            EditorUtility.SetDirty(Piece);
            OnClose?.Invoke();
            Close();
        }
    }

}

#endif