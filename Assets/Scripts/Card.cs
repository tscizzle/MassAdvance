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

    // State.
    private Vector3 cardPosition;

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
        cardName = GameLogic.G.cardsById[cardId].cardName;
        
        Color backgroundColor = cardNameToBackgroundColor[cardName];
        backgroundObj.GetComponent<Image>().color = backgroundColor;
        
        Color iconColor = cardNameToIconColor[cardName];
        iconObj.GetComponent<Image>().color = iconColor;
    }

    void Update()
    {
        // Place this Card in the correct vertical position along the left-side of the screen.
        // For 1 Card put it at 1/2. For 2 Cards put them at 1/3 and 2/3. Etc.
        // Note however, we move the Card there non-instantaneously, using moveTowardPosition().
        int handSize = GameLogic.G.hand.Count;
        int idxInHand = GameLogic.G.hand.IndexOf(cardId);
        float totalDist = cardAreaTopBound - cardAreaBottomBound;
        float cardSpacing = totalDist / (handSize + 1);
        float distFromTop = (idxInHand + 1) * cardSpacing;
        float cardY = cardAreaTopBound - distFromTop;
        cardPosition = new Vector3(0, cardY, 0);
        moveTowardPosition();
        
        // Default to this Card not being highlighted (regular size, regular depth).
        setCardSize(1);

        // Highlight this card if the mouse is hovered near it, by enlarging the card and moving it
        // to the front.
        Vector2 mousePos = Input.mousePosition;
        bool isMouseInHandArea = (
            0 <= mousePos.x
            && mousePos.x <= cardAreaRightBound
            && cardAreaBottomBound <= mousePos.y
            && mousePos.y <= cardAreaTopBound
        );
        string hoveredCardId = null;
        if (isMouseInHandArea)
        {
            float mouseDistFromTop = cardAreaTopBound - mousePos.y;
            float areaPerCard = totalDist / handSize;
            int hoveredIdx = (int)Mathf.Floor(mouseDistFromTop / areaPerCard);
            hoveredCardId = GameLogic.G.hand[hoveredIdx];
            if (cardId == hoveredCardId)
            {
                setCardSize(1.3f);
            }
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
        GameLogic.G.selectedCardId = cardId;
    }

    /* HELPERS */

    private static string getSingleBlockCardName(BlockType blockType)
    /* Given a BlockType, give the cardName of the card that places a single block of that type. */
    {
        return $"single_block_{blockType}";
    }

    private void moveTowardPosition()
    /* We update each Card's cardPosition instantly when it needs to move, but the actual object
        doesn't move there instantly. Use this function to move there smoothly.
    */
    {
        Vector3 currentPosition = transform.position;
        Vector3 diff = cardPosition - currentPosition;
        float diffHalfDist = diff.magnitude / 2;
        float movementSpeed = Mathf.Max(diffHalfDist / movementHalftime, movementMinSpeed);
        float movementDist = movementSpeed * Time.deltaTime;
        Vector3 movement = diff.normalized * movementDist;
        transform.position = currentPosition + movement;
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
        List<string> desiredHierarchyOrder = new List<string>(GameLogic.G.hand);
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
        Debug.Log(desiredHierarchyOrder.ToString());
        Debug.Log(currentHierarchyOrder.ToString());
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
