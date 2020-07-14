using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    private float mouseScrollGain = 30f;
    private float scrollZUpperLimit = -3;
    private float scrollZLowerLimit = -6;

    void Start()
    {

    }

    void Update()
    {
        mouseScroll();

        mouseClick();
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
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GameObject clickedObj = hit.transform.gameObject;
                FloorSquare floorSquare = clickedObj.GetComponent<FloorSquare>();

                if (floorSquare != null)
                {
                    handleClickFloor(floorSquare);
                }
            }
        } else if (Input.GetMouseButtonDown(1))
        {
            handleRightClick();
        } else if (Input.GetMouseButtonUp(1))
        {
            handleRightUnclick();
        }
    }

    private void handleClickFloor(FloorSquare floorSquare)
    {
        Vector2 gridIndices = floorSquare.gridIndices;
        GameLogic.G.attemptToPlaceBlock(gridIndices);
    }

    private void handleRightClick()
    {
        GameLogic.G.speedUpGame();
    }

    private void handleRightUnclick()
    {
        GameLogic.G.slowDownGame();
    }
}
