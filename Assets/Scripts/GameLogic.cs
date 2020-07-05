﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameLogic : MonoBehaviour
{
    public GameObject prefabInstantiatorObj;

    private PrefabInstantiator prefabInstantiator;
    
    float secondsBetweenActions = 0.1f;
    public int numGridSquaresWide = 6;
    public int numGridSquaresDeep = 10;
    public Dictionary<Vector2, Block> placedBlocks = new Dictionary<Vector2, Block>();

    void Awake()
    {
        prefabInstantiator = prefabInstantiatorObj.GetComponent<PrefabInstantiator>();
    }

    void Start()
    {
        placeStartingBlocks();
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

    public void attackBlock(Vector2 gridIndices)
    /* Apply the mass's current attack to a player's block.

    :param Vector2 gridIndices: The square with the block being attacked.
    */
    {
        Block block = placedBlocks[gridIndices];
        if (block.isDamaged)
        {
            destroyBlock(gridIndices);
        } else
        {
            block.damageBlock();
        }
    }

    public void destroyBlock(Vector2 gridIndices)
    /* Remove a Block from the grid, applying any onDestroy effects.
    
    :param Vector2 gridIndices: The square with the block being destroyed.
    */
    {
        Block block = placedBlocks[gridIndices];
        // TODO: collect any onDestroy effects and run them.
        
        GameObject blockObj = block.gameObject;
        Destroy(blockObj);
        
        placedBlocks.Remove(gridIndices);
    }

    public void endTurn()
    /* Do the steps that should occur when player's turn ends, like evaluate combos and produce,
        trigger the enemy's turn, etc.
    */
    {
        // TODO: From top left, as if reading, take a square at a time. Detect what combos it's part
        //      of and trigger their "production".
        
        spreadMass();
    }

    /* HELPERS */

    private void placeStartingBlocks()
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
        }

        Vector2 startingBlueSquare = new Vector2(0, numGridSquaresDeep - 2);
        placeBlock("blue", startingBlueSquare);
        Vector2 startingYellowSquare = new Vector2(numGridSquaresWide - 1, numGridSquaresDeep - 2);
        placeBlock("yellow", startingYellowSquare);
    }

    private List<Vector2> getNextMassTargets()
    /* Get a list of all squares that current mass blocks borders, which are the squares it expands
        to or attacks if there are player blocks there.
    
    The result is ordered from top-left and going across, as if reading.
    
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

    private void spreadMass()
    /* Play the enemy's turn, where the mass spreads to empty squares and attacks the player's
        blocks
    */
    {
        List<Vector2> nextTargets = getNextMassTargets();

        int idx = 0;
        
        // TODO: freeze user input
        
        foreach (Vector2 gridIndices in nextTargets)
        {
            string blockType = getBlockTypeOfSquare(gridIndices);
            float delay = idx * secondsBetweenActions;
            bool isLastAction = idx == nextTargets.Count - 1;
            
            // If nothing is there, expand the mass into it.
            // Otherwise, attack the player block that's there.
            if (blockType == null)
            {
                Action placeBlockFunc = () =>
                {
                    placeBlock("mass", gridIndices);
                    if (isLastAction)
                    {
                        // TODO: unfreeze user input
                    }
                };
                StartCoroutine(MiscHelpers.getScheduledFunc(delay, placeBlockFunc));
            } else
            {
                Action attackBlockFunc = () =>
                {
                    attackBlock(gridIndices);
                    if (isLastAction)
                    {
                        // TODO: unfreeze user input
                    }
                };
                StartCoroutine(MiscHelpers.getScheduledFunc(delay, attackBlockFunc));
            }

            idx += 1;
        }
    }
}
