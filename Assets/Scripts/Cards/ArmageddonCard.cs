using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmageddonCard : Card
{
    public static string armageddonCardName = "armageddon";

    /* SATISFYING CARD'S API */

    public override void setCardParams()
    /* See setCardParams on base class Card. */
    {
        iumCost = 3;
        displayName = "Armageddon";
        description = "Destroy block.";
        isConsumable = false;
    }

    public override bool getIsAbleToPlay()
    /* Return true if there is a block in the targeted square.
    
    See getIsAbleToPlay on base class Card.
    */
    {
        // Make sure the mouse down and up were on the same square.
        Vector2? gridIndices = TrialLogic.mouseDownGridIndices;
        if (TrialLogic.mouseUpGridIndices != gridIndices)
        {
            return false;
        }

        BlockType? blockType = TrialLogic.getBlockTypeOfSquare((Vector2)gridIndices);
        bool isBlockThere = blockType != null;
        return isBlockThere;
    }

    public override void cardAction()
    /* See cardAction on base class Card. */
    {
        Vector2 gridIndices = (Vector2)TrialLogic.mouseDownGridIndices;
        Block block = TrialLogic.placedBlocks[gridIndices];
        block.destroy();
    }
}
