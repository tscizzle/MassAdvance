﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ShopLogic : MonoBehaviour
{
    // Parameters.
    private static float maxDiscountRatio = 3;
    private static float discountDampener = 1/10f;
    private static int giantPackSize = 20;
    private static int bigPackSize = 15;
    private static int mediumPackSize = 10;
    private static int smallPackSize = 5;
    private static int numUniquesInVarietyPack = 5;
    private static int numUniquesInSpecialistPack = 2;
    private static int numSingleCardPacks = 20;
    // State.
    public static Dictionary<string, Pack> packInventory;
    public static List<string> packOrder;
    public static List<string> packsAddedToCart;
    private static List<string> disposalsAddedToCart;
    
    void Awake()
    {
        // Initialize state.
        packInventory = new Dictionary<string, Pack>();
        packOrder = new List<string>();
        packsAddedToCart = new List<string>();
        disposalsAddedToCart = new List<string>();

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
        Pack pack = packInventory[packId];
        List<CardInfo> packCards = pack.cardList;
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

    public static void clearCart()
    /* Empty everything from the cart. */
    {
        packsAddedToCart = new List<string>();
    }

    /* HELPERS */

    private static void populateShopInventory()
    /* Randomly generate some packs and individual cards to be bought */
    {
        System.Random rand = new System.Random();

        // Giant specialist pack.
        List<CardInfo> cardsInGiantSpecialist = new List<CardInfo>();
        int numRepeatsInGiantSpecialty = giantPackSize / numUniquesInSpecialistPack;
        foreach (int _0 in Enumerable.Range(0, numUniquesInSpecialistPack))
        {
            string cardName = MiscHelpers.getWeightedChoice(cardNameToFrequency, rand);
            foreach (int _1 in Enumerable.Range(0, numRepeatsInGiantSpecialty))
            {
                string cardId = MiscHelpers.getRandomId();
                CardInfo cardInfo = new CardInfo(cardName, cardId);
                cardsInGiantSpecialist.Add(cardInfo);
            }
        }
        string giantSpecialistPackId = MiscHelpers.getRandomId();
        Pack giantSpecialistPack = new Pack(
            giantSpecialistPackId, "Giant Specialist", cardsInGiantSpecialist
        );
        packInventory[giantSpecialistPackId] = giantSpecialistPack;
        packOrder.Add(giantSpecialistPackId);

        // Giant variety pack.
        List<CardInfo> cardsInGiantVariety = new List<CardInfo>();
        int numRepeatsInGiantVariety = giantPackSize / numUniquesInVarietyPack;
        foreach (int _0 in Enumerable.Range(0, numUniquesInVarietyPack))
        {
            string cardName = MiscHelpers.getWeightedChoice(cardNameToFrequency, rand);
            foreach (int _1 in Enumerable.Range(0, numRepeatsInGiantVariety))
            {
                string cardId = MiscHelpers.getRandomId();
                CardInfo cardInfo = new CardInfo(cardName, cardId);
                cardsInGiantVariety.Add(cardInfo);
            }
        }
        string giantVarietyPackId = MiscHelpers.getRandomId();
        Pack giantVarietyPack = new Pack(giantVarietyPackId, "Giant Variety", cardsInGiantVariety);
        packInventory[giantVarietyPackId] = giantVarietyPack;
        packOrder.Add(giantVarietyPackId);

        // Big specialist pack.
        List<CardInfo> cardsInBigSpecialist = new List<CardInfo>();
        int numRepeatsInBigSpecialty = bigPackSize / numUniquesInSpecialistPack;
        foreach (int _0 in Enumerable.Range(0, numUniquesInSpecialistPack))
        {
            string cardName = MiscHelpers.getWeightedChoice(cardNameToFrequency, rand);
            foreach (int _1 in Enumerable.Range(0, numRepeatsInBigSpecialty))
            {
                string cardId = MiscHelpers.getRandomId();
                CardInfo cardInfo = new CardInfo(cardName, cardId);
                cardsInBigSpecialist.Add(cardInfo);
            }
        }
        string bigSpecialistPackId = MiscHelpers.getRandomId();
        Pack bigSpecialistPack = new Pack(
            bigSpecialistPackId, "Big Specialist", cardsInBigSpecialist
        );
        packInventory[bigSpecialistPackId] = bigSpecialistPack;
        packOrder.Add(bigSpecialistPackId);

        // Big variety pack.
        List<CardInfo> cardsInBigVariety = new List<CardInfo>();
        int numRepeatsInBigVariety = bigPackSize / numUniquesInVarietyPack;
        foreach (int _0 in Enumerable.Range(0, numUniquesInVarietyPack))
        {
            string cardName = MiscHelpers.getWeightedChoice(cardNameToFrequency, rand);
            foreach (int _1 in Enumerable.Range(0, numRepeatsInBigVariety))
            {
                string cardId = MiscHelpers.getRandomId();
                CardInfo cardInfo = new CardInfo(cardName, cardId);
                cardsInBigVariety.Add(cardInfo);
            }
        }
        string bigVarietyPackId = MiscHelpers.getRandomId();
        Pack bigVarietyPack = new Pack(bigVarietyPackId, "Big Variety", cardsInBigVariety);
        packInventory[bigVarietyPackId] = bigVarietyPack;
        packOrder.Add(bigVarietyPackId);

        // Medium specialist pack.
        List<CardInfo> cardsInMediumSpecialist = new List<CardInfo>();
        int numRepeatsInMediumSpecialty = mediumPackSize / numUniquesInSpecialistPack;
        foreach (int _0 in Enumerable.Range(0, numUniquesInSpecialistPack))
        {
            string cardName = MiscHelpers.getWeightedChoice(cardNameToFrequency, rand);
            foreach (int _1 in Enumerable.Range(0, numRepeatsInMediumSpecialty))
            {
                string cardId = MiscHelpers.getRandomId();
                CardInfo cardInfo = new CardInfo(cardName, cardId);
                cardsInMediumSpecialist.Add(cardInfo);
            }
        }
        string mediumSpecialistPackId = MiscHelpers.getRandomId();
        Pack mediumSpecialistPack = new Pack(
            mediumSpecialistPackId, "Medium Specialist", cardsInMediumSpecialist
        );
        packInventory[mediumSpecialistPackId] = mediumSpecialistPack;
        packOrder.Add(mediumSpecialistPackId);

        // Medium variety pack.
        List<CardInfo> cardsInMediumVariety = new List<CardInfo>();
        int numRepeatsInMediumVariety = mediumPackSize / numUniquesInVarietyPack;
        foreach (int _0 in Enumerable.Range(0, numUniquesInVarietyPack))
        {
            string cardName = MiscHelpers.getWeightedChoice(cardNameToFrequency, rand);
            foreach (int _1 in Enumerable.Range(0, numRepeatsInMediumVariety))
            {
                string cardId = MiscHelpers.getRandomId();
                CardInfo cardInfo = new CardInfo(cardName, cardId);
                cardsInMediumVariety.Add(cardInfo);
            }
        }
        string mediumVarietyPackId = MiscHelpers.getRandomId();
        Pack mediumVarietyPack = new Pack(
            mediumVarietyPackId, "Medium Variety", cardsInMediumVariety
        );
        packInventory[mediumVarietyPackId] = mediumVarietyPack;
        packOrder.Add(mediumVarietyPackId);

        // Small specialist pack.
        List<CardInfo> cardsInSmallSpecialist = new List<CardInfo>();
        int numRepeatsInSmallSpecialty = smallPackSize / numUniquesInSpecialistPack;
        foreach (int _0 in Enumerable.Range(0, numUniquesInSpecialistPack))
        {
            string cardName = MiscHelpers.getWeightedChoice(cardNameToFrequency, rand);
            foreach (int _1 in Enumerable.Range(0, numRepeatsInSmallSpecialty))
            {
                string cardId = MiscHelpers.getRandomId();
                CardInfo cardInfo = new CardInfo(cardName, cardId);
                cardsInSmallSpecialist.Add(cardInfo);
            }
        }
        string smallSpecialistPackId = MiscHelpers.getRandomId();
        Pack smallSpecialistPack = new Pack(
            smallSpecialistPackId, "Small Specialist", cardsInSmallSpecialist
        );
        packInventory[smallSpecialistPackId] = smallSpecialistPack;
        packOrder.Add(smallSpecialistPackId);

        // Small variety pack.
        List<CardInfo> cardsInSmallVariety = new List<CardInfo>();
        int numRepeatsInSmallVariety = smallPackSize / numUniquesInVarietyPack;
        foreach (int _0 in Enumerable.Range(0, numUniquesInVarietyPack))
        {
            string cardName = MiscHelpers.getWeightedChoice(cardNameToFrequency, rand);
            foreach (int _1 in Enumerable.Range(0, numRepeatsInSmallVariety))
            {
                string cardId = MiscHelpers.getRandomId();
                CardInfo cardInfo = new CardInfo(cardName, cardId);
                cardsInSmallVariety.Add(cardInfo);
            }
        }
        string smallVarietyPackId = MiscHelpers.getRandomId();
        Pack smallVarietyPack = new Pack(smallVarietyPackId, "Small Variety", cardsInSmallVariety);
        packInventory[smallVarietyPackId] = smallVarietyPack;
        packOrder.Add(smallVarietyPackId);

        // Single-card packs.
        foreach (int idx in Enumerable.Range(0, numSingleCardPacks))
        {
            string cardName = MiscHelpers.getWeightedChoice(cardNameToFrequency, rand);
            string cardId = MiscHelpers.getRandomId();
            CardInfo cardInfo = new CardInfo(cardName, cardId);
            string packId = MiscHelpers.getRandomId();
            Pack pack = new Pack(
                packId, $"Single-card Pack {idx}", new List<CardInfo> { cardInfo }
            );
            packInventory[packId] = pack;
            packOrder.Add(packId);
        }
    }

    private static void renderShopInventory()
    /* Put the available packs on the screen as UI elements. */
    {
        Transform inventoryContainer = GameObject.Find("ShopInventoryContainer").transform;
        Transform inventoryViewport = inventoryContainer.Find("Viewport");
        Transform inventoryContent = inventoryViewport.Find("Content");

        foreach (string packId in packOrder)
        {
            GameObject packDisplayObj = PrefabInstantiator.P.CreatePackDisplay(packId);
            
            packDisplayObj.transform.SetParent(inventoryContent);
        }
    }

    /* COLLECTIONS */

    public static Dictionary<string, int> cardNameToBaseCashValue = new Dictionary<string, int>
    {
        { PlaceSingleBlockCard.getSingleBlockCardName(BlockType.BLUE), 500 },
        { PlaceSingleBlockCard.getSingleBlockCardName(BlockType.YELLOW), 450 },
        { PlaceSingleBlockCard.getSingleBlockCardName(BlockType.RED), 350 },
        { PlaceThreePieceCard.getThreePieceCardName(BlockType.BLUE), 1000 },
        { PlaceThreePieceCard.getThreePieceCardName(BlockType.YELLOW), 900 },
        { PlaceThreePieceCard.getThreePieceCardName(BlockType.RED), 700 },
        { PlaceThreePieceCard.getThreePieceCardName(BlockType.BLUE, isFlipped: true), 1000 },
        { PlaceThreePieceCard.getThreePieceCardName(BlockType.YELLOW, isFlipped: true), 900 },
        { PlaceThreePieceCard.getThreePieceCardName(BlockType.RED, isFlipped: true), 700 },
        { RepairBlockCard.repairBlockCardName, 300 },
        { WashFloorSquareCard.washFloorSquareCardName, 300 },
        { ArmageddonCard.armageddonCardName, 550 },
        { PunchCard.punchCardName, 1000 }
    };

    public static Dictionary<string, float> cardNameToFrequency = new Dictionary<string, float>
    {
        { PlaceSingleBlockCard.getSingleBlockCardName(BlockType.BLUE), 1 },
        { PlaceSingleBlockCard.getSingleBlockCardName(BlockType.YELLOW), 1 },
        { PlaceSingleBlockCard.getSingleBlockCardName(BlockType.RED), 1 },
        { PlaceThreePieceCard.getThreePieceCardName(BlockType.BLUE), 0.5f },
        { PlaceThreePieceCard.getThreePieceCardName(BlockType.YELLOW), 0.5f },
        { PlaceThreePieceCard.getThreePieceCardName(BlockType.RED), 0.5f },
        { PlaceThreePieceCard.getThreePieceCardName(BlockType.BLUE, isFlipped: true), 0.5f },
        { PlaceThreePieceCard.getThreePieceCardName(BlockType.YELLOW, isFlipped: true), 0.5f },
        { PlaceThreePieceCard.getThreePieceCardName(BlockType.RED, isFlipped: true), 0.5f },
        { RepairBlockCard.repairBlockCardName, 1 },
        { WashFloorSquareCard.washFloorSquareCardName, 1 },
        { ArmageddonCard.armageddonCardName, 1 },
        { PunchCard.punchCardName, 1 }
    };
}
