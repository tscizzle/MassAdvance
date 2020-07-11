using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public Material damagedMaterial;

    public static Color massColor = new Color(85/255f, 85/255f, 85/255f);
    public static Color blueColor = new Color(0, 119/255f, 238/255f);
    public static Color yellowColor = new Color(238/255f, 238/255f, 85/255f);
    public static Color redColor = new Color(170/255f, 0, 34/255f);
    private Dictionary<BlockType, Color> blockTypeToColor = new Dictionary<BlockType, Color>
    {
        { BlockType.MASS, massColor },
        { BlockType.BLUE, blueColor },
        { BlockType.YELLOW, yellowColor },
        { BlockType.RED, redColor },
    };
    public static float blockHeight = 0.5f;

    // Parameters.
    public BlockType blockType;
    public Vector2 gridIndices;
    // State.
    public bool isDamaged = false;

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
