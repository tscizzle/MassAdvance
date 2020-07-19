using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampaignLogic : MonoBehaviour
{
    public static CampaignLogic C = null;

    public static float startingGold;

    

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
}
