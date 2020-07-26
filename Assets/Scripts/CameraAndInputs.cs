using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAndInputs : MonoBehaviour
{
    // Parameters.
    private float mouseScrollGain = 30f;
    private float scrollZUpperLimit = -3;
    private float scrollZLowerLimit = -5.5f;
    private float initialScrollInput = 0.4f;

    IEnumerator Start()
    {
        // Start the camera scrolled down, viewing the whole grid.
        Vector3 currentPos = transform.position;        
        transform.position = new Vector3(currentPos.x, currentPos.y, scrollZLowerLimit);

        // Wait until the trial's starting setup is done.
        while (!TrialLogic.isSetupFinished)
        {
            yield return new WaitForSeconds(0);
        }

        // Smoothly scroll the camera up to the top position.
        while (transform.position.z < scrollZUpperLimit)
        {
            scrollGridView(initialScrollInput);
            yield return new WaitForSeconds(0);
        }
    }

    void Update()
    {
        float scrollInput = Input.mouseScrollDelta.y;
        scrollGridView(scrollInput);

        mouseClick();

        spacebarPress();
    }

    /* HELPERS */

    private void scrollGridView(float scrollInput)
    /* Scroll the camera up or down.
    
    :param float scrollInput: Typically the output of Input.mouseScrollDelta. Roughly -1 to 1,
        negative for down, positive for up.
    */
    {
        if (!TrialLogic.isSetupFinished)
        {
            return;
        }

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
