using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiscHelpers : MonoBehaviour
{
    public static MiscHelpers mh;

    void Start()
    {
        mh = this;
    }

    /* PUBLIC API */

    public void runAsync(Action funcToCall, float secondsDelay)
    /* Delay the execution of some code by some amount of time.
    
    :param float time: Number of seconds to wait.
    :param func func: Function to call (no arguments, no return value) after `time` seconds.
    */
    {
        StartCoroutine(asyncHelper(funcToCall, secondsDelay));
    }

    /* HELPERS */

    private static IEnumerator asyncHelper(Action funcToCall, float secondsDelay)
    /* Helper for runAsync so it can call StartCoroutine on the IEnumerator this returns.
    
    :param float time: Number of seconds to wait.
    :param func func: Function to call (no arguments, no return value) after `time` seconds.

    :returns IEnumerator:
    */
    {
        yield return new WaitForSeconds(secondsDelay);
 
        funcToCall();
    }
}
