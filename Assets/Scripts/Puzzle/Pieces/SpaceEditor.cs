#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.EditorTools;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector;
using ConditionGroups;
using System.Collections;
using System.Linq;

public partial class Puzzle
{

    [Space]
	[PropertyOrder(SpacesOrder + 2)]
	[NonSerialized]
	[HideLabel]
	[ShowInInspector]
	[ShowIf(nameof(IsSpaceToolActive))]
	[ValueDropdown(nameof(SpaceEnumerable))]
	[OnValueChanged(nameof(SetSpaceData))]
	[DisableIf(nameof(IsDataNull))]
    [InfoBox("Select a space from this puzzle in the Scene view.", InfoMessageType.Warning, VisibleIf = nameof(IsSpaceDataNull))]
    private SpacePiece space = default;

	[FoldoutGroup("Selected Space", Order = SpacesOrder + 3)]
	[NonSerialized]
	[ShowInInspector]
	[InlineEditor(InlineEditorObjectFieldModes.CompletelyHidden)]
	[ShowIf(nameof(IsSpaceToolActiveAndSelected))]
	[DisableIf(nameof(IsDataNull))]
    private SpacePieceData spaceData = default;


    [ButtonGroup("Tools/Buttons")]
	[PropertyOrder(0)]
	[LabelText("$" + nameof(GetEditorSpaceLabel))]
	[DisableIf(nameof(IsSpaceToolDisabled))]
    private void SetSpaceTool() => ToolManager.SetActiveTool<SpaceEditor>();
    private bool IsSpaceToolDisabled() => IsDataNull() || IsSpaceToolActive();
    private string GetEditorSpaceLabel() => IsSpaceToolActive() ? "- Spaces -" : "Spaces";
    public static bool IsSpaceToolActive() => ToolManager.activeToolType == typeof(SpaceEditor);

    private bool IsSpaceDataNull() => spaceData == null;
    private bool IsSpaceSelected() => space != null;
    private bool IsSpaceToolActiveAndSelected() => IsSpaceToolActive() && IsSpaceSelected();
    private void SetSpaceData(SpacePiece space) => spaceData = !space ? null : space.Data as SpacePieceData;





    [PropertyOrder(SpacesOrder + 4)]
    [Button]
    [ShowIf(nameof(IsSpaceToolActiveAndSelected))]
    [GUIColor(1, 0.85f, 0.85f)]
    [DisableIf(nameof(IsSelectedSpaceOccupied))]
    [LabelText("$"+nameof(DeleteSpaceLabel))]
    private void DeleteSelectedSpace()
    {
        if (IsSpaceOccupied(space))
        {
            return;
        }
        DeleteSpace(space);
        space = null;
        spaceData = null;
    }

    private void DeleteSpace(SpacePiece space)
    {
        AssetDatabase.RemoveObjectFromAsset(data.Spaces[space.Data.pieceIndex]);
        data.Spaces.RemoveAt(space.Data.pieceIndex);
        spaces.RemoveAt(space.Data.pieceIndex);
        FixIndicesInSpaceReferencers(space.Data.pieceIndex);

        Debug.Log($"Deleted space {space.name}.");
        DestroyImmediate(space.gameObject);
        EditorUtility.SetDirty(this);
        SaveData();
    }

    private bool IsSelectedSpaceOccupied() => data == null || space == null ? false : IsSpaceOccupied(space);
    private string DeleteSpaceLabel() => space != null && IsSpaceOccupied(space) ? "Cannot Delete (Space Occupied)" : "Delete Selected Space";
    private bool IsSpaceOccupied(SpacePiece space)
    {
        float sqrRadius = CellAbsoluteRadius * CellAbsoluteRadius;
        for (int j = 0; j < pieces.Count; j++)
        {
            for (int k = 0; k < pieces[j].Bodies.Count; k++)
            {
                Vector3 bodyPositionInPuzzle = pieces[j].GetBodyPositionInPuzzle(k);
                if (Utils.SqrDistance(bodyPositionInPuzzle, space.transform.localPosition) < sqrRadius)
                {
                    return true;
                }
            }
        }
        return false;
    }



    private void FixIndicesInSpaceReferencers(int removedSpaceIndex)
    {
        for (int i = 0; i < spaces.Count; i++)
        {
            ISpaceReferencer data = spaces[i].Data as ISpaceReferencer;
            if (data != null) data.OnSpaceRemoved(removedSpaceIndex);
        }
        for (int i = 0; i < pieces.Count; i++)
        {
            ISpaceReferencer data = pieces[i].Data as ISpaceReferencer;
            if (data != null) data.OnSpaceRemoved(removedSpaceIndex);
        }
        for (int i = 0; i < conditionSets.ConditionGroups.Count; i++)
        {
            ConditionGroup conditionGroup = conditionSets.ConditionGroups[i];
            for (int k = 0; k < conditionGroup.Conditions.Count; k++)
            {
                Condition condition = conditionGroup.Conditions[k];
                ISpaceReferencer data = condition.Data as ISpaceReferencer;
                if (data != null) data.OnSpaceRemoved(removedSpaceIndex);
            }
        }

    }


    private IEnumerable SpaceEnumerable()
    {
        ValueDropdownList<SpacePiece> list = new ValueDropdownList<SpacePiece>();
        if (IsSpaceToolActive() == false) return list;
        foreach (SpacePiece piece in spaces)
        {
            list.Add($"{piece.name} (index: {piece.Data.pieceIndex})", piece);
        }
        return list;
    }



    [EditorTool("Space Editor")]
    public class SpaceEditor : EditorTool
    {
        private bool isControlDown;

        public override void OnToolGUI(EditorWindow window)
        {
            GameObject selection = Selection.activeGameObject;
            if (selection == null) return;
            Puzzle puzzle = selection.GetComponent<Puzzle>();
            if (puzzle == null) return;


            if (puzzle.data == null)
            {
                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.red;
                Handles.Label(puzzle.transform.position, "No Data Set", style);
                return;
            }

            Color defaultColor = Handles.color;

            HandleInput(Event.current);
            AddSpaceMode(puzzle);
            SelectAndRemoveSpaceMode(puzzle);

            Handles.color = defaultColor;
        }

        private void HandleInput(Event evnt)
        {
            if (evnt.type == EventType.KeyDown && evnt.keyCode == KeyCode.LeftControl)
            {
                isControlDown = true;
            }
            if (evnt.type == EventType.KeyUp && evnt.keyCode == KeyCode.LeftControl)
            {
                isControlDown = false;
            }
            if (evnt.type == EventType.Layout)
            {
                HandleUtility.AddDefaultControl(0);
            }
        }

        private void AddSpaceMode(Puzzle puzzle)
        {
            const float size = 0.02f;
            SpacePiece prefab = PieceDatabase.Instance.SpacePrefab;
            Handles.color = Color.green;
            Handles.CapFunction func = Handles.CylinderHandleCap;
            if (puzzle.spaces.Count == 0)
            {
                if (Handles.Button(puzzle.transform.position, puzzle.transform.rotation, size, size, func))
                {
                    AddSpace(puzzle, prefab, Vector3.zero);
                }
            }
            else
            {
                HashSet<Vector3> unoccupiedSlots = GetAdjacentUnoccupiedSlots(puzzle);
                foreach (Vector3 position in unoccupiedSlots)
                {
                    if (Handles.Button(puzzle.transform.TransformPoint(position), puzzle.transform.rotation, size, size, func))
                    {
                        AddSpace(puzzle, prefab, position);
                    }
                }
            }
        }

        private void SelectAndRemoveSpaceMode(Puzzle puzzle)
        {
            const float size = 0.02f;
            if (isControlDown)
            {
                Handles.color = Color.red;
            }
            else
            {
                Handles.color = Color.white;
            }
            for (int i = 0; i < puzzle.spaces.Count; i++)
            {
                SpacePiece space = puzzle.spaces[i];
                if (puzzle.IsSpaceOccupied(space) && isControlDown)
                {
                    continue;
                }

                Handles.CapFunction func = puzzle.space == puzzle.spaces[i] ? (Handles.CapFunction)Handles.CubeHandleCap : (Handles.CapFunction)Handles.SphereHandleCap;
                if (Handles.Button(space.transform.position, puzzle.transform.rotation, size, size, func))
                {
                    if (isControlDown)
                    {
                        puzzle.DeleteSpace(space);
                        i -= 1;
                    }
                    else
                    {
                        puzzle.space = space;
                        puzzle.spaceData = space.Data as SpacePieceData;
                        EditorUtility.SetDirty(puzzle);
                    }
                }
            }
        }

        private static void AddSpace(Puzzle puzzle, SpacePiece prefab, Vector3 position)
        {
            SpacePiece space = (SpacePiece)PrefabUtility.InstantiatePrefab(prefab);
            space.transform.SetParent(puzzle.transform, false);
            space.transform.localPosition = position;
            SpacePieceData data = space.CreateData() as SpacePieceData;
            data.name = space.name;
            space.Save(data);
            space.SetData(data);
            space.Data.pieceIndex = puzzle.spaces.Count;
            puzzle.data.Spaces.Add(data);
            puzzle.spaces.Add(space);
            puzzle.space = space;
            puzzle.spaceData = data;
            space.UpdateDataInPuzzle(puzzle);
            AssetDatabase.AddObjectToAsset(data, puzzle.data);
            puzzle.SaveData();
            EditorUtility.SetDirty(data);
            EditorUtility.SetDirty(space);
            EditorUtility.SetDirty(puzzle);
            EditorUtility.SetDirty(puzzle.Data);
        }

        private HashSet<Vector3> GetAdjacentUnoccupiedSlots(Puzzle puzzle)
        {
            float sqrEpsilon = puzzle.CellAbsoluteRadius * puzzle.CellAbsoluteRadius;
            HashSet<Vector3> unoccupiedSlots = new HashSet<Vector3>();
            for (int i = 0; i < puzzle.spaces.Count; i++)
            {
                SpacePiece space = puzzle.spaces[i];
                for (int j = 0; j < space.Connections.Length; j++)
                {
                    Vector3 slotPosition = space.GetDirectionPositionLocalToPuzzle(j).Round(5);
                    unoccupiedSlots.Add(slotPosition);
                }
            }
            for (int i = 0; i < puzzle.spaces.Count; i++)
            {
                SpacePiece space = puzzle.spaces[i];
                Vector3 piecePosition = space.transform.localPosition.Round(5);
                foreach (Vector3 unoccupiedSlot in unoccupiedSlots)
                {
                    if (Utils.SqrDistance(unoccupiedSlot, piecePosition) < sqrEpsilon)
                    {
                        unoccupiedSlots.Remove(unoccupiedSlot);
                        break;
                    }
                }
            }
            return unoccupiedSlots;
        }


    }
}

#endif