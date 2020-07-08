using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CurrentIumText : MonoBehaviour
{
    void Start()
    {
        ColorUtility.TryParseHtmlString(Block.blueHex, out Color color);
        GetComponent<Text>().color = color;
    }

    void Update()
    {
        GetComponent<Text>().text = $"{GameLogic.gl.currentIum} Ium";
    }
}
