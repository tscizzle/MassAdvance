using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    private float mouseScrollGain = 100f; // increases overall scroll speed
    private float mouseScrollDampening = 10f; // lessens the increase in scroll speed as the mouse approaches the edge
    private float mouseScrollEdgeSize = 0.1f; // portion of screen height
    private float scrollZUpperLimit = -3;
    private float scrollZLowerLimit = -6;

    void Start()
    {
        
    }

    void Update()
    {
        mouseScroll();
    }

    /* HELPERS */

    private void mouseScroll()
    /* Detect that the mouse is near an edge and scroll the camera that direction. */
    {
        float upperEdgeBoundary = Screen.height * (1 - mouseScrollEdgeSize);
        float lowerEdgeBoundary = Screen.height * mouseScrollEdgeSize;

        float moveAmount = 0;
        if (Input.mousePosition.y > upperEdgeBoundary)
        {
            float distFromEdge = Mathf.Abs(Screen.height - Input.mousePosition.y);
            moveAmount = 1 / (distFromEdge + mouseScrollDampening);
        } else if (Input.mousePosition.y < lowerEdgeBoundary)
        {
            float distFromEdge = Mathf.Abs(Input.mousePosition.y);
            moveAmount = -1 / (distFromEdge + mouseScrollDampening);
        }

        if (moveAmount != 0)
        {
            Vector3 currentPos = transform.position;
            float moveDist = moveAmount * mouseScrollGain * Time.deltaTime;
            float newZ = currentPos.z + moveDist;
            newZ = Mathf.Min(newZ, scrollZUpperLimit);
            newZ = Mathf.Max(newZ, scrollZLowerLimit);
            transform.position = new Vector3(currentPos.x, currentPos.y, newZ);
        }
    }
}
