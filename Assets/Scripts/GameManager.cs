using System.Collections.Generic;

public enum EGameState
{
    Start,
    Game,
    Over,
}
public class GameManager : SingletonComp<GameManager>
{
    public List<Controller> startCtrls = new List<Controller>();
    public List<Controller> gameCtrls = new List<Controller>();
    public List<Controller> overCtrls = new List<Controller>();
    public EGameState gameState = EGameState.Start;
    public GameInfo gameInfo = new GameInfo();
    public List<Controller> curCtrls
    {
        get
        {
            switch (gameState)
            {
                case EGameState.Game:
                    return gameCtrls;
                case EGameState.Over:
                    return overCtrls;
                case EGameState.Start:
                default:
                    return startCtrls;
            }
        }
    }
    public void OnEnterGame()
    {
        gameInfo.Init();
        curCtrls.ForEach(e => e.OnEnter());

    }
    public void OnExitGame()
    {
        curCtrls.ForEach(e => e.OnExit());
    }
    public void Update()
    {
        curCtrls.ForEach(e => e.OnUpdate());
    }
    public void SwitchState(EGameState state)
    {
        curCtrls.ForEach(e => e.OnExit());
        gameState = state;
        curCtrls.ForEach(e => e.OnEnter());
    }
}