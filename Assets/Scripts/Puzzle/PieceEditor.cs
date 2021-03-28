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

public partial class Puzzle
{

    [PropertyOrder(PiecesOrder + 1.5f)]
    [NonSerialized]
    [ShowInInspector]
    [ValueDropdown(nameof(PiecePrefabEnumerable))]
    [ShowIf(nameof(IsPieceToolActive))]
    [DisableIf(nameof(IsDataNull))]
    private Piece piecePrefab = default;

    [PropertyOrder(PiecesOrder + 1.6f)]
    [NonSerialized]
    [ShowInInspector]
    [ShowIf(nameof(IsPieceToolActive))]
    [DisableIf(nameof(IsDataNull))]
    private Quaternion piecePrefabRotation = Quaternion.identity;

    [PropertySpace]
    [PropertyOrder(PiecesOrder + 2)]
    [NonSerialized]
    [HideLabel]
    [ShowInInspector]
    [ValueDropdown(nameof(PieceEnumerable))]
    [OnValueChanged(nameof(SetPieceData))]
    [ShowIf(nameof(IsPieceToolOrDerivedActive))]
    [DisableIf(nameof(IsDataNull))]
    [InfoBox("Select a piece from this puzzle in the Scene view.", InfoMessageType.Warning, VisibleIf = nameof(IsPieceDataNull))]
    private Piece piece = default;

    [FoldoutGroup("Selected Piece", Order = PiecesOrder + 4)]
    [NonSerialized]
    [ShowInInspector]
    [InlineEditor(InlineEditorObjectFieldModes.CompletelyHidden)]
    [ShowIf(nameof(IsPieceToolOrDerivedActiveAndSelected))]
    [DisableIf(nameof(IsDataNull))]
    private PieceData pieceData = default;


    [TitleGroup("Tools", order: ToolsOrder - 10), ButtonGroup("Tools/Buttons", -1), PropertyOrder(1), LabelText("$" + nameof(GetPieceToolLabel)), DisableIf(nameof(IsPiecesToolDisabled))]
    private void SetPiecesTool() => ToolManager.SetActiveTool<PieceEditor>();
    private bool IsPiecesToolDisabled() => IsDataNull() || IsPieceToolActive();
    private string GetPieceToolLabel() => IsPieceToolActive() ? "- Pieces -" : "Pieces";
    public static bool IsPieceToolOrDerivedActive() => typeof(PieceEditor).IsAssignableFrom(ToolManager.activeToolType);
    public static bool IsPieceToolActive() => typeof(PieceEditor) == ToolManager.activeToolType;


    private bool IsPieceDataNull() => pieceData == null;
    private bool IsPieceSelected() => piece != null;
    private bool IsPieceToolActiveAndSelected() => IsPieceToolActive() && IsPieceSelected();
    private bool IsPieceToolOrDerivedActiveAndSelected() => IsPieceToolOrDerivedActive() && IsPieceSelected();
    private void SetPieceData(Piece piece)
    {
        pieceData = !piece ? null : piece.Data;
        modifier = null; 
    }



    [PropertyOrder(PiecesOrder + 4)]
    [Button]
    [ShowIf(nameof(IsPieceToolActiveAndSelected))]
    [GUIColor(1, 0.85f, 0.85f)]
    private void DeleteSelectedPiece()
    {
        AssetDatabase.RemoveObjectFromAsset(this.data.Pieces[piece.Data.pieceIndex]);
        this.data.Pieces.RemoveAt(piece.Data.pieceIndex);
        pieces.RemoveAt(piece.Data.pieceIndex);
        InteractablePieceData data = piece.Data as InteractablePieceData;
        for (int i = 0; i < data.modifiers.Count; i++)
        {
            AssetDatabase.RemoveObjectFromAsset(data.modifiers[i]);
        }
        for (int i = piece.Data.pieceIndex; i < pieces.Count; i++)
        {
            pieces[i].Data.pieceIndex -= 1;
            EditorUtility.SetDirty(pieces[i]);
        }
        Debug.Log($"Deleted piece {piece.name}.");
        DestroyImmediate(piece.gameObject);
        piece = null;
        pieceData = null;
        modifier = null;
        EditorUtility.SetDirty(this);
        SaveData();
    }

    public static Puzzle GetSelectedPuzzle()
    {
        GameObject activeGameObject = UnityEditor.Selection.activeGameObject;
        if (activeGameObject == null) return null;
        Puzzle puzzle = activeGameObject.GetComponent<Puzzle>();
        return puzzle;
    }


    private IEnumerable PieceEnumerable()
    {
        ValueDropdownList<Piece> list = new ValueDropdownList<Piece>();
        if (IsPieceToolOrDerivedActive() == false) return list;
        foreach (Piece piece in pieces)
        {
            list.Add($"{piece.name} (index: {piece.Data.pieceIndex})", piece);
        }
        return list;
    }
    private IEnumerable PiecePrefabEnumerable()
    {
        ValueDropdownList<Piece> list = new ValueDropdownList<Piece>();
        if (IsPieceToolActive() == false) return list;
        foreach (Piece piece in PieceDatabase.Instance.OrderedPieceMenuList)
        {
            list.Add(piece.ContextPath, piece);
        }
        return list;
    }




    [EditorTool("Piece Editor")]
    public class PieceEditor : EditorTool
    {
        private Vector3 mousePosition;
        private Vector3? lastPuzzlePosition;
        private InteractablePiece selectedPiecePrefab;
        private MeshRenderer selectedPieceGhost;
        private int lastSpaceIndex;
        protected bool isControlDown;
        protected bool isShiftDown;
        protected bool onClick = true;

        public override void OnActivated()
        {
            base.OnActivated();
            GameObject selection = Selection.activeGameObject;
            if (selection == null) return;
            Puzzle puzzle = selection.GetComponent<Puzzle>();
            if (puzzle == null) return;

            CreateGhost(puzzle.piecePrefab);
        }
        public override void OnWillBeDeactivated()
        {
            base.OnWillBeDeactivated();
            DestroyPieceGhost();
        }

        public override void OnToolGUI(EditorWindow window)
        {
            GameObject selection = Selection.activeGameObject;
            if (selection == null)
            {
                DestroyPieceGhost();
                return;
            }
            Puzzle puzzle = selection.GetComponent<Puzzle>();
            if (puzzle == null)
            {
                DestroyPieceGhost();
                return;
            }


            if (puzzle.data == null)
            {
                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.red;
                Handles.Label(puzzle.transform.position, "No Data Set", style);
                DestroyPieceGhost();
                return;
            }

            Color defaultColor = Handles.color;

            HandleInput(Event.current);
            RaycastMouse(puzzle);
            HandleGhost(puzzle);
            PieceAddMode(puzzle);
            PieceSelectMode(puzzle);

            Handles.color = defaultColor;
            onClick = false;
        }

        private void HandleGhost(Puzzle puzzle)
        {
            CreateGhost(puzzle.piecePrefab);
            if (selectedPieceGhost != null)
            {
                lastSpaceIndex = -1;
                if (!lastPuzzlePosition.HasValue)
                {
                    // Do nothing
                }
                else if (!IsGhostPlaceable(puzzle, lastPuzzlePosition.Value))
                {
                    // Do nothing
                }
                else
                {
                    lastSpaceIndex = GetEmptySpaceAtPosition(puzzle, lastPuzzlePosition.Value);
                }

                Vector3 position;
                if (lastSpaceIndex != -1)
                {
                    position = puzzle.spaces[lastSpaceIndex].transform.position;
                    selectedPieceGhost.sharedMaterial.color = Color.white;
                }
                else if (lastPuzzlePosition.HasValue)
                {
                    position = puzzle.transform.TransformPoint(lastPuzzlePosition.Value);
                    selectedPieceGhost.sharedMaterial.color = Color.red;
                }
                else
                {
                    selectedPieceGhost.gameObject.SetActive(false);
                    return;
                }
                selectedPieceGhost.gameObject.SetActive(true);
                selectedPieceGhost.transform.position = position;
                selectedPieceGhost.transform.localScale = puzzle.transform.lossyScale;
                selectedPieceGhost.transform.rotation = puzzle.transform.rotation * puzzle.piecePrefabRotation;
                SceneView.lastActiveSceneView.Repaint();
            }
        }

        private bool IsGhostPlaceable(Puzzle puzzle, Vector3 position)
        {
            float avreageScale = puzzle.transform.lossyScale.Avreage();

            for (int j = 0; j < selectedPiecePrefab.Bodies.Count; j++)
            {
                Vector3 positionInPuzzle = puzzle.piecePrefabRotation * selectedPiecePrefab.Bodies[j].localPosition;
                Vector3 bodyPosition = position + positionInPuzzle;
                int hoveredPieceIndex = GetPieceAtPosition(puzzle, bodyPosition);
                if (hoveredPieceIndex != -1)
                {
                    return false;
                }
                int spaceIndex = GetEmptySpaceAtPosition(puzzle, bodyPosition);
                if (spaceIndex == -1)
                {
                    return false;
                }
            }

            return true;
        }

        private void RaycastMouse(Puzzle puzzle)
        {
            Plane plane = new Plane(puzzle.transform.forward, puzzle.transform.position);
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
            if (plane.Raycast(ray, out float enter))
            {
                lastPuzzlePosition = puzzle.transform.InverseTransformPoint(ray.GetPoint(enter));
            }
            else
            {
                lastPuzzlePosition = null;
            }
        }

        private static int GetEmptySpaceAtPosition(Puzzle puzzle, Vector3 position)
        {
            float sqrRadius = puzzle.CellAbsoluteRadius * puzzle.CellAbsoluteRadius;
            for (int i = 0; i < puzzle.spaces.Count; i++)
            {
                SpacePiece space = puzzle.spaces[i];
                if (space.OccupyCount() > 0)
                {
                    continue;
                }
                float sqrDistance = Utils.SqrDistance(space.transform.localPosition, position);
                if (sqrDistance < sqrRadius)
                {
                    return i;
                }
            }
            return -1;
        }

        private static int GetPieceAtPosition(Puzzle puzzle, Vector3 position)
        {
            float sqrRadius = puzzle.CellAbsoluteRadius * puzzle.CellAbsoluteRadius;
            for (int i = 0; i < puzzle.pieces.Count; i++)
            {
                InteractablePiece interactablePiece = puzzle.pieces[i];
                for (int j = 0; j < interactablePiece.Bodies.Count; j++)
                {
                    float sqrDistance = Utils.SqrDistance(interactablePiece.GetBodyPositionInPuzzle(j), position);
                    if (sqrDistance < sqrRadius)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        private void CreateGhost(Piece piecePrefab)
        {
            if (selectedPiecePrefab == piecePrefab)
            {
                return;
            }
            if (selectedPiecePrefab != piecePrefab)
            {
                DestroyPieceGhost();
                selectedPiecePrefab = (InteractablePiece)piecePrefab;
                selectedPieceGhost = new GameObject("PuzzleGhost").AddComponent<MeshRenderer>();
                selectedPieceGhost.material = new Material(Shader.Find("Unlit/Color"));
                MeshFilter meshFilter = selectedPieceGhost.gameObject.AddComponent<MeshFilter>();
                MeshFilter[] filters = selectedPiecePrefab.GetComponentsInChildren<MeshFilter>();
                CombineInstance[] combines = new CombineInstance[filters.Length];

                for (int i = 0; i < filters.Length; i++)
                {
                    combines[i].mesh = filters[i].sharedMesh;
                    combines[i].transform = filters[i].transform.localToWorldMatrix;
                }
                meshFilter.sharedMesh = new Mesh();
                meshFilter.sharedMesh.CombineMeshes(combines);
                selectedPieceGhost.gameObject.hideFlags = HideFlags.HideAndDontSave;
                selectedPieceGhost.gameObject.SetActive(false);
            }
        }

        protected void DestroyPieceGhost()
        {
            if (selectedPieceGhost != null)
            {
                DestroyImmediate(selectedPieceGhost.gameObject);
            }
            selectedPieceGhost = null;
            selectedPiecePrefab = null;
        }

        protected virtual void HandleInput(Event evnt)
        {
            if (evnt.type == EventType.KeyDown && evnt.keyCode == KeyCode.LeftControl)
            {
                isControlDown = true;
            }
            if (evnt.type == EventType.KeyUp && evnt.keyCode == KeyCode.LeftControl)
            {
                isControlDown = false;
            }
            if (evnt.type == EventType.KeyDown && evnt.keyCode == KeyCode.LeftShift)
            {
                isShiftDown = true;
            }
            if (evnt.type == EventType.KeyUp && evnt.keyCode == KeyCode.LeftShift)
            {
                isShiftDown = false;
            }
            if (evnt.type == EventType.MouseMove && SceneView.lastActiveSceneView.camera != null)
            {
                mousePosition = evnt.mousePosition;
            }
            if (evnt.type == EventType.Layout)
            {
                HandleUtility.AddDefaultControl(0);
            }
            if (evnt.type == EventType.MouseDown)
            {
                onClick = true;
            }
            if (evnt.type == EventType.MouseUp)
            {
                onClick = false;
            }
        }

        private void PieceSelectMode(Puzzle puzzle)
        {
            if (puzzle.pieces.Count == 0)
            {
                return;
            }
            const float size = 0.02f;
            for (int i = 0; i < puzzle.pieces.Count; i++)
            {
                if (isControlDown)
                {
                    Handles.color = Color.red;
                }
                Handles.CapFunction func = puzzle.piece == puzzle.pieces[i] ? (Handles.CapFunction)Handles.CubeHandleCap : (Handles.CapFunction)Handles.SphereHandleCap;
                Handles.Button(puzzle.pieces[i].transform.position, puzzle.transform.rotation, size, size, func);
            }


            if (!lastPuzzlePosition.HasValue)
            {
                return;
            }
            if (onClick == false)
            {
                return;
            }

            int pieceIndex = GetPieceAtPosition(puzzle, lastPuzzlePosition.Value);
            if (pieceIndex == -1)
            {
                return;
            }


            if (isControlDown)
            {
                puzzle.piece = puzzle.pieces[pieceIndex];
                puzzle.pieceData = puzzle.piece.Data;
                puzzle.modifier = null;
                puzzle.DeleteSelectedPiece();
            }
            else
            {
                puzzle.piece = puzzle.pieces[pieceIndex];
                puzzle.pieceData = puzzle.piece.Data;
                puzzle.modifier = null;
                EditorUtility.SetDirty(puzzle);
            }
        }

        private void PieceAddMode(Puzzle puzzle)
        {
            if (puzzle.spaces.Count == 0)
            {
                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.red;
                Handles.Label(puzzle.transform.position, "No available spaces", style);
                return;
            }

            if (puzzle.piecePrefab == null)
            {
                return;
            }
            if (lastSpaceIndex == -1)
            {
                return;
            }

            if (onClick)
            {
                OnCreatePiece(puzzle, selectedPiecePrefab, puzzle.spaces[lastSpaceIndex].transform.localPosition);
            }
            

        }

        private void OnCreatePiece(Puzzle puzzle, Piece piecePrefab, Vector3 position) 
        {
            InteractablePiece piece = (InteractablePiece)PrefabUtility.InstantiatePrefab(piecePrefab);
            piece.transform.SetParent(puzzle.transform, false);
            piece.transform.localPosition = position;
            piece.transform.localRotation = puzzle.piecePrefabRotation;
            InteractablePieceData data = (InteractablePieceData)piece.CreateData();
            MovableModifier movable = CreateInstance<MovableModifier>();
            movable.name = $"{piece.name} - MovableModifier";
            data.name = piece.name;
            piece.Save(data);
            piece.SetData(data);
            data.modifiers.Add(movable);
            piece.Data.pieceIndex = puzzle.pieces.Count;
            puzzle.data.Pieces.Add(data);
            puzzle.pieces.Add(piece);
            puzzle.piece = piece;
            puzzle.SetPieceData(piece);
            data.OnPieceMoved(piece, puzzle);
            AssetDatabase.AddObjectToAsset(data, puzzle.data);
            AssetDatabase.AddObjectToAsset(movable, data);
            EditorUtility.SetDirty(data);
            EditorUtility.SetDirty(movable);
            EditorUtility.SetDirty(piece);
            EditorUtility.SetDirty(puzzle);
            EditorUtility.SetDirty(puzzle.Data);
            puzzle.SaveData();
        }


    }
}

#endif