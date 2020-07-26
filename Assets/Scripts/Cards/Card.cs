using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Card : MonoBehaviour, IPointerClickHandler
{
    // Class-wide parameters.
    private static Color cardTextColor = new Color(80/255, 80/255, 80/255);
    private static int iumCostFontSize = 14;
    private static int displayNameFontSize = 8;
    private static int descriptionFontSize = 8;
    private static float cardHeight;
    private static float cardWidth;
    private static float highlightedCardSizeMultiplier;
    private static float shopCardSizeMultiplier;
    private static float cardAreaBottomBound;
    private static float cardAreaTopBound;
    private static float cardAreaRightBound;
    private static float handFanAngle;
    private static float movementHalftime;
    private static float movementMinSpeed;
    private static float growthSpeed;
    private static float rotationSpeed;
    // Hierarchy references.
    public GameObject backgroundObj;
    public GameObject iconObj;
    private GameObject iumCostObj;
    private GameObject displayNameObj;
    private GameObject descriptionObj;
    private GameObject handObj;
    private Canvas canvas;
    // Card-specific parameters.
    public string cardId;
    public string cardName;
    public bool isInTrial;
    // Card-specific parameters to be set by each subclass for that type of card.
    public int iumCost;
    public string displayName;
    public string description;
    public bool isConsumable;

    void Awake()
    {
        backgroundObj = transform.Find("Background").gameObject;
        iconObj = transform.Find("Icon").gameObject;
        iumCostObj = transform.Find("IumCostText").gameObject;
        displayNameObj = transform.Find("NameText").gameObject;
        descriptionObj = transform.Find("DescriptionText").gameObject;
        handObj = GameObject.Find("Hand");
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();

        cardHeight = 70;
        cardWidth = 110;
        highlightedCardSizeMultiplier = 1.3f;
        shopCardSizeMultiplier = 1.5f;
        cardAreaBottomBound = Screen.height * 0.05f;
        cardAreaTopBound = Screen.height * 0.7f;
        cardAreaRightBound = cardWidth * canvas.scaleFactor;
        handFanAngle = 20;
        movementHalftime = 0.05f;
        movementMinSpeed = 0.01f;
        growthSpeed = 2;
        rotationSpeed = 100;
    }

    public virtual void Start()
    {
        iumCostObj.GetComponent<Text>().color = cardTextColor;
        displayNameObj.GetComponent<Text>().color = cardTextColor;
        descriptionObj.GetComponent<Text>().color = cardTextColor;

        iumCostObj.GetComponent<Text>().fontSize = iumCostFontSize;
        displayNameObj.GetComponent<Text>().fontSize = displayNameFontSize;
        descriptionObj.GetComponent<Text>().fontSize = descriptionFontSize;

        iumCostObj.GetComponent<Text>().text = iumCost.ToString();
        displayNameObj.GetComponent<Text>().text = displayName;
        descriptionObj.GetComponent<Text>().text = description;

        if (isInTrial)
        {
            setCardSize(1);
        } else
        {
            setCardSize(shopCardSizeMultiplier);
        }
    }

    void Update()
    {
        if (isInTrial)
        {
            moveTowardPosition();

            growTowardSizeMultiplier();
            
            rotateTowardAngle();

            setAllCardsDepth();
        }
    }

    /* PUBLIC API */

    public void OnPointerClick(PointerEventData eventData)
    /* Override this function of IPointerClickHandler. Triggers when this Card is clicked.
    
    :param PointerEventData eventData: This interface is defined by Unity.
    */
    {
        // Select this Card, or deselect it if it's already selected.
        TrialLogic.selectedCardId = TrialLogic.selectedCardId == cardId ? null : cardId;
    }

    public void playCard()
    /* Check that there is enough ium to play this Card, perform this Card's Action, then discard it
        and unset the selected Card.
    */
    {
        bool isAbleToPlay = getIsAbleToPlay();
        if (!isAbleToPlay)
        {
            return;
        }
        
        int costToPlay = getCostToPlay();
        bool canAffordToPlay = TrialLogic.currentIum >= costToPlay;
        if (!canAffordToPlay)
        {
            return;
        }

        TrialLogic.gainIum(-costToPlay);
        cardAction();
        TrialLogic.discardCard(cardId);
        TrialLogic.selectedCardId = null;
    }

    /* INTERFACE FOR SUBCLASSES TO OVERRIDE */

    public virtual void setCardParams()
    /* Sets the fields:
    - iumCost
    - displayName
    - isConsumable
    
    Must be overridden in each subclass.
    */
    {
        
    }

    public virtual int getCostToPlay()
    /* Calculates the ium cost to play this Card at a particular square currently.
    
    May be overridden in each subclass.
    */
    {
        return iumCost;
    }

    public virtual bool getIsAbleToPlay()
    /* Checks if this Card can be played at a particular square.
    
    May be overridden in each subclass.

    :param Vector2 gridIndices: Square on the grid this Card is being applied to.

    :returns bool:
    */
    {
        return false;
    }

    public virtual void cardAction()
    /* Performs whatever action this Card is for.
    
    May be overridden in each subclass.
    */
    {
        
    }

    /* HELPERS */

    private float getCardSpacing()
    /* Return the distance between Cards displayed in the hand.
    
    :returns float cardSpacing:
    */
    {
        float totalDist = cardAreaTopBound - cardAreaBottomBound;
        int handSize = TrialLogic.hand.Count;
        float cardSpacing = totalDist / (handSize + 1);

        return cardSpacing;
    }

    private Vector3 getEventualPosition()
    /* Calculate this Card's veritcal position along the left-side of the screen. For 1 Card put it
        at 1/2, for 2 Cards put them at 1/3 and 2/3, etc.
    
    :returns Vector3 cardPosition: Position where this Card belongs.
    */
    {
        float cardX = -cardWidth / 4;
        string hoveredCardId = getHoveredCardId();
        if (TrialLogic.selectedCardId == cardId)
        {
            cardX = cardAreaRightBound;
        } else if (hoveredCardId == cardId)
        {
            cardX = cardWidth / 4;
        }

        int idxInHand = TrialLogic.hand.IndexOf(cardId);
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
        if (cardId == TrialLogic.selectedCardId || cardId == hoveredCardId)
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
        iconSize.x = iconSize.y; // Make the icon a square.
        iconObj.GetComponent<RectTransform>().sizeDelta = iconSize;

        descriptionObj.GetComponent<RectTransform>().sizeDelta = iconSize;

        iumCostObj.GetComponent<Text>().fontSize = (int)(iumCostFontSize * multiplier);
        displayNameObj.GetComponent<Text>().fontSize = (int)(displayNameFontSize * multiplier);
        descriptionObj.GetComponent<Text>().fontSize = (int)(descriptionFontSize * multiplier);
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

    private float getEventualAngle()
    /* Calculate the angle for this Card, so that the hand fans out a bit, but hovered and selected
        Cards are straight.
    
    :returns float: Angle to rotate this Card by.
    */
    {
        // Leave the Card flat horizontal if it is selected or hovered.
        bool isSelectedCard = TrialLogic.selectedCardId == cardId;
        string hoveredCardId = getHoveredCardId();
        bool isHoveredCard = hoveredCardId == cardId;
        if (isSelectedCard || isHoveredCard)
        {
            return 0;
        }

        // Fan out the hand so the top Card is angled up by handFanAngle, and the bottom Card is
        // angled down the same amount.
        int idxInHand = TrialLogic.hand.IndexOf(cardId);
        int handSize = TrialLogic.hand.Count;
        float angleSpacing = 2 * handFanAngle / handSize;
        float cardAngle = handFanAngle - (idxInHand * angleSpacing);

        return cardAngle;
    }

    private void rotateTowardAngle()
    /* Rotate this Card smoothly toward its eventual angle. */
    {
        float currentAngle = transform.localEulerAngles.z;
        float eventualAngle = getEventualAngle();
        float diff = eventualAngle - currentAngle;
        
        // Handle the case where Unity uses the range 180 to 360 instead of -180 to 0.
        float alternativeAngle = eventualAngle + 360;
        float alternativeDiff = alternativeAngle - currentAngle;
        if (Mathf.Abs(alternativeDiff) < Mathf.Abs(diff))
        {
            eventualAngle = alternativeAngle;
            diff = alternativeDiff;
        }

        if (diff == 0)
        {
            return;
        }

        float diffSign = diff / Mathf.Abs(diff);
        float angleChange = rotationSpeed * diffSign * Time.deltaTime;
        // If we are ever about to go past the desired size, instead just go straight to it.
        if (Mathf.Abs(angleChange) > Mathf.Abs(diff))
        {
            angleChange = diff;
        }

        float newAngle = currentAngle + angleChange;

        transform.localRotation = Quaternion.Euler(0, 0, newAngle);
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
        int handSize = TrialLogic.hand.Count;
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
        
        string hoveredCardId = hoveredIdx > -1 ? TrialLogic.hand[hoveredIdx] : null;

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
        List<string> desiredHierarchyOrder = new List<string>(TrialLogic.hand);
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
