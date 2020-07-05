using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public Material damagedMaterial;

    public static float blockHeight = 0.5f;

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
