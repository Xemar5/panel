#if UNITY_EDITOR
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

public partial class Puzzle
{
    [TitleGroup("Modifiers")]
    [PropertyOrder(ModifiersOrder + 2)]
    [NonSerialized]
    [HideLabel]
    [ShowInInspector]
    [ValueDropdown(nameof(ModifierEnumerable))]
    [ShowIf(nameof(IsModifierToolActive))]
    [EnableIf(nameof(IsPieceSelected))]
    [InfoBox("$"+nameof(GetModifiersToolInfoBoxMessage), InfoMessageType.Warning, VisibleIf = "@!"+nameof(IsModifierSelected)+"()")]
    private Modifier modifier = default;

    [BoxGroup("Modifiers/Box", Order = ModifiersOrder + 3, ShowLabel = false)]
    [HideLabel]
    [ShowInInspector]
    [ShowIf(nameof(IsModifierToolActiveAndSelected))]
    [DisableIf(nameof(IsDataNull))]
    [InlineEditor(InlineEditorObjectFieldModes.CompletelyHidden)]
    private Modifier Modifier
    {
        get => modifier;
        set => modifier = value;
    }


    [TitleGroup("Tools", order: ToolsOrder - 10)]
    [ButtonGroup("Tools/Buttons", -1)]
    [PropertyOrder(1.5f)]
    [LabelText("$" + nameof(GetModifiersToolLabel))]
    [DisableIf(nameof(IsModifierToolDisabled))]
    private void SetModifierTool() => ToolManager.SetActiveTool<ModifierEditor>();
    private bool IsModifierSelected() => modifier != null;
    private bool IsModifierToolActiveAndSelected() => IsModifierToolActive() && IsModifierSelected();
    private bool IsModifierToolDisabled() => IsDataNull() || IsModifierToolActive();
    private string GetModifiersToolLabel() => IsModifierToolActive() ? "- Modifiers -" : "Modifiers";
    private string GetModifiersToolInfoBoxMessage() => IsPieceSelected() ? "Select an existing modifier or add a new one." : "Select an existing piece first.";
    public static bool IsModifierToolActive() => ToolManager.activeToolType == typeof(ModifierEditor);

    [TitleGroup("Modifiers")]
    [PropertyOrder(ModifiersOrder + 1)]
    [Button]
    [ShowIf(nameof(IsPieceToolOrDerivedActiveAndSelected))]
    private void AddModifier()
    {
        ModifierCreationPopup popup = ScriptableObject.CreateInstance<ModifierCreationPopup>();
        popup.OnFinished += OnModifierCreated;
        popup.PieceData = pieceData as InteractablePieceData;
        popup.position = new Rect(0, 0, 300, 30);
        popup.ShowAuxWindow();

    }
    private void OnModifierCreated(Modifier modifier)
    {
        InteractablePieceData data = pieceData as InteractablePieceData;
        modifier.name = $"{piece.name} - {modifier.GetType().Name}";
        data.modifiers.Add(modifier);
        this.modifier = modifier;
        AssetDatabase.AddObjectToAsset(modifier, data);
        EditorUtility.SetDirty(modifier);
        EditorUtility.SetDirty(piece);
        EditorUtility.SetDirty(data);
        EditorUtility.SetDirty(this);
    }

    [PropertyOrder(ModifiersOrder + 10)]
    [Button]
    [ShowIf(nameof(IsModifierToolActiveAndSelected))]
    [GUIColor(1, 0.85f, 0.85f)]
    private void DeleteSelectedModifier()
    {
        InteractablePieceData data = pieceData as InteractablePieceData;
        Modifier modifier = this.modifier;
        this.modifier = null;
        data.modifiers.Remove(modifier);
        AssetDatabase.RemoveObjectFromAsset(modifier);
        EditorUtility.SetDirty(piece);
        EditorUtility.SetDirty(data);
        EditorUtility.SetDirty(this);
    }

    private IEnumerable ModifierEnumerable()
    {
        ValueDropdownList<Modifier> list = new ValueDropdownList<Modifier>();
        if (!IsModifierToolActive() || !IsPieceSelected()) return list;
        InteractablePieceData data = piece.Data as InteractablePieceData;
        for (int i = 0; i < data.modifiers.Count; i++)
        {
            Modifier modifier = data.modifiers[i];
            list.Add($"{modifier.name} (index: {i})", modifier);
        }
        return list;
    }


    [EditorTool("Modifier Editor")]
    private class ModifierEditor : PieceEditor
    {
        public abstract class Specialization
        {
            protected Puzzle Puzzle { get; private set; }
            protected InteractablePiece Piece { get; private set; }
            protected InteractablePieceData PieceData { get; private set; }

            public bool Run(Puzzle puzzle)
            {
                this.Puzzle = puzzle;
                if (puzzle == null)
                {
                    return false;
                }
                this.Piece = puzzle.piece as InteractablePiece;
                if (this.Piece == null)
                {
                    return false;
                }
                this.PieceData = this.Piece.Data as InteractablePieceData;
                if (this.PieceData == null)
                {
                    return false;
                }
                return Run();
            }
            protected abstract bool Run();
        }
        public abstract class Specialization<M> : Specialization
            where M : Modifier
        {
            protected M Modifier { get; private set; }

            protected override bool Run()
            {
                if (Puzzle.modifier == null)
                {
                    return false;
                }
                if (Puzzle.modifier is M modifier)
                {
                    this.Modifier = modifier;
                    return true;
                }
                else
                {
                    return false;
                }

            }
        }

        private Specialization[] specializations = default;


        public override void OnToolGUI(EditorWindow window)
        {
            if (specializations == null)
            {
                specializations = GatherModifierSpecializations();
            }

            if (isShiftDown)
            {
                base.OnToolGUI(window);
                return;
            }

            GameObject selection = Selection.activeGameObject;
            if (selection == null)
            {
                return;
            }
            Puzzle puzzle = selection.GetComponent<Puzzle>();
            if (puzzle == null)
            {
                return;
            }

            puzzle.piecePrefab = null;
            DestroyPieceGhost();

            if (puzzle.data == null)
            {
                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.red;
                Handles.Label(puzzle.transform.position, "No Data Set", style);
                return;
            }


            HandleInput(Event.current);
            ShowModifiedPiece(puzzle);
            if (puzzle.piece != null && puzzle.modifier != null)
            {
                foreach (Specialization specialization in specializations)
                {
                    specialization.Run(puzzle);
                }
            }
        }

        private static void ShowModifiedPiece(Puzzle puzzle)
        {
            const float size = 0.02f;
            if (puzzle.piece != null)
            {
                Handles.Button(puzzle.piece.transform.position, puzzle.transform.rotation, size, size, Handles.CubeHandleCap);
            }
        }



        private Specialization[] GatherModifierSpecializations()
        {
            Type[] types = Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .Where(IsTypeModifierEditorSpecialization)
                .ToArray();

            Specialization[] specializations = new Specialization[types.Length];
            for (int i = 0; i < types.Length; i++)
            {
                Type type = (Type)types[i];
                specializations[i] = (Specialization)Activator.CreateInstance(type);
                Debug.Log($"Modifier specialization {types[i].Name} added.");
            }
            Debug.Log($"Modifier specializations count: {specializations.Length}.");
            return specializations;
        }
        private static bool IsTypeModifierEditorSpecialization(Type x)
        {
            if (!typeof(Specialization).IsAssignableFrom(x)) return false;
            while (x != typeof(Specialization) && x.BaseType != null && (!x.IsGenericType || x.GetGenericTypeDefinition() != typeof(Specialization<>)))
                x = x.BaseType;
            if (!typeof(Specialization).IsAssignableFrom(x)) return false;
            if (!x.IsConstructedGenericType) return false;
            return true;
        }



    }
}

#endif