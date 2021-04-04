using UnityEngine;

public class DominoModifierNode : MonoBehaviour
{

    public void Initialize(DominoCondition condition, InteractablePieceData pieceData, DominoModifier.Socket socket)
    {
        SpacePiece space = condition.Master.Spaces[pieceData.occupiedSpaceIndices[socket.occupiedSpaceIndex]];
        transform.localRotation = Quaternion.LookRotation(socket.spaceDirection, Vector3.up);
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material.color = condition.Master.Palette[socket.value].color;
        }
    }
}
