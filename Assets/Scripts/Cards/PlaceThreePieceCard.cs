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
        iumCost = 6;
        displayName = getDisplayName();
        description = blockTypeToDescription[blockType];
        isConsumable = true;
    }

    public override int getCostToPlay()
    /* Use this card's normal cost, except double it if any of the squares it will be on are
        stained.
    
    See getCostToPlay on base class Card.
    */
    {
        Vector2 mouseDownSquare = (Vector2)TrialLogic.mouseDownGridIndices;
        Vector2 mouseUpSquare = (Vector2)TrialLogic.mouseUpGridIndices;

        int costToPlace = iumCost;

        foreach (Vector2 square in getSquaresToPlaceOn(mouseDownSquare, mouseUpSquare))
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

    public override bool getIsAbleToPlay()
    /* Return true as long as there is no block already in the squares the blocks would be placed.
    
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
        
        Vector2 mouseDownSquare = (Vector2)TrialLogic.mouseDownGridIndices;
        Vector2 mouseUpSquare = (Vector2)TrialLogic.mouseUpGridIndices;

        bool allSquaresAvailable = true;
        
        foreach (Vector2 square in getSquaresToPlaceOn(mouseDownSquare, mouseUpSquare))
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

    public override void cardAction()
    /* Place blocks in a 3-cube shape of the number "7" or letter "L".
    
    See cardAction on base class Card.
    */
    {
        Vector2 mouseDownSquare = (Vector2)TrialLogic.mouseDownGridIndices;
        Vector2 mouseUpSquare = (Vector2)TrialLogic.mouseUpGridIndices;
        foreach (Vector2 square in getSquaresToPlaceOn(mouseDownSquare, mouseUpSquare))
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

    private List<Vector2> getSquaresToPlaceOn(Vector2 mouseDownSquare, Vector2 mouseUpSquare)
    /* Given where the mouse clicked down and up, get the squares this card would place blocks on.
    
    :param Vector2 mouseDownSquare: Before rotating this shape, take the top-left block. This arg
        is the grid square to put that block on.
    :param Vector2 mouseUpSquare: Before rotating this shape, take the top-right block. This arg is
        the grid square to put that block on.
    
    :returns List<Vector2>: List of squares where this card would place blocks.
    */
    {
        // 2 of the blocks to place are always where the mouse went down and where it went up.
        List<Vector2> squares = new List<Vector2> { mouseDownSquare, mouseUpSquare };

        // Add the 3rd square based on isFlipped as well as which direction the mouse up is from the
        // mouse down.
        float thirdSquareX = -1;
        float thirdSquareY = -1;
        // If mouse up is to the right of mouse down.
        if (mouseUpSquare.x == mouseDownSquare.x + 1)
        {
            thirdSquareX = isFlipped ? mouseDownSquare.x : mouseUpSquare.x;
            thirdSquareY = mouseDownSquare.y - 1;
        // If mouse up is to the left of mouse down.
        } else if (mouseUpSquare.x == mouseDownSquare.x - 1)
        {
            thirdSquareX = isFlipped ? mouseDownSquare.x : mouseUpSquare.x;
            thirdSquareY = mouseDownSquare.y + 1;
        // If mouse up is below mouse down.
        } else if (mouseUpSquare.y == mouseDownSquare.y - 1)
        {
            thirdSquareX = mouseDownSquare.x - 1;
            thirdSquareY = isFlipped ? mouseDownSquare.y : mouseUpSquare.y;
        // If mouse up is above mouse down.
        } else if (mouseUpSquare.y == mouseDownSquare.y + 1)
        {
            thirdSquareX = mouseDownSquare.x + 1;
            thirdSquareY = isFlipped ? mouseDownSquare.y : mouseUpSquare.y;
        }

        squares.Add(new Vector2(thirdSquareX, thirdSquareY));

        return squares;
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
