using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabInstantiator : MonoBehaviour
{
    public GameObject blockPrefab;
    public GameObject floorObj;

    private Floor floor;

    void Start()
    {
        floor = floorObj.GetComponent<Floor>();
    }

    /* PUBLIC API */

    public GameObject CreateBlock(string blockType, Vector2 gridIndices)
    /* Create a Block.

    :param string blockType: One of [ "black", "blue", "yellow", "red" ]
    
    :returns GameObject block:
    */
    {
        Vector3 position = floor.getGridSquareCenter(gridIndices);
        position.y = Block.blockHeight / 2;

        GameObject blockObj = Instantiate(blockPrefab, position, Quaternion.identity);

        Color color = blockTypeToColor(blockType);
        blockObj.GetComponent<Renderer>().material.color = color;
        
        return blockObj;
    }

    /* HELPERS */

    private Color blockTypeToColor(string blockType)
    {
        Color color;
        ColorUtility.TryParseHtmlString("#333333", out color);
        
        switch (blockType)
        {
            case "black":
                break;
            case "blue":
                ColorUtility.TryParseHtmlString("#0055ee", out color);
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
