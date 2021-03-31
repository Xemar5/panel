using Sirenix.OdinInspector;
using UnityEngine;
using System;

public partial class Puzzle
{
    [Serializable]
    public struct PaletteColor : IEquatable<PaletteColor>
    {
        [HorizontalGroup, HideLabel, OnValueChanged(nameof(OnColorChanged))]
        public Color color;


        public static bool operator ==(PaletteColor first, PaletteColor second) => first.color.Equals(second.color);
        public static bool operator !=(PaletteColor first, PaletteColor second) => !first.color.Equals(second.color);
        public override bool Equals(object obj) => obj is PaletteColor color && this.color.Equals(color.color);
        public bool Equals(PaletteColor other) => this.color.Equals(other.color);
        public override int GetHashCode() => 790427672 + color.GetHashCode();


#if UNITY_EDITOR
        [HorizontalGroup(HideWhenChildrenAreInvisible = true), HideIf("@"+nameof(SetColorLabel)+"().Length == 0"), Button("$"+nameof(SetColorLabel)), DisableIf(nameof(IsColorDisabled))]
        private void SetColor()
        {
            IColored colored = null;
            Puzzle puzzle = null;
            puzzle = Puzzle.GetSelectedPuzzle();
            if (puzzle == null) return;
            if (IsConditionToolActive())
            {
                if (puzzle.condition == null) return;
                colored = puzzle.condition.Data as IColored;
            }
            else if (IsModifierToolActive())
            {
                if (puzzle.modifier == null) return;
                colored = puzzle.modifier as IColored;
            }
            if (colored != null)
            {
                Color color = this.color;
                int index = FindColorIndexInPalette(puzzle, color);
                colored.ColorIndex = index;
            }
        }
        private string SetColorLabel()
        {
            Puzzle puzzle = Puzzle.GetSelectedPuzzle();
            if (puzzle == null) return "";
            if (IsConditionToolActive())
            {
                if (puzzle.condition == null) return "";
                if (puzzle.condition.Data is IColored colored)
                {
                    if (colored.ColorIndex == FindColorIndexInPalette(puzzle, color))
                        return "Current Color";
                    else
                        return "Set Color";
                }
            }
            else if (IsModifierToolActive())
            {
                if (puzzle.modifier == null) return "";
                if (puzzle.modifier is IColored colored)
                {
                    if (colored.ColorIndex == FindColorIndexInPalette(puzzle, color))
                        return "Current Color";
                    else
                        return "Set Color";
                }
            }
            return "";
        }
        private void OnColorChanged()
        {
            Puzzle puzzle = Puzzle.GetSelectedPuzzle();
            if (puzzle == null) return;
            int index = FindPaletteColorInPalette(puzzle, this);
            if (index == -1) return;
            puzzle.data.Palette[index] = new PaletteColor()
            {
                color = color,
            };
            UnityEditor.EditorUtility.SetDirty(puzzle.data);
        }
        private bool IsColorDisabled()
        {
            Puzzle puzzle = Puzzle.GetSelectedPuzzle();
            if (puzzle == null) return true;
            if (IsConditionToolActive())
            {
                if (puzzle.condition == null) return true;
                if (puzzle.condition.Data is IColored colored)
                {
                    return colored.ColorIndex == FindColorIndexInPalette(puzzle, color);
                }
            }
            else if (IsModifierToolActive())
            {
                if (puzzle.modifier == null) return true;
                if (puzzle.modifier is IColored colored)
                {
                    return colored.ColorIndex == FindColorIndexInPalette(puzzle, color);
                }
            }
            return true;
        }
        private static int FindColorIndexInPalette(Puzzle puzzle, Color color)
        {
            for (int i = 0; i < puzzle.Palette.Count; i++)
            {
                if (color == puzzle.Palette[i].color)
                {
                    return i;
                }
            }
            return -1;
        }
        private static int FindPaletteColorInPalette(Puzzle puzzle, PaletteColor paletteColor)
        {
            for (int i = 0; i < puzzle.Palette.Count; i++)
            {
                if (paletteColor == puzzle.Palette[i])
                {
                    return i;
                }
            }
            return -1;
        }


#endif

    }
}
