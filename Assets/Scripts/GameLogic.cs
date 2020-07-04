using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLogic : MonoBehaviour
{
    public GameObject prefabInstantiatorObj;

    private PrefabInstantiator prefabInstantiator;
    public Dictionary<Vector2, Block> placedBlocks = new Dictionary<Vector2, Block>();
    public string selectedBlockType = "blue";

    void Start()
    {
        prefabInstantiator = prefabInstantiatorObj.GetComponent<PrefabInstantiator>();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.A))
        {
            selectedBlockType = "black";
        } else if (Input.GetKey(KeyCode.S))
        {
            selectedBlockType = "blue";
        } else if (Input.GetKey(KeyCode.D))
        {
            selectedBlockType = "yellow";
        } else if (Input.GetKey(KeyCode.F))
        {
            selectedBlockType = "red";
        }
    }

    /* PUBLIC API */

    public void placeBlock(string blockType, Vector2 gridIndices)
    {
        prefabInstantiator.CreateBlock(blockType, gridIndices);
    }
}
