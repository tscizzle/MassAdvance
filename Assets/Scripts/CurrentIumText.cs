using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CurrentIumText : MonoBehaviour
{
    void Start()
    {
        GetComponent<Text>().color = Block.blueColor;
    }

    void Update()
    {
        GetComponent<Text>().text = $"{TrialLogic.T.currentIum} ium";
    }
}
