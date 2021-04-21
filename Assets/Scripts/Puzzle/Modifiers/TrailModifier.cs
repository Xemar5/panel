using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class TrailModifier : Modifier
{
    private const string Simple1x1Id = "Simple/1x1";

    [SerializeField]
    [ValueDropdown(PieceDatabase.KeysString)]
    private string piecePrefabId = Simple1x1Id;
    [NonSerialized]
    private List<InteractablePiece> trailPieces = new List<InteractablePiece>();


    protected override void Initialize()
    {
        ColoredModifier colored = null;
        foreach (Modifier modifier in base.OwnerData.modifiers)
        {
            if (modifier is ColoredModifier coloredModifier)
            {
                colored = coloredModifier;
                break;
            }
        }
        foreach (Modifier modifier in base.OwnerData.modifiers)
        {
            void onMovePerformed(MovableModifier _, int[] previousSpaces, int[] newSpaces)
            {
                OnDisplacementPerformed(colored, previousSpaces, newSpaces);
            }
            void onRotationPerformed(RotatableModifier _, int[] previousSpaces, int[] newSpaces)
            {
                OnDisplacementPerformed(colored, previousSpaces, newSpaces);
            }

            if (modifier is MovableModifier movable)
            {
                movable.OnMovePerformed += onMovePerformed;
                break;
            }
            if (modifier is RotatableModifier rotatable)
            {
                rotatable.OnRotationPerformed += onRotationPerformed;
                break;
            }
        }

    }

    private void OnDisplacementPerformed(ColoredModifier colored, int[] previousSpaces, int[] newSpaces)
    {
        List<int> previousExclusiveSpaces = GetExclusivePreviousSpaces(previousSpaces, newSpaces);
        foreach (int spaceIndex in previousExclusiveSpaces)
        {
            InteractablePiece piecePrefab = PieceDatabase.Instance.PieceDictionary[piecePrefabId];
            InteractablePieceData interactablePieceData = piecePrefab.CreateData() as InteractablePieceData;

            ColoredModifier clonedColored = Instantiate(colored);
            interactablePieceData.modifiers.Add(clonedColored);
            interactablePieceData.occupiedSpaceIndices = new int[] { spaceIndex };
            interactablePieceData.localPosition = Master.Spaces[spaceIndex].transform.localPosition;
            interactablePieceData.localRotation = Quaternion.identity;

            Piece piece = Master.AddPiece(piecePrefab, interactablePieceData);
            InteractablePiece interactablePiece = piece as InteractablePiece;
            trailPieces.Add(interactablePiece);
        }
    }

    private static List<int> GetExclusivePreviousSpaces(int[] previousSpaces, int[] newSpaces)
    {
        List<int> previousExclusiveSpaces = new List<int>();
        for (int i = 0; i < previousSpaces.Length; i++)
        {
            int previousSpaceIndex = previousSpaces[i];
            if (!IsSpaceOcupiedInAfterMove(newSpaces, previousSpaceIndex))
            {
                previousExclusiveSpaces.Add(previousSpaceIndex);
            }
        }

        return previousExclusiveSpaces;
    }

    private static bool IsSpaceOcupiedInAfterMove(int[] newSpaces, int previousSpaceIndex)
    {
        for (int j = 0; j < newSpaces.Length; j++)
        {
            if (newSpaces[j] == previousSpaceIndex)
            {
                return true;
            }
        }
        return false;
    }

    protected override void Restart(InteractablePieceData previousPieceData, InteractablePieceData restartedPieceData, Modifier previousModifierData)
    {
        foreach (InteractablePiece piece in trailPieces)
        {
            Master.RemovePiece(piece);
            Destroy(piece.gameObject);
        }
        trailPieces.Clear();
    }

}
