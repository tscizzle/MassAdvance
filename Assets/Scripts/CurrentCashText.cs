using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CurrentCashText : MonoBehaviour
{
    void Update()
    {
        GetComponent<Text>().text = CampaignLogic.currentCash.ToString("N0");
    }
}
