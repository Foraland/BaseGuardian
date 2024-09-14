using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class TimerInfo
{
    public readonly string id;  //计时器id
    public Action<float> update = null;   //每帧行为
    public Action pause = null;     //暂停行为
    public Action resume = null;    //取消暂停行为
    public Action<int> end = null;       //计时结束行为（单轮）
    public float timer = 0;         //计时器
    public float time = 1f;           //设定时间
    public bool isPause = false;  //暂停标识
    public int loop = 1;                  //剩余循环次数
    public bool isEnd => timer <= 0 || loop == 0;//是否结束
    public TimerInfo(string id)
    {
        this.id = id;
    }
}
public class TM : Singleton<TM>
{
    private Dictionary<string, TimerInfo> timerMap = new Dictionary<string, TimerInfo>();
    private Dictionary<string, TimerInfo> sleepTimers = new Dictionary<string, TimerInfo>();
    private static void SleepTimer(string timerId, TimerInfo timer)
    {
        if (Ins.sleepTimers.ContainsKey(timerId))
            Ins.sleepTimers[timerId] = timer;
        else
            Ins.sleepTimers.Add(timerId, timer);
    }
    public static TimerInfo GetTimer(string timerId)
    {
        if (!Ins.timerMap.ContainsKey(timerId))
        {
            if (Ins.sleepTimers.ContainsKey(timerId))
                Ins.timerMap.Add(timerId, Ins.sleepTimers[timerId]);
            else
                Ins.timerMap.Add(timerId, new TimerInfo(timerId));
        }
        return Ins.timerMap[timerId];
    }
    ///<summary> 设置计时器（不要在end里重启计时器） </summary>
    /// <param name="timerId"> 计时器标识id  </param>
    /// <param name="time"> 计时设定时间  </param>
    /// <param name="update"> 每帧行为(参数为当前循环进度0~1)  </param>
    /// <param name="end"> 每轮计时结束行为(参数为该轮结束后剩余循环次数)  </param>
    /// <param name="loop"> 给定循环次数（-1则无限循环） </param>
    /// <param name="pause"> 被暂停时行为 </param>
    /// <param name="resume"> 取消暂停时行为 </param>
    /// <returns>  </returns>
    public static void SetTimer(string timerId, float time, Action<float> update = null, Action<int> end = null, int loop = 1, Action pause = null, Action resume = null)
    {
        TimerInfo info = GetTimer(timerId);
        info.update = update;
        info.pause = pause;
        info.end = end;
        info.resume = resume;
        info.time = info.timer = time;
        info.loop = loop;
    }
    public static void SetTime(string timerId, float time)
    {
        TimerInfo info = GetTimer(timerId);
        info.time = time;
    }
    public static void AddTimeBy(string timerId, float time)
    {
        GetTimer(timerId).timer += time;
    }
    public static void SetRestart(string timerId, int loop = 1)
    {
        TimerInfo info = GetTimer(timerId);
        info.timer = info.time;
        info.loop = loop;
    }
    public static void SetPause(string timerId)
    {
        TimerInfo info = GetTimer(timerId);
        if (info.isPause)
            return;
        info.isPause = true;
        info.pause?.Invoke();
    }
    public static void SetResume(string timerId)
    {
        TimerInfo info = GetTimer(timerId);
        if (!info.isPause)
            return;
        info.isPause = false;
        info.resume?.Invoke();
    }
    public static void SetEnd(string timerId, bool excuteEnd = true)
    {
        TimerInfo info = GetTimer(timerId);
        info.loop = 0;
        info.timer = 0;
        if (excuteEnd)
            info.end?.Invoke(0);
        SleepTimer(timerId, info);
        ClearTimer(timerId);
    }
    public static bool IsRunning(string timerId)
    {
        if (!Ins.timerMap.ContainsKey(timerId))
            return false;
        return !(Ins.timerMap[timerId].isPause || Ins.timerMap[timerId].isEnd);
    }
    public static bool IsEnd(string timerId)
    {
        if (!Ins.timerMap.ContainsKey(timerId))
            return true;
        return Ins.timerMap[timerId].isEnd;
    }
    public static void ClearTimer(string timerId)
    {
        Ins.timerMap.Remove(timerId);
    }
    public static void ClearAllTimer()
    {
        Ins.timerMap.Clear();
        Ins.sleepTimers.Clear();
    }
    public static void OnUpdate()
    {
        List<TimerInfo> clears = new List<TimerInfo>();
        foreach (var e in Ins.timerMap)
        {
            TimerInfo info = e.Value;
            if (info.isEnd)
            {
                SetEnd(info.id);
                clears.Add(info);
            }
            else if (!info.isPause)
            {
                info.timer -= Time.deltaTime;
                info.update?.Invoke(Mathf.Min(1, 1 - info.timer / info.time));
                if (info.timer <= 0)
                {
                    if (info.loop < 0)
                        info.timer = info.time;
                    else
                    {
                        info.loop--;
                        if (info.loop > 0)
                            info.timer = info.time;
                        else
                        {
                            info.timer = 0;
                            clears.Add(info);
                        }
                    }
                    info.end?.Invoke(info.loop);
                }
            }
        }
        clears.ForEach(e =>
        {
            SleepTimer(e.id, e);
            ClearTimer(e.id);
        });
    }
}
