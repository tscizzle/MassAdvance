using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public Material damagedMaterial;

    public static float blockHeight = 0.5f;
    public static string massHex = "#555555";
    public static string blueHex = "#0077ee";
    public static string yellowHex = "#eeee55";
    public static string redHex = "#aa0022";

    public string blockType;
    public bool isDamaged = false;

    void Start()
    {

    }

    void Update()
    {
        
    }

    /* PUBLIC API */

    public void damageBlock()
    {
        isDamaged = true;
        
        GetComponent<Renderer>().material = damagedMaterial;

        Color color = blockTypeToColor(blockType);
        GetComponent<Renderer>().material.SetColor("_Color", color);
    }

    public Color blockTypeToColor(string blockType)
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
}
