using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FloorSquare : MonoBehaviour
{
    private static float gridSquareSize = 1; // in Unity units
    
    // Parameters.
    public Vector2 gridIndices;

    void Start()
    {
        // Size.
        // Plane defaults to 10 units across.
        float gridSquareScale = (transform.localScale.x / 10) * gridSquareSize;
        gridSquareScale -= 0.005f; // Slight margin between squares.
        transform.localScale = new Vector3(gridSquareScale, 1, gridSquareScale);

        // Color.
        Color floorColor;
        ColorUtility.TryParseHtmlString("#aaaaaa", out floorColor);
        GetComponent<MeshRenderer>().material.SetColor("_Color", floorColor);
    }

    void Update()
    {
        
    }

    /* PUBLIC API */

    public Vector2 getGridIndices(Vector3 worldPoint)
    /* Take a clicked point on the Floor in Unity coordinates and return the clicked square's
        position in the grid, where (0, 0) is the bottom-left square.
    
    :param Vector3 worldPoint: The clicked point in Unity coordinates. x is the width axis, z is the
        depth axis, y doesn't matter.

    :returns Vector2 gridIndices:
    */
    {
        float floorWidth = GameLogic.numGridSquaresWide * gridSquareSize;
        float floorDepth = GameLogic.numGridSquaresDeep * gridSquareSize;

        float shiftedX = worldPoint.x + (floorWidth / 2);
        float shiftedY = worldPoint.z + (floorDepth / 2);

        int squareX = (int)Mathf.Floor(shiftedX / gridSquareSize);
        int squareY = (int)Mathf.Floor(shiftedY / gridSquareSize);

        Vector2 gridIndices = new Vector2(squareX, squareY);

        return gridIndices;
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

    /* HELPERS */

    void drawGridLine(Vector3[] points)
    {
        Color lineColor;
        ColorUtility.TryParseHtmlString("#888888", out lineColor);
        Material mat = new Material(Shader.Find("Sprites/Default"));
        float lineWidth = 0.01f;
        
        GameObject lineObj = new GameObject();
        LineRenderer line = lineObj.AddComponent<LineRenderer>();
        line.material = mat;
        line.startColor = lineColor;
        line.endColor = lineColor;
        line.widthMultiplier = lineWidth;
        line.SetPositions(points);
    }

    void drawGridLines()
    {
        // Horizontal lines
        foreach (int idx in Enumerable.Range(1, GameLogic.numGridSquaresWide - 1))
        {
            float x = -(GameLogic.numGridSquaresWide / 2) + idx;
            float y = 0.01f;
            float z_0 = -GameLogic.numGridSquaresDeep / 2;
            float z_1 = GameLogic.numGridSquaresDeep / 2;
            Vector3[] points =
            {
                new Vector3(x, y, z_0),
                new Vector3(x, y, z_1),
            };
            drawGridLine(points);
        }
        // Vertical lines
        foreach (int idx in Enumerable.Range(1, GameLogic.numGridSquaresDeep - 1))
        {
            float x_0 = -GameLogic.numGridSquaresWide / 2;
            float x_1 = GameLogic.numGridSquaresWide / 2;
            float y = 0.01f;
            float z = -(GameLogic.numGridSquaresDeep / 2) + idx;
            Vector3[] points =
            {
                new Vector3(x_0, y, z),
                new Vector3(x_1, y, z),
            };
            drawGridLine(points);
        }
    }
}
