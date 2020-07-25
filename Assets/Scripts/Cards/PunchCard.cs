using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PunchCard : Card
{
    public static string punchCardName = "punch";

    /* SATISFYING CARD'S API */

    public override void setCardParams()
    /* See setCardParams on base class Card. */
    {
        iumCost = 8;
        displayName = "Punch";
        description = "Destroy all blocks in 3x3 area.";
        isConsumable = false;
    }

    public override bool getIsAbleToPlay()
    /* Punch can be played anywhere.
    
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

        int centerX = (int)gridIndices.x;
        int centerY = (int)gridIndices.y;
        foreach (int xIdx in Enumerable.Range(centerX - 1, 3))
        {
            foreach (int yIdx in Enumerable.Range(centerY - 1, 3))
            {
                Vector2 target = new Vector2(xIdx, yIdx);
                BlockType? blockType = TrialLogic.getBlockTypeOfSquare(target);
                if (blockType != null)
                {
                    Block block = TrialLogic.placedBlocks[target];
                    block.destroy();
                }
            }
        }
    }
}
