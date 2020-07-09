﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabInstantiator : MonoBehaviour
{
    // Global var that even a prefab can reference. Will be assigned our 1 instance of MiscHelpers.
    public static PrefabInstantiator P;

    public GameObject blockPrefab;
    public GameObject floorObj;
    public GameObject pointerPrefab;

    private Floor floor;

    void Awake()
    {
        // Since there should only be 1 PrefabInstantiator instance, assign this instance to a global var.
        P = this;

        floor = floorObj.GetComponent<Floor>();
    }

    void Start()
    {

    }

    /* PUBLIC API */

    public GameObject CreateBlock(string blockType, Vector2 gridIndices)
    /* Create a Block.

    :param string blockType: One of [ "mass", "blue", "yellow", "red" ]
    :param Vector2 gridIndices: The square in which to put the block ((0, 0) is the bottom-left).
    
    :returns GameObject block:
    */
    {
        Vector3 position = Floor.getGridSquareCenter(gridIndices);

        GameObject blockObj = Instantiate(blockPrefab, position, Quaternion.identity);
        
        Block block = blockObj.GetComponent<Block>();
        block.blockType = blockType;
        block.gridIndices = gridIndices;
        
        return blockObj;
    }

    public GameObject CreateCard(string cardId)
    /* TODO: do */
    {
        return new GameObject();
    }

    public GameObject CreatePointer(Vector2 gridIndices)
    /* TODO: do */
    {
        Vector3 position = Floor.getGridSquareCenter(gridIndices);
        position.y = 1;

        GameObject pointerObj = Instantiate(pointerPrefab, position, Quaternion.identity);

        return pointerObj;
    }

    /* HELPERS */
}
