using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Block : MonoBehaviour
{
    public Material regularMaterial;
    public Material damagedMaterial;

    public static Color massColor = new Color(85/255f, 85/255f, 85/255f);
    public static Color blueColor = new Color(0, 119/255f, 238/255f);
    public static Color yellowColor = new Color(238/255f, 238/255f, 85/255f);
    public static Color redColor = new Color(170/255f, 0, 34/255f);
    public static float blockHeight = 0.5f;

    // Parameters.
    public BlockType blockType;
    public Vector2 gridIndices;
    // State.
    public bool isDamaged = false;
    public bool isBeingDestroyed = false;

    void Start()
    {
        Vector3 scale = transform.localScale;
        transform.localScale = new Vector3(scale.x, blockHeight, scale.z);

        Vector3 position = transform.position;
        transform.position = new Vector3(position.x, blockHeight / 2, position.z);

        Color color = blockTypeToColor[blockType];
        GetComponent<Renderer>().material.SetColor("_Color", color);

        regularMaterial = GetComponent<Renderer>().material;
    }

    void OnMouseDown()
    {
        if (TrialLogic.isGameplayUserInputsFrozen)
        {
            return;
        }
        
        // Store which square was clicked down on.
        TrialLogic.mouseDownGridIndices = gridIndices;
    }

    void OnMouseUp()
    {
        if (TrialLogic.isGameplayUserInputsFrozen)
        {
            return;
        }
        
        // Store which square was clicked up on.
        TrialLogic.mouseUpGridIndices = null;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            FloorSquare floorSquare = hit.transform.gameObject.GetComponent<FloorSquare>();
            Block block = hit.transform.gameObject.GetComponent<Block>();
            if (floorSquare != null)
            {
                TrialLogic.mouseUpGridIndices = floorSquare.gridIndices;
            } else if (block != null)
            {
                TrialLogic.mouseUpGridIndices = block.gridIndices;
            }
        }

        if (String.IsNullOrEmpty(TrialLogic.selectedCardId))
        {
            return;
        }
        Card selectedCard = TrialLogic.trialDeck[TrialLogic.selectedCardId].card;
        selectedCard.playCard();

        TrialLogic.mouseDownGridIndices = null;
        TrialLogic.mouseUpGridIndices = null;
    }

    /* PUBLIC API */

    public void produce()
    /* Depending on the blockType, perform any production effects. */
    {
        if (blockType == BlockType.BLUE)
        {
            TrialLogic.gainIum(1);
        } else if (blockType == BlockType.YELLOW)
        {
            TrialLogic.drawCard();
        }
        
        EventLog.LogEvent($"Produced for blockType {blockType} at {gridIndices}.");
    }

    public void attack()
    /* Apply the mass's current attack to a player's block. */
    {
        if (isDamaged || blockType == BlockType.RED)
        {
            queueToBeDestroyed();
        } else
        {
            damage();
        }

        EventLog.LogEvent($"Was attacked at {gridIndices}.");
    }

    public void damage()
    /* Change this Block from healthy to damaged. */
    {
        bool oldVal = isDamaged;
        bool newVal = true;

        isDamaged = newVal;
        
        GetComponent<Renderer>().material = damagedMaterial;

        Color color = blockTypeToColor[blockType];
        GetComponent<Renderer>().material.SetColor("_Color", color);

        EventLog.LogEvent($"isDamaged changed from {oldVal} to {newVal} at {gridIndices}.");
    }

    public void repair()
    /* Change this Block from damaged to healthy. */
    {
        bool oldVal = isDamaged;
        bool newVal = false;

        isDamaged = newVal;

        GetComponent<Renderer>().material = regularMaterial;

        EventLog.LogEvent($"isDamaged changed from {oldVal} to {newVal} at {gridIndices}.");
    }

    public void queueToBeDestroyed()
    /* Mark this Block to be destroyed in the upcoming destruction phase. */
    {
        isBeingDestroyed = true;

        // Visually indicate the fact that this block is queued to be destroyed.
        transform.Find("QueuedToBeDestroyedMark").gameObject.SetActive(true);

        EventLog.LogEvent($"Was queued to be destroyed at {gridIndices}.");
    }

    public void destroy()
    /* Remove this Block from the grid, applying any onDestroy effects. */
    {
        // If this Block has any onDestroy effects, run them.
        if (blockType == BlockType.RED)
        {
            destroyNeighboringMass();
        }

        FloorSquare floorSquare = TrialLogic.floorSquaresMap[gridIndices];
        floorSquare.addStainTurns(1);
        
        TrialLogic.placedBlocks.Remove(gridIndices);

        EventLog.LogEvent($"Was destroyed at {gridIndices}.");

        Destroy(gameObject);
    }

    public void shift(Vector2 newGridIndices)
    /* Move this block to a new square.
    
    :param Vector2 newGridIndices:
    */
    {
        // Reposition this block visually.
        Vector3 currentPosition = transform.position;
        Vector3 newPosition = FloorSquare.getGridSquareCenter(newGridIndices);
        newPosition.y = currentPosition.y;
        transform.position = newPosition;

        // Update the trial's knowledge of this block's placement.
        TrialLogic.placedBlocks[newGridIndices] = TrialLogic.placedBlocks[gridIndices];
        TrialLogic.placedBlocks.Remove(gridIndices);

        // Reset this block's stored grid indices.
        gridIndices = newGridIndices;
    }

    public static bool isProductive(BlockType? blockType)
    /* Return whether or not this block type has a produce effect.
    
    :param BlockType? blockType:
    
    :returns bool:
    */
    {
        return blockType == BlockType.BLUE || blockType == BlockType.YELLOW;
    }

    public static bool hasDestroyEffect(BlockType? blockType)
    /* Return whether or not this block type has a destroy effect.
    
    :param BlockType? blockType:
    
    :returns bool:
    */
    {
        return blockType == BlockType.RED;
    }

    /* HELPERS */

    private void destroyNeighboringMass()
    /* Remove mass Blocks that neighbor this Block. */
    {
        Vector2[] neighbors = MiscHelpers.getNeighbors(gridIndices);
        foreach (Vector2 neighborIndices in neighbors)
        {
            BlockType? neighborBlockType = TrialLogic.getBlockTypeOfSquare(neighborIndices);
            if (neighborBlockType == BlockType.MASS)
            {
                Block neighborBlock = TrialLogic.placedBlocks[neighborIndices];
                neighborBlock.destroy();
            }
        }
    }

    /* COLLECTIONS */
    
    private Dictionary<BlockType, Color> blockTypeToColor = new Dictionary<BlockType, Color>
    {
        { BlockType.MASS, massColor },
        { BlockType.BLUE, blueColor },
        { BlockType.YELLOW, yellowColor },
        { BlockType.RED, redColor },
    };
}

public enum BlockType
{
    MASS,
    BLUE,
    YELLOW,
    RED
}
