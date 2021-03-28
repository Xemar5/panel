using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ConditionGroupData
{
    [ReadOnly]
    public List<ConditionData> conditionDatas = new List<ConditionData>();
}
