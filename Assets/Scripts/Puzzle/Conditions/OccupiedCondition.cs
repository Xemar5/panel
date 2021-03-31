using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class OccupiedCondition : Condition
{
    [SerializeField]
    private Transform nodePrefab = default;

    public override string ConditionId => "Occupied";

    public override void Initialize(Puzzle master)
    {
        base.Initialize(master);
        OccupiedConditionData data = Data as OccupiedConditionData;
        Color color;
        if (data.colorIndex < 0 || data.colorIndex >= master.Palette.Count)
        {
            color = Color.magenta;
        }
        else
        {
            color = master.Palette[data.colorIndex].color;
        }
        color.r = Mathf.Lerp(color.r, 1, 0.7f);
        color.g = Mathf.Lerp(color.g, 1, 0.7f);
        color.b = Mathf.Lerp(color.b, 1, 0.7f);

        for (int j = 0; j < data.spaceIndices.Count; j++)
        {
            Renderer[] renderers = master.Spaces[data.spaceIndices[j]].GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                renderer.material.color = color;
            }

        }

    }

    public override bool IsSatisfied()
    {
        OccupiedConditionData data = Data as OccupiedConditionData;
        List<int> unsatisfiedSpaces = new List<int>(data.spaceIndices);
        float sqrRadius = Master.CellAbsoluteRadius * Master.CellAbsoluteRadius;

        for (int i = 0; i < data.spaceIndices.Count; i++)
        {
            SpacePiece space = Master.Spaces[data.spaceIndices[i]];
            if (!space.IsOccupied())
            {
                /// At least one space required by this condition is not occupied.
                return false;
            }
            bool containsPieceOfRequiredColor = false;
            foreach (InteractablePiece piece in space.OccupyingPieces)
            {
                InteractablePieceData pieceData = piece.Data as InteractablePieceData;
                foreach (Modifier modifier in pieceData.modifiers)
                {
                    if (modifier is ColoredModifier colored)
                    {
                        if (colored.ColorIndex == data.colorIndex)
                        {
                            containsPieceOfRequiredColor = true;
                            break;
                        }
                    }
                }
                if (containsPieceOfRequiredColor)
                {
                    break;
                }
            }

            if (!containsPieceOfRequiredColor)
            {
                /// None of the pieces which occupy this space is of required color.
                return false;
            }
        }

        Debug.Log($"Condition {name} in puzzle {Master.name} satisfied.");
        return true;
    }


    public override ConditionData CreateData() => ScriptableObject.CreateInstance<OccupiedConditionData>();
//    public override void Save(ConditionData conditionData)
//    {
//    }
//    public override void Load(ConditionData conditionData)
//    {
//        OccupiedConditionData data = conditionData as OccupiedConditionData;
//        int i = 0;
//        while (i < data.spaceIndices.Length)
//        {
//            Transform child;
//            if (i < transform.childCount)
//            {
//                child = transform.GetChild(i);
//            }
//            else
//            {
//#if UNITY_EDITOR
//                if (!Application.isPlaying)
//                {
//                    child = (Transform)UnityEditor.PrefabUtility.InstantiatePrefab(nodePrefab);
//                }
//                else
//#endif
//                {
//                    child = Instantiate(nodePrefab);
//                }
//                child.name = $"OccupiedNode {i}";
//                child.SetParent(transform, false);
//                nodes.nodes.Add(child);
//            }
//            child.localPosition = data.nodes[i];
//            i += 1;
//        }
//        while (i < transform.childCount)
//        {
//            Transform child = transform.GetChild(i);
//            child.SetParent(null);
//            nodes.nodes.RemoveAt(i);
//            if (Application.isPlaying)
//            {
//                Destroy(child.gameObject);
//            }
//            else
//            {
//                DestroyImmediate(child.gameObject);
//            }
//        }
//    }

}
