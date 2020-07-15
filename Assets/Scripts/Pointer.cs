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

    public static IEnumerator displayPointer(Vector2 gridIndices, Action callback = null)
    {
        GameObject pointerObj = PrefabInstantiator.P.CreatePointer(gridIndices);

        yield return new WaitForSeconds(GameLogic.G.secondsBetweenActions);
        
        Destroy(pointerObj);

        if (callback != null)
        {
            callback();
        }
    }
}
