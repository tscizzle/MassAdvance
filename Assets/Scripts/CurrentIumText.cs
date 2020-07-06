using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CurrentIumText : MonoBehaviour
{
    public GameObject gameLogicObj;

    private GameLogic gameLogic;

    void Awake()
    {
        gameLogic = gameLogicObj.GetComponent<GameLogic>();
    }

    void Start()
    {
        ColorUtility.TryParseHtmlString(Block.blueHex, out Color color);
        GetComponent<Text>().color = color;
    }

    void Update()
    {
        GetComponent<Text>().text = $"{gameLogic.currentIum} Ium";
    }
}
