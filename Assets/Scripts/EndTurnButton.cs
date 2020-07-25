using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndTurnButton : MonoBehaviour
{
    /* PUBLIC API */

    public void clickEndTurn()
    /* Click handler for button that ends the turn. */
    {
        if (TrialLogic.isGameplayUserInputsFrozen)
        {
            return;
        }
        
        StartCoroutine(TrialLogic.endTurn());
    }
}
