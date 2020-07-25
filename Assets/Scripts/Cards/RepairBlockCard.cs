using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepairBlockCard : Card
{
    public static string repairBlockCardName = "repair_block";

    /* SATISFYING CARD'S API */

    public override void setCardParams()
    /* See setCardParams on base class Card. */
    {
        iumCost = 1;
        displayName = "Repair";
        description = "Remove damage from block.";
        isConsumable = false;
    }

    public override bool getIsAbleToPlay()
    /* Return true if there is a player's Block in the targeted square.
    
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
        bool playerBlockIsThere = blockType != null && blockType != BlockType.MASS;
        return playerBlockIsThere;
    }

    public override void cardAction()
    /* See cardAction on base class Card. */
    {
        Vector2 gridIndices = (Vector2)TrialLogic.mouseDownGridIndices;
        Block block = TrialLogic.placedBlocks[gridIndices];
        block.repair();
    }
}
