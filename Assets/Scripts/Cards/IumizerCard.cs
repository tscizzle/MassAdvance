using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IumizerCard : Card
{
    public static string iumizerCardName = "iumizer";

    /* SATISFYING CARD'S API */

    public override void setCardParams()
    /* See setCardParams on base class Card. */
    {
        iumCost = 0;
        displayName = "Iumizer";
        description = "Gain 3 ium.";
        isConsumable = true;
    }

    public override bool getIsAbleToPlay()
    /* Iumizer can always be played.
    
    See getIsAbleToPlay on base class Card.
    */
    {
        return true;
    }

    public override void cardAction()
    /* See cardAction on base class Card. */
    {
        TrialLogic.gainIum(3);
    }
}
