using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabInstantiator : MonoBehaviour
{
    public GameObject blockPrefab;
    public GameObject floorObj;

    private Floor floor;

    void Awake()
    {
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
        Vector3 position = floor.getGridSquareCenter(gridIndices);
        position.y = Block.blockHeight / 2;

        GameObject blockObj = Instantiate(blockPrefab, position, Quaternion.identity);
        Block block = blockObj.GetComponent<Block>();

        Color color = block.blockTypeToColor(blockType);
        blockObj.GetComponent<Renderer>().material.SetColor("_Color", color);

        block.blockType = blockType;
        
        return blockObj;
    }

    /* HELPERS */
}
