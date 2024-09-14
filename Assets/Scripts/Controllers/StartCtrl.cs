using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartCtrl : Controller
{
    public override EGameState gameState => EGameState.Start;
    public override void OnEnter()
    {
        gameObject.SetActive(true);
    }

    public override void OnExit()
    {
        gameObject.SetActive(false);
    }

    public override void OnUpdate()
    {

    }
    public void SwitchToGame()
    {
        GameManager.Ins.SwitchState(EGameState.Game);
    }

}
