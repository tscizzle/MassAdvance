using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlaceThreePieceCard : Card
{
    public BlockType blockType;

    public override void Start()
    {
        base.Start();

        Color backgroundColor = PlaceSingleBlockCard.blockTypeToBackgroundColor[blockType];
        backgroundObj.GetComponent<Image>().color = backgroundColor;
        
        Color iconColor = PlaceSingleBlockCard.blockTypeToIconColor[blockType];
        iconObj.GetComponent<Image>().color = iconColor;
    }

    /* SATISFYING CARD'S API */

    public override void setCardParams()
    /* See setCardParams on base class Card. */
    {
        // Params unique to this subclass.
        blockType = cardNameToBlockType[cardName];
        // Standard params.
        iumCost = 5;
        displayName = blockTypeToDisplayName[blockType];
        description = blockTypeToDescription[blockType];
        isConsumable = true;
    }

    public override int getCostToPlay(Vector2 gridIndices)
    /* Use this card's normal cost, except double it if any of the squares it will be on are
        stained.
    
    See getCostToPlay on base class Card.
    */
    {
        int costToPlace = iumCost;

        foreach (Vector2 square in getSquaresToPlaceOn(gridIndices))
        {
            if (TrialLogic.floorSquaresMap.ContainsKey(square))
            {
                FloorSquare floorSquare = TrialLogic.floorSquaresMap[square];
                if (floorSquare.isStained())
                {
                    costToPlace *= 2;
                    break;
                }
            }
        }

        return costToPlace;
    }

    public override bool getIsAbleToPlay(Vector2 gridIndices)
    /* Return true as long as there is no block already in the squares the blocks would be placed.
    
    See getIsAbleToPlay on base class Card.
    */
    {
        bool allSquaresAvailable = true;
        
        foreach (Vector2 square in getSquaresToPlaceOn(gridIndices))
        {
            bool isBlockInTheWay = TrialLogic.getBlockTypeOfSquare(square) != null;
            bool isOutOfBounds = !TrialLogic.floorSquaresMap.ContainsKey(square);
            
            if (isBlockInTheWay || isOutOfBounds)
            {
                allSquaresAvailable = false;
            }
        }

        return allSquaresAvailable;
    }

    public override void cardAction(Vector2 gridIndices)
    /* Place blocks in a 3-cube shape of the number "7".
    
    See cardAction on base class Card.
    */
    {
        foreach (Vector2 square in getSquaresToPlaceOn(gridIndices))
        {
            TrialLogic.placeBlock(blockType, square);
        }
    }

    /* PUBLIC API */

    public static string getThreePieceCardName(BlockType blockType)
    /* Given a BlockType, give the cardName of the card that places a three-piece of that type.
    
    :param BlockType blockType:

    :returns string cardName:
    */
    {
        return $"three_piece_{blockType}";
    }

    public static bool isCardNameThisCard(string cardName)
    /* Whether or not the provided card name is this type of card.
    
    :param string cardName:

    :param bool:
    */
    {
        return cardNamesForThisCard.Contains(cardName);
    }

    /* HELPERS */

    private List<Vector2> getSquaresToPlaceOn(Vector2 gridIndices)
    /* Given an anchor position, get the squares this card would place blocks on.
    
    :param Vector2 gridIndices: Position this group of blocks is anchored at, the "top-left" square.
    
    :returns List<Vector2>: List of squares where this card would place blocks.
    */
    {
        return new List<Vector2>
        {
            gridIndices,
            new Vector2(gridIndices.x + 1, gridIndices.y),
            new Vector2(gridIndices.x + 1, gridIndices.y - 1)
        };
    }

    /* COLLECTIONS */

    private static List<string> cardNamesForThisCard = new List<string>
    {
        getThreePieceCardName(BlockType.BLUE),
        getThreePieceCardName(BlockType.YELLOW),
        getThreePieceCardName(BlockType.RED),
    };

    private static Dictionary<string, BlockType> cardNameToBlockType = new Dictionary<string, BlockType>
    {
        { getThreePieceCardName(BlockType.BLUE), BlockType.BLUE },
        { getThreePieceCardName(BlockType.YELLOW), BlockType.YELLOW },
        { getThreePieceCardName(BlockType.RED), BlockType.RED },
    };
    
    private static Dictionary<BlockType, string> blockTypeToDisplayName = new Dictionary<BlockType, string>
    {
        { BlockType.BLUE, "3-Piece Blue" },
        { BlockType.YELLOW, "3-Piece Yellow" },
        { BlockType.RED, "3-Piece Red" },
    };
    
    private static Dictionary<BlockType, string> blockTypeToDescription = new Dictionary<BlockType, string>
    {
        { BlockType.BLUE, "Place 3 blue blocks." },
        { BlockType.YELLOW, "Place 3 yellow blocks." },
        { BlockType.RED, "Place 3 red blocks." },
    };
}
