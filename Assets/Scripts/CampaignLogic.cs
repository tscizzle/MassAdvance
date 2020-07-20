using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CampaignLogic : MonoBehaviour
{
    // This lets us keep only 1 instance existing at a time.
    public static CampaignLogic C = null;

    // Parameters.
    private static int startingCash;
    // State.
    public static int currentCash;
    public static Dictionary<string, CardInfo> campaignDeck = new Dictionary<string, CardInfo>();

    static CampaignLogic()
    {
        // Initialize parameters.
        startingCash = 10500;

        // Initialize state.
        currentCash = startingCash;
    }

    void Awake()
    {
        // Keep only 1 instance existing at a time.
        if (C == null)
        {
            C = this;
        } else
        {
            Destroy(this);
        }

        // Persist this object across scenes.
        DontDestroyOnLoad(this);
    }

    IEnumerator Start()
    {
        yield return new WaitForSeconds(2);

        // Open an actual scene of the game.
        SceneManager.LoadScene("DeckBuildingScene");
    }
}
