using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Block : MonoBehaviour, IPointerClickHandler
{
    public Material regularMaterial;
    public Material damagedMaterial;

    public static Color massColor = new Color(85/255f, 85/255f, 85/255f);
    public static Color blueColor = new Color(0, 119/255f, 238/255f);
    public static Color yellowColor = new Color(238/255f, 238/255f, 85/255f);
    public static Color redColor = new Color(170/255f, 0, 34/255f);
    private Dictionary<BlockType, Color> blockTypeToColor = new Dictionary<BlockType, Color>
    {
        { BlockType.MASS, massColor },
        { BlockType.BLUE, blueColor },
        { BlockType.YELLOW, yellowColor },
        { BlockType.RED, redColor },
    };
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

    void Update()
    {
        
    }

    /* PUBLIC API */

    public void OnPointerClick(PointerEventData eventData)
    /* Override this function of IPointerClickHandler. Triggers when this Block is clicked.
    
    :param PointerEventData eventData: This interface is defined by Unity.
    */
    {
        if (String.IsNullOrEmpty(TrialLogic.T.selectedCardId))
        {
            return;
        }
        
        Card selectedCard = TrialLogic.T.cardsById[TrialLogic.T.selectedCardId].card;
        selectedCard.playCard(gridIndices);
    }

    public void produce()
    /* Depending on the blockType, perform any production effects. */
    {
        if (blockType == BlockType.BLUE)
        {
            TrialLogic.T.gainIum(1);
        } else if (blockType == BlockType.YELLOW)
        {
            TrialLogic.T.drawCard();
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

        // TODO: visually indicate that this block is queued to be destroyed

        EventLog.LogEvent($"Was queued to be destroyed at {gridIndices}.");
    }

    public void destroy()
    /* Remove this Block from the grid, applying any onDestroy effects. */
    {
        // If this Block has any onDestroy effects, run them.
        if (blockType == BlockType.RED)
        {
            clearNeighboringMass();
        }

        FloorSquare floorSquare = FloorSquare.floorSquaresMap[gridIndices];
        floorSquare.addStainTurns(1);
        
        TrialLogic.T.placedBlocks.Remove(gridIndices);

        EventLog.LogEvent($"Was destroyed at {gridIndices}.");

        Destroy(gameObject);
    }

    /* HELPERS */

    private void clearNeighboringMass()
    /* Remove mass Blocks that neighbor this Block. */
    {
        Vector2[] neighbors = MiscHelpers.getNeighbors(gridIndices);
        foreach (Vector2 neighborIndices in neighbors)
        {
            BlockType? neighborBlockType = TrialLogic.T.getBlockTypeOfSquare(neighborIndices);
            if (neighborBlockType == BlockType.MASS)
            {
                Block neighborBlock = TrialLogic.T.placedBlocks[neighborIndices];
                neighborBlock.destroy();
            }
        }
    }
}

public enum BlockType
{
    MASS,
    BLUE,
    YELLOW,
    RED
}
