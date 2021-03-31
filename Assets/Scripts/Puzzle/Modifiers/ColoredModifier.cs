using Sirenix.OdinInspector;
using UnityEngine;

public class ColoredModifier : Modifier, IColored
{
    [ReadOnly]
    [HideInInlineEditors]
    [SerializeField]
    private int colorIndex;

    [ShowInInlineEditors]
    [HideLabel]
    [PropertyRange(0, nameof(ColorIndexMax))]
    [InfoBox("Color index out of range.", VisibleIf = nameof(IsInvalidColor))]
    public int ColorIndex
    {
        get => colorIndex;
        set => colorIndex = value;
    }


    public override void Initialize(InteractablePiece owner)
    {
        Renderer[] renderers = owner.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (colorIndex < 0 || colorIndex >= owner.Master.Palette.Count)
            {
                renderer.material.color = Color.magenta;
            }
            else
            {
                renderer.material.color = owner.Master.Palette[colorIndex].color;
            }
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