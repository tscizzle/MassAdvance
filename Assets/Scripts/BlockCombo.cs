using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BlockCombo
{
    public BlockCombo(List<Block> blocks_arg, BlockComboType blockComboType_arg)
    {
        blocks = blocks_arg;
        blockComboType = blockComboType_arg;
    }

    public enum BlockComboType
    {
        PRODUCTION_LINE,
        RAINBOW,
        CHECKER_BLUE_YELLOW,
        CHECKER_BLUE_RED,
        CHECKER_YELLOW_RED,
    }

    public List<Block> blocks;
    public BlockComboType blockComboType;

    /* PUBLIC API */

    public void produce()
    /* Depending on this combo's type, perform the production effect. */
    {
        if (blockComboType == BlockComboType.PRODUCTION_LINE)
        {
            // Produce on one of the blocks.
            blocks[0].produce();
        } else if (blockComboType == BlockComboType.CHECKER_BLUE_YELLOW)
        {
            // TODO: find blocks bordering this combo and call repair on them.
        }
    }

    public void onBlockDestroy()
    /* Run this destroy function if any block in this combo gets destroyed. */
    {
        if (blockComboType == BlockComboType.CHECKER_BLUE_RED)
        {
            TrialLogic.gainIum(1);
        } else if (blockComboType == BlockComboType.CHECKER_YELLOW_RED)
        {
            TrialLogic.drawCard();
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

    public static bool isProductive(BlockComboType blockComboType)
    /* Return whether or not this block combo type has a produce effect.
    
    :param BlockComboType blockComboType:
    
    :returns bool:
    */
    {
        return (
            blockComboType == BlockComboType.PRODUCTION_LINE
            || blockComboType == BlockComboType.CHECKER_BLUE_YELLOW
        );
    }

    public static bool hasDestroyEffect(BlockComboType blockComboType)
    /* Return whether or not this block combo type has a destroy effect.
    
    :param BlockComboType blockComboType:
    
    :returns bool:
    */
    {
        return (
            blockComboType == BlockComboType.CHECKER_BLUE_RED
            || blockComboType == BlockComboType.CHECKER_YELLOW_RED
        );
    }

    public static List<BlockCombo> combosAnchoredAt(Vector2 gridIndices)
    /* Get all block combos whose anchor square would be here. Need to check for each type, but only
        the orientations that have this square as the top-left point (a combo's anchor square).
    
    E.g. Would look for 4-in-a-row combos that start here and extend down or extend to the right.

    :param Vector2 gridIndices:
    
    :returns List<BlockCombo>:
    */
    {
        List<BlockCombo> combos = new List<BlockCombo>();
        
        List<BlockCombo> productionLineCombos = productionLineCombosAnchoredAt(gridIndices);
        combos.AddRange(productionLineCombos);
        
        List<BlockCombo> checkerBlueYellowCombos = checkerCombosAnchoredAt(
            gridIndices,
            BlockComboType.CHECKER_BLUE_YELLOW
        );
        combos.AddRange(checkerBlueYellowCombos);

        List<BlockCombo> checkerBlueRedCombos = checkerCombosAnchoredAt(
            gridIndices,
            BlockComboType.CHECKER_BLUE_RED
        );
        combos.AddRange(checkerBlueRedCombos);

        List<BlockCombo> checkerYellowRedCombos = checkerCombosAnchoredAt(
            gridIndices,
            BlockComboType.CHECKER_YELLOW_RED
        );
        combos.AddRange(checkerYellowRedCombos);
        
        return combos;
    }

    /* HELPERS */

    private static List<BlockCombo> productionLineCombosAnchoredAt(Vector2 gridIndices)
    /* Look for the production line combo at this square.

    :param Vector2 gridIndices:
    
    :returns List<BlockCombo>:
    */
    {
        List<BlockCombo> combos = new List<BlockCombo>();
        
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
                BlockCombo combo = new BlockCombo(
                    sameTypeBlocksDown,
                    BlockComboType.PRODUCTION_LINE
                );
                combos.Add(combo);
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
                BlockCombo combo = new BlockCombo(
                    sameTypeBlocksRight,
                    BlockComboType.PRODUCTION_LINE
                );
                combos.Add(combo);
            }
        }

        return combos;
    }

    private static List<BlockCombo> checkerCombosAnchoredAt(Vector2 gridIndices, BlockComboType blockComboType)
    /* Look for the checker combos at this square, for a certain pair of block types.

    :param Vector2 gridIndices:
    :param BlockType[] blockTypes: Pair of block types we are checking for. Must be length 2.
    
    :returns List<BlockCombo>:
    */
    {
        List<BlockCombo> combos = new List<BlockCombo>();

        BlockType[] blockTypes;
        if (blockComboType == BlockComboType.CHECKER_BLUE_YELLOW)
        {
            blockTypes = new BlockType[2] { BlockType.BLUE, BlockType.YELLOW };
        } else if (blockComboType == BlockComboType.CHECKER_BLUE_RED)
        {
            blockTypes = new BlockType[2] { BlockType.BLUE, BlockType.RED };
        } else if (blockComboType == BlockComboType.CHECKER_YELLOW_RED)
        {
            blockTypes = new BlockType[2] { BlockType.YELLOW, BlockType.RED };
        } else
        {
            // Invalid BlockComboType was passed in.
            return combos;
        }

        BlockType? blockTypeA = TrialLogic.getBlockTypeOfSquare(gridIndices);

        // If this square doesn't have one of the block types we're checking for, return no combos.
        if (blockTypeA == null || !blockTypes.Contains((BlockType)blockTypeA))
        {
            return combos;
        }

        BlockType blockTypeB = blockTypeA == blockTypes[0] ? blockTypes[1] : blockTypes[0];

        Vector2 topRight = new Vector2(gridIndices.x + 1, gridIndices.y);
        Vector2 bottomLeft = new Vector2(gridIndices.x, gridIndices.y - 1);
        Vector2 bottomRight = new Vector2(gridIndices.x + 1, gridIndices.y - 1);

        bool isAMatch = (
            TrialLogic.getBlockTypeOfSquare(topRight) == blockTypeB
            && TrialLogic.getBlockTypeOfSquare(bottomLeft) == blockTypeB
            && TrialLogic.getBlockTypeOfSquare(bottomRight) == blockTypeA
        );

        // If the 4 squares aren't the combo we're looking for, return no combos.
        if (!isAMatch)
        {
            return combos;
        }

        List<Block> blocks = new List<Block>
        {
            TrialLogic.placedBlocks[gridIndices],
            TrialLogic.placedBlocks[topRight],
            TrialLogic.placedBlocks[bottomLeft],
            TrialLogic.placedBlocks[bottomRight]
        };
        BlockCombo combo = new BlockCombo(blocks, blockComboType);
        combos.Add(combo);

        return combos;
    }
}
