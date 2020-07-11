using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    public static Color blueCardColor = new Color(51/255f, 170/255f, 250/255f);
    public static Color yellowCardColor = new Color(250/255f, 250/255f, 136/255f);
    public static Color redCardColor = new Color(238/255f, 68/255f, 102/255f);
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
    private static int cardHeight;
    private static int cardWidth;

    private GameObject backgroundObj;
    private GameObject iconObj;

    // Parameters.
    public string cardId;
    public string cardName;

    void Awake()
    {
        cardHeight = 200;
        cardWidth = 150;
    }

    void Start()
    {
        cardName = GameLogic.G.cardsById[cardId].cardName;

        iconObj = transform.Find("Icon").gameObject;
        backgroundObj = transform.Find("Background").gameObject;
        
        Color iconColor = cardNameToIconColor[cardName];
        iconObj.GetComponent<Image>().color = iconColor;
        
        Color backgroundColor = cardNameToBackgroundColor[cardName];
        backgroundObj.GetComponent<Image>().color = backgroundColor;
    }

    void Update()
    {
        // Place this card in the correct vertical position along the left-side of the screen.
        // For 1 card put it at 1/2. For 2 cards put them at 1/3 and 2/3. Etc.
        int handSize = GameLogic.G.hand.Count;
        int idxInHand = GameLogic.G.hand.IndexOf(cardId);
        float bottomBound = 0;
        float topBound = Screen.height;
        float totalDist = topBound - bottomBound;
        float cardSpacing = totalDist / (handSize + 1);
        float distFromTop = (idxInHand + 1) * cardSpacing;
        float cardY = topBound - distFromTop;
        transform.position = new Vector3(0, cardY, 0);
        
        // Default to this card not being highlighted (regular size, regular depth).
        setCardSize(1);
        GUI.depth = -(idxInHand + 1);

        // Highlight this card if the mouse is hovered near it, by enlarging the card and moving it
        // to the front.
        Vector2 mousePos = Input.mousePosition;
        bool isMouseInHandArea = (
            0 <= mousePos.x
            && mousePos.x <= cardWidth
            && bottomBound <= mousePos.y
            && mousePos.y <= topBound
        );
        if (isMouseInHandArea)
        {
            float mouseDistFromTop = topBound - mousePos.y;
            float areaPerCard = totalDist / handSize;
            int hoveredIdx = (int)Mathf.Floor(mouseDistFromTop / areaPerCard);
            string hoveredCardId = GameLogic.G.hand[hoveredIdx];
            if (cardId == hoveredCardId)
            {
                setCardSize(1.3f);
                GUI.depth = 0;
            }
        }
    }

    /* HELPERS */

    private static string getSingleBlockCardName(BlockType blockType)
    /* Given a BlockType, give the cardName of the card that places a single block of that type. */
    {
        return $"single_block_{blockType}";
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
}
