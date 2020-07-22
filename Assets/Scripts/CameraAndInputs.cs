﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAndInputs : MonoBehaviour
{
    private float mouseScrollGain = 30f;
    private float scrollZUpperLimit = -3;
    private float scrollZLowerLimit = -6;

    void Update()
    {
        mouseScroll();

        mouseClick();

        spacebarPress();
    }

    /* HELPERS */

    private void mouseScroll()
    {
        float scrollInput = Input.mouseScrollDelta.y;

        if (scrollInput != 0)
        {
            Vector3 currentPos = transform.position;

            float newZ = currentPos.z + (scrollInput * mouseScrollGain * Time.deltaTime);
            newZ = Mathf.Min(newZ, scrollZUpperLimit);
            newZ = Mathf.Max(newZ, scrollZLowerLimit);
            
            transform.position = new Vector3(currentPos.x, currentPos.y, newZ);
        }
    }

    private void mouseClick()
    {
        if (Input.GetMouseButtonDown(1))
        {
            TrialLogic.setRapidMode(true);
        } else if (Input.GetMouseButtonUp(1))
        {
            TrialLogic.setRapidMode(false);
        }
    }

    private void spacebarPress()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TrialLogic.setPauseMode(true);
        } else if (Input.GetKeyUp(KeyCode.Space))
        {
            TrialLogic.setPauseMode(false);
        }
    }
}
