using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

//public enum ConditionId
//{
//    Occupied,
//}

[CreateAssetMenu(fileName = "ConditionDatabase", menuName = "Create Condition Database")]
public class ConditionDatabase : ScriptableObject
{

    [SerializeField]
#if UNITY_EDITOR
    [ValidateInput(nameof(ValidateInput))]
#endif
    private List<Condition> conditionPrefabs = default;
    private Dictionary<string, Condition> prefabsDictionary = null;

    private static ConditionDatabase instance = null;

    public IReadOnlyList<Condition> ConditionList => conditionPrefabs;
    public IReadOnlyDictionary<string, Condition> ConditionDictionary
    {
        get
        {
            if (prefabsDictionary == null || Application.isPlaying == false)
            {
                prefabsDictionary = new Dictionary<string, Condition>();
                foreach (Condition prefab in conditionPrefabs)
                {
                    prefabsDictionary.Add(prefab.ConditionId, prefab);
                }
            }
            return prefabsDictionary;
        }
    }
    public static ConditionDatabase Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Resources.Load<ConditionDatabase>("ConditionDatabase");
            }
            return instance;
        }
    }

#if UNITY_EDITOR
    private bool ValidateInput(List<Condition> conditionPrefabs, ref string errorMessage)
    {
        HashSet<string> ids = new HashSet<string>();
        for (int i = 0; i < conditionPrefabs.Count; i++)
        {
            Condition prefab = conditionPrefabs[i];
            if (prefab == null)
            {
                errorMessage = $"Condition prefab at {i} cannot be null.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(prefab.ConditionId))
            {
                errorMessage = $"Condition prefab at {i} with has a null or white space only ID.";
                return false;
            }
            if (ids.Add(prefab.ConditionId) == false)
            {
                errorMessage = $"Condition prefab at {i} with ID {prefab.ConditionId} already in the database.";
                return false;
            }
        }
        return true;
    }
#endif
}
