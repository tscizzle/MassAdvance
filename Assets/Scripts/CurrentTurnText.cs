using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CurrentTurnText : MonoBehaviour
{
    public static Color winningColor = new Color(51/255f, 250/255f, 170/255f);

    void Start()
    {
        
    }

    void Update()
    {
        GetComponent<Text>().text = $"Turn {TrialLogic.turnNumber} / {TrialLogic.turnsToSurvive}";
        
        if (TrialLogic.isTrialWin)
        {
            Transform background = transform.parent;
            background.GetComponent<Image>().color = winningColor;
        }
    }
}
