using Sirenix.OdinInspector;
using System;
using UnityEngine;

public class ColoredModifier : Modifier, IColored
{
    [ReadOnly]
    [HideInInlineEditors]
    [SerializeField]
    private int colorIndex;

    [NonSerialized]
    private Renderer[] renderers = null;

    [ShowInInlineEditors]
    [HideLabel]
    [PropertyRange(0, nameof(ColorIndexMax))]
    [InfoBox("Color index out of range.", VisibleIf = nameof(IsInvalidColor))]
    public int ColorIndex
    {
        get => colorIndex;
        set => colorIndex = value;
    }
    public Renderer[] Renderers
    {
        get
        {
            if (renderers == null)
            {
                renderers = Owner.GetComponentsInChildren<Renderer>();
            }
            return renderers;
        }
    }
    public Color DefaultColor => colorIndex < 0 || colorIndex >= Owner.Master.Palette.Count
            ? Color.magenta
            : Owner.Master.Palette[colorIndex].color;

    protected override void Initialize()
    {
        SetAllRenderersColor(DefaultColor);
    }
    protected override void Restart(InteractablePieceData previousPieceData, InteractablePieceData restartedPieceData, Modifier previousModifierData)
    {
        SetAllRenderersColor(DefaultColor);
    }

    private void SetAllRenderersColor(Color color)
    {
        for (int i = 0; i < Renderers.Length; i++)
        {
            Renderers[i].material.color = color;
        }
    }


#if UNITY_EDITOR
    [ShowInInspector]
    [HideLabel]
    [InfoBox("Color index out of range.", VisibleIf = nameof(IsInvalidColor))]
    public Color Color
    {
        get
        {
            Puzzle puzzle = Puzzle.GetSelectedPuzzle();
            if (puzzle == null) return Color.magenta;
            if (colorIndex < 0 || colorIndex >= puzzle.Palette.Count) return Color.magenta;
            return puzzle.Palette[colorIndex].color;
        }
    }

    private float ColorIndexMax
    {
        get
        {
            Puzzle puzzle = Puzzle.GetSelectedPuzzle();
            if (puzzle == null) return 0;
            return puzzle.Palette.Count - 1;
        }
    }
    private bool IsInvalidColor
    {
        get
        {
            Puzzle puzzle = Puzzle.GetSelectedPuzzle();
            if (puzzle == null) return true;
            return colorIndex < 0 || colorIndex >= puzzle.Palette.Count;
        }
    }


#endif
}