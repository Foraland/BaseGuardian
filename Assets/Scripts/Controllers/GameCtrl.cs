using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCtrl : Controller
{
    public override EGameState gameState => EGameState.Game;
    public GameInfo info => GameManager.Ins.gameInfo;
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
        info.playerEnergy = Mathf.Min(info.playerMaxEnergy, info.playerEnergy + info.energySpeed * Time.deltaTime);
        EC.Send(EC.REFRESH);
    }

}
