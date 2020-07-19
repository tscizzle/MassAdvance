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
    }

    public override bool getIsAbleToPlay(Vector2 gridIndices)
    /* Return true if there is a player's Block in the targeted square.
    
    See getIsAbleToPlay on base class Card.
    */
    {
        BlockType? blockType = TrialLogic.T.getBlockTypeOfSquare(gridIndices);
        bool playerBlockIsThere = blockType != null && blockType != BlockType.MASS;
        return playerBlockIsThere;
    }

    public override void cardAction(Vector2 gridIndices)
    /* See cardAction on base class Card. */
    {
        Block block = TrialLogic.T.placedBlocks[gridIndices];
        block.repair();
    }
}
