using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "PieceDatabase", menuName = "Create Piece Database")]
public class PieceDatabase : ScriptableObject
{
    [SerializeField]
    private SpacePiece spacePrefab = default;

    [SerializeField]
#if UNITY_EDITOR
    [ValidateInput(nameof(ValidateInput))]
#endif
    private List<InteractablePiece> piecePrefabs = default;
    private Dictionary<string, InteractablePiece> prefabsDictionary = null;
#if UNITY_EDITOR
    private List<InteractablePiece> orderedPieceMenuList = default;
#endif

    private static PieceDatabase instance = null;

    public SpacePiece SpacePrefab => spacePrefab;
    public IReadOnlyList<InteractablePiece> PieceList => piecePrefabs;
    public IReadOnlyDictionary<string, InteractablePiece> PieceDictionary
    {
        get
        {
            if (prefabsDictionary == null || Application.isPlaying == false)
            {
                prefabsDictionary = new Dictionary<string, InteractablePiece>();
                foreach (InteractablePiece prefab in piecePrefabs)
                {
                    prefabsDictionary.Add(prefab.ContextPath, prefab);
                }
            }
            return prefabsDictionary;
        }
    }

    public static PieceDatabase Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Resources.Load<PieceDatabase>("PieceDatabase");
            }
            return instance;
        }
    }

#if UNITY_EDITOR
    public IReadOnlyList<Piece> OrderedPieceMenuList
    {
        get
        {
            if (orderedPieceMenuList == null || Application.isPlaying == false)
            {
                orderedPieceMenuList = new List<InteractablePiece>();
                foreach (InteractablePiece prefab in piecePrefabs)
                {
                    orderedPieceMenuList.Add(prefab);
                }
                orderedPieceMenuList.Sort((x, y) => Mathf.Clamp(x.ContextPathOrder - y.ContextPathOrder, -1, 1));
            }
            return orderedPieceMenuList;
        }
    }

    private bool ValidateInput(List<InteractablePiece> piecePrefabs, ref string errorMessage)
    {
        HashSet<string> ids = new HashSet<string>();
        for (int i = 0; i < piecePrefabs.Count; i++)
        {
            InteractablePiece prefab = piecePrefabs[i];
            if (prefab == null)
            {
                errorMessage = $"Piece prefab at {i} cannot be null.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(prefab.contextPath))
            {
                errorMessage = $"Piece prefab at {i} with has a null or empty ID.";
                return false;
            }
            if (ids.Add(prefab.ContextPath) == false)
            {
                errorMessage = $"Piece prefab at {i} with ID {prefab.ContextPath} already in the database.";
                return false;
            }
        }
        return true;
    }
#endif
}
