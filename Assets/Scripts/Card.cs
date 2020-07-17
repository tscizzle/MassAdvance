using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Card : MonoBehaviour, IPointerClickHandler
{
    public static Color blueCardColor = new Color(51/255f, 170/255f, 250/255f);
    public static Color yellowCardColor = new Color(250/255f, 250/255f, 136/255f);
    public static Color redCardColor = new Color(238/255f, 68/255f, 102/255f);
    public static Color iumCostTextColor = new Color(50/255, 50/255, 50/255);
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
    private static float cardHeight;
    private static float cardWidth;
    private static float highlightedCardSizeMultiplier;
    private static float cardAreaBottomBound;
    private static float cardAreaTopBound;
    private static float cardAreaRightBound;
    private static float movementHalftime;
    private static float movementMinSpeed;
    private static float growthSpeed;

    private GameObject backgroundObj;
    private GameObject iconObj;
    private GameObject iumCostObj;
    private GameObject handObj;
    private Canvas canvas;

    // Parameters.
    public string cardId;
    public string cardName;

    void Awake()
    {
        backgroundObj = transform.Find("Background").gameObject;
        iconObj = transform.Find("Icon").gameObject;
        iumCostObj = transform.Find("IumCostText").gameObject;
        handObj = GameObject.Find("Hand");
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();

        cardHeight = 50;
        cardWidth = 35;
        highlightedCardSizeMultiplier = 1.3f;
        cardAreaBottomBound = 0;
        cardAreaTopBound = Screen.height * 0.75f;
        cardAreaRightBound = cardWidth * canvas.scaleFactor;
        movementHalftime = 0.05f;
        movementMinSpeed = 0.01f;
        growthSpeed = 2;
    }

    void Start()
    {
        cardName = TrialLogic.T.cardsById[cardId].cardName;
        
        Color backgroundColor = cardNameToBackgroundColor[cardName];
        backgroundObj.GetComponent<Image>().color = backgroundColor;
        
        Color iconColor = cardNameToIconColor[cardName];
        iconObj.GetComponent<Image>().color = iconColor;

        iumCostObj.GetComponent<Text>().color = iumCostTextColor;

        setCardSize(1);

        // Ium cost display.
        iumCostObj.GetComponent<Text>().text = TrialLogic.baseIumCostForBlock.ToString();
    }

    void Update()
    {
        moveTowardPosition();

        growTowardSizeMultiplier();

        // Order the cards, front-to-back-wise.
        setAllCardsDepth();
    }

    /* PUBLIC API */

    public void OnPointerClick(PointerEventData eventData)
    /* Override this function of IPointerClickHandler. Triggers when this Card is clicked.
    
    :param PointerEventData eventData: This interface is defined by Unity.
    */
    {
        // Select this Card, or deselect it if it's already selected.
        TrialLogic.T.selectedCardId = TrialLogic.T.selectedCardId == cardId ? null : cardId;
    }

    /* HELPERS */

    private static string getSingleBlockCardName(BlockType blockType)
    /* Given a BlockType, give the cardName of the card that places a single block of that type. */
    {
        return $"single_block_{blockType}";
    }

    private float getCardSpacing()
    /* Return the distance between Cards displayed in the hand.
    
    :returns float cardSpacing:
    */
    {
        float totalDist = cardAreaTopBound - cardAreaBottomBound;
        int handSize = TrialLogic.T.hand.Count;
        float cardSpacing = totalDist / (handSize + 1);

        return cardSpacing;
    }

    private Vector3 getEventualPosition()
    /* Calculate this Card's veritcal position along the left-side of the screen. For 1 Card put it
        at 1/2, for 2 Cards put them at 1/3 and 2/3, etc.
    
    :returns Vector3 cardPosition: Position where this Card belongs.
    */
    {
        float cardX = TrialLogic.T.selectedCardId == cardId ? cardAreaRightBound : 0;

        int idxInHand = TrialLogic.T.hand.IndexOf(cardId);
        float cardSpacing = getCardSpacing();
        float distFromTop = (idxInHand + 1) * cardSpacing;
        float cardY = cardAreaTopBound - distFromTop;

        Vector3 cardPosition = new Vector3(cardX, cardY, 0);
        
        return cardPosition;
    }

    private void moveTowardPosition()
    /* Move smoothly toward this Card's eventual position. */
    {
        Vector3 currentPosition = transform.position;
        Vector3 eventualPosition = getEventualPosition();
        Vector3 diffVector = eventualPosition - currentPosition;

        if (diffVector == Vector3.zero)
        {
            return;
        }
        
        float diffVectorHalfDist = diffVector.magnitude / 2;
        float movementSpeed = Mathf.Max(diffVectorHalfDist / movementHalftime, movementMinSpeed);
        float movementDist = movementSpeed * Time.deltaTime;
        
        // If would have moved past the desired point, just go right to it.
        if (movementDist > diffVector.magnitude)
        {
            transform.position = eventualPosition;
        } else
        {
            Vector3 movement = diffVector.normalized * movementDist;
            transform.position = currentPosition + movement;
        }
    }

    private float getEventualSizeMultiplier()
    /* Calculate the size multiplier for this Card, making it larger than others if it is hovered or
        selected.
    
    :returns float: Number to multiply the Card's size by when we want to highlight it.
    */
    {
        string hoveredCardId = getHoveredCardId();
        if (cardId == TrialLogic.T.selectedCardId || cardId == hoveredCardId)
        {
            return highlightedCardSizeMultiplier;
        } else
        {
            return 1;
        }
    }

    private void setCardSize(float multiplier)
    /* Set the Rect size of this Card.
    
    :param float multiplier: The size, relative to normal, to make this Card.
    */
    {
        Vector2 backgroundSize = new Vector2(cardWidth, cardHeight) * multiplier;
        GetComponent<RectTransform>().sizeDelta = backgroundSize;
        backgroundObj.GetComponent<RectTransform>().sizeDelta = backgroundSize;

        Vector2 iconSize = backgroundSize * 0.67f;
        iconSize.y = iconSize.x; // Make the icon a square.
        iconObj.GetComponent<RectTransform>().sizeDelta = iconSize;
    }

    private void growTowardSizeMultiplier()
    /* Change size smoothly toward this Card's eventual size. */
    {
        float currentMultiplier = GetComponent<RectTransform>().sizeDelta.x / cardWidth;
        float eventualMultiplier = getEventualSizeMultiplier();
        float diff = eventualMultiplier - currentMultiplier;

        if (diff == 0)
        {
            return;
        }

        float diffSign = diff / Mathf.Abs(diff);
        float multiplierChange = growthSpeed * diffSign * Time.deltaTime;
        // If we are ever about to go past the desired size, instead just go straight to it.
        if (Mathf.Abs(multiplierChange) > Mathf.Abs(diff))
        {
            multiplierChange = diff;
        }

        float newMultiplier = currentMultiplier + multiplierChange;
        
        setCardSize(newMultiplier);
    }

    private bool getIsMouseInHandArea()
    /* Return a bool for whether or not the mouse is in the area of the player's hand of Cards.
    
    :returns bool isInArea:
    */
    {
        Vector2 mousePos = Input.mousePosition;

        bool isInArea = (
            0 <= mousePos.x
            && mousePos.x <= cardAreaRightBound
            && cardAreaBottomBound <= mousePos.y
            && mousePos.y <= cardAreaTopBound
        );

        return isInArea;
    }

    private string getHoveredCardId()
    /* Get the id of the Card nearest the mouse cursor, if the mouse cursor is in the hand area.
    
    :returns string hoveredCardId:
    */
    {
        bool isMouseInHandArea = getIsMouseInHandArea();
        if (!isMouseInHandArea)
        {
            return null;
        }

        Vector2 mousePos = Input.mousePosition;
        int handSize = TrialLogic.T.hand.Count;
        float cardSpacing = getCardSpacing();
        float mouseDistFromTop = cardAreaTopBound - mousePos.y;

        int hoveredIdx = -1;
        foreach (int idx in Enumerable.Range(0, handSize))
        {
            float cardDistFromTop = (idx + 1) * cardSpacing;
            float distFromCard = Mathf.Abs(mouseDistFromTop - cardDistFromTop);
            float tolerance = cardSpacing / 2;
            if (distFromCard < tolerance)
            {
                hoveredIdx = idx;
                break;
            }
        }
        
        string hoveredCardId = hoveredIdx > -1 ? TrialLogic.T.hand[hoveredIdx] : null;

        return hoveredCardId;
    }

    private void setAllCardsDepth()
    /* Set the order of Cards in the Hierarchy so they appear correctly front-to-back-wise,
        including putting the hovered Card in front.
    
    Note that if another Card's Update already ran and did this, nothing needs to happen here.
    */
    {
        string hoveredCardId = getHoveredCardId();

        // Desired order of Cards in the GameObject Hierarchy.
        List<string> desiredHierarchyOrder = new List<string>(TrialLogic.T.hand);
        if (hoveredCardId != null)
        {
            desiredHierarchyOrder.Remove(hoveredCardId);
            desiredHierarchyOrder.Add(hoveredCardId);
        }

        // Order of Cards in the GameObject Hierarchy right now.
        List<string> currentHierarchyOrder = new List<string>();
        Dictionary<string, Transform> cardIdToTransformMap = new Dictionary<string, Transform>();
        foreach (Transform cardTransform in handObj.transform)
        {
            GameObject childObj = cardTransform.gameObject;
            Card childCard = childObj.GetComponent<Card>();
            string childId = childCard.cardId;
            currentHierarchyOrder.Add(childId);
            cardIdToTransformMap[childId] = cardTransform;
        }

        // If the Cards are where they need to be, don't move anything.
        // Otherwise, in order, place each Card at the end, so they end up in the correct order.
        if (desiredHierarchyOrder.SequenceEqual(currentHierarchyOrder))
        {
            return;
        } else
        {
            foreach (string childId in desiredHierarchyOrder)
            {
                cardIdToTransformMap[childId].SetAsLastSibling();
            }
        }
    }
}
