﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TrialLogic : MonoBehaviour
{   
    // Parameters (user-interaction).
    private static float secondsBetweenActions_fast;
    private static float secondsBetweenActions_slow;
    public static float secondsBetweenActions;
    // Parameters (gameplay).
    public static int difficultyLevel;
    public static int numGridSquaresWide;
    public static int numGridSquaresDeep;
    public static int turnsToSurvive;
    private static int baseIumPerTurn;
    private static int baseDrawPerTurn;
    private static int startingIum;
    private static int startingHandSize;
    private static int startingUnstainedRows;
    private static List<Vector2> startingMassSquares;
    private static List<MassSpecial> activeMassSpecials;
    // State (gameplay).
    public static int currentIum;
    public static int turnNumber;
    public static Dictionary<Vector2, FloorSquare> floorSquaresMap;
    public static Dictionary<Vector2, Block> placedBlocks;
    public static Dictionary<Vector2, List<BlockCombo>> blockCombosKeyedByAnchor;
    public static Dictionary<Block, List<BlockCombo>> blockCombosKeyedByBlock;
    public static Dictionary<string, CardInfo> trialDeck;
    private static List<string> drawPile;
    public static List<string> hand;
    private static List<string> discardPile;
    public static bool isTrialWin;
    public static bool isTrialLoss;
    private static bool isTrialOver;
    // State (user-interaction)
    public static bool isSetupFinished;
    public static string selectedCardId;
    public static Vector2? mouseDownGridIndices;
    public static Vector2? mouseUpGridIndices;
    public static bool isPauseModeOn;
    public static bool isGameplayUserInputsFrozen;

    static TrialLogic()
    {
        initializeParameters();
    }

    void Awake()
    {
        CampaignLogic.trialNumber += 1;

        initializeParameters(CampaignLogic.trialNumber);
        initializeState();
    }

    IEnumerator Start()
    {
        setGameplayUserInputsFrozen(true);

        initializeFloor();

        initializeCards();
        
        yield return StartCoroutine(placeStartingBlocks());

        foreach (int _ in Enumerable.Range(0, startingHandSize))
        {
            drawCard();
        }

        startTurn();

        setGameplayUserInputsFrozen(false);

        isSetupFinished = true;
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
        setGameplayUserInputsFrozen(true);

        TrialLogic.selectedCardId = null;

        decrementStain();

        findAndStoreBlockCombos();

        yield return productionPhase();
        
        yield return massSpreadingPhase();

        clearBlockCombos();

        evaluateTrialEndConditions();

        if (isTrialOver)
        {
            yield return handleTrialEnd();
        } else
        {
            startTurn();
        }

        setGameplayUserInputsFrozen(false);
    }

    public static void setRapidMode(bool turnOn)
    /* Speed up of slow down the game, by adjusting secondsBetweenActions.
    
    :param bool turnOn: Use true to speed up the game, and false to slow it down.
    */
    {
        secondsBetweenActions = turnOn ? secondsBetweenActions_fast : secondsBetweenActions_slow;
    }

    public static void setPauseMode(bool turnOn)
    /* Toggle on or off pause mode, which pauses the game at next pointer-display. Turning off pause
        mode resumes the game, and to pause again pause mode would have to be turned back on.
    
    :param bool turnOn: Use true to turn on pause mode, and false to turn it off.
    */
    {
        isPauseModeOn = turnOn;
    }

    public static void setGameplayUserInputsFrozen(bool freeze)
    /* Toggle on or off the freezing of gameplay user inputs. For example, while the between-turn
        phases are happening, don't allow placing blocks, or ending turn again.
    
    :param bool freeze: true to freeze user input, false to unfreeze it.
    */
    {
        isGameplayUserInputsFrozen = freeze;
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
    /* Pick up a card from the draw pile and put it into the current hand. */
    {
        if (drawPile.Count == 0)
        {
            drawPile = discardPile.OrderBy(_ => UnityEngine.Random.value).ToList();
            discardPile.Clear();

            EventLog.LogEvent($"Replenished draw pile from discard pile.");
        }

        if (drawPile.Count == 0)
        {
            EventLog.LogEvent($"Attempted to draw, but draw pile was empty.");
            return;
        }

        string cardId = drawPile[drawPile.Count - 1];
        drawPile.RemoveAt(drawPile.Count - 1);
        hand.Add(cardId);

        CardInfo cardInfo = trialDeck[cardId];
        Transform handTransform = GameObject.Find("Hand").transform;

        GameObject cardObj = PrefabInstantiator.P.CreateCard(cardInfo, handTransform);
        
        Card card = cardObj.GetComponent<Card>();
        cardInfo.card = card;
        trialDeck[cardId] = cardInfo;

        EventLog.LogEvent($"Drew card {cardInfo.cardName} (id: {cardId}).");
    }

    public static void discardCard(string cardId)
    /* Put a card from the hand to the discard pile (or put it nowhere, if card is consumable).
    
    :param string cardId: id of the Card in the hand to discard
    */
    {
        hand.Remove(cardId);

        CardInfo cardInfo = trialDeck[cardId];
        bool isConsumable = cardInfo.card.isConsumable;
        GameObject cardObj = cardInfo.card.gameObject;
        Destroy(cardObj);
        cardInfo.card = null;
        trialDeck[cardId] = cardInfo;

        if (!isConsumable)
        {
            discardPile.Add(cardId);
        }

        string verb = isConsumable ? "Consumed" : "Discarded";
        EventLog.LogEvent($"{verb} card {cardInfo.cardName} (id: {cardId}).");
    }

    public static BlockType? getBlockTypeOfSquare(Vector2 gridIndices)
    /* Get the block type of a place in the grid.
    
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

    private static void initializeParameters(int trialNumber = 0)
    /* Set parameters of the trial at the beginning of the game.
    
    :param int trialNumber: (optional) which trial in the campaign is it
    */
    {
        secondsBetweenActions_fast = 0.1f;
        secondsBetweenActions_slow = 0.6f;
        secondsBetweenActions = secondsBetweenActions_slow;
        numGridSquaresWide = 6;
        numGridSquaresDeep = 10;
        turnsToSurvive = 14;
        baseIumPerTurn = 3;
        baseDrawPerTurn = 1;
        startingIum = 3;
        startingHandSize = 4;
        startingUnstainedRows = 4;
        startingMassSquares = new List<Vector2>
        {
            new Vector2((numGridSquaresWide / 2) - 1, numGridSquaresDeep - 1),
            new Vector2(numGridSquaresWide / 2, numGridSquaresDeep - 1)
        };
        activeMassSpecials = new List<MassSpecial>();

        if (trialNumber >= 2)
        {
            turnsToSurvive = 16;
            
            startingMassSquares.Add(new Vector2(0, numGridSquaresDeep - 1));
            startingMassSquares.Add(new Vector2(numGridSquaresWide - 1, numGridSquaresDeep - 1));
            
            activeMassSpecials.Add(MassSpecial.EXTRA_START_CORNER);
        }
        if (trialNumber >= 3)
        {
            turnsToSurvive = 20;
            
            startingMassSquares.Add(new Vector2(0, numGridSquaresDeep - 3));
            startingMassSquares.Add(new Vector2(numGridSquaresWide - 1, numGridSquaresDeep - 3));

            activeMassSpecials.Add(MassSpecial.EXTRA_SPREAD_PHASE);
        }
    }

    private static void initializeState()
    /* Set the initial values of all per-trial state.
    
    Used at the beginning of the game, as well as to reset this object for each new trial.
    */
    {
        // Gameplay.
        currentIum = startingIum;
        turnNumber = 0;
        floorSquaresMap = new Dictionary<Vector2, FloorSquare>();
        placedBlocks = new Dictionary<Vector2, Block>();
        trialDeck = new Dictionary<string, CardInfo>();
        drawPile = new List<string>();
        hand = new List<string>();
        discardPile = new List<string>();
        isTrialWin = false;
        isTrialLoss = false;
        isTrialOver = false;
        // User-interaction.
        isSetupFinished = false;
        selectedCardId = null;
        mouseDownGridIndices = null;
        mouseUpGridIndices = null;
        isPauseModeOn = false;
        isGameplayUserInputsFrozen = false;
    }

    private static void initializeFloor()
    /* Create the grid of floor squares. */
    {
        // Create the grid.
        foreach (int xIdx in Enumerable.Range(0, numGridSquaresWide))
        {
            foreach (int yIdx in Enumerable.Range(0, numGridSquaresDeep))
            {
                Vector2 gridIndices = new Vector2(xIdx, yIdx);
                bool isSacred = yIdx == 0;
                PrefabInstantiator.P.CreateFloorSquare(gridIndices, isSacred: isSacred);
            }
        }

        // Stain all but the starting unstained rows, 1 extra turn of stain per row you move back.
        foreach (int xIdx in Enumerable.Range(0, numGridSquaresWide))
        {
            foreach (int yIdx in Enumerable.Range(0, numGridSquaresDeep - startingUnstainedRows))
            {
                Vector2 gridIndices = new Vector2(xIdx, yIdx);
                FloorSquare floorSquare = floorSquaresMap[gridIndices];
                int stainTurns = numGridSquaresDeep - startingUnstainedRows - yIdx;
                floorSquare.addStainTurns(stainTurns);
            }
        }
    }

    private static void initializeCards()
    /* Shuffle some set of cards I chose and put them in the draw pile. */
    {
        trialDeck = new Dictionary<string, CardInfo>(CampaignLogic.campaignDeck);

        // For testing trials not as part of a campaign.
        if (trialDeck.Count == 0)
        {
            trialDeck = fakeDeck();
        }
        
        List<CardInfo> shuffledDeck = trialDeck.Values.OrderBy(_ => UnityEngine.Random.value).ToList();

        foreach (CardInfo cardInfo in shuffledDeck)
        {
            drawPile.Add(cardInfo.cardId);
        }
    }

    private static IEnumerator placeStartingBlocks()
    /* Place the blocks that start out on the grid at the beginning of a round. */
    {
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
    /* Each FloorSquare may be stained for a number of turns. For each floor square, tell it a turn
        has passed.
    */
    {
        foreach (FloorSquare floorSquare in floorSquaresMap.Values)
        {
            floorSquare.addStainTurns(-1);
        }
    }

    private static void findAndStoreBlockCombos()
    /* Search the grid for all block combos that exist this turn, and store them in a few ways for
        easy lookup.
    
    Populate blockCombosKeyedByAnchor and blockCombosKeyedByBlock.
    */
    {
        blockCombosKeyedByAnchor = new Dictionary<Vector2, List<BlockCombo>>();
        blockCombosKeyedByBlock = new Dictionary<Block, List<BlockCombo>>();

        foreach (int xIdx in Enumerable.Range(0, numGridSquaresWide))
        {
            foreach (int yIdx in Enumerable.Range(0, numGridSquaresDeep))
            {
                Vector2 gridIndices = new Vector2(xIdx, yIdx);
                List<BlockCombo> combosAnchoredOnSquare = BlockCombo.combosAnchoredAt(gridIndices);
                blockCombosKeyedByAnchor[gridIndices] = combosAnchoredOnSquare;

                foreach (BlockCombo blockCombo in combosAnchoredOnSquare)
                {
                    foreach (Block block in blockCombo.blocks)
                    {
                        if (blockCombosKeyedByBlock.ContainsKey(block))
                        {
                            blockCombosKeyedByBlock[block].Add(blockCombo);
                        } else
                        {
                            blockCombosKeyedByBlock[block] = new List<BlockCombo> { blockCombo };
                        }
                    }
                }
            }
        }
    }

    private static List<Block> getProductiveBlocks()
    /* Get a list of all blocks that have `produce` effects, in standard order.
    
    :returns List<Block> productiveBlocks:
    */
    {
        List<Block> productiveBlocks = new List<Block>();

        foreach (int xIdx in Enumerable.Range(0, numGridSquaresWide))
        {
            foreach (int yIdx in Enumerable.Range(0, numGridSquaresDeep))
            {
                Vector2 gridIndices = new Vector2(xIdx, yIdx);
                BlockType? blockType = getBlockTypeOfSquare(gridIndices);

                if (Block.isProductive(blockType))
                {
                    Block block = placedBlocks[gridIndices];
                    productiveBlocks.Add(block);
                }
            }
        }

        productiveBlocks = MiscHelpers.standardOrder(productiveBlocks, b => b.gridIndices);

        return productiveBlocks;
    }

    private static List<BlockCombo> getProductiveCombos()
    /* Get a list of all productive combos, in standard order.
    
    :returns List<Vector2> productiveCombos:
    */
    {
        List<BlockCombo> productiveCombos = new List<BlockCombo>();

        foreach (List<BlockCombo> blockCombos in blockCombosKeyedByAnchor.Values)
        {
            foreach (BlockCombo blockCombo in blockCombos)
            {
                if (BlockCombo.isProductive(blockCombo.blockComboType))
                {
                    productiveCombos.Add(blockCombo);
                }
            }
        }

        productiveCombos = MiscHelpers.standardOrder(productiveCombos, pc => pc.getAnchorSquare());

        return productiveCombos;
    }

    private static IEnumerator productionPhase()
    /* For all a player's blocks and block combos on the grid, trigger their produce ability. */
    {
        // Produce for individual blocks.
        List<Block> productiveBlocks = getProductiveBlocks();
        foreach (Block block in productiveBlocks)
        {
            block.produce();
            yield return Pointer.displayPointer(block.gridIndices, "Produce");
        }

        // Produce for block combos.
        List<BlockCombo> productiveCombos = getProductiveCombos();
        foreach (BlockCombo productiveCombo in productiveCombos)
        {
            productiveCombo.produce();
            Vector2 gridIndices = productiveCombo.getAnchorSquare();
            yield return Pointer.displayPointer(gridIndices, "Combo Produce");
        }
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

    private static List<Vector2> getNextMassTargets()
    /* Get a list of all squares that current mass blocks border, which are the squares it expands
        to or attacks if there are player blocks there. Return in standard order.
    
    The exceptional case is if no mass blocks exist, place mass in the starting spots.
    
    :returns List<Vector2> nextTargets:
    */
    {
        HashSet<Vector2> uniqueNextTargets = new HashSet<Vector2>();
        bool foundAnyMass = false;

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

                if (isSquareMass)
                {
                    foundAnyMass = true;
                }
            }
        }

        List<Vector2> nextTargets = foundAnyMass ? uniqueNextTargets.ToList() : startingMassSquares;
        
        nextTargets = MiscHelpers.standardOrder(nextTargets, MiscHelpers.identity);

        return nextTargets;
    }

    private static List<Block> getBlocksQueuedToBeDestroyed()
    /* Get a list of all Blocks that are about to be destroyed.
    
    The result is ordered from top-left and going down, so column by column from the left.
    
    :returns List<Block> blocksToBeDestroyed:
    */
    {
        List<Block> blocksToBeDestroyed = new List<Block>();

        foreach (int xIdx in Enumerable.Range(0, numGridSquaresWide))
        {
            foreach (int yIdx in Enumerable.Range(0, numGridSquaresDeep))
            {
                Vector2 gridIndices = new Vector2(xIdx, yIdx);
                bool isBlockExists = placedBlocks.ContainsKey(gridIndices);
                if (isBlockExists && placedBlocks[gridIndices].isBeingDestroyed)
                {
                    Block block = placedBlocks[gridIndices];
                    blocksToBeDestroyed.Add(block);
                }
            }
        }

        blocksToBeDestroyed = MiscHelpers.standardOrder(blocksToBeDestroyed, b => b.gridIndices);

        return blocksToBeDestroyed;
    }

    private static IEnumerator destroyQueuedBlocks()
    /* Destroy all the player blocks that were queued up to be destroyed during mass-spreading. */
    {
        List<Block> blocksToBeDestroyed = getBlocksQueuedToBeDestroyed();

        foreach (Block block in blocksToBeDestroyed)
        {
            // Destroy the block, triggering any on destroy effects of that block.
            Vector2 gridIndices = block.gridIndices;
            block.destroy();
            yield return Pointer.displayPointer(gridIndices, "Destroy");
            
            // Execute any on destroy effects of block combos that involved that block.
            if (blockCombosKeyedByBlock.ContainsKey(block))
            {
                List<BlockCombo> blockCombos = blockCombosKeyedByBlock[block];
                foreach (BlockCombo blockCombo in blockCombos)
                {
                    if (BlockCombo.hasDestroyEffect(blockCombo.blockComboType))
                    {
                        blockCombo.onBlockDestroy();
                        yield return Pointer.displayPointer(gridIndices, "Combo Destroy");
                    }
                }
            }
        }
    }

    private static IEnumerator massSpreadingPhase()
    /* Play the enemy's turn, where the mass spreads to empty squares and attacks the player's
        blocks.
    */
    {
        int numSpreads = activeMassSpecials.Contains(MassSpecial.EXTRA_SPREAD_PHASE) ? 2 : 1;

        foreach (int _ in Enumerable.Range(0, numSpreads))
        {
            List<Vector2> nextTargets = getNextMassTargets();
            
            foreach (Vector2 gridIndices in nextTargets)
            {
                string text;
                BlockType? blockType = getBlockTypeOfSquare(gridIndices);
                // If nothing is there, expand the mass into it.
                // Otherwise, attack the player block that's there.
                if (blockType == null)
                {
                    placeBlock(BlockType.MASS, gridIndices);
                    text = "Spread";
                } else
                {
                    Block block = placedBlocks[gridIndices];
                    block.attack();
                    text = "Attack";
                }
                yield return Pointer.displayPointer(gridIndices, text: text);
            }

            yield return destroyQueuedBlocks();
        }
    }

    private static void clearBlockCombos()
    /* Since combos are evaluated anew each turn, unregister the combos we identified this turn. */
    {
        blockCombosKeyedByAnchor.Clear();
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
            List<Vector2> gridIndicesList = MiscHelpers.standardOrder(
                placedBlocks.Keys,
                MiscHelpers.identity
            );
            foreach (Vector2 gridIndices in gridIndicesList)
            {
                Block block = placedBlocks[gridIndices];
                if (block.blockType == BlockType.MASS)
                {
                    block.destroy();
                    yield return Pointer.displayPointer(gridIndices);
                }
            }

            CampaignLogic.currentCash += CampaignLogic.cashRewardPerTrial;
        }

        PostTrialScreen postTrialScreen = GameObject.Find("PostTrialScreen").GetComponent<PostTrialScreen>();
        postTrialScreen.show();
    }

    /* TESTING */

    private static Dictionary<string, CardInfo> fakeDeck()
    /* Create a fake deck from every card, to let us test the trial, and cards, without making a
        whole campaign.
    */
    {
        Dictionary<string, CardInfo> fakeDeck = new Dictionary<string, CardInfo>();

        // Create a deck of 1 of each card.
        foreach (string cardName in ShopLogic.cardNameToBaseCashValue.Keys)
        {
            string cardId = MiscHelpers.getRandomId();
            fakeDeck[cardId] = new CardInfo(cardName, cardId);
        }

        string cardId1 = MiscHelpers.getRandomId();
        fakeDeck[cardId1] = new CardInfo(PlaceThreePieceCard.getThreePieceCardName(BlockType.BLUE, isFlipped: true), cardId1);
        string cardId2 = MiscHelpers.getRandomId();
        fakeDeck[cardId2] = new CardInfo(PlaceThreePieceCard.getThreePieceCardName(BlockType.BLUE), cardId2);
        string cardId3 = MiscHelpers.getRandomId();
        fakeDeck[cardId3] = new CardInfo(PlaceThreePieceCard.getThreePieceCardName(BlockType.RED, isFlipped: true), cardId3);
        string cardId4 = MiscHelpers.getRandomId();
        fakeDeck[cardId4] = new CardInfo(PlaceThreePieceCard.getThreePieceCardName(BlockType.RED), cardId4);

        return fakeDeck;
    }
}

public enum MassSpecial
{
    EXTRA_START_CORNER,
    EXTRA_SPREAD_PHASE,
}