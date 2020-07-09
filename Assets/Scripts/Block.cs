using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public Material damagedMaterial;

    public static string massHex = "#555555";
    public static string blueHex = "#0077ee";
    public static string yellowHex = "#eeee55";
    public static string redHex = "#aa0022";
    public static float blockHeight = 0.5f;

    // Parameters.
    public string blockType;
    public Vector2 gridIndices;
    // State.
    public bool isDamaged = false;

    void Start()
    {
        Vector3 scale = transform.localScale;
        transform.localScale = new Vector3(scale.x, blockHeight, scale.z);

        Vector3 position = transform.position;
        transform.position = new Vector3(position.x, blockHeight / 2, position.z);

        Color color = blockTypeToColor(blockType);
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

        if (blockType == "blue")
        {
            isProductive = true;
            GameLogic.G.currentIum += 1;
        } else if (blockType == "yellow")
        {
            isProductive = true;
            // TODO: put draw here when that's a thing
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

        Color color = blockTypeToColor(blockType);
        GetComponent<Renderer>().material.SetColor("_Color", color);
    }

    public Color blockTypeToColor(string blockType)
    /* Return a color for the given blockType. */
    {
        Color color;
        ColorUtility.TryParseHtmlString("#ffffff", out color);
        
        switch (blockType)
        {
            case "mass":
                ColorUtility.TryParseHtmlString(massHex, out color);
                break;
            case "blue":
                ColorUtility.TryParseHtmlString(blueHex, out color);
                break;
            case "yellow":
                ColorUtility.TryParseHtmlString(yellowHex, out color);
                break;
            case "red":
                ColorUtility.TryParseHtmlString(redHex, out color);
                break;
        }

        return color;
    }

    /* HELPERS */
}
