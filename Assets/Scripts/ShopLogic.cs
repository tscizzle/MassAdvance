using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopLogic : MonoBehaviour
{
    private static List<string> shopInventory = new List<string>(); // cardIds
    private static List<string> cardsAddedToCart = new List<string>(); // cardIds
    private static List<string> disposalsAddedToCart = new List<string>(); // cardIds
    
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    /* HELPERS */

    private static int getExpensesInCart()
    /* Adds up all the costs of the deck modifications currently added to cart. */
    {
        return -1;
    }
}
