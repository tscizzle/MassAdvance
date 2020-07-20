using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ShopLogic : MonoBehaviour
{
    public static Dictionary<string, List<CardInfo>> packInventory =
        new Dictionary<string, List<CardInfo>>();
    public static List<string> packsAddedToCart = new List<string>();
    private static List<string> disposalsAddedToCart = new List<string>();
    private static float maxDiscountRatio = 3;
    private static float discountDampener = 1/10f;
    
    void Awake()
    {
        populateShopInventory();
    }

    void Start()
    {
        renderShopInventory();
    }

    /* PUBLIC API */

    public static int getBaseCashValueOfPack(string packId)
    /* A pack's cash value is based on the value of the cards inside it, with a discount for
        containing more cards.
    
    :param string packId:

    :returns int value: The value of the specified pack, before randomization or anything.
    */
    {
        List<CardInfo> packCards = packInventory[packId];
        int fullValue = packCards.Sum(cardInfo => cardNameToBaseCashValue[cardInfo.cardName]);
        int packSize = packCards.Count;
        
        // Discount based on pack-size.
        // For single card, it's the minimum of 1. For infinity cards, it's the maximum discount.
        // Check: When packSize is 1, it should be 1. When packSize is infinity, it should be max.
        // Commented next to each expression is its range, as packSize ranges from 1 to infinity.
        float discountRatio = (
            (
                (
                    1 - ( // 0 -> 1
                        1 / ( // 1 -> 0
                            (packSize * discountDampener) + (1 - discountDampener) // 1 -> infinity
                        )
                    )
                )
                * (maxDiscountRatio - 1) // 0 -> maxDiscountRatio - 1
            )
            + 1 // 1 -> maxDiscountRatio
        );

        int value = (int)(fullValue / discountRatio);

        return value;
    }

    public static int getExpensesInCart()
    /* Adds up all the costs of the deck modifications currently added to cart.
    
    :returns int total:
    */
    {
        int total = packsAddedToCart.Sum(packId => getBaseCashValueOfPack(packId));
        return total;
    }

    /* HELPERS */

    private static void populateShopInventory()
    /* Randomly generate some packs and individual cards to be bought */
    {
        System.Random rand = new System.Random();

        string[] cardNameOptions = cardNameToBaseCashValue.Keys.ToArray();

        // Create packs of various sizes, including singletons.
        int[] packSizes = { 30, 30, 20, 20, 10, 10, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
        foreach (int packSize in packSizes)
        {
            List<CardInfo> packCards = new List<CardInfo>();
            foreach (int _ in Enumerable.Range(0, packSize))
            {
                string cardId = MiscHelpers.getRandomId();
                string cardName = MiscHelpers.getRandomChoice(cardNameOptions, rand);
                CardInfo cardInfo = new CardInfo(cardName, cardId);
                packCards.Add(cardInfo);
            }

            string packId = MiscHelpers.getRandomId();
            packInventory[packId] = packCards;
        }
    }

    private static void renderShopInventory()
    /* Put the available packs on the screen as UI elements. */
    {
        Transform inventoryContainer = GameObject.Find("ShopInventoryContainer").transform;
        Transform inventoryViewport = inventoryContainer.Find("Viewport");
        Transform inventoryContent = inventoryViewport.Find("Content");

        foreach (string packId in packInventory.Keys)
        {
            GameObject packDisplayObj = PrefabInstantiator.P.CreatePackDisplay(packId);
            
            packDisplayObj.transform.SetParent(inventoryContent);
        }
    }

    /* COLLECTIONS */

    private static Dictionary<string, int> cardNameToBaseCashValue = new Dictionary<string, int>
    {
        { RepairBlockCard.repairBlockCardName, 300 },
        { PlaceSingleBlockCard.getSingleBlockCardName(BlockType.BLUE), 500 },
        { PlaceSingleBlockCard.getSingleBlockCardName(BlockType.YELLOW), 450 },
        { PlaceSingleBlockCard.getSingleBlockCardName(BlockType.RED), 350 },
    };
}
