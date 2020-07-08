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
        GetComponent<Text>().text = $"Turn {GameLogic.gl.turnsTaken} / {GameLogic.gl.turnsToSurvive}";
    }
}
