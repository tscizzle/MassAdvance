using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickHandlers : MonoBehaviour
{
    public GameObject gameLogicObj;
    public GameObject floorObj;

    private GameLogic gameLogic;
    private Floor floor;

    void Start()
    {
        gameLogic = gameLogicObj.GetComponent<GameLogic>();
        floor = floorObj.GetComponent<Floor>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GameObject clickedObj = hit.transform.gameObject;

                if (clickedObj == floorObj)
                {
                    handleClickFloor(hit.point);
                }
            }
        }
    }

    /* CLICK HANDLERS */

    private void handleClickFloor(Vector3 clickPoint)
    {
        Vector2 gridIndices = floor.getGridIndices(clickPoint);
        string blockType = "blue";
        gameLogic.placeBlock(blockType, gridIndices);
    }
}
