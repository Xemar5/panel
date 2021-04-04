using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "ModifierComponents", menuName = "Create Modifier Components Database")]
public class ModifierComponents : ScriptableObject
{
    private static ModifierComponents instance;
    public static ModifierComponents Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Resources.Load<ModifierComponents>("ModifierComponents");
            }
            return instance;
        }
    }


    public RotatableModifierRotator rotatableModifierRotatorPrefab = default;
    public DominoModifierNode dominoModifierNodePrefab = default;
}
