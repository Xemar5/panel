using Sirenix.OdinInspector;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class ConditionData : ScriptableObject
{
    [ReadOnly, HideInInlineEditors]
    public string conditionIdentifier;
    [ReadOnly, HideInInlineEditors]
    public int conditionIndex;
}
