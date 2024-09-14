using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverCtrl : Controller
{
    public override EGameState gameState => EGameState.Over;
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

}
