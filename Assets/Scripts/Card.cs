using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    private static Dictionary<string, Color> cardNameToColor;

    // Parameters.
    public string cardId;
    public string cardName;

    void Start()
    {
        cardNameToColor = new Dictionary<string, Color>
        {
            { getSingleBlockCardName(BlockType.BLUE), Block.blueColor },
            { getSingleBlockCardName(BlockType.YELLOW), Block.yellowColor },
            { getSingleBlockCardName(BlockType.RED), Block.redColor },
        };

        cardName = GameLogic.G.cardsById[cardId].cardName;
        
        Color color = cardNameToColor[cardName];
        GetComponentInChildren<Image>().color = color;
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

    /* PUBLIC API */

    public static string getSingleBlockCardName(BlockType blockType)
    /* Given a BlockType, give the cardName of the card that places a single block of that type. */
    {
        return $"single_block_{blockType}";
    }
}
