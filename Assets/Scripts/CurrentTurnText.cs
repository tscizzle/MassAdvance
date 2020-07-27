using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CurrentTurnText : MonoBehaviour
{
    public static Color winningColor = new Color(51/255f, 250/255f, 170/255f);

    void Update()
    {
        GetComponent<Text>().text = $"Turn {TrialLogic.turnNumber} / {TrialLogic.turnsToSurvive}";
        
        if (TrialLogic.isTrialWin)
        {
            transform.parent.GetComponent<Image>().color = winningColor;
        }
    }
}
