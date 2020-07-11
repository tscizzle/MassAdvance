using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    private static string blueCardHex = "#33aafa";
    public static Color blueCardColor;
    private static string yellowCardHex = "#fafa88";
    public static Color yellowCardColor;
    private static string redCardHex = "#ee4466";
    public static Color redCardColor;
    private static Dictionary<string, Color> cardNameToIconColor;
    private static Dictionary<string, Color> cardNameToBackgroundColor;

    // Parameters.
    public string cardId;
    public string cardName;

    void Awake()
    {
        ColorUtility.TryParseHtmlString(blueCardHex, out blueCardColor);
        ColorUtility.TryParseHtmlString(yellowCardHex, out yellowCardColor);
        ColorUtility.TryParseHtmlString(redCardHex, out redCardColor);

        cardNameToBackgroundColor = new Dictionary<string, Color>
        {
            { getSingleBlockCardName(BlockType.BLUE), blueCardColor },
            { getSingleBlockCardName(BlockType.YELLOW), yellowCardColor },
            { getSingleBlockCardName(BlockType.RED), redCardColor },
        };
    }

    void Start()
    {
        // Block colors need to be defined before this. Happens in Block.cs's Awake, so this is in
        // Start.
        cardNameToIconColor = new Dictionary<string, Color>
        {
            { getSingleBlockCardName(BlockType.BLUE), Block.blueColor },
            { getSingleBlockCardName(BlockType.YELLOW), Block.yellowColor },
            { getSingleBlockCardName(BlockType.RED), Block.redColor },
        };

        cardName = GameLogic.G.cardsById[cardId].cardName;
        
        Color iconColor = cardNameToIconColor[cardName];
        GameObject iconObj = transform.Find("Icon").gameObject;
        iconObj.GetComponent<Image>().color = iconColor;
        
        Color backgroundColor = cardNameToBackgroundColor[cardName];
        GameObject backgroundObj = transform.Find("Background").gameObject;
        backgroundObj.GetComponent<Image>().color = backgroundColor;
    }

    void Update()
    {
        int handSize = GameLogic.G.hand.Count;
        int idxInHand = GameLogic.G.hand.IndexOf(cardId);
        
        float cardHeight = GetComponent<RectTransform>().sizeDelta.y;
        float bottomBound = cardHeight;
        float topBound = Screen.height;
        float cardSpacing = (topBound - bottomBound) / Mathf.Max(handSize - 1, 1);
        float cardY = topBound - (idxInHand * cardSpacing);
        transform.position = new Vector3(0, cardY, 0);

        GUI.depth = idxInHand;

        // TODO: make a card slightly larger and in front, based on mouse position
    }

    /* HELPERS */

    private static string getSingleBlockCardName(BlockType blockType)
    /* Given a BlockType, give the cardName of the card that places a single block of that type. */
    {
        return $"single_block_{blockType}";
    }
}
