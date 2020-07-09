using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameLogic : MonoBehaviour
{
    // Global var that even a prefab can reference. Will be assigned our 1 instance of GameLogic.
    public static GameLogic G;
    
    // User interaction parameters.
    private float secondsBetweenActions_fast;
    private float secondsBetweenActions_slow;
    public float secondsBetweenActions;
    // Gameplay parameters.
    public static int numGridSquaresWide;
    public static int numGridSquaresDeep;
    private static int blockIumCost;
    // Game state.
    public Dictionary<Vector2, Block> placedBlocks = new Dictionary<Vector2, Block>();
    public int currentIum;
    public int turnsToSurvive;
    public int turnsTaken;
    public Dictionary<string, Card> cardsById = new Dictionary<string, Card>();
    public List<string> wholeDeck = new List<string>();
    public List<string> currentHand = new List<string>();
    public List<string> discardPile = new List<string>();

    void Awake()
    {
        // Since there should only be 1 GameLogic instance, assign this instance to a global var.
        G = this;

        // Initialize variables.
        secondsBetweenActions_fast = 0.1f;
        secondsBetweenActions_slow = 0.6f;
        secondsBetweenActions = secondsBetweenActions_slow;
        numGridSquaresWide = 6;
        numGridSquaresDeep = 10;
        currentIum = 5;
        blockIumCost = 2;
        turnsToSurvive = 20;
        turnsTaken = 0;
    }

    IEnumerator Start()
    {
        // TODO: freeze user input

        yield return StartCoroutine(placeStartingBlocks());
    }

    void Update()
    {

    }

    /* PUBLIC API */

    public void attemptToPlaceBlock(string blockType, Vector2 gridIndices)
    /* Attempty to put a block into play in the grid, but may not succeed due to constraints like
        ium and placement restrictions.
    
    :param string blockType: One of [ "mass", "blue", "yellow", "red" ]
    :param Vector2 gridIndices: The square in which to put the block ((0, 0) is the bottom-left).
    */
    {
        if (blockIumCost <= currentIum)
        {
            currentIum -= blockIumCost;
            placeBlock(blockType, gridIndices);
        }
    }

    public void placeBlock(string blockType, Vector2 gridIndices)
    /* Put a block into play in the grid.
    
    :param string blockType: One of [ "mass", "blue", "yellow", "red" ]
    :param Vector2 gridIndices: The square in which to put the block ((0, 0) is the bottom-left).
    */
    {
        GameObject blockObj = PrefabInstantiator.P.CreateBlock(blockType, gridIndices);
        
        Block block = blockObj.GetComponent<Block>();
        
        placedBlocks[gridIndices] = block;

        StartCoroutine(Pointer.displayPointer(gridIndices));
    }

    public void attackBlock(Vector2 gridIndices)
    /* Apply the mass's current attack to a player's block.

    :param Vector2 gridIndices: The square with the block being attacked.
    */
    {
        Block block = placedBlocks[gridIndices];
        if (block.isDamaged)
        {
            destroyBlock(gridIndices);
            StartCoroutine(Pointer.displayPointer(gridIndices));
        } else
        {
            block.damageBlock();
            StartCoroutine(Pointer.displayPointer(gridIndices));
        }
    }

    public void destroyBlock(Vector2 gridIndices)
    /* Remove a Block from the grid, applying any onDestroy effects.
    
    :param Vector2 gridIndices: The square with the block being destroyed.
    */
    {
        Block block = placedBlocks[gridIndices];

        // TODO: run any onDestroy effects of this Block.
        
        GameObject blockObj = block.gameObject;
        Destroy(blockObj);
        
        placedBlocks.Remove(gridIndices);
    }

    public IEnumerator endTurn()
    /* Do the steps that should occur when player's turn ends, like evaluate combos and produce,
        trigger the enemy's turn, etc.
    */
    {
        // TODO: freeze user input

        yield return executeProducePhase();
        
        yield return executeMassSpreadingPhase();
        
        // TODO: unfreeze user input after above phases are finished (careful, they're async)
        
        turnsTaken += 1;
    }

    public void speedUpGame()
    /* Speeds up the game by lowering secondsBetweenActions */
    {
        secondsBetweenActions = secondsBetweenActions_fast;
    }

    public void slowDownGame()
    /* Slows down the game by raising secondsBetweenActions */
    {
        secondsBetweenActions = secondsBetweenActions_slow;
    }

    /* HELPERS */

    private IEnumerator placeStartingBlocks()
    /* Place the Blocks that start out on the grid at the beginning of a round. */
    {
        Vector2[] startingMassSquares =
        {
            new Vector2((numGridSquaresWide / 2) - 1, numGridSquaresDeep - 1),
            new Vector2(numGridSquaresWide / 2, numGridSquaresDeep - 1),
        };
        foreach (Vector2 gridIndices in startingMassSquares)
        {
            placeBlock("mass", gridIndices);
            yield return new WaitForSeconds(secondsBetweenActions);
        }

        Vector2 startingBlueSquare = new Vector2(0, numGridSquaresDeep - 2);
        placeBlock("blue", startingBlueSquare);
        yield return new WaitForSeconds(secondsBetweenActions);
        Vector2 startingYellowSquare = new Vector2(numGridSquaresWide - 1, numGridSquaresDeep - 2);
        placeBlock("yellow", startingYellowSquare);
        yield return new WaitForSeconds(secondsBetweenActions);
    }

    private List<Vector2> getProductiveBlocks()
    /* Get a list of all squares that have player Blocks with `produce` effects.
    
    The result is ordered from top-left and going down, so column by column from the left.
    
    :returns List<Vector2> productiveBlocks:
    */
    {
        List<Vector2> unorderedProductiveBlocks = new List<Vector2>();

        foreach (int xIdx in Enumerable.Range(0, numGridSquaresWide))
        {
            foreach (int yIdx in Enumerable.Range(0, numGridSquaresDeep))
            {
                Vector2 gridIndices = new Vector2(xIdx, yIdx);
                string blockType = getBlockTypeOfSquare(gridIndices);

                if (blockType == "blue")
                {
                    unorderedProductiveBlocks.Add(gridIndices);
                }
            }
        }

        List<Vector2> productiveBlocks = unorderedProductiveBlocks
            .OrderBy(el => el.x)
            .ThenByDescending(el => el.y)
            .ToList();

        return productiveBlocks;
    }

    private IEnumerator executeProducePhase()
    /* For all a player's Blocks on the grid, trigger their produce ability. */
    {
        List<Vector2> productiveBlocks = getProductiveBlocks();

        foreach (Vector2 gridIndices in productiveBlocks)
        {
            Block block = placedBlocks[gridIndices];
            block.produce();
            yield return new WaitForSeconds(secondsBetweenActions);
        }
    }

    private List<Vector2> getNextMassTargets()
    /* Get a list of all squares that current mass blocks borders, which are the squares it expands
        to or attacks if there are player blocks there.
    
    The result is ordered from top-left and going down, so column by column from the left.
    
    :returns List<Vector2> nextTargets:
    */
    {
        HashSet<Vector2> uniqueNextTargets = new HashSet<Vector2>();

        foreach (int xIdx in Enumerable.Range(0, numGridSquaresWide))
        {
            foreach (int yIdx in Enumerable.Range(0, numGridSquaresDeep))
            {
                Vector2 gridIndices = new Vector2(xIdx, yIdx);
                string blockType = getBlockTypeOfSquare(gridIndices);

                bool isSquareMass = blockType == "mass";
                bool isSquareNeighboredByMass = isNeighboredByMass(gridIndices);

                if (!isSquareMass && isSquareNeighboredByMass)
                {
                    uniqueNextTargets.Add(gridIndices);
                }
            }
        }

        List<Vector2> nextTargets = uniqueNextTargets
            .OrderBy(el => el.x)
            .ThenByDescending(el => el.y)
            .ToList();

        return nextTargets;
    }

    private string getBlockTypeOfSquare(Vector2 gridIndices)
    /* Get the blockType of a place in the grid.
    
    :param Vector2 gridIndices: Position of square we want the block type of.

    :returns string blockType: One of ["mass", "blue", "yellow", "red"]
    */
    {
        bool isSquareOccupied = placedBlocks.ContainsKey(gridIndices);
        string blockType = isSquareOccupied ? placedBlocks[gridIndices].blockType : null;
        return blockType;
    }

    private bool isNeighboredByMass(Vector2 gridIndices)
    /* Return whether or not any neighbors (no diagonals, no self) are mass.
    
    :param Vector2 gridIndices: Position of square whose neighbors we are checking.

    :returns bool:
    */
    {
        float xIdx = gridIndices.x;
        float yIdx = gridIndices.y;

        bool didFindMass = false;
        Vector2[] neighbors =
        {
            new Vector2(xIdx, yIdx - 1),
            new Vector2(xIdx - 1, yIdx),
            new Vector2(xIdx + 1, yIdx),
            new Vector2(xIdx, yIdx + 1),
        };
        foreach (Vector2 neighbor in neighbors)
        {
            if (getBlockTypeOfSquare(neighbor) == "mass")
            {
                didFindMass = true;
            }
        }

        return didFindMass;
    }

    private IEnumerator executeMassSpreadingPhase()
    /* Play the enemy's turn, where the mass spreads to empty squares and attacks the player's
        blocks
    */
    {
        List<Vector2> nextTargets = getNextMassTargets();
        
        foreach (Vector2 gridIndices in nextTargets)
        {
            string blockType = getBlockTypeOfSquare(gridIndices);
            
            // If nothing is there, expand the mass into it.
            // Otherwise, attack the player block that's there.
            if (blockType == null)
            {
                placeBlock("mass", gridIndices);
            } else
            {
                attackBlock(gridIndices);
            }
            yield return new WaitForSeconds(secondsBetweenActions);
        }
    }
}
