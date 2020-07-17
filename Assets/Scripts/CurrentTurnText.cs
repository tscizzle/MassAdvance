using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CurrentTurnText : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        GetComponent<Text>().text = $"Turn {TrialLogic.T.turnsTaken} / {TrialLogic.T.turnsToSurvive}";
    }
}
