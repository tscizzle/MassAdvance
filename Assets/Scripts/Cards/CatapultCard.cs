using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatapultCard : Card
{
    public static string catapultCardName = "catapult";

    /* SATISFYING CARD'S API */

    public override void setCardParams()
    /* See setCardParams on base class Card. */
    {
        iumCost = 0;
        displayName = "Catapult";
        description = "Move block 1 square.";
        isConsumable = false;
    }

    public override bool getIsAbleToPlay()
    /* Return true if there was a block where the mouse went down, and no block where the mouse went
        up.
    
    See getIsAbleToPlay on base class Card.
    */
    {
        // Make sure mouse up was on the grid at all.
        if (TrialLogic.mouseUpGridIndices == null)
        {
            return false;
        }
        // Make sure the mouse up was 1 away from the mouse down.
        bool isValidInstruction = (
            (Vector2)TrialLogic.mouseDownGridIndices - (Vector2)TrialLogic.mouseUpGridIndices
        ).magnitude == 1;
        if (!isValidInstruction)
        {
            return false;
        }

        BlockType? blockTypeDown =
            TrialLogic.getBlockTypeOfSquare((Vector2)TrialLogic.mouseDownGridIndices);
        bool blockIsClicked = blockTypeDown != null;

        BlockType? blockTypeUp =
            TrialLogic.getBlockTypeOfSquare((Vector2)TrialLogic.mouseUpGridIndices);
        bool isAvailableSpace = blockTypeUp == null;
        
        bool isAbleToPlay = blockIsClicked && isAvailableSpace;
        
        return isAbleToPlay;
    }

    public override void cardAction()
    /* See cardAction on base class Card. */
    {
        Vector2 gridIndices = (Vector2)TrialLogic.mouseDownGridIndices;
        Block block = TrialLogic.placedBlocks[gridIndices];
        
        Vector2 newGridIndices = (Vector2)TrialLogic.mouseUpGridIndices;
        block.shift(newGridIndices);
    }
}
