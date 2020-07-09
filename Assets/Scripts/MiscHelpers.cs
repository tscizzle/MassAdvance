using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiscHelpers : MonoBehaviour
{
    // Global var that even a prefab can reference. Will be assigned our 1 instance of MiscHelpers.
    public static MiscHelpers M;

    void Awake()
    {
        // Since there should only be 1 MiscHelpers instance, assign this instance to a global var.
        M = this;
    }

    void Start()
    {

    }
}
