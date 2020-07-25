using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class FloorSquare : MonoBehaviour
{
    private static float gridSquareSize = 1; // in Unity units
    private static float gridSquareMargin = 0.01f;
    private static Color floorColor = new Color(200/255f, 200/255f, 200/255f);
    private static Color stainedFloorColor = new Color(100/255f, 100/255f, 100/255f);

    // Parameters.
    public Vector2 gridIndices;
    public bool isSacred;
    // State.
    public int numTurnsStained;

    void Start()
    {
        // Size.
        float defaultPlaneSize = 10; // Plane defaults to 10 units across.
        float gridSquareScale = (transform.localScale.x / defaultPlaneSize) * gridSquareSize;
        gridSquareScale -= gridSquareMargin / 2;
        transform.localScale = new Vector3(gridSquareScale, 1, gridSquareScale);

        // Color.
        setColor();

        // Stain text.
        setStainText();

        // Red line for sacred squares.
        setSacredLine();
    }

    void OnMouseDown()
    {
        // Store which square was clicked down on.
        TrialLogic.mouseDownGridIndices = gridIndices;
    }

    void OnMouseUp()
    {
        // Store which square was clicked up on.
        TrialLogic.mouseUpGridIndices = null;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            FloorSquare floorSquare = hit.transform.gameObject.GetComponent<FloorSquare>();
            Block block = hit.transform.gameObject.GetComponent<Block>();
            if (floorSquare != null)
            {
                TrialLogic.mouseUpGridIndices = floorSquare.gridIndices;
            } else if (block != null)
            {
                TrialLogic.mouseUpGridIndices = block.gridIndices;
            }
        }

        if (String.IsNullOrEmpty(TrialLogic.selectedCardId))
        {
            return;
        }
        Card selectedCard = TrialLogic.trialDeck[TrialLogic.selectedCardId].card;
        selectedCard.playCard();

        TrialLogic.mouseDownGridIndices = null;
        TrialLogic.mouseUpGridIndices = null;
    }

    /* PUBLIC API */

    public void addStainTurns(int numTurns)
    /* Add more turns of stain to this FloorSquare.

    Note that numTurns can be negative, to reduce the number of turns of stain remaining.
    
    :param bool newIsStained: Whether setting to stained or unstained.
    */
    {
        int oldVal = numTurnsStained;
        int newVal = Mathf.Max(numTurnsStained + numTurns, 0);

        if (newVal != oldVal)
        {
            numTurnsStained = Mathf.Max(numTurnsStained + numTurns, 0);
            setStainText();
            setColor();

            EventLog.LogEvent($"Stain at {gridIndices} changed from {oldVal} to {newVal}.");
        }
    }

    public bool isStained()
    /* Return whether or not this FloorSquare is currently stained.
    
    :returns bool:
    */
    {
        return numTurnsStained > 0;
    }

    public static Vector3 getGridSquareCenter(Vector2 gridIndices)
    /* Take grid indices and return the center of the corresponding grid square in Unity
        coordinates.
    
    :param Vector2 gridIndices: The square in the grid we want the center of, where (0, 0) is the
        bottom-left square.

    :returns Vector3 squareCenter: World coordinates of the point in the middle of the desired grid
        square. y doesn't matter, since z is the depth axis in world coordinates.
    */
    {
        float shiftedXIdx = gridIndices.x - (TrialLogic.numGridSquaresWide / 2);
        float shiftedZIdx = gridIndices.y - (TrialLogic.numGridSquaresDeep / 2);

        float scaledX = shiftedXIdx * gridSquareSize;
        float scaledZ = shiftedZIdx * gridSquareSize;

        float centerX = scaledX + (gridSquareSize / 2);
        float centerZ = scaledZ + (gridSquareSize / 2);

        Vector3 squareCenter = new Vector3(centerX, 0, centerZ);

        return squareCenter;
    }

    /* HELPERS */

    private void setColor()
    /* Color this FloorSquare, based on if it is stained or not. */
    {
        Color newColor = isStained() ? stainedFloorColor : floorColor;
        GetComponent<MeshRenderer>().material.SetColor("_Color", newColor);
    }

    private void setStainText()
    /* Set the number that displays on this FloorSquare, for how many turns of stain is has left. */
    {
        GetComponentInChildren<TextMeshPro>().text = numTurnsStained > 0
            ? numTurnsStained.ToString()
            : "";
    }

    private void setSacredLine()
    /* For sacred squares (the player needs to stop the mass from reaching them), draw a red line. */
    {
        float width = isSacred ? 0.1f : 0;
        GetComponent<LineRenderer>().startWidth = width;
        GetComponent<LineRenderer>().endWidth = width;
    }
}
