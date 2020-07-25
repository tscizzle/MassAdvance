using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartTrialButton : MonoBehaviour
{
    void Start()
    {
        GetComponentInChildren<Text>().text = $"Start Trial {CampaignLogic.trialNumber + 1}";
    }

    /* PUBLIC API */

    public void clickStartTrial()
    /* Click handler for button that starts the campaign. */
    {
        // Put the purchased packs into the deck.
        foreach (string packId in ShopLogic.packsAddedToCart)
        {
            Pack pack = ShopLogic.packInventory[packId];
            List<CardInfo> packCards = pack.cardList;
            foreach (CardInfo cardInfo in packCards)
            {
                CampaignLogic.campaignDeck[cardInfo.cardId] = cardInfo;
            }
        }

        // Pay the money.
        CampaignLogic.currentCash -= ShopLogic.getExpensesInCart();
        ShopLogic.clearCart();

        // Start the trial.
        SceneManager.LoadScene("TrialScene");
    }
}
