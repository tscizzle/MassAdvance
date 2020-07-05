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

        Color color = blockTypeToColor(blockType);
        blockObj.GetComponent<Renderer>().material.color = color;

        // Set fields on the instance of Block
        Block block = blockObj.GetComponent<Block>();
        block.blockType = blockType;
        
        return blockObj;
    }

    /* HELPERS */

    private Color blockTypeToColor(string blockType)
    {
        Color color;
        ColorUtility.TryParseHtmlString("#ffffff", out color);
        
        switch (blockType)
        {
            case "mass":
                ColorUtility.TryParseHtmlString("#555555", out color);
                break;
            case "blue":
                ColorUtility.TryParseHtmlString("#0077ee", out color);
                break;
            case "yellow":
                ColorUtility.TryParseHtmlString("#eeee55", out color);
                break;
            case "red":
                ColorUtility.TryParseHtmlString("#aa0022", out color);
                break;
        }

        return color;
    }
}
