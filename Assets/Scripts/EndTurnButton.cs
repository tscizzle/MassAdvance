using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndTurnButton : MonoBehaviour
{
    /* PUBLIC API */

    public void clickEndTurn()
    {
        StartCoroutine(TrialLogic.endTurn());
    }
}
