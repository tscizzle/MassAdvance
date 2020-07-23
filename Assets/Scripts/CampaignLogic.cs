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
    public static int cashRewardPerTrial;
    // State.
    public static int currentCash;
    public static Dictionary<string, CardInfo> campaignDeck;
    public static int trialNumber;

    static CampaignLogic()
    {
        // Initialize parameters.
        startingCash = 10500;
        cashRewardPerTrial = 5000;

        // Initialize state.
        currentCash = startingCash;
        campaignDeck = new Dictionary<string, CardInfo>();
        trialNumber = 0;
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
        SceneManager.LoadScene("ShopScene");
    }
}
