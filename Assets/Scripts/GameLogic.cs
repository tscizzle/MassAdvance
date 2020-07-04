using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLogic : MonoBehaviour
{
    public GameObject prefabInstantiatorObj;

    private PrefabInstantiator prefabInstantiator;

    public int numGridSquaresWide = 6;
    public int numGridSquaresDeep = 10;
    public Dictionary<Vector2, Block> placedBlocks = new Dictionary<Vector2, Block>();

    void Awake()
    {
        prefabInstantiator = prefabInstantiatorObj.GetComponent<PrefabInstantiator>();
    }

    void Start()
    {
        Vector2[] startingMassSquares =
        {
            new Vector2((numGridSquaresWide / 2) - 1, numGridSquaresDeep - 1),
            new Vector2(numGridSquaresWide / 2, numGridSquaresDeep - 1),
        };
        foreach (Vector2 gridIndices in startingMassSquares)
        {
            placeBlock("mass", gridIndices);
        }

        Vector2 startingBlueSquare = new Vector2(0, numGridSquaresDeep - 2);
        placeBlock("blue", startingBlueSquare);
        Vector2 startingYellowSquare = new Vector2(numGridSquaresWide - 1, numGridSquaresDeep - 2);
        placeBlock("yellow", startingYellowSquare);
    }

    void Update()
    {

    }

    /* PUBLIC API */

    public void placeBlock(string blockType, Vector2 gridIndices)
    /* Put a block into play in the grid.
    
    :param string blockType: One of [ "mass", "blue", "yellow", "red" ]
    :param Vector2 gridIndices: The square in which to put the block ((0, 0) is the bottom-left).
    */
    {
        GameObject blockObj = prefabInstantiator.CreateBlock(blockType, gridIndices);
        
        Block block = blockObj.GetComponent<Block>();
        
        placedBlocks[gridIndices] = block;
    }

    public void endTurn()
    /* Do the steps that should occur when player's turn ends, like evaluate combos and produce,
        trigger the enemy's turn, etc.
    */
    {
        // TODO: From top left, as if reading, take a square at a time. Detect what combos it's part
        //      of and trigger their "production".
        // TODO: Give control to the mass. Detect all the spaces it is next to, and from top left,
        //      apply either expansion or attack.
    }

    /* HELPERS */
}
