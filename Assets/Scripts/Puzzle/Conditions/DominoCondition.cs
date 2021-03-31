using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
public class DominoCondition : Condition
{
    private struct Model
    {
        public DominoModifier modifier;
        public InteractablePiece piece;
        public InteractablePieceData pieceData;
        public DominoModifier.Socket socket;
        public DominoNode node;
    }

    [SerializeField]
    private DominoNode nodePrefab = default;

    private Dictionary<int, Dictionary<InteractablePiece, Model>> dominos = new Dictionary<int, Dictionary<InteractablePiece, Model>>();

    public override string ConditionId => "Domino";

    public override ConditionData CreateData() => ScriptableObject.CreateInstance<DominoConditionData>();

    public override void Initialize(Puzzle master)
    {
        base.Initialize(master);
        foreach (InteractablePiece piece in master.Pieces)
        {
            InteractablePieceData data = piece.Data as InteractablePieceData;
            for (int i = 0; i < data.modifiers.Count; i++)
            {
                DominoModifier dominoModifier = data.modifiers[i] as DominoModifier;
                if (dominoModifier != null)
                {
                    foreach (DominoModifier.Socket socket in dominoModifier.sockets)
                    {
                        if (dominos.TryGetValue(socket.value, out Dictionary<InteractablePiece, Model> modifiers) == false)
                        {
                            modifiers = new Dictionary<InteractablePiece, Model>();
                            dominos.Add(socket.value, modifiers);
                        }

                        DominoNode node = Instantiate(nodePrefab, piece.transform, false);
                        node.Initialize(this, data, socket);

                        Model model = new Model()
                        {
                            modifier = dominoModifier,
                            piece = piece,
                            socket = socket,
                            node = node,
                            pieceData = piece.Data as InteractablePieceData,
                        };
                        modifiers.Add(piece, model);
                    }

                }
            }
        }
    }
    public override bool IsSatisfied()
    {
        DominoConditionData data = Data as DominoConditionData;
        foreach (int value in data.Values)
        {
            if (dominos.TryGetValue(value, out Dictionary<InteractablePiece, Model> valueDominos))
            {
                foreach ((InteractablePiece piece, Model model) in valueDominos)
                {
                    DominoModifier.Socket socket = model.socket;
                    SpacePiece space = Master.Spaces[model.pieceData.occupiedSpaceIndices[socket.occupiedSpaceIndex]];
                    SpacePieceData spaceData = space.Data as SpacePieceData;
                    int adjacentSpaceIndex = spaceData.adjacentSpaceIndices[socket.spaceDirectionIndex];
                    if (adjacentSpaceIndex == -1)
                    {
                        /// This socket is at the edge of the puzzle.
                        return false;
                    }

                    SpacePiece adjacentSpace = Master.Spaces[adjacentSpaceIndex];
                    if (!adjacentSpace.IsOccupied())
                    {
                        /// Space adjacent to the socket is not occupied.
                        return false;
                    }

                    Vector3 socketDirection = space.GetDirectionLocalToPuzzle(socket.spaceDirectionIndex);
                    int adjacentSocketDirectionIndex = adjacentSpace.GetClosestDirectionIndex(-socketDirection, 90);
                    bool dominoMatched = IsDominoMatched(valueDominos, piece, adjacentSpace, adjacentSocketDirectionIndex);
                    if (!dominoMatched)
                    {
                        /// Currently checked domino socket is not matched.
                        return false;
                    }
                }
            }
        }
        return true;
    }

    private bool IsDominoMatched(Dictionary<InteractablePiece, Model> valueDominos, InteractablePiece piece, SpacePiece adjacentSpace, int adjacentSocketDirectionIndex)
    {
        foreach ((InteractablePiece otherPiece, Model otherModel) in valueDominos)
        {
            if (piece == otherPiece)
            {
                continue;
            }
            if (Master.Spaces[otherModel.pieceData.occupiedSpaceIndices[otherModel.socket.occupiedSpaceIndex]] != adjacentSpace)
            {
                continue;
            }
            if (otherModel.socket.spaceDirectionIndex != adjacentSocketDirectionIndex)
            {
                continue;
            }
            return true;
        }

        return false;
    }

}
