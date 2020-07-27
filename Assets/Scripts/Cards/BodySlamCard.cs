using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BodySlamCard : Card
{
    public static string bodySlamCardName = "body_slam";

    /* SATISFYING CARD'S API */

    public override void setCardParams()
    /* See setCardParams on base class Card. */
    {
        iumCost = 5;
        displayName = "Body Slam";
        description = "Destroy a whole column.";
        isConsumable = false;
    }

    public override bool getIsAbleToPlay()
    /* Body slam can be played anywhere.
    
    See getIsAbleToPlay on base class Card.
    */
    {
        // Make sure the mouse down and up were on the same square.
        Vector2 gridIndices = (Vector2)TrialLogic.mouseDownGridIndices;
        if (TrialLogic.mouseUpGridIndices != gridIndices)
        {
            return false;
        }

        return true;
    }

    public override void cardAction()
    /* See cardAction on base class Card. */
    {
        Vector2 gridIndices = (Vector2)TrialLogic.mouseDownGridIndices;

        foreach (int yIdx in Enumerable.Range(0, TrialLogic.numGridSquaresDeep))
        {
            Vector2 target = new Vector2(gridIndices.x, yIdx);
            BlockType? blockType = TrialLogic.getBlockTypeOfSquare(target);
            if (blockType != null)
            {
                Block block = TrialLogic.placedBlocks[target];
                block.destroy();
            }
        }
    }
}
