using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public static T getRandomChoice<T>(IList<T> collection, System.Random rand)
    /* Get a random element from a collection.
    
    :param IList<T> collection: An object that supports []-indexing and has a Count field.
    :param System.Random rand: So if we call this many times, we don't keep making a new Random().

    :returns T randElement:
    */
    {
        T randElement = collection[rand.Next(collection.Count)];
        return randElement;
    }

    public static T getWeightedChoice<T>(Dictionary<T, float> frequencies, System.Random rand)
    /* Get a random element from a collection.
    
    :param Dictionary<T, float> frequencies: Each candidate choice is mapped to a relative weight.
        Proportionally larger weights grant proportionally higher probability of being chosen.
    :param System.Random rand: So if we call this many times, we don't keep making a new Random().

    :returns T randElement:
    */
    {
        float weightsTotal = frequencies.Values.Sum();
        float randTarget = (float)(rand.NextDouble() * weightsTotal);
        float weightsSoFar = 0;
        foreach (KeyValuePair<T, float> kvp in frequencies)
        {
            float candidateWeight = kvp.Value;
            weightsSoFar += candidateWeight;
            if (weightsSoFar > randTarget)
            {
                T candidate = kvp.Key;
                return candidate;
            }
        }
        return frequencies.First().Key;
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

    public static List<T> standardOrder<T>(IEnumerable<T> itemList, Func<T, Vector2> converter)
    /* Often our game actions occur to a list of blocks, and instead of doing it in a random order,
        we use a standardized ordering of starting at the top-left, going down the column, then
        repeating for columns left to right.
    
    :params List<Vector2> gridIndicesList: List of floor square positions in the grid.

    :returns List<Vector2> sortedGridIndicesList: Same floor square positions but sorted as
        described above.
    */
    {
        List<T> sortedGridIndicesList = itemList
            .OrderBy(el => converter(el).x)
            .ThenByDescending(el => converter(el).y)
            .ToList();
        
        return sortedGridIndicesList;
    }

    public static T identity<T>(T inp)
    /* Basic converter to be used in standardOrder. Returns the same thing it receives as input. */
    {
        return inp;
    }
}
