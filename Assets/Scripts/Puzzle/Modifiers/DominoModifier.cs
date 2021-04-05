using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DominoModifier : Modifier
{
    [Serializable]
    public class Socket
    {

        [ReadOnly]
        [HideInInlineEditors]
        public int occupiedSpaceIndex;

        [ReadOnly]
        [HideInInlineEditors]
        public Vector3 spaceDirection;

        [LabelText("$"+nameof(ValueLabel))]
        public int value;

#if UNITY_EDITOR
        private string ValueLabel
        {
            get
            {
                return $"[{occupiedSpaceIndex}:{spaceDirection}] Value";
            }
        }
#endif
    }

    public List<Socket> sockets = new List<Socket>();

    protected override void Initialize()
    {
    }

    protected override void Restart(InteractablePieceData previousPieceData, InteractablePieceData restartedPieceData, Modifier previousModifierData)
    {
    }
}
