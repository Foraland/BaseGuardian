//Command Trigger
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class CT : Singleton<CT>
{
    private event Action currentCmds;
    private event Action nextCmds;
    // private static Dictionary<int,System.Object> onceCmdsAgent = new Dictionary<int, System.Object>();
    private static List<(Func<bool>, Action)> conditionCmds = new List<(Func<bool>, Action)>();
    ///<summary> 延迟触发（time=0则下一帧触发）</summary>
    public static void DelayCmd(Action cmd, float time = 0)
    {
        if (time == 0)
            Ins.nextCmds += cmd;
        else
        {
            TM.SetTimer(cmd.Hash("Delay"), time, null, s => cmd());
        }
    }
    public static void SuspendCmd(Action cmd, Func<bool> condition)
    {
        conditionCmds.Add((condition, cmd));
    }
    // public static void SuspendCmd(Action cmd,string evtKey){
    //     Debug.Log("suspend");
    //     int hash = evtKey.GetHashCode();
    //     Debug.Log(hash);
    //     System.Object empty = new System.Object();
    //     // onceCmdsAgent.Add(hash,empty);
    //     bool flag = true;
    //     empty.On(evtKey, () =>
    //     {
    //         if(!flag)
    //             return;
    //         flag = false;
    //         cmd.Invoke();
    //         DelayCmd(()=>empty.Off(evtKey)) ;
    //         // onceCmdsAgent.Remove(hash);
    //     });
    // }
    public static void OnUpdate()
    {
        Ins.currentCmds?.Invoke();
        Ins.currentCmds = Ins.nextCmds;
        Ins.nextCmds = null;

        List<(Func<bool>, Action)> removeCmds = new List<(Func<bool>, Action)>();
        conditionCmds.ForEach(e =>
        {
            if (e.Item1.Invoke())
            {
                e.Item2.Invoke();
                removeCmds.Add(e);
            }
        });
        removeCmds.ForEach(e => conditionCmds.Remove(e));
    }
}
