using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public partial class Puzzle
{
    private class DominoModifierSpecialization : ModifierEditor.Specialization<DominoModifier>
    {
        protected override void Run()
        {
            const float size = 0.02f;
            for (int i = 0; i < PieceData.occupiedSpaceIndices.Length; i++)
            {
                SpacePiece space = Puzzle.spaces[PieceData.occupiedSpaceIndices[i]];
                SpacePieceData spaceData = space.Data as SpacePieceData;
                for (int j = 0; j < spaceData.adjacentSpaceIndices.Length; j++)
                {
                    if (PieceData.occupiedSpaceIndices.Contains(spaceData.adjacentSpaceIndices[j]))
                    {
                        continue;
                    }
                    int socketIndex = Modifier.sockets.FindIndex(x => x.occupiedSpaceIndex == i && x.spaceDirectionIndex == j);
                    Handles.color = socketIndex == -1 ? Color.green : Color.red;
                    if (Handles.Button((space.transform.position + space.Connections[j].transform.position) / 2, Puzzle.transform.rotation, size, size, Handles.SphereHandleCap))
                    {
                        if (socketIndex == -1)
                        {
                            Modifier.sockets.Add(new DominoModifier.Socket()
                            {
                                occupiedSpaceIndex = i,
                                spaceDirectionIndex = j
                            });
                            EditorUtility.SetDirty(Modifier);
                            EditorUtility.SetDirty(Puzzle);
                        }
                        else
                        {
                            Modifier.sockets.RemoveAt(socketIndex);
                            EditorUtility.SetDirty(Modifier);
                            EditorUtility.SetDirty(Puzzle);
                        }
                    }
                }
            }

        }
    }
}
