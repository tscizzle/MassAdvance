using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CurrentIumText : MonoBehaviour
{
    public static Color whiteTextColor = new Color(230/255f, 230/255f, 230/255f);
    
    void Start()
    {
        transform.parent.gameObject.GetComponent<Image>().color = Block.blueColor;
        GetComponent<Text>().color = whiteTextColor;
    }

    void Update()
    {
        GetComponent<Text>().text =
            $"<b>{TrialLogic.currentIum}</b> ium (+{TrialLogic.baseIumPerTurn})";
    }
}
