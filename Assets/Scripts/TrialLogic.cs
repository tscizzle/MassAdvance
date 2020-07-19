﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TrialLogic : MonoBehaviour
{
    private static PostTrialScreen postTrialScreen;
    
    // Parameters (user-interaction).
    private static float secondsBetweenActions_fast;
    private static float secondsBetweenActions_slow;
    public static float secondsBetweenActions;
    // Parameters (gameplay).
    public static int numGridSquaresWide;
    public static int numGridSquaresDeep;
    public static int baseIumCostForBlock;
    public static int turnsToSurvive;
    private static int baseIumPerTurn;
    private static int baseDrawPerTurn;
    private static int startingIum;
    private static int startingHandSize;
    private static int startingUnstainedRows;
    // State.
    public static int currentIum;
    public static int turnNumber;
    public static Dictionary<Vector2, Block> placedBlocks = new Dictionary<Vector2, Block>();
    public static Dictionary<string, CardInfo> trialDeck = new Dictionary<string, CardInfo>();
    private static List<string> drawPile = new List<string>(); // cardIds
    public static List<string> hand = new List<string>(); // cardIds
    private static List<string> discardPile = new List<string>(); // cardIds
    public static string selectedCardId = null;
    public static bool isTrialWin = false;
    public static bool isTrialLoss = false;
    private static bool isTrialOver = false;

    static TrialLogic()
    {
        // Initialize parameters.
        secondsBetweenActions_fast = 0.1f;
        secondsBetweenActions_slow = 0.6f;
        secondsBetweenActions = secondsBetweenActions_slow;
        numGridSquaresWide = 6;
        numGridSquaresDeep = 10;
        baseIumCostForBlock = 2;
        turnsToSurvive = 15;
        baseIumPerTurn = 1;
        baseDrawPerTurn = 1;
        startingIum = 4;
        startingHandSize = 4;
        startingUnstainedRows = 3;

        // Initialize state.
        currentIum = startingIum;
        turnNumber = 0;
    }

    void Awake()
    {
        GameObject postTrialScreenObj = GameObject.Find("PostTrialScreen");
        postTrialScreen = postTrialScreenObj.GetComponent<PostTrialScreen>();
    }

    IEnumerator Start()
    {
        // TODO: freeze user input

        initializeFloor();

        initializeCards();
        
        yield return StartCoroutine(placeStartingBlocks());

        foreach (int _ in Enumerable.Range(0, startingHandSize))
        {
            drawCard();
        }

        startTurn();

        // TODO: unfreeze user input
    }

    /* PUBLIC API */

    public static void placeBlock(BlockType blockType, Vector2 gridIndices)
    /* Put a block into play in the grid.
    
    :param BlockType blockType: enum defined in Block.cs
    :param Vector2 gridIndices: The square in which to put the block ((0, 0) is the bottom-left).
    */
    {
        GameObject blockObj = PrefabInstantiator.P.CreateBlock(blockType, gridIndices);
        
        Block block = blockObj.GetComponent<Block>();

        placedBlocks[gridIndices] = block;

        EventLog.LogEvent($"Placed block {blockType} at {gridIndices}");
    }

    public static void startTurn()
    /* Begin the player's turn, e.g. gain a base amount of ium and draw a base number of cards. */
    {
        turnNumber += 1;

        gainIum(baseIumPerTurn);
        
        foreach (int _ in Enumerable.Range(0, baseDrawPerTurn))
        {
            drawCard();
        }
    }

    public static IEnumerator endTurn()
    /* Do the steps that should occur when player's turn ends, like evaluate combos and produce,
        trigger the enemy's turn, etc.
    */
    {
        // TODO: freeze user input

        TrialLogic.selectedCardId = null;

        decrementStain();

        yield return productionPhase();
        
        yield return massSpreadingPhase();

        yield return destructionPhase();

        evaluateTrialEndConditions();

        if (isTrialOver)
        {
            yield return handleTrialEnd();
        } else
        {
            startTurn();
        }

        // TODO: unfreeze user input
    }

    public static void speedUpGame()
    /* Speeds up the game by lowering secondsBetweenActions */
    {
        secondsBetweenActions = secondsBetweenActions_fast;
    }

    public static void slowDownGame()
    /* Slows down the game by raising secondsBetweenActions */
    {
        secondsBetweenActions = secondsBetweenActions_slow;
    }

    public static void gainIum(int ium)
    /* Gain ium equal to the amount passed in.
    
    :param int ium: Amount of ium to gain.
    */
    {
        int oldVal = currentIum;
        int newVal = currentIum + ium;

        if (newVal != oldVal)
        {
            currentIum = newVal;

            EventLog.LogEvent($"Current ium changed from {oldVal} to {newVal}.");
        }
    }

    public static void drawCard()
    /* Pick up a Card from drawPile to currentHand. */
    {
        if (drawPile.Count == 0)
        {
            drawPile = discardPile.OrderBy(_ => UnityEngine.Random.value).ToList();
            discardPile.Clear();

            EventLog.LogEvent($"Replenished draw pile from discard pile.");
        }

        string cardId = drawPile[drawPile.Count - 1];
        drawPile.RemoveAt(drawPile.Count - 1);
        hand.Add(cardId);

        GameObject cardObj = PrefabInstantiator.P.CreateCard(cardId);
        
        Card card = cardObj.GetComponent<Card>();
        CardInfo cardInfo = trialDeck[cardId];

        EventLog.LogEvent($"Drew card {cardInfo.cardName} (id: {cardId}).");
    }

    public static void discardCard(string cardId)
    /* Put a Card from the hand to the discard pile
    
    :param string cardId: id of the Card in the hand to discard
    */
    {
        hand.Remove(cardId);

        CardInfo cardInfo = trialDeck[cardId];
        GameObject cardObj = cardInfo.card.gameObject;
        Destroy(cardObj);
        cardInfo.card = null;
        trialDeck[cardId] = cardInfo;

        discardPile.Add(cardId);

        EventLog.LogEvent($"Discarded card {cardInfo.cardName} (id: {cardId}).");
    }

    public static BlockType? getBlockTypeOfSquare(Vector2 gridIndices)
    /* Get the blockType of a place in the grid.
    
    :param Vector2 gridIndices: Position of square we want the block type of.

    :returns BlockType blockType: enum defined in Block.cs
    */
    {
        bool isSquareOccupied = placedBlocks.ContainsKey(gridIndices);
        BlockType? blockType = isSquareOccupied
            ? placedBlocks[gridIndices].blockType
            : (BlockType?)null;
        return blockType;
    }

    /* HELPERS */

    private static void initializeFloor()
    /* Create the grid of FloorSquares. */
    {
        // Create the grid.
        foreach (int xIdx in Enumerable.Range(0, numGridSquaresWide))
        {
            foreach (int yIdx in Enumerable.Range(0, numGridSquaresDeep))
            {
                Vector2 gridIndices = new Vector2(xIdx, yIdx);
                PrefabInstantiator.P.CreateFloorSquare(gridIndices);
            }
        }

        // Stain all but the starting unstained rows, 1 extra turn of stain per row you move back.
        foreach (int xIdx in Enumerable.Range(0, numGridSquaresWide))
        {
            foreach (int yIdx in Enumerable.Range(0, numGridSquaresDeep - startingUnstainedRows))
            {
                Vector2 gridIndices = new Vector2(xIdx, yIdx);
                FloorSquare floorSquare = FloorSquare.floorSquaresMap[gridIndices];
                int stainTurns = numGridSquaresDeep - startingUnstainedRows - yIdx;
                floorSquare.addStainTurns(stainTurns);
            }
        }
    }

    private static void initializeCards()
    /* Shuffle some set of cards I chose and put them in the draw pile. */
    {
        List<string> cardNames = new List<string>();

        // Put in some PlaceSingleBlockCards.
        BlockType[] playerBlockTypes = new[] { BlockType.BLUE, BlockType.YELLOW, BlockType.RED };
        foreach (BlockType blockType in playerBlockTypes)
        {
            foreach (int idx in Enumerable.Range(0, 20))
            {
                string cardName = PlaceSingleBlockCard.getSingleBlockCardName(blockType);
                cardNames.Add(cardName);
            }
        }
        // Put in some RepairBlockCards.
        foreach (int idx in Enumerable.Range(0, 20))
        {
            string cardName = RepairBlockCard.repairBlockCardName;
            cardNames.Add(cardName);
        }

        foreach (string cardName in cardNames)
        {
            string cardId = MiscHelpers.getRandomId();
            trialDeck[cardId] = new CardInfo(cardName, cardId);
        }
        
        List<CardInfo> shuffledDeck = trialDeck.Values.OrderBy(_ => UnityEngine.Random.value).ToList();

        foreach (CardInfo cardInfo in shuffledDeck)
        {
            drawPile.Add(cardInfo.cardId);
        }
    }

    private static IEnumerator placeStartingBlocks()
    /* Place the Blocks that start out on the grid at the beginning of a round. */
    {
        Vector2[] startingMassSquares =
        {
            new Vector2((numGridSquaresWide / 2) - 1, numGridSquaresDeep - 1),
            new Vector2(numGridSquaresWide / 2, numGridSquaresDeep - 1),
        };
        foreach (Vector2 gridIndices in startingMassSquares)
        {
            placeBlock(BlockType.MASS, gridIndices);
            yield return Pointer.displayPointer(gridIndices);
        }

        Vector2 startingBlueSquare = new Vector2(0, numGridSquaresDeep - 2);
        placeBlock(BlockType.BLUE, startingBlueSquare);
        yield return Pointer.displayPointer(startingBlueSquare);

        Vector2 startingYellowSquare = new Vector2(numGridSquaresWide - 1, numGridSquaresDeep - 2);
        placeBlock(BlockType.YELLOW, startingYellowSquare);
        yield return Pointer.displayPointer(startingYellowSquare);
    }

    private static void decrementStain()
    /* Each FloorSquare may be stained for a number of turns. For each FloorSquare, tell it a turn
        has passed.
    */
    {
        foreach (FloorSquare floorSquare in FloorSquare.floorSquaresMap.Values)
        {
            floorSquare.addStainTurns(-1);
        }
    }

    private static List<Vector2> standardOrder(IEnumerable<Vector2> gridIndicesList)
    /* Often our game actions occur to a list of Blocks, and instead of doing it in a random order,
        we use a standardized ordering of starting at the top-left, going down the column, then
        repeating for columns left to right.
    
    :params List<Vector2> gridIndicesList: List of floor square positions in the grid.

    :returns List<Vector2> sortedGridIndicesList: Same floor square positions but sorted as
        described above.
    */
    {
        List<Vector2> sortedGridIndicesList = gridIndicesList
            .OrderBy(el => el.x)
            .ThenByDescending(el => el.y)
            .ToList();
        
        return sortedGridIndicesList;
    }

    private static List<Vector2> getProductiveBlocks()
    /* Get a list of all squares that have player Blocks with `produce` effects, in standard order.
    
    :returns List<Vector2> productiveBlocks:
    */
    {
        List<Vector2> productiveBlocks = new List<Vector2>();

        foreach (int xIdx in Enumerable.Range(0, numGridSquaresWide))
        {
            foreach (int yIdx in Enumerable.Range(0, numGridSquaresDeep))
            {
                Vector2 gridIndices = new Vector2(xIdx, yIdx);
                BlockType? blockType = getBlockTypeOfSquare(gridIndices);

                if (blockType == BlockType.BLUE || blockType == BlockType.YELLOW)
                {
                    productiveBlocks.Add(gridIndices);
                }
            }
        }

        productiveBlocks = standardOrder(productiveBlocks);

        return productiveBlocks;
    }

    private static IEnumerator productionPhase()
    /* For all a player's Blocks on the grid, trigger their produce ability. */
    {
        List<Vector2> productiveBlocks = getProductiveBlocks();

        foreach (Vector2 gridIndices in productiveBlocks)
        {
            Block block = placedBlocks[gridIndices];
            block.produce();
            yield return Pointer.displayPointer(gridIndices);
        }
    }

    private static List<Vector2> getNextMassTargets()
    /* Get a list of all squares that current Mass Blocks border, which are the squares it expands
        to or attacks if there are player blocks there. Return in standard order.
    
    :returns List<Vector2> nextTargets:
    */
    {
        HashSet<Vector2> uniqueNextTargets = new HashSet<Vector2>();

        foreach (int xIdx in Enumerable.Range(0, numGridSquaresWide))
        {
            foreach (int yIdx in Enumerable.Range(0, numGridSquaresDeep))
            {
                Vector2 gridIndices = new Vector2(xIdx, yIdx);
                BlockType? blockType = getBlockTypeOfSquare(gridIndices);

                bool isSquareMass = blockType == BlockType.MASS;
                bool isSquareNeighboredByMass = isNeighboredByMass(gridIndices);

                if (!isSquareMass && isSquareNeighboredByMass)
                {
                    uniqueNextTargets.Add(gridIndices);
                }
            }
        }

        List<Vector2> nextTargets = standardOrder(uniqueNextTargets);

        return nextTargets;
    }

    private static bool isNeighboredByMass(Vector2 gridIndices)
    /* Return whether or not any neighbors (no diagonals, no self) are mass.
    
    :param Vector2 gridIndices: Position of square whose neighbors we are checking.

    :returns bool:
    */
    {
        bool didFindMass = false;
        Vector2[] neighbors = MiscHelpers.getNeighbors(gridIndices);
        foreach (Vector2 neighbor in neighbors)
        {
            if (getBlockTypeOfSquare(neighbor) == BlockType.MASS)
            {
                didFindMass = true;
            }
        }

        return didFindMass;
    }

    private static IEnumerator massSpreadingPhase()
    /* Play the enemy's turn, where the mass spreads to empty squares and attacks the player's
        blocks.
    */
    {
        List<Vector2> nextTargets = getNextMassTargets();
        
        foreach (Vector2 gridIndices in nextTargets)
        {
            BlockType? blockType = getBlockTypeOfSquare(gridIndices);
            // If nothing is there, expand the mass into it.
            // Otherwise, attack the player block that's there.
            if (blockType == null)
            {
                placeBlock(BlockType.MASS, gridIndices);
            } else
            {
                Block block = placedBlocks[gridIndices];
                block.attack();
            }
            yield return Pointer.displayPointer(gridIndices);
        }
    }

    private static List<Vector2> getBlocksQueuedToBeDestroyed()
    /* Get a list of all squares that have player Blocks that are about to be destroyed.
    
    The result is ordered from top-left and going down, so column by column from the left.
    
    :returns List<Vector2> blocksToBeDestroyed:
    */
    {
        List<Vector2> blocksToBeDestroyed = new List<Vector2>();

        foreach (int xIdx in Enumerable.Range(0, numGridSquaresWide))
        {
            foreach (int yIdx in Enumerable.Range(0, numGridSquaresDeep))
            {
                Vector2 gridIndices = new Vector2(xIdx, yIdx);
                bool isBlockExists = placedBlocks.ContainsKey(gridIndices);
                if (isBlockExists && placedBlocks[gridIndices].isBeingDestroyed)
                {
                    blocksToBeDestroyed.Add(gridIndices);
                }
            }
        }

        blocksToBeDestroyed = standardOrder(blocksToBeDestroyed);

        return blocksToBeDestroyed;
    }

    private static IEnumerator destructionPhase()
    /* Destroy all the player blocks that were queued up to be destroyed during mass-spreading. */
    {
        List<Vector2> blocksToBeDestroyed = getBlocksQueuedToBeDestroyed();

        foreach (Vector2 gridIndices in blocksToBeDestroyed)
        {
            Block block = placedBlocks[gridIndices];
            block.destroy();
            yield return Pointer.displayPointer(gridIndices);
        }
    }

    private static void evaluateTrialEndConditions()
    /* Check conditions for losing and for winning and set variables isTrialLoss, isTrialWin, and
        isTrialOver.
    */
    {
        bool hasMassReachedLastRow = false;
        int lastRowYIdx = 0;
        foreach (int xIdx in Enumerable.Range(0, numGridSquaresWide))
        {
            Vector2 gridIndices = new Vector2(xIdx, lastRowYIdx);
            if (getBlockTypeOfSquare(gridIndices) == BlockType.MASS)
            {
                hasMassReachedLastRow = true;
            }
        }

        if (hasMassReachedLastRow)
        {
            isTrialLoss = true;
        }
        else if (turnNumber >= turnsToSurvive)
        {
            isTrialWin = true;
        }
        
        isTrialOver = isTrialLoss || isTrialWin;

        if (isTrialOver)
        {
            EventLog.LogEvent($"Trial is over. isTrialLoss: {isTrialLoss}. IsTrialWin: {isTrialWin}");
        }
    }

    private static IEnumerator handleTrialEnd()
    /* Perform actions that happen after a win or loss (wipe out mass, show post-trial screen, ...)
    */
    {
        if (isTrialWin)
        {
            List<Vector2> gridIndicesList = standardOrder(placedBlocks.Keys.ToList());
            foreach (Vector2 gridIndices in gridIndicesList)
            {
                Block block = placedBlocks[gridIndices];
                if (block.blockType == BlockType.MASS)
                {
                    block.destroy();
                    yield return Pointer.displayPointer(block.gridIndices);
                }
            }
        }

        postTrialScreen.show();
    }
}

public struct CardInfo
{
    public CardInfo(string cardName_arg, string cardId_arg)
    {
        cardName = cardName_arg;
        cardId = cardId_arg;
        card = null;
    }

    public string cardName { get; }
    public string cardId { get; }
    public Card card;
};
