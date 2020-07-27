using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CurrentTrialText : MonoBehaviour
{
    void Update()
    {
        GetComponent<Text>().text = $"Trial {CampaignLogic.trialNumber}";
        
        if (TrialLogic.isTrialWin)
        {
            transform.parent.GetComponent<Image>().color = CurrentTurnText.winningColor;
        }
    }
}
