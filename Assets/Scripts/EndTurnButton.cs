using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndTurnButton : MonoBehaviour
{
    public GameObject gameLogicObj;

    private GameLogic gameLogic;

    void Awake()
    {
        gameLogic = gameLogicObj.GetComponent<GameLogic>();
    }

    void Start()
    {

    }

    /* PUBLIC API */

    public void clickEndTurn()
    {
        gameLogic.endTurn();
    }
}
