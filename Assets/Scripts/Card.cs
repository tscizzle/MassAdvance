﻿using System.Collections;
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
    private static float cardAreaBottomBound;
    private static float cardAreaTopBound;
    private static float cardAreaRightBound;
    private static float movementHalftime;
    private static float movementMinSpeed;

    private GameObject backgroundObj;
    private GameObject iconObj;
    private GameObject handObj;
    private Canvas canvas;

    // Parameters.
    public string cardId;
    public string cardName;

    void Awake()
    {
        backgroundObj = transform.Find("Background").gameObject;
        iconObj = transform.Find("Icon").gameObject;
        handObj = GameObject.Find("Hand");
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();

        cardHeight = 50;
        cardWidth = 35;
        cardAreaBottomBound = 0;
        cardAreaTopBound = Screen.height * 0.75f;
        cardAreaRightBound = cardWidth * canvas.scaleFactor;
        movementHalftime = 0.05f;
        movementMinSpeed = 0.01f;
    }

    void Start()
    {
        cardName = TrialLogic.T.cardsById[cardId].cardName;
        
        Color backgroundColor = cardNameToBackgroundColor[cardName];
        backgroundObj.GetComponent<Image>().color = backgroundColor;
        
        Color iconColor = cardNameToIconColor[cardName];
        iconObj.GetComponent<Image>().color = iconColor;
    }

    void Update()
    {
        moveTowardPosition();
        
        string hoveredCardId = getHoveredCardId();
        if (hoveredCardId == cardId)
        {
            setCardSize(1.3f);
        } else
        {
            setCardSize(1);
        }

        // Order the cards, front-to-back-wise.
        setAllCardsDepth(hoveredCardId);
    }

    /* PUBLIC API */

    public void OnPointerClick(PointerEventData eventData)
    /* Override this function of IPointerClickHandler. Triggers when this Card is clicked.
    
    :param PointerEventData eventData: This interface is defined by Unity.
    */
    {
        TrialLogic.T.selectedCardId = cardId;
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
        int idxInHand = TrialLogic.T.hand.IndexOf(cardId);
        float cardSpacing = getCardSpacing();
        float distFromTop = (idxInHand + 1) * cardSpacing;
        float cardY = cardAreaTopBound - distFromTop;
        Vector3 cardPosition = new Vector3(0, cardY, 0);
        
        return cardPosition;
    }

    private void moveTowardPosition()
    /* Move smoothly toward this Card's eventual position. */
    {
        Vector3 currentPosition = transform.position;
        Vector3 eventualPosition = getEventualPosition();
        Vector3 diffVector = eventualPosition - currentPosition;
        
        float diffVectorHalfDist = diffVector.magnitude / 2;
        float movementSpeed = Mathf.Max(diffVectorHalfDist / movementHalftime, movementMinSpeed);
        float movementDist = movementSpeed * Time.deltaTime;
        
        Vector3 movement = diffVector.normalized * movementDist;
        
        transform.position = currentPosition + movement;
    }

    private string getHoveredCardId()
    /* Get the id of the Card nearest the mouse cursor, if the mouse cursor is in the hand area.
    
    :returns string hoveredCardId:
    */
    {
        Vector2 mousePos = Input.mousePosition;

        bool isMouseInHandArea = (
            0 <= mousePos.x
            && mousePos.x <= cardAreaRightBound
            && cardAreaBottomBound <= mousePos.y
            && mousePos.y <= cardAreaTopBound
        );
        if (!isMouseInHandArea)
        {
            return null;
        }

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

    private void setCardSize(float multiplier = 1)
    /* Set this Card's size to some multiple of the default height and width. Keep the icon on the
        card a square, and slightly in from the sides of the card.

    :param float multiplier: Proportion by which to enlarge this Card's size by. 1 keeps it regular.
    */
    {

        Vector2 backgroundSize = new Vector2(cardWidth, cardHeight) * multiplier;
        GetComponent<RectTransform>().sizeDelta = backgroundSize;
        backgroundObj.GetComponent<RectTransform>().sizeDelta = backgroundSize;

        Vector2 iconSize = backgroundSize * 0.67f;
        iconSize.y = iconSize.x; // Make the icon a square.
        iconObj.GetComponent<RectTransform>().sizeDelta = iconSize;
    }

    private void setAllCardsDepth(string hoveredCardId)
    /* Set the order of Cards in the Hierarchy so they appear correctly front-to-back-wise.
    
    Note that if another Card's Update already ran and did this, nothing needs to happen here.

    :param string hoveredCardId: id of Card that should be highlighted because the mouse is near it,
        meaning it is "taken out" of order and placed at the end of the siblings in the Hierarchy.
    */
    {
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
