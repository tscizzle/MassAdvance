using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiscHelpers : MonoBehaviour
{
    private static System.Random rng = new System.Random();

    void Start()
    {

    }

    /* PUBLIC API */

    public static string getRandomId()
    {
        string randomId = rng.Next(10000000, 99999999).ToString();
        return randomId;
    }
}
