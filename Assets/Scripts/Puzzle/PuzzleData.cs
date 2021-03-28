using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "PuzzleData", menuName = "Create Puzzle Data")]
public class PuzzleData : ScriptableObject
{
    [SerializeField]
    private float cellRadius = 1;
    [SerializeField]
    private float minDragDelta = 5;
    [SerializeField, ReadOnly, HideInInlineEditors]
    private List<SpacePieceData> spaces = new List<SpacePieceData>();
    [SerializeField, ReadOnly, HideInInlineEditors]
    private List<PieceData> pieces = new List<PieceData>();
    [SerializeField, ReadOnly, HideInInlineEditors]
    private List<ConditionGroupData> conditionGroups = new List<ConditionGroupData>();
    [SerializeField, ReadOnly, HideInInlineEditors]
    private List<Puzzle.PaletteColor> palette = new List<Puzzle.PaletteColor>();

    public float CellRadius => cellRadius;
    public float MinDragDelta => minDragDelta;
    public List<SpacePieceData> Spaces => spaces;
    public List<PieceData> Pieces => pieces;
    public List<ConditionGroupData> ConditionGroups => conditionGroups;
    public List<Puzzle.PaletteColor> Palette { get => palette; set => palette = value; }

}
