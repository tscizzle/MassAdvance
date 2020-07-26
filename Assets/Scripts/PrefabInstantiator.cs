using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabInstantiator : MonoBehaviour
{
    // Global var that even a prefab can reference. Will be assigned our 1 instance of
    // PrefabInstantiator.
    public static PrefabInstantiator P;

    public GameObject blockPrefab;
    public GameObject pointerPrefab;
    public GameObject floorSquarePrefab;
    public GameObject packDisplayPrefab;
    public GameObject placeSingleBlockCardPrefab;
    public GameObject placeThreePieceCardPrefab;
    public GameObject repairBlockCardPrefab;
    public GameObject washFloorSquareCardPrefab;
    public GameObject armageddonCardPrefab;
    public GameObject punchCardPrefab;

    void Awake()
    {
        // Since there should only be 1 PrefabInstantiator instance, assign this instance to a
        // global var.
        P = this;
    }

    /* PUBLIC API */

    public GameObject CreateBlock(BlockType blockType, Vector2 gridIndices)
    /* Create a Block.

    :param BlockType blockType: One of [ "mass", "blue", "yellow", "red" ]
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

    public GameObject CreateCard(CardInfo cardInfo, Transform parent, bool isInTrial = true)
    /* Create a Card.

    :param CardInfo cardInfo:
    :param Transform parent:
    :param bool isInTrial:

    :returns GameObject cardObj:
    */
    {
        string cardId = cardInfo.cardId;
        string cardName = cardInfo.cardName;

        GameObject cardPrefab = cardNameToPrefabMap(cardName);
        Vector3 position = isInTrial ? new Vector3(0, -Screen.height, 0) : Vector3.zero;
        GameObject cardObj = Instantiate(cardPrefab, position, Quaternion.identity);
        
        cardObj.transform.SetParent(parent, worldPositionStays: false);

        Card card = cardObj.GetComponent<Card>();
        card.cardId = cardId;
        card.cardName = cardName;
        card.isInTrial = isInTrial;
        card.setCardParams();

        return cardObj;
    }

    public GameObject CreateFloorSquare(Vector2 gridIndices, bool isSacred = false)
    /* Create a FloorSquare.

    :param Vector2 gridIndices: Which square of the grid this object is ((0, 0) is the bottom-left).

    :returns GameObject floorSquareObj:
    */
    {
        Vector3 position = FloorSquare.getGridSquareCenter(gridIndices);
        GameObject floorSquareObj = Instantiate(floorSquarePrefab, position, Quaternion.identity);

        FloorSquare floorSquare = floorSquareObj.GetComponent<FloorSquare>();
        floorSquare.gridIndices = gridIndices;
        floorSquare.isSacred = isSacred;
        floorSquare.numTurnsStained = 0;

        TrialLogic.floorSquaresMap[gridIndices] = floorSquare;

        return floorSquareObj;
    }

    public GameObject CreatePointer(Vector2 gridIndices, string text = null)
    /* Create a Pointer.

    :param Vector2 gridIndices: The square over which to point the pointer ((0, 0) is the
        bottom-left).

    :returns GameObject pointerObj:
    */
    {
        Vector3 position = FloorSquare.getGridSquareCenter(gridIndices);
        position.y = 1;

        GameObject pointerObj = Instantiate(pointerPrefab, position, Quaternion.identity);

        Pointer pointer = pointerObj.GetComponent<Pointer>();
        pointer.text = text;

        return pointerObj;
    }

    public GameObject CreatePackDisplay(string packId)
    /* Create a PackDisplay.
    
    :param string packId: Id of this pack to display in the shop.

    :returns GameObject packDisplayObj:
    */
    {
        GameObject packDisplayObj = Instantiate(
            packDisplayPrefab, Vector3.zero, Quaternion.identity
        );

        PackDisplay packDisplay = packDisplayObj.GetComponent<PackDisplay>();
        packDisplay.packId = packId;

        return packDisplayObj;
    }

    /* HELPERS */

    private GameObject cardNameToPrefabMap(string cardName)
    /* Of the Prefabs that are attached to this Script, return the one that's associated with the
        given cardName.
    
    :param string cardName:

    :returns GameObject cardPrefab:
    */
    {
        if (cardName == RepairBlockCard.repairBlockCardName)
        {
            return repairBlockCardPrefab;
        } else if (cardName == WashFloorSquareCard.washFloorSquareCardName)
        {
            return washFloorSquareCardPrefab;
        } else if (cardName == ArmageddonCard.armageddonCardName)
        {
            return armageddonCardPrefab;
        } else if (cardName == PunchCard.punchCardName)
        {
            return punchCardPrefab;
        } else if (PlaceSingleBlockCard.isCardNameThisCard(cardName))
        {
            return placeSingleBlockCardPrefab;
        } else if (PlaceThreePieceCard.isCardNameThisCard(cardName))
        {
            return placeThreePieceCardPrefab;
        } else
        {
            // Shouldn't ever get here. It would mean we have a card without an associated prefab.
            return new GameObject();
        }
    }
}
