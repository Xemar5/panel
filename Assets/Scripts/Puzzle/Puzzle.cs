using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using ConditionGroups;
using System.Collections;
using System;
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;
#endif

public partial class Puzzle : MonoBehaviour
{
    private const float SettingsOrder = 0f;
    private const float PaletteOrder = 1000f;
    private const float ToolsOrder = 2000f;
    private const float SpacesOrder = 3000f;
    private const float PiecesOrder = 4000f;
    private const float ModifiersOrder = 5000f;
    private const float ConditionsOrder = 6000f;

    public event Action<Puzzle, int> OnPuzzleCompleted;
    public event Action<Puzzle> OnPuzzleBroken;

    [TitleGroup("Settings")]
    [PropertyOrder(SettingsOrder)]
	[SerializeField]
	[InlineEditor]
	[OnValueChanged(nameof(SetData))]
	[InlineButton(nameof(CreatePuzzleDataAsset), "+")]
    [InfoBox("Set Puzzle data to start.", InfoMessageType.Error, VisibleIf = "@!" + nameof(data))]
    private PuzzleData data = default;

    [TitleGroup("Palette")]
    [PropertyOrder(PaletteOrder)]
	[ShowInInspector]
	[DisableIf(nameof(IsDataNull))]
	[LabelText("Palette")]
	[ListDrawerSettings(CustomAddFunction = nameof(AddToPalette))]
    [InfoBox("Puzzle has no colors in the palette.", InfoMessageType.Warning, nameof(IsPaletteEmpty))]
    private List<PaletteColor> PaletteInspector
    {
        get => data == null ? new List<PaletteColor>() : data.Palette;
        set { if (data != null) data.Palette = value; }
    }

    [PropertyOrder(SpacesOrder)]
	[TitleGroup("Spaces")]
    [SerializeField]
	[ReadOnly]
	[ShowIf(nameof(IsSpaceToolActive))]
    private List<SpacePiece> spaces = default;

    [PropertyOrder(PiecesOrder)]
	[TitleGroup("Pieces")]
    [SerializeField]
	[ReadOnly]
	[ShowIf(nameof(IsPieceToolOrDerivedActive))]
    private List<InteractablePiece> pieces = default;

    [TitleGroup("Conditions")]
    [FoldoutGroup("Conditions/Condition Set", 0)]
	[HideLabel]
    [PropertyOrder(0)]
    [SerializeField]
	[ReadOnly]
	[ShowIf(nameof(IsConditionToolActive))]
    private ConditionSets conditionSets = default;


    public PuzzleData Data => data;
    public IReadOnlyList<SpacePiece> Spaces => spaces;
    public IReadOnlyList<InteractablePiece> Pieces => pieces;
    public IReadOnlyList<PaletteColor> Palette => data.Palette;
    public ConditionSets ConditionSets => conditionSets;
    public float CellAbsoluteRadius => data.CellRadius * 0.5f;
    public float MinDragDelta => data.MinDragDelta;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetPuzzle();
        }
    }

    private void Awake()
    {
        Debug.Log($"Initializing puzzle {name} (pieces: {pieces.Count}).");
        for (int i = 0; i < pieces.Count; i++)
        {
            pieces[i].Initialize(this);
        }
        conditionSets.InitializeConditions(this);
        RegisterMove();
    }

    public void RegisterMove()
    {
        bool isAnyGroupSatisfied = false;
        for (int i = 0; i < conditionSets.ConditionGroups.Count; i++)
        {
            if (conditionSets.IsGroupSatisfied(i))
            {
                Debug.Log($"Condition group {i} in puzzle {name} completed.");
                isAnyGroupSatisfied = true;
                OnPuzzleCompleted?.Invoke(this, i);
            }
        }
        if (!isAnyGroupSatisfied)
        {
            OnPuzzleBroken?.Invoke(this);
        }
    }

    public void ResetPuzzle()
    {
        for (int i = 0; i < pieces.Count; i++)
        {
            Piece piece = pieces[i];
            piece.Restart();
        }
        RegisterMove();
    }

    private bool IsDataNull() => data == null;

#if UNITY_EDITOR
    private bool IsPaletteEmpty() => data == null ? false : data.Palette.Count == 0;
    private PaletteColor AddToPalette() => new PaletteColor() { color = Color.white, };

    private void CreatePuzzleDataAsset()
    {
        PuzzleData data = PuzzleData.CreateInstance<PuzzleData>();
        string path = EditorUtility.SaveFilePanelInProject("Create puzzle data asset", "PuzzleData", "asset", "Select a path for a new puzzle data asset.");
        if (string.IsNullOrWhiteSpace(path) == false)
        {
            AssetDatabase.CreateAsset(data, path);
            AssetDatabase.SaveAssets();
            SetData(data);
        }
    }

    private void SetData(PuzzleData data)
    {
        Debug.Log($"Updating puzzle {name}");
        this.conditionSets = new ConditionSets(this);
        this.pieces.Clear();
        this.spaces.Clear();
        this.piece = null;
        this.pieceData = null;
        this.modifier = null;
        this.space = null;
        this.spaceData = null;
        this.condition = null;
        this.conditionData = null;
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
        this.data = data;
        if (this.data == null)
        {
            return;
        }
        for (int i = 0; i < this.data.Spaces.Count; i++)
        {
            PieceData pieceData = this.data.Spaces[i];
            SpacePiece piecePrefab = PieceDatabase.Instance.SpacePrefab;
            SpacePiece piece = (SpacePiece)PrefabUtility.InstantiatePrefab(piecePrefab);
            piece.transform.SetParent(this.transform, false);
            piece.SetData(pieceData);
            spaces.Add(piece);
            piece.Load(pieceData);
            EditorUtility.SetDirty(piece);
            EditorUtility.SetDirty(pieceData);
        }
        for (int i = 0; i < this.data.Pieces.Count; i++)
        {
            PieceData pieceData = this.data.Pieces[i];
            InteractablePiece piecePrefab = PieceDatabase.Instance.PieceDictionary[pieceData.pieceIdentifier];
            InteractablePiece piece = (InteractablePiece)PrefabUtility.InstantiatePrefab(piecePrefab);
            piece.transform.SetParent(this.transform, false);
            piece.SetData(pieceData);
            pieces.Add(piece);
            piece.Load(pieceData);
            EditorUtility.SetDirty(piece);
            EditorUtility.SetDirty(pieceData);
        }
        for (int i = 0; i < this.data.ConditionGroups.Count; i++)
        {
            this.conditionSets.AddGroup();
            for (int j = 0; j < this.data.ConditionGroups[i].conditionDatas.Count; j++)
            {
                ConditionData conditionData = this.data.ConditionGroups[i].conditionDatas[j];
                Condition conditionPrefab = ConditionDatabase.Instance.ConditionDictionary[conditionData.conditionIdentifier];
                Condition condition = (Condition)PrefabUtility.InstantiatePrefab(conditionPrefab);
                condition.transform.SetParent(transform, false);
                condition.Data = conditionData;
                conditionSets.AddCondition(condition, i);
                condition.Load(conditionData);
                EditorUtility.SetDirty(condition);
            }
        }
    }
    [PropertyOrder(SettingsOrder + 10)]
    [Button("Clear Puzzle (Cannot Undo)")]
    [DisableIf(nameof(IsDataNull))]
    [GUIColor(1, 0.85f, 0.85f)]
    private void ClearPuzzle()
    {
        Debug.Log($"Clearing puzzle {name}");
        this.piece = null;
        this.pieceData = null;
        this.space = null;
        this.modifier = null;
        this.spaceData = null;
        this.condition = null;
        this.conditionData = null;

        this.pieces.Clear();
        this.spaces.Clear();
        this.conditionSets = new ConditionSets(this);
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
        foreach (PieceData pieceData in this.data.Pieces)
        {
            AssetDatabase.RemoveObjectFromAsset(pieceData);
            InteractablePieceData data = pieceData as InteractablePieceData;
            foreach (Modifier modifier in data.modifiers)
            {
                AssetDatabase.RemoveObjectFromAsset(modifier);
            }
        }
        foreach (PieceData spaceData in this.data.Spaces)
        {
            AssetDatabase.RemoveObjectFromAsset(spaceData);
        }
        foreach (ConditionGroupData conditionGroupData in this.data.ConditionGroups)
        {
            foreach (ConditionData conditionData in conditionGroupData.conditionDatas)
            {
                AssetDatabase.RemoveObjectFromAsset(conditionData);
            }
        }
        this.data.Pieces.Clear();
        this.data.Spaces.Clear();
        this.data.ConditionGroups.Clear();
        this.data.Palette.Clear();
    }
    [PropertyOrder(SettingsOrder + 11)]
    [Button]
    [DisableIf(nameof(IsDataNull))]
    private void ForceSaveData()
    {
        SaveData();
        AssetDatabase.SaveAssets();
    }
    private void SaveData()
    {
        Debug.Log($"Saving puzzle {name}");
        for (int i = 0; i < this.pieces.Count; i++)
        {
            PieceData pieceData = this.data.Pieces[i];
            this.pieces[i].Save(pieceData);
            this.data.Pieces[i] = pieceData;
            EditorUtility.SetDirty(pieceData);
        }
        int j = 0;
        foreach (ConditionGroupData groupData in this.conditionSets.SaveConditionsEnumerable())
        {
            this.data.ConditionGroups[j] = groupData;
            j += 1;
        }
        EditorUtility.SetDirty(this);
        EditorUtility.SetDirty(this.data);
    }

#endif

}
