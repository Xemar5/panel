using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public abstract class Piece : MonoBehaviour
{
    [PropertyOrder(0), SerializeField, ReadOnly]
    private PieceData data = null;
    [PropertyOrder(1)]
    public int contextPathOrder = 0;
    [PropertyOrder(2)]
    public string contextPath = default;

    private PieceData runtimeData = null;

    public Puzzle Master { get; private set; }
    public string ContextPath => contextPath;
    public int ContextPathOrder => contextPathOrder;
    public PieceData Data
    {
        get
        {
            if (!Application.isPlaying)
            {
                return data;
            }
            else if (runtimeData == null)
            {
                runtimeData = Instantiate(data);
            }
            return runtimeData;
        }
    }

    public virtual void Initialize(Puzzle master)
    {
        Master = master;
    }

    public abstract PieceData CreateData();
    public virtual void Save(PieceData pieceData)
    {
        pieceData.localPosition = Utils.Round(transform.localPosition, 4);
        pieceData.localRotation = transform.localRotation;
    }
    public virtual void Load(PieceData pieceData)
    {
        transform.localPosition = pieceData.localPosition;
        transform.localRotation = pieceData.localRotation;
    }

    public void ResetToStartingValues()
    {
        Load(data);
    }


#if UNITY_EDITOR
    public void SetData(PieceData data) => this.data = data;
#endif
}
