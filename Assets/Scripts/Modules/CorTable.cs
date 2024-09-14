using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CorTable : MonoBehaviour
{
    #region perhaps useless
    public List<IEnumerator> tabList;
    /// <summary> 将若干协程打包成组加入迭代器程序缓存列表 </summary>
    /// <param><c>overCall</c> 协程结束事件 </param>
    /// <param><c>IEs</c> 要打包的协程 </param>
    public void AddGroup(UnityAction overCall, params IEnumerator[] IEs)
    {
        if (tabList == null)
            tabList = new List<IEnumerator>();
        tabList.Add(IEGroup(overCall, IEs));
    }
    /// <summary> 将若干协程打包成队列加入迭代器程序缓存列表 </summary>
    /// <param><c>overCall</c> 协程结束事件 </param>
    /// <param><c>IEs</c> 要打包的协程 </param>
    public void AddQueue(UnityAction overCall, params IEnumerator[] IEs)
    {
        if (tabList == null)
            tabList = new List<IEnumerator>();
        tabList.Add(IEQueue(overCall, IEs));
    }

    /// <summary> 将所有缓存的协程以队列的形式打包成协程 </summary>
    /// <param><c>overCall</c> 协程结束事件 </param>
    /// <param><c>IEs</c> 要打包的协程 </param>
    public IEnumerator QueuePack(UnityAction overCall = null)
    {
        if (tabList == null)
            yield break;
        IEnumerator[] tmp = new IEnumerator[tabList.Count];
        for (int i = 0; i < tabList.Count; i++)
        {
            tmp[i] = tabList[i];
        }
        yield return IEQueue(overCall, tmp);
    }
    /// <summary> 将所有缓存的协程以组的形式打包成协程 </summary>
    /// <param><c>overCall</c> 协程结束事件 </param>
    /// <param><c>IEs</c> 要打包的协程 </param>
    public IEnumerator GroupPack(UnityAction overCall = null)
    {

        if (tabList == null)
            yield break;
        IEnumerator[] tmp = new IEnumerator[tabList.Count];
        for (int i = 0; i < tabList.Count; i++)
        {
            tmp[i] = tabList[i];
        }
        yield return IEGroup(overCall, tmp);
    }

    #endregion

    /// <summary> 将若干协程以组的形式打包成协程 </summary>
    /// <param><c>overCall</c> 协程结束事件 </param>
    /// <param><c>IEs</c> 要打包的协程 </param>
    public IEnumerator IEGroup(UnityAction overCall = null, params IEnumerator[] IEs)
    {
        SignalBox box = new SignalBox();
        for (int i = 0; i < IEs.Length; i++)
        {
            StartCoroutine(IEQueueAdapt(box, IEs[i]));
        }
        float t = 0;
        while (t < 10)
        {
            t += Time.deltaTime;
            if (box.x >= IEs.Length)
            {
                break;
            }
            yield return 0;
        }
        overCall?.Invoke();
    }
    /// <summary> 将若干协程以队列的形式打包成协程 </summary>
    /// <param><c>overCall</c> 协程结束事件 </param>
    /// <param><c>IEs</c> 要打包的协程 </param>
    public IEnumerator IEQueue(UnityAction overCall = null, params IEnumerator[] IEs)
    {
        for (int i = 0; i < IEs.Length; i++)
        {
            yield return IEs[i];
        }
        overCall?.Invoke();
    }
    //用于group打包queue时的适配
    IEnumerator IEQueueAdapt(SignalBox box, IEnumerator queue)
    {
        yield return queue;
        box.x++;
    }
    public Coroutine ActiveCor(IEnumerator IE)
    {
        return StartCoroutine(IE);
    }
    //用于父协程监听子协程的单一信号量
    private class SignalBox
    {
        public int x;
        public SignalBox()
        {
            x = 0;
        }
    }

}
