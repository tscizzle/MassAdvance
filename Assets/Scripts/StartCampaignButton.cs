using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartCampaignButton : MonoBehaviour
{
    /* PUBLIC API */

    public void clickStartCampaign()
    /* Click handler for button that starts the campaign. */
    {
        // TODO: take the packs in ShopLogic.packsAddedToCart and put the CardInfos into CampaignLogic.campaignDeck
        foreach (string packId in ShopLogic.packsAddedToCart)
        {
            List<CardInfo> packCards = ShopLogic.packInventory[packId];
            foreach (CardInfo cardInfo in packCards)
            {
                CampaignLogic.campaignDeck[cardInfo.cardId] = cardInfo;
            }
        }

        CampaignLogic.currentCash -= ShopLogic.getExpensesInCart();

        SceneManager.LoadScene("TrialScene");
    }
}
