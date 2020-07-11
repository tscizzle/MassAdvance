using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public Material damagedMaterial;

    private static string massHex = "#555555";
    public static Color massColor;
    private static string blueHex = "#0077ee";
    public static Color blueColor;
    private static string yellowHex = "#eeee55";
    public static Color yellowColor;
    private static string redHex = "#aa0022";
    public static Color redColor;
    private Dictionary<BlockType, Color> blockTypeToColor;
    public static float blockHeight = 0.5f;

    // Parameters.
    public BlockType blockType;
    public Vector2 gridIndices;
    // State.
    public bool isDamaged = false;

    void Awake()
    {
        ColorUtility.TryParseHtmlString(massHex, out massColor);
        ColorUtility.TryParseHtmlString(blueHex, out blueColor);
        ColorUtility.TryParseHtmlString(yellowHex, out yellowColor);
        ColorUtility.TryParseHtmlString(redHex, out redColor);
        
        blockTypeToColor = new Dictionary<BlockType, Color>
        {
            { BlockType.MASS, massColor },
            { BlockType.BLUE, blueColor },
            { BlockType.YELLOW, yellowColor },
            { BlockType.RED, redColor },
        };
    }

    void Start()
    {
        Vector3 scale = transform.localScale;
        transform.localScale = new Vector3(scale.x, blockHeight, scale.z);

        Vector3 position = transform.position;
        transform.position = new Vector3(position.x, blockHeight / 2, position.z);

        Color color = blockTypeToColor[blockType];
        GetComponent<Renderer>().material.SetColor("_Color", color);
    }

    void Update()
    {
        
    }

    /* PUBLIC API */

    public void produce()
    /* Depending on the blockType, perform any production effects. */
    {
        bool isProductive = false;

        if (blockType == BlockType.BLUE)
        {
            GameLogic.G.gainIum(1);
            isProductive = true;
        } else if (blockType == BlockType.YELLOW)
        {
            GameLogic.G.drawCard();
            isProductive = true;
        }

        if (isProductive)
        {
            StartCoroutine(Pointer.displayPointer(gridIndices));
        }
    }

    public void damageBlock()
    /* Change this Block from healthy to damaged. */
    {
        isDamaged = true;
        
        GetComponent<Renderer>().material = damagedMaterial;

        Color color = blockTypeToColor[blockType];
        GetComponent<Renderer>().material.SetColor("_Color", color);
    }
}

public enum BlockType
{
    MASS,
    BLUE,
    YELLOW,
    RED
}
