using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabInstantiator : MonoBehaviour
{
    // Global var that even a prefab can reference. Will be assigned our 1 instance of MiscHelpers.
    public static PrefabInstantiator P;

    public GameObject blockPrefab;
    public GameObject pointerPrefab;
    public GameObject floorSquarePrefab;
    public GameObject repairBlockCardPrefab;
    public GameObject placeSingleBlockCardPrefab;

    private Canvas canvas;
    private GameObject handObj;

    void Awake()
    {
        // Since there should only be 1 PrefabInstantiator instance, assign this instance to a global var.
        P = this;
    }

    void Start()
    {
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        handObj = canvas.transform.Find("Hand").gameObject;
    }

    /* PUBLIC API */

    public GameObject CreateBlock(BlockType blockType, Vector2 gridIndices)
    /* Create a Block.

    :param string blockType: One of [ "mass", "blue", "yellow", "red" ]
    :param Vector2 gridIndices: The square in which to put the block ((0, 0) is the bottom-left).
    
    :returns GameObject blockObj:
    */
    {
        Vector3 position = FloorSquare.getGridSquareCenter(gridIndices);

        GameObject blockObj = Instantiate(blockPrefab, position, Quaternion.identity);
        
        Block block = blockObj.GetComponent<Block>();
        block.blockType = blockType;
        block.gridIndices = gridIndices;
        
        return blockObj;
    }

    public GameObject CreateCard(string cardId)
    /* Create a Card.

    :param string cardId: String identifying the Card to create.

    :returns GameObject cardObj:
    */
    {
        CardInfo cardInfo = TrialLogic.T.cardsById[cardId];
        string cardName = cardInfo.cardName;

        GameObject cardPrefab = cardNameToPrefabMap(cardName);
        Vector3 position = new Vector3(0, -Screen.height, 0);
        GameObject cardObj = Instantiate(cardPrefab, position, Quaternion.identity);
        cardObj.transform.SetParent(handObj.transform, worldPositionStays: false);

        Card card = cardObj.GetComponent<Card>();
        card.cardId = cardId;
        card.cardName = cardName;
        card.setCardParams();

        cardInfo.card = card;
        TrialLogic.T.cardsById[cardId] = cardInfo;

        return cardObj;
    }

    public GameObject CreateFloorSquare(Vector2 gridIndices, int numTurnsStained = 0)
    /* Create a FloorSquare.

    :param Vector2 gridIndices: Which square of the grid this object is ((0, 0) is the bottom-left).

    :returns GameObject floorSquareObj:
    */
    {
        Vector3 position = FloorSquare.getGridSquareCenter(gridIndices);
        GameObject floorSquareObj = Instantiate(floorSquarePrefab, position, Quaternion.identity);

        FloorSquare floorSquare = floorSquareObj.GetComponent<FloorSquare>();
        floorSquare.gridIndices = gridIndices;
        floorSquare.numTurnsStained = numTurnsStained;

        FloorSquare.floorSquaresMap[gridIndices] = floorSquare;

        return floorSquareObj;
    }

    public GameObject CreatePointer(Vector2 gridIndices)
    /* Create a Pointer.

    :param Vector2 gridIndices: The square over which to point the pointer ((0, 0) is the
        bottom-left).

    :returns GameObject pointerObj:
    */
    {
        Vector3 position = FloorSquare.getGridSquareCenter(gridIndices);
        position.y = 1;

        GameObject pointerObj = Instantiate(pointerPrefab, position, Quaternion.identity);

        return pointerObj;
    }

    /* HELPERS */

    private GameObject cardNameToPrefabMap(string cardName)
    /* Of the Prefabs that are attached to this Script, return the one that's associated with the
        given cardName.
    
    :param string cardName:

    :returns GameObject cardPrefab:
    */
    {
        if (cardName == "repair_block")
        {
            return repairBlockCardPrefab;
        } else if (PlaceSingleBlockCard.cardNameToBlockType.ContainsKey(cardName))
        {
            return placeSingleBlockCardPrefab;
        } else
        {
            // Shouldn't ever get here. It would mean we have a card without an associated prefab.
            return new GameObject();
        }
    }
}
