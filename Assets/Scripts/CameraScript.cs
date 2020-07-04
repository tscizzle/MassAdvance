﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public GameObject gameLogicObj;
    public GameObject floorObj;

    private GameLogic gameLogic;
    private Floor floor;

    private float mouseScrollGain = 30f;
    private float scrollZUpperLimit = -3;
    private float scrollZLowerLimit = -6;

    // Temporary while there's not a real way to place blocks
    public string selectedBlockType = "blue";

    void Awake()
    {
        gameLogic = gameLogicObj.GetComponent<GameLogic>();
        floor = floorObj.GetComponent<Floor>();
    }

    void Start()
    {

    }

    void Update()
    {
        // Temporary while there's not a real way to place blocks
        setSelectedBlockType();

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

                if (clickedObj == floorObj)
                {
                    handleClickFloor(hit.point);
                }
            }
        }
    }

    private void handleClickFloor(Vector3 clickPoint)
    {
        Vector2 gridIndices = floor.getGridIndices(clickPoint);
        gameLogic.placeBlock(selectedBlockType, gridIndices);
    }

    private void setSelectedBlockType()
    {
        if (Input.GetKey(KeyCode.A))
        {
            selectedBlockType = "blue";
        } else if (Input.GetKey(KeyCode.S))
        {
            selectedBlockType = "yellow";
        } else if (Input.GetKey(KeyCode.D))
        {
            selectedBlockType = "red";
        }
    }
}
