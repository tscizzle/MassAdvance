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
        isConsumable = false;
    }

    public override bool getIsAbleToPlay(Vector2 gridIndices)
    /* Return true if there is a block in the targeted square.
    
    See getIsAbleToPlay on base class Card.
    */
    {
        BlockType? blockType = TrialLogic.getBlockTypeOfSquare(gridIndices);
        bool isBlockThere = blockType != null;
        return isBlockThere;
    }

    public override void cardAction(Vector2 gridIndices)
    /* See cardAction on base class Card. */
    {
        Block block = TrialLogic.placedBlocks[gridIndices];
        block.destroy();
    }
}
