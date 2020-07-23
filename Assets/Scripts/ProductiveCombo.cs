using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProductiveCombo
{
    public ProductiveCombo(List<Block> blocks_arg, ProductionComboType productionType_arg)
    {
        blocks = blocks_arg;
        productionType = productionType_arg;
    }

    public enum ProductionComboType
    {
        LINE_4_IN_A_ROW
    }

    public List<Block> blocks;
    public ProductionComboType productionType;

    /* PUBLIC API */

    public void produce()
    /* Depending on this combo's type, perform the production effect. */
    {
        if (productionType == ProductionComboType.LINE_4_IN_A_ROW)
        {
            // Produce on the middle blocks.
            blocks[1].produce();
            blocks[2].produce();
        }
    }

    public Vector2 getAnchorSquare()
    /* Get the square of this combo which is used as this combo's "position" in some cases.
    
    :returns Vector2 anchor:
    */
    {
        List<Block> sortedBlocks = MiscHelpers.standardOrder(blocks, b => b.gridIndices);
        Vector2 anchor = sortedBlocks[0].gridIndices; 
        return anchor;
    }

    public static List<ProductiveCombo> combosAnchoredOnSquare(Vector2 gridIndices)
    /* Get all productive combos whose anchor square would be here. Need to check for each type, but
        only the orientations that have this square as the top-left point (a combo's anchor square).
    
    E.g. Would look for 4-in-a-row combos that start here and extend down or extend to the right.

    :param Vector2 gridIndices:
    
    :returns List<ProductiveCombo>:
    */
    {
        List<ProductiveCombo> productiveCombos = new List<ProductiveCombo>();
        
        // Type: 4-in-a-row
        BlockType? blockTypeHere = TrialLogic.getBlockTypeOfSquare(gridIndices);
        if (Block.isProductive(blockTypeHere))
        {
            // Check from here going down.
            List<Block> sameTypeBlocksDown = new List<Block>();
            foreach (int yIdx in Enumerable.Range((int)gridIndices.y - 3, 4))
            {
                Vector2 otherSquare = new Vector2(gridIndices.x, yIdx);
                BlockType? blockTypeThere = TrialLogic.getBlockTypeOfSquare(otherSquare);
                if (blockTypeThere == blockTypeHere)
                {
                    Block block = TrialLogic.placedBlocks[otherSquare];
                    sameTypeBlocksDown.Add(block);
                }
            }
            if (sameTypeBlocksDown.Count == 4)
            {
                ProductiveCombo combo = new ProductiveCombo(
                    sameTypeBlocksDown,
                    ProductionComboType.LINE_4_IN_A_ROW
                );
                productiveCombos.Add(combo);
            }

            // Check from here going right.
            List<Block> sameTypeBlocksRight = new List<Block>();
            foreach (int xIdx in Enumerable.Range((int)gridIndices.x, 4))
            {
                Vector2 otherSquare = new Vector2(xIdx, gridIndices.y);
                BlockType? blockTypeThere = TrialLogic.getBlockTypeOfSquare(otherSquare);
                if (blockTypeThere == blockTypeHere)
                {
                    Block block = TrialLogic.placedBlocks[otherSquare];
                    sameTypeBlocksRight.Add(block);
                }
            }
            if (sameTypeBlocksRight.Count == 4)
            {
                ProductiveCombo combo = new ProductiveCombo(
                    sameTypeBlocksRight,
                    ProductionComboType.LINE_4_IN_A_ROW
                );
                productiveCombos.Add(combo);
            }
        }
        
        return productiveCombos;
    }
}
