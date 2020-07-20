using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlaceSingleBlockCard : Card
{
    private static Color blueCardColor = new Color(51/255f, 170/255f, 250/255f);
    private static Color yellowCardColor = new Color(250/255f, 250/255f, 136/255f);
    private static Color redCardColor = new Color(238/255f, 68/255f, 102/255f);

    public BlockType blockType;

    public override void Start()
    {
        base.Start();

        Color backgroundColor = cardNameToBackgroundColor[cardName];
        backgroundObj.GetComponent<Image>().color = backgroundColor;
        
        Color iconColor = cardNameToIconColor[cardName];
        iconObj.GetComponent<Image>().color = iconColor;
    }

    /* SATISFYING CARD'S API */

    public override void setCardParams()
    /* See setCardParams on base class Card. */
    {
        // Standard params.
        iumCost = 2;
        displayName = cardNameToDisplayName[cardName];
        isConsumable = true;
        // Params unique to this subclass.
        blockType = cardNameToBlockType[cardName];
    }

    public override int getCostToPlay(Vector2 gridIndices)
    /* Use this Card's normal cost, except double it if the targeted square is stained.
    
    See getCostToPlay on base class Card.
    */
    {
        int costToPlace = iumCost;

        FloorSquare floorSquare = FloorSquare.floorSquaresMap[gridIndices];
        if (floorSquare.isStained())
        {
            costToPlace *= 2;
        }

        return costToPlace;
    }

    public override bool getIsAbleToPlay(Vector2 gridIndices)
    /* Return true as long as there is no Block already in the targeted square.
    
    See getIsAbleToPlay on base class Card.
    */
    {
        BlockType? blockTypeThere = TrialLogic.getBlockTypeOfSquare(gridIndices);
        bool nothingIsThere = blockTypeThere == null;
        return nothingIsThere;
    }

    public override void cardAction(Vector2 gridIndices)
    /* Simply place a single Block in the targeted square.
    
    See cardAction on base class Card.
    */
    {
        TrialLogic.placeBlock(blockType, gridIndices);
    }

    /* PUBLIC API */

    public static string getSingleBlockCardName(BlockType blockType)
    /* Given a BlockType, give the cardName of the card that places a single block of that type.
    
    :param BlockType blockType:

    :returns string cardName:
    */
    {
        return $"single_block_{blockType}";
    }

    /* COLLECTIONS */

    public static Dictionary<string, BlockType> cardNameToBlockType = new Dictionary<string, BlockType>
    {
        { getSingleBlockCardName(BlockType.BLUE), BlockType.BLUE },
        { getSingleBlockCardName(BlockType.YELLOW), BlockType.YELLOW },
        { getSingleBlockCardName(BlockType.RED), BlockType.RED },
    };
    private static Dictionary<string, Color> cardNameToIconColor = new Dictionary<string, Color>
    {
        { getSingleBlockCardName(BlockType.BLUE), Block.blueColor },
        { getSingleBlockCardName(BlockType.YELLOW), Block.yellowColor },
        { getSingleBlockCardName(BlockType.RED), Block.redColor },
    };
    private static Dictionary<string, Color> cardNameToBackgroundColor = new Dictionary<string, Color>
    {
        { getSingleBlockCardName(BlockType.BLUE), blueCardColor },
        { getSingleBlockCardName(BlockType.YELLOW), yellowCardColor },
        { getSingleBlockCardName(BlockType.RED), redCardColor },
    };
    private static Dictionary<string, string> cardNameToDisplayName = new Dictionary<string, string>
    {
        { getSingleBlockCardName(BlockType.BLUE), "Mini Blue" },
        { getSingleBlockCardName(BlockType.YELLOW), "Mini Yellow" },
        { getSingleBlockCardName(BlockType.RED), "Mini Red" },
    };
}
