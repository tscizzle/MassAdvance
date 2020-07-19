using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiscHelpers : MonoBehaviour
{
    private static System.Random rng = new System.Random();

    /* PUBLIC API */

    public static string getRandomId()
    /* Get a random string of numbers, 8 characters long. */
    {
        string randomId = rng.Next(10000000, 99999999).ToString();
        return randomId;
    }

    public static Vector2[] getNeighbors(Vector2 gridIndices)
    /* Get neighboring squares (no diagonals).
    
    :param Vector2 gridIndices: Position of square whose neighbors we are finding.

    :returns Vector2[] neighbors: Array of positions within 1 square of here.
    */
    {
        float xIdx = gridIndices.x;
        float yIdx = gridIndices.y;

        Vector2[] neighbors =
        {
            new Vector2(xIdx, yIdx - 1),
            new Vector2(xIdx - 1, yIdx),
            new Vector2(xIdx + 1, yIdx),
            new Vector2(xIdx, yIdx + 1),
        };
        
        return neighbors;
    }
}
