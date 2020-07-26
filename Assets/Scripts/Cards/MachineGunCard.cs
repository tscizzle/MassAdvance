using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MachineGunCard : Card
{
    public static string machineGunCardName = "machine_gun";

    /* SATISFYING CARD'S API */

    public override void setCardParams()
    /* See setCardParams on base class Card. */
    {
        iumCost = 5;
        displayName = "Machine Gun";
        description = "Destroy 3 blocks in a line.";
        isConsumable = false;
    }

    public override bool getIsAbleToPlay()
    /* Machine gun can be played anywhere.
    
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

        return true;
    }

    public override void cardAction()
    /* See cardAction on base class Card. */
    {
        Vector2 gridIndices = (Vector2)TrialLogic.mouseDownGridIndices;
        Vector2 directionGridIndices = (Vector2)TrialLogic.mouseUpGridIndices;

        // The mouse down square, the mouse up square, and the square in line with them but on the
        // other side of the clicked square.
        List<Vector2> squaresHit = new List<Vector2>
        {
            gridIndices,
            directionGridIndices,
            gridIndices + (gridIndices - directionGridIndices)
        };

        foreach (Vector2 target in squaresHit)
        {
            BlockType? blockType = TrialLogic.getBlockTypeOfSquare(target);
            if (blockType != null)
            {
                Block block = TrialLogic.placedBlocks[target];
                block.destroy();
            }
        }
    }
}
