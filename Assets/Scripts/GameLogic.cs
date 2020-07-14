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
    private static int baseIumCostForBlock;
    public int turnsToSurvive;
    private int baseIumPerTurn;
    private int baseDrawPerTurn;
    private int startingIum;
    private int startingHandSize;
    private int startingUnstainedRows;
    // Game state.
    public Dictionary<Vector2, Block> placedBlocks = new Dictionary<Vector2, Block>();
    public int currentIum;
    public int turnsTaken;
    public Dictionary<string, CardInfo> cardsById = new Dictionary<string, CardInfo>();
    public List<string> drawPile = new List<string>();
    public List<string> hand = new List<string>();
    public List<string> discardPile = new List<string>();
    public string selectedCardId = null;

    void Awake()
    {
        // Since there should only be 1 GameLogic instance, assign this instance to a global var.
        G = this;

        // Initialize parameters.
        secondsBetweenActions_fast = 0.1f;
        secondsBetweenActions_slow = 0.6f;
        secondsBetweenActions = secondsBetweenActions_slow;
        numGridSquaresWide = 6;
        numGridSquaresDeep = 10;
        baseIumCostForBlock = 2;
        turnsToSurvive = 20;
        baseIumPerTurn = 1;
        baseDrawPerTurn = 1;
        startingIum = 4;
        startingHandSize = 4;
        startingUnstainedRows = 3;
    }

    IEnumerator Start()
    {
        // TODO: freeze user input

        initializeFloor();

        initializeCards();
        
        yield return StartCoroutine(placeStartingBlocks());

        currentIum = startingIum;
        foreach (int _ in Enumerable.Range(0, startingHandSize))
        {
            drawCard();
        }
        turnsTaken = 0;

        startTurn();

        // TODO: unfreeze user input
    }

    void Update()
    {

    }

    /* PUBLIC API */

    public void attemptToPlaceBlock(Vector2 gridIndices)
    /* Attempty to put a block into play in the grid, but may not succeed due to constraints like
        ium and placement restrictions.
    
    :param Vector2 gridIndices: The square in which to put the block ((0, 0) is the bottom-left).
    */
    {
        if (!String.IsNullOrEmpty(selectedCardId))
        {
            CardInfo selectedCard = cardsById[selectedCardId];
            string cardName = selectedCard.cardName;
            bool isSelectedCardABlock = Card.cardNameToBlockType.ContainsKey(cardName);

            FloorSquare floorSquare = FloorSquare.floorSquaresMap[gridIndices];
            int iumCost = baseIumCostForBlock;
            if (floorSquare.isStained)
            {
                iumCost *= 2;
            }
            bool canAffordBlock = currentIum >= iumCost;
            
            if (isSelectedCardABlock && canAffordBlock)
            {
                BlockType blockType = Card.cardNameToBlockType[cardName];
                currentIum -= iumCost;
                placeBlock(blockType, gridIndices);
                discardCard(selectedCardId);
                selectedCardId = null;
            }
        }
    }

    public void placeBlock(BlockType blockType, Vector2 gridIndices)
    /* Put a block into play in the grid.
    
    :param BlockType blockType: enum defined in Block.cs
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

    public void startTurn()
    /* Begin the player's turn, e.g. gain a base amount of ium and draw a base number of cards. */
    {
        gainIum(baseIumPerTurn);
        
        foreach (int _ in Enumerable.Range(0, baseDrawPerTurn))
        {
            drawCard();
        }
    }

    public IEnumerator endTurn()
    /* Do the steps that should occur when player's turn ends, like evaluate combos and produce,
        trigger the enemy's turn, etc.
    */
    {
        // TODO: freeze user input
        
        turnsTaken += 1;

        yield return producePhase();
        
        yield return massSpreadingPhase();

        unstainRow();

        startTurn();

        // TODO: unfreeze user input after above phases are finished (careful, they're async)
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

    public void gainIum(int ium)
    /* Gain ium equal to the amount passed in.
    
    :param int ium: Amount of ium to gain.
    */
    {
        currentIum += ium;
    }

    public void drawCard()
    /* Pick up a Card from drawPile to currentHand. */
    {
        if (drawPile.Count == 0)
        {
            drawPile = discardPile.OrderBy(_ => UnityEngine.Random.value).ToList();
            discardPile.Clear();
        }

        string drawnCardId = drawPile[drawPile.Count - 1];
        drawPile.RemoveAt(drawPile.Count - 1);
        hand.Add(drawnCardId);

        GameObject cardObj = PrefabInstantiator.P.CreateCard(drawnCardId);
        
        Card card = cardObj.GetComponent<Card>();
        CardInfo cardInfo = cardsById[drawnCardId];
        cardInfo.card = card;
        cardsById[drawnCardId] = cardInfo;
    }

    public void discardCard(string cardId)
    /* Put a Card from the hand to the discard pile
    
    :param string cardId: id of the Card in the hand to discard
    */
    {
        hand.Remove(cardId);

        CardInfo cardInfo = cardsById[cardId];
        GameObject cardObj = cardInfo.card.gameObject;
        Destroy(cardObj);
        cardInfo.card = null;
        cardsById[cardId] = cardInfo;

        discardPile.Add(cardId);
    }

    /* HELPERS */

    private void initializeFloor()
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

        // Stain all but the starting unstained rows.
        foreach (int xIdx in Enumerable.Range(0, numGridSquaresWide))
        {
            foreach (int yIdx in Enumerable.Range(0, numGridSquaresDeep - startingUnstainedRows))
            {
                Vector2 gridIndices = new Vector2(xIdx, yIdx);
                FloorSquare floorSquare = FloorSquare.floorSquaresMap[gridIndices];
                floorSquare.setFloorSquareStain(true);
            }
        }
    }

    private void initializeCards()
    /* Shuffle the player's cards and put them in the draw pile. */
    {
        BlockType[] playerBlockTypes = new[] { BlockType.BLUE, BlockType.YELLOW, BlockType.RED };
        foreach (BlockType blockType in playerBlockTypes)
        {
            foreach (int idx in Enumerable.Range(0, 100))
            {
                string cardName = $"single_block_{blockType}";
                string cardId = MiscHelpers.getRandomId();

                cardsById[cardId] = new CardInfo(cardName, cardId);
            }
        }
        List<CardInfo> shuffledDeck = cardsById.Values.OrderBy(_ => UnityEngine.Random.value).ToList();
        foreach (CardInfo cardInfo in shuffledDeck)
        {
            drawPile.Add(cardInfo.cardId);
        }
    }

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
            placeBlock(BlockType.MASS, gridIndices);
            yield return new WaitForSeconds(secondsBetweenActions);
        }

        Vector2 startingBlueSquare = new Vector2(0, numGridSquaresDeep - 2);
        placeBlock(BlockType.BLUE, startingBlueSquare);
        yield return new WaitForSeconds(secondsBetweenActions);
        Vector2 startingYellowSquare = new Vector2(numGridSquaresWide - 1, numGridSquaresDeep - 2);
        placeBlock(BlockType.YELLOW, startingYellowSquare);
        yield return new WaitForSeconds(secondsBetweenActions);
    }

    private void discardHand()
    /* Discard all Cards in the current hand. */
    {
        List<string> handCopy = new List<string>(hand);
        
        foreach (string cardId in handCopy)
        {
            discardCard(cardId);
        }
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
                BlockType? blockType = getBlockTypeOfSquare(gridIndices);

                if (blockType == BlockType.BLUE || blockType == BlockType.YELLOW)
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

    private IEnumerator producePhase()
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
                BlockType? blockType = getBlockTypeOfSquare(gridIndices);

                bool isSquareMass = blockType == BlockType.MASS;
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

    private BlockType? getBlockTypeOfSquare(Vector2 gridIndices)
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
            if (getBlockTypeOfSquare(neighbor) == BlockType.MASS)
            {
                didFindMass = true;
            }
        }

        return didFindMass;
    }

    private IEnumerator massSpreadingPhase()
    /* Play the enemy's turn, where the mass spreads to empty squares and attacks the player's
        blocks
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
                attackBlock(gridIndices);
            }
            yield return new WaitForSeconds(secondsBetweenActions);
        }
    }

    private void unstainRow()
    /* Each turn that elapses allows another row of squares to become unstained. */
    {
        int rowToUnstain = numGridSquaresDeep - startingUnstainedRows - turnsTaken;
        if (rowToUnstain >= 0)
        {
            foreach (int xIdx in Enumerable.Range(0, numGridSquaresWide))
            {
                Vector2 gridIndices = new Vector2(xIdx, rowToUnstain);
                FloorSquare floorSquare = FloorSquare.floorSquaresMap[gridIndices];
                floorSquare.setFloorSquareStain(false);
            }
        }
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
