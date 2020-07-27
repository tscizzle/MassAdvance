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
        
        Color iconColor = blockTypeToIconColor[blockType];
        iconObj.GetComponent<Image>().color = iconColor;
    }

    /* SATISFYING CARD'S API */

    public override void setCardParams()
    /* See setCardParams on base class Card. */
    {
        // Params unique to this subclass.
        blockType = cardNameToBlockType[cardName];
        // Standard params.
        iumCost = blockTypeToIumCost[blockType];
        displayName = blockTypeToDisplayName[blockType];
        description = blockTypeToDescription[blockType];
        isConsumable = false;
    }

    public override int getCostToPlay()
    /* Use this card's normal cost, except double it if the targeted square is stained.
    
    See getCostToPlay on base class Card.
    */
    {
        int costToPlace = iumCost;

        Vector2 gridIndices = (Vector2)TrialLogic.mouseDownGridIndices;
        FloorSquare floorSquare = TrialLogic.floorSquaresMap[gridIndices];
        if (floorSquare.isStained())
        {
            costToPlace *= 2;
        }

        return costToPlace;
    }

    public override bool getIsAbleToPlay()
    /* Return true as long as there is no block already in the targeted square.
    
    See getIsAbleToPlay on base class Card.
    */
    {
        // Make sure the mouse down and up were on the same square.
        Vector2 gridIndices = (Vector2)TrialLogic.mouseDownGridIndices;
        if (TrialLogic.mouseUpGridIndices != gridIndices)
        {
            return false;
        }

        BlockType? blockTypeThere = TrialLogic.getBlockTypeOfSquare(gridIndices);
        bool nothingIsThere = blockTypeThere == null;
        return nothingIsThere;
    }

    public override void cardAction()
    /* Simply place a single block in the targeted square.
    
    See cardAction on base class Card.
    */
    {
        Vector2 gridIndices = (Vector2)TrialLogic.mouseDownGridIndices;
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

    public static bool isCardNameThisCard(string cardName)
    /* Whether or not the provided card name is this type of card.
    
    :param string cardName:

    :param bool:
    */
    {
        return cardNamesForThisCard.Contains(cardName);
    }

    public static Dictionary<BlockType, Color> blockTypeToIconColor = new Dictionary<BlockType, Color>
    {
        { BlockType.BLUE, Block.blueColor },
        { BlockType.YELLOW, Block.yellowColor },
        { BlockType.RED, Block.redColor },
    };
    
    public static Dictionary<BlockType, Color> blockTypeToBackgroundColor = new Dictionary<BlockType, Color>
    {
        { BlockType.BLUE, blueCardColor },
        { BlockType.YELLOW, yellowCardColor },
        { BlockType.RED, redCardColor },
    };

    /* COLLECTIONS */

    private static List<string> cardNamesForThisCard = new List<string>
    {
        getSingleBlockCardName(BlockType.BLUE),
        getSingleBlockCardName(BlockType.YELLOW),
        getSingleBlockCardName(BlockType.RED),
    };

    private static Dictionary<string, BlockType> cardNameToBlockType = new Dictionary<string, BlockType>
    {
        { getSingleBlockCardName(BlockType.BLUE), BlockType.BLUE },
        { getSingleBlockCardName(BlockType.YELLOW), BlockType.YELLOW },
        { getSingleBlockCardName(BlockType.RED), BlockType.RED },
    };
    
    private static Dictionary<BlockType, int> blockTypeToIumCost = new Dictionary<BlockType, int>
    {
        { BlockType.BLUE, 2 },
        { BlockType.YELLOW, 3 },
        { BlockType.RED, 1 },
    };
    
    private static Dictionary<BlockType, string> blockTypeToDisplayName = new Dictionary<BlockType, string>
    {
        { BlockType.BLUE, "Mini Blue" },
        { BlockType.YELLOW, "Mini Yellow" },
        { BlockType.RED, "Mini Red" },
    };
    
    private static Dictionary<BlockType, string> blockTypeToDescription = new Dictionary<BlockType, string>
    {
        { BlockType.BLUE, "Place 1 blue block." },
        { BlockType.YELLOW, "Place 1 yellow block." },
        { BlockType.RED, "Place 1 red block." },
    };
}
