using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Pointer : MonoBehaviour
{
    private static bool pointerLock = false;

    public string text;

    void Start()
    {
        if (text != null)
        {
            GameObject textObj = transform.Find("PointerText").gameObject;
            textObj.GetComponent<TextMeshPro>().text = text;
            textObj.SetActive(true);
        }
    }

    /* PUBLIC API */

    public static IEnumerator displayPointer(Vector2 gridIndices, string text = null)
    {
        while (pointerLock)
        {
            yield return new WaitForSeconds(0);
        }
        pointerLock = true;

        GameObject pointerObj = PrefabInstantiator.P.CreatePointer(gridIndices, text: text);

        yield return new WaitForSeconds(TrialLogic.secondsBetweenActions);

        while (TrialLogic.isPauseModeOn)
        {
            yield return new WaitForSeconds(0);
        }
        
        Destroy(pointerObj);

        pointerLock = false;
    }
}
