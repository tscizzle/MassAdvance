using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlaceThreePieceCard : Card
{
    public BlockType blockType;
    public bool isFlipped;

    public override void Start()
    {
        base.Start();

        Color backgroundColor = PlaceSingleBlockCard.blockTypeToBackgroundColor[blockType];
        backgroundObj.GetComponent<Image>().color = backgroundColor;
        
        Color iconColor = PlaceSingleBlockCard.blockTypeToIconColor[blockType];
        iconObj.GetComponent<Image>().color = iconColor;

        // Flip the icon.
        if (isFlipped)
        {
            Vector3 currentScale = iconObj.transform.localScale;
            Vector3 newScale = new Vector3(-currentScale.x, currentScale.y, currentScale.z);
            iconObj.transform.localScale = newScale;
            Vector2 currentPivot = iconObj.GetComponent<RectTransform>().pivot;
            iconObj.GetComponent<RectTransform>().pivot = new Vector2(1, currentPivot.y);
        }
    }

    /* SATISFYING CARD'S API */

    public override void setCardParams()
    /* See setCardParams on base class Card. */
    {
        // Params unique to this subclass.
        blockType = cardNameToBlockType[cardName];
        isFlipped = cardNameToIsFlipped[cardName];
        // Standard params.
        iumCost = 5;
        displayName = getDisplayName();
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

    public static string getThreePieceCardName(BlockType blockType, bool isFlipped = false)
    /* Given a BlockType, give the cardName of the card that places a three-piece of that type.
    
    :param BlockType blockType:

    :returns string cardName:
    */
    {
        string name = $"three_piece_{blockType}";
        if (isFlipped)
        {
            name += "_flipped";
        }
        return name;
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
    /* Given an anchor position, get the squares this card would place blocks on (the L shape).
    
    :param Vector2 gridIndices: Position this group of blocks is anchored at, the "top-left" square.
    
    :returns List<Vector2>: List of squares where this card would place blocks.
    */
    {
        float thirdSquareX = isFlipped ? gridIndices.x : gridIndices.x + 1;
        return new List<Vector2> {
            gridIndices,
            new Vector2(gridIndices.x + 1, gridIndices.y),
            new Vector2(thirdSquareX, gridIndices.y - 1),
        };
    }

    private string getDisplayName()
    /* Get the display name for this card, based on the color and if it's flipped. */
    {
        string name = "";
        switch (blockType)
        {
            case BlockType.BLUE:
                name = "3-Piece Blue";
                break;
            case BlockType.YELLOW:
                name = "3-Piece Yellow";
                break;
            case BlockType.RED:
                name = "3-Piece Red";
                break;
        }
        if (isFlipped)
        {
            name += " (Flip)";
        }
        return name;
    }

    /* COLLECTIONS */

    private static List<string> cardNamesForThisCard = new List<string>
    {
        getThreePieceCardName(BlockType.BLUE),
        getThreePieceCardName(BlockType.YELLOW),
        getThreePieceCardName(BlockType.RED),
        getThreePieceCardName(BlockType.BLUE, isFlipped: true),
        getThreePieceCardName(BlockType.YELLOW, isFlipped: true),
        getThreePieceCardName(BlockType.RED, isFlipped: true),
    };

    private static Dictionary<string, BlockType> cardNameToBlockType = new Dictionary<string, BlockType>
    {
        { getThreePieceCardName(BlockType.BLUE), BlockType.BLUE },
        { getThreePieceCardName(BlockType.YELLOW), BlockType.YELLOW },
        { getThreePieceCardName(BlockType.RED), BlockType.RED },
        { getThreePieceCardName(BlockType.BLUE, isFlipped: true), BlockType.BLUE },
        { getThreePieceCardName(BlockType.YELLOW, isFlipped: true), BlockType.YELLOW },
        { getThreePieceCardName(BlockType.RED, isFlipped: true), BlockType.RED },
    };

    private static Dictionary<string, bool> cardNameToIsFlipped = new Dictionary<string, bool>
    {
        { getThreePieceCardName(BlockType.BLUE), false },
        { getThreePieceCardName(BlockType.YELLOW), false },
        { getThreePieceCardName(BlockType.RED), false },
        { getThreePieceCardName(BlockType.BLUE, isFlipped: true), true },
        { getThreePieceCardName(BlockType.YELLOW, isFlipped: true), true },
        { getThreePieceCardName(BlockType.RED, isFlipped: true), true },
    };
    
    private static Dictionary<BlockType, string> blockTypeToDescription = new Dictionary<BlockType, string>
    {
        { BlockType.BLUE, "Place 3 blue blocks." },
        { BlockType.YELLOW, "Place 3 yellow blocks." },
        { BlockType.RED, "Place 3 red blocks." },
    };
}
