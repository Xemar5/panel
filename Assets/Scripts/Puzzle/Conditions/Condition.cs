using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class Condition : MonoBehaviour
{
    [SerializeField, ReadOnly]
    private ConditionData data;

    public Puzzle Master { get; private set; }
    public abstract string ConditionId { get; }

    public ConditionData Data { get => data; set => data = value; }

    public virtual void Initialize(Puzzle master)
    {
        Master = master;
    }

    public abstract bool IsSatisfied();


    public abstract ConditionData CreateData();
    public virtual void Save(ConditionData conditionData) { }
    public virtual void Load(ConditionData conditionData) { }

}
