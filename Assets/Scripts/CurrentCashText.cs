using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CurrentCashText : MonoBehaviour
{
    void Update()
    {
        int availableCash = CampaignLogic.currentCash - ShopLogic.getExpensesInCart();
        GetComponent<Text>().text = availableCash.ToString("N0");
    }
}
