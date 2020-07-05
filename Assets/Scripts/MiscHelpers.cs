using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiscHelpers : MonoBehaviour
{
    /* PUBLIC API */

    public static IEnumerator getScheduledFunc(float secondsDelay, Action funcToCall)
    /* Delay the execution of some code by some amount of time.
    
    :param float time: Number of seconds to wait.
    :param func func: Function to call (no arguments, no return value) after `time` seconds.
    */
    {
        yield return new WaitForSeconds(secondsDelay);
 
        funcToCall();
    }
}
