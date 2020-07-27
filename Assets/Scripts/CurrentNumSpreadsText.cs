using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CurrentNumSpreadsText : MonoBehaviour
{
    void Start()
    {
        transform.parent.gameObject.GetComponent<Image>().color = Block.massColor;
        GetComponent<Text>().color = CurrentIumText.whiteTextColor;
    }

    void Update()
    {
        string pluralized = TrialLogic.currentNumSpreads == 1 ? "" : "s";
        GetComponent<Text>().text = $"<b>{TrialLogic.currentNumSpreads}</b> spread{pluralized})";
    }
}
