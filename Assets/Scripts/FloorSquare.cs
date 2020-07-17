using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class FloorSquare : MonoBehaviour, IPointerClickHandler
{
    private static float gridSquareSize = 1; // in Unity units
    private static float gridSquareMargin = 0.01f;
    private static Color floorColor = new Color(200/255f, 200/255f, 200/255f);
    private static Color stainedFloorColor = new Color(100/255f, 100/255f, 100/255f);
    // So we can find FloorSquare objects from their grid indices.
    public static Dictionary<Vector2, FloorSquare> floorSquaresMap = new Dictionary<Vector2, FloorSquare>();

    // Parameters.
    public Vector2 gridIndices;
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
    }

    void Update()
    {
        
    }

    /* PUBLIC API */

    public void OnPointerClick(PointerEventData eventData)
    /* Override this function of IPointerClickHandler. Triggers when this FloorSquare is clicked.
    
    :param PointerEventData eventData: This interface is defined by Unity.
    */
    {
        TrialLogic.T.playSelectedCardOnFloorSquare(gridIndices);
    }

    public void addStainTurns(int numTurns)
    /* Add more turns of stain to this FloorSquare.

    Note that numTurns can be negative, to reduce the number of turns of stain remaining.
    
    :param bool newIsStained: Whether setting to stained or unstained.
    */
    {
        numTurnsStained = Mathf.Max(numTurnsStained + numTurns, 0);
        setStainText();
        setColor();
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

    private void setStainText()
    /* Set the number that displays on this FloorSquare, for how many turns of stain is has left. */
    {
        GetComponentInChildren<TextMeshPro>().text = numTurnsStained > 0
            ? numTurnsStained.ToString()
            : "";
    }

    private void setColor()
    /* Color this FloorSquare, based on if it is stained or not. */
    {
        Color newColor = isStained() ? stainedFloorColor : floorColor;
        GetComponent<MeshRenderer>().material.SetColor("_Color", newColor);
    }
}
