using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLogic : MonoBehaviour
{
    public GameObject prefabInstantiatorObj;

    private PrefabInstantiator prefabInstantiator;
    public static Dictionary<Vector2, Block> placedBlocks = new Dictionary<Vector2, Block>();

    void Start()
    {
        prefabInstantiator = prefabInstantiatorObj.GetComponent<PrefabInstantiator>();
    }

    public void placeBlock(string blockType, Vector2 gridIndices)
    {
        prefabInstantiator.CreateBlock(blockType, gridIndices);
    }
}
