using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ConditionGroups
{
    [Serializable, DisableIf("@true")]
    public class ConditionGroup
    {
        [SerializeField]
        private List<Condition> conditions = new List<Condition>();

        public IReadOnlyList<Condition> Conditions => conditions;

        public void AddCondition(Condition condition)
        {
            conditions.Add(condition);
        }
        public int RemoveCondition(Condition condition)
        {
            int index = conditions.IndexOf(condition);
            conditions.RemoveAt(index);
            return index;
        }
        public void RemoveConditionAt(int index)
        {
            conditions.RemoveAt(index);
        }

    }

    [Serializable, DisableIf("@true")]
    public class ConditionSets
    {
        [SerializeField]
        private Puzzle master = default;
        [SerializeField]
        private int totalCount = default;
        [SerializeField]
        private List<ConditionGroup> conditionGroups = new List<ConditionGroup>();

        public IReadOnlyList<ConditionGroup> ConditionGroups => conditionGroups;

        public ConditionSets(Puzzle master)
        {
            this.master = master;
            UnityEditor.EditorUtility.SetDirty(master);
        }

        public int AddGroup()
        {
            conditionGroups.Add(new ConditionGroup());
            UnityEditor.EditorUtility.SetDirty(master.Data);
            return conditionGroups.Count - 1;
        }
        public int AddCondition(Condition condition, int groupIndex)
        {
            conditionGroups[groupIndex].AddCondition(condition);
            int conditionIndex = totalCount;
            condition.Data.conditionIndex = conditionIndex;
            UnityEditor.EditorUtility.SetDirty(condition.Data);
            UnityEditor.EditorUtility.SetDirty(master.Data);
            UnityEditor.EditorUtility.SetDirty(master);
            totalCount += 1;
            return conditionIndex;
        }
        public void ClearSet()
        {
            for (int i = 0; i < conditionGroups.Count; i++)
            {
                for (int j = 0; j < conditionGroups[i].Conditions.Count; j++)
                {
                    conditionGroups[i].Conditions[j].Data.conditionIndex = -1;
                    UnityEditor.EditorUtility.SetDirty(conditionGroups[i].Conditions[j].Data);
                }
            }
            master.Data.ConditionGroups.Clear();
            UnityEditor.EditorUtility.SetDirty(master.Data);
            UnityEditor.EditorUtility.SetDirty(master);
            conditionGroups.Clear();
            totalCount = 0;
        }
        public void RemoveGroup(int index)
        {
            if (index < 0 || index >= conditionGroups.Count)
            {
                throw new IndexOutOfRangeException();
            }
            int removedCount = 0;
            for (int i = 0; i < conditionGroups[index].Conditions.Count; i++)
            {
                conditionGroups[index].Conditions[i].Data.conditionIndex = -1;
                conditionGroups[index].RemoveConditionAt(i);
                removedCount += 1;
            }
            master.Data.ConditionGroups.RemoveAt(index);
            conditionGroups.RemoveAt(index);
            for (int i = index; i < conditionGroups.Count; i++)
            {
                for (int j = 0; j < conditionGroups[i].Conditions.Count; j++)
                {
                    conditionGroups[i].Conditions[j].Data.conditionIndex -= removedCount;
                    UnityEditor.EditorUtility.SetDirty(conditionGroups[i].Conditions[j].Data);
                }
            }
            totalCount -= removedCount;
            UnityEditor.EditorUtility.SetDirty(master);
            UnityEditor.EditorUtility.SetDirty(master.Data);
        }
        public void RemoveCondition(Condition condition)
        {
            int groupIndex = GetGroupIndex(condition);
            int index = conditionGroups[groupIndex].RemoveCondition(condition);
            master.Data.ConditionGroups[groupIndex].conditionDatas.RemoveAt(index);
            condition.Data.conditionIndex = -1;
            for (int i = index; i < conditionGroups[groupIndex].Conditions.Count; i++)
            {
                conditionGroups[groupIndex].Conditions[i].Data.conditionIndex -= 1;
                UnityEditor.EditorUtility.SetDirty(conditionGroups[groupIndex].Conditions[i].Data);
            }
            for (int i = groupIndex + 1; i < conditionGroups.Count; i++)
            {
                for (int j = 0; j < conditionGroups[i].Conditions.Count; j++)
                {
                    conditionGroups[i].Conditions[j].Data.conditionIndex -= 1;
                    UnityEditor.EditorUtility.SetDirty(conditionGroups[i].Conditions[j].Data);
                }
            }
            if (conditionGroups[groupIndex].Conditions.Count == 0)
            {
                master.Data.ConditionGroups.RemoveAt(groupIndex);
                conditionGroups.RemoveAt(groupIndex);
            }
            totalCount -= 1;
            UnityEditor.EditorUtility.SetDirty(master);
            UnityEditor.EditorUtility.SetDirty(master.Data);
        }

        public int GetGroupIndex(Condition condition)
        {
            int currentConditionCount = 0;
            for (int i = 0; i < conditionGroups.Count; i++)
            {
                currentConditionCount += conditionGroups[i].Conditions.Count;
                if (currentConditionCount > condition.Data.conditionIndex)
                {
                    return i;
                }
            }
            return -1;
        }

        public void InitializeConditions(Puzzle master)
        {
            for (int i = 0; i < conditionGroups.Count; i++)
            {
                for (int j = 0; j < conditionGroups[i].Conditions.Count; j++)
                {
                    conditionGroups[i].Conditions[j].Initialize(master);
                }
            }
        }
        public bool IsGroupSatisfied(int grouIndex)
        {
            for (int i = 0; i < conditionGroups[grouIndex].Conditions.Count; i++)
            {
                if (conditionGroups[grouIndex].Conditions[i].IsSatisfied() == false)
                {
                    return false;
                }
            }
            return true;
        }
        public IEnumerable<ConditionGroupData> SaveConditionsEnumerable()
        {
            for (int i = 0; i < conditionGroups.Count; i++)
            {
                ConditionGroupData data = new ConditionGroupData()
                {
                    conditionDatas = new List<ConditionData>(),
                };

                for (int j = 0; j < conditionGroups[i].Conditions.Count; j++)
                {
                    ConditionData conditionData = conditionGroups[i].Conditions[j].Data;
                    conditionGroups[i].Conditions[j].Save(conditionData);
                    data.conditionDatas.Add(conditionData);
                    UnityEditor.EditorUtility.SetDirty(conditionData);
                }
                yield return data;
            }
        }
    }
}
