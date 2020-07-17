using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pointer : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    /* PUBLIC API */

    public static IEnumerator displayPointer(Vector2 gridIndices)
    {
        GameObject pointerObj = PrefabInstantiator.P.CreatePointer(gridIndices);

        yield return new WaitForSeconds(TrialLogic.secondsBetweenActions);
        
        Destroy(pointerObj);
    }
}
