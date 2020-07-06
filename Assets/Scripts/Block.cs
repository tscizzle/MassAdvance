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

    private GameLogic gameLogic;    
    private Animator thisAnimator;

    void Awake()
    {
        thisAnimator = GetComponent<Animator>();
    }

    void Start()
    {

    }

    void Update()
    {
        
    }

    /* PUBLIC API */

    public void produce()
    /* Depending on the blockType, perform any production effects. */
    {
        if (blockType == "blue")
        {
            GetComponent<Animator>().SetTrigger("produce");
            GameLogic.gameLogic.currentIum += 1;
        } else if (blockType == "yellow")
        {
            GetComponent<Animator>().SetTrigger("produce");
            // TODO: put draw here when that's a thing
        }
    }

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
