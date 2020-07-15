using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

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
    public bool isStained;

    void Start()
    {
        // Size.
        float defaultPlaneSize = 10; // Plane defaults to 10 units across.
        float gridSquareScale = (transform.localScale.x / defaultPlaneSize) * gridSquareSize;
        gridSquareScale -= gridSquareMargin / 2;
        transform.localScale = new Vector3(gridSquareScale, 1, gridSquareScale);

        // Color.
        setFloorSquareStain(isStained);
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
        GameLogic.G.playSelectedCardOnFloorSquare(gridIndices);
    }

    public void setFloorSquareStain(bool newIsStained)
    /* Set this FloorSquare to stained or not.
    
    :param bool newIsStained: Whether setting to stained or unstained.
    */
    {
        isStained = newIsStained;
        Color newColor = isStained ? stainedFloorColor : floorColor;
        GetComponent<MeshRenderer>().material.SetColor("_Color", newColor);
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
        float shiftedXIdx = gridIndices.x - (GameLogic.numGridSquaresWide / 2);
        float shiftedZIdx = gridIndices.y - (GameLogic.numGridSquaresDeep / 2);

        float scaledX = shiftedXIdx * gridSquareSize;
        float scaledZ = shiftedZIdx * gridSquareSize;

        float centerX = scaledX + (gridSquareSize / 2);
        float centerZ = scaledZ + (gridSquareSize / 2);

        Vector3 squareCenter = new Vector3(centerX, 0, centerZ);

        return squareCenter;
    }
}
