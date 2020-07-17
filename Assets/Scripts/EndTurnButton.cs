using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndTurnButton : MonoBehaviour
{
    void Start()
    {

    }

    void Update()
    {
        
    }

    /* PUBLIC API */

    public void clickEndTurn()
    {
        StartCoroutine(TrialLogic.T.endTurn());
    }
}
