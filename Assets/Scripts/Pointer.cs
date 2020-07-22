using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pointer : MonoBehaviour
{
    private static bool pointerLock = false;

    /* PUBLIC API */

    public static IEnumerator displayPointer(Vector2 gridIndices)
    {
        while (pointerLock)
        {
            yield return new WaitForSeconds(0);
        }
        pointerLock = true;

        GameObject pointerObj = PrefabInstantiator.P.CreatePointer(gridIndices);

        yield return new WaitForSeconds(TrialLogic.secondsBetweenActions);

        while (TrialLogic.isPauseModeOn)
        {
            yield return new WaitForSeconds(0);
        }
        
        Destroy(pointerObj);

        pointerLock = false;
    }
}
