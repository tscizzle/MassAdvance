﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabInstantiator : MonoBehaviour
{
    // Global var that even a prefab can reference. Will be assigned our 1 instance of MiscHelpers.
    public static PrefabInstantiator P;

    public GameObject blockPrefab;
    public GameObject pointerPrefab;
    public GameObject cardPrefab;

    private Canvas canvas;
    private GameObject hand;

    void Awake()
    {
        // Since there should only be 1 PrefabInstantiator instance, assign this instance to a global var.
        P = this;
    }

    void Start()
    {
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        hand = canvas.transform.Find("Hand").gameObject;
    }

    /* PUBLIC API */

    public GameObject CreateBlock(BlockType blockType, Vector2 gridIndices)
    /* Create a Block.

    :param string blockType: One of [ "mass", "blue", "yellow", "red" ]
    :param Vector2 gridIndices: The square in which to put the block ((0, 0) is the bottom-left).
    
    :returns GameObject blockObj:
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
    /* Create a Card.

    :param string cardId: String identifying the Card to create.

    :returns GameObject cardObj:
    */
    {
        Vector3 position = new Vector3(0, -Screen.height, 0);
        GameObject cardObj = Instantiate(cardPrefab, Vector3.zero, Quaternion.identity);
        cardObj.transform.SetParent(hand.transform, worldPositionStays: false);

        Card card = cardObj.GetComponent<Card>();
        card.cardId = cardId;

        return cardObj;
    }

    public GameObject CreatePointer(Vector2 gridIndices)
    /* Create a Pointer.

    :param Vector2 gridIndices: The square over which to point the pointer ((0, 0) is the bottom-left).

    :returns GameObject pointerObj:
    */
    {
        Vector3 position = Floor.getGridSquareCenter(gridIndices);
        position.y = 1;

        GameObject pointerObj = Instantiate(pointerPrefab, position, Quaternion.identity);

        return pointerObj;
    }
}
