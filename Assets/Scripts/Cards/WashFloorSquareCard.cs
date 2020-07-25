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
        description = "Remove 1 turn of stain.";
        isConsumable = false;
    }

    public override bool getIsAbleToPlay()
    /* Return true if there is no block in the targeted square.
    
    See getIsAbleToPlay on base class Card.
    */
    {
        // Make sure the mouse down and up were on the same square.
        Vector2 gridIndices = (Vector2)TrialLogic.mouseDownGridIndices;
        if (TrialLogic.mouseUpGridIndices != gridIndices)
        {
            return false;
        }

        BlockType? blockType = TrialLogic.getBlockTypeOfSquare(gridIndices);
        bool isEmpty = blockType == null;
        return isEmpty;
    }

    public override void cardAction()
    /* See cardAction on base class Card. */
    {
        Vector2 gridIndices = (Vector2)TrialLogic.mouseDownGridIndices;
        FloorSquare floorSquare = TrialLogic.floorSquaresMap[gridIndices];
        floorSquare.addStainTurns(-1);
    }
}
