using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WashFloorSquareCard : Card
{
    public static string washFloorSquareCardName = "wash_floor_square";

    /* SATISFYING CARD'S API */

    public override void setCardParams()
    /* See setCardParams on base class Card. */
    {
        iumCost = 0;
        displayName = "Wash";
        isConsumable = false;
    }

    public override bool getIsAbleToPlay(Vector2 gridIndices)
    /* Return true if there is no block in the targeted square.
    
    See getIsAbleToPlay on base class Card.
    */
    {
        BlockType? blockType = TrialLogic.getBlockTypeOfSquare(gridIndices);
        bool isEmpty = blockType == null;
        return isEmpty;
    }

    public override void cardAction(Vector2 gridIndices)
    /* See cardAction on base class Card. */
    {
        FloorSquare floorSquare = TrialLogic.floorSquaresMap[gridIndices];
        floorSquare.addStainTurns(-1);
    }
}
