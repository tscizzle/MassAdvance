using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PackDisplay : MonoBehaviour
{
    private GameObject labelTextObj;

    public string packId;

    void Start()
    {
        labelTextObj = transform.Find("LabelText").gameObject;

        // Count each type of Card.
        Pack pack = ShopLogic.packInventory[packId];
        string packName = pack.packName;
        List<CardInfo> packCards = pack.cardList;
        Dictionary<string, int> cardNameCounts = new Dictionary<string, int>();
        foreach (CardInfo cardInfo in packCards)
        {
            if (!cardNameCounts.ContainsKey(cardInfo.cardName))
            {
                cardNameCounts[cardInfo.cardName] = 0;
            }
            cardNameCounts[cardInfo.cardName] += 1;
        }

        // Display something like "3 x [ ]" for each type of Card.
        Transform cardsContainer = transform.Find("CardsContainer");
        Transform cardsViewport = cardsContainer.Find("Viewport");
        Transform cardsContent = cardsViewport.Find("Content");

        foreach (string cardName in cardNameCounts.Keys)
        {
            string cardId = MiscHelpers.getRandomId();
            CardInfo cardInfo = new CardInfo(cardName, cardId);
            GameObject cardObj = PrefabInstantiator.P.CreateCard(
                cardInfo, cardsContent, isInTrial: false
            );

            GameObject aboveCardTextObj = cardObj.transform.Find("AboveCardText").gameObject;
            Text numCardRepeatsText = aboveCardTextObj.GetComponent<Text>();
            numCardRepeatsText.text = $"{cardNameCounts[cardName]} x ";
        }

        // Display the price of this pack.
        int price = ShopLogic.getBaseCashValueOfPack(packId);
        labelTextObj.GetComponent<Text>().text = $"{packName} - Price: {price}";
    }

    /* PUBLIC API */

    public void clickAddPack()
    /* Override this function of IPointerClickHandler. Triggers when this PackDisplay is clicked.
    
    :param PointerEventData eventData: This interface is defined by Unity.
    */
    {
        if (ShopLogic.packsAddedToCart.Contains(packId))
        {
            ShopLogic.packsAddedToCart.Remove(packId);
            labelTextObj.GetComponent<Text>().fontStyle = FontStyle.Normal;
        } else
        {
            int availableCash = CampaignLogic.currentCash - ShopLogic.getExpensesInCart();
            int packPrice = ShopLogic.getBaseCashValueOfPack(packId);
            if (availableCash >= packPrice)
            {
                ShopLogic.packsAddedToCart.Add(packId);
                labelTextObj.GetComponent<Text>().fontStyle = FontStyle.Bold;
            }
        }
    }
}
