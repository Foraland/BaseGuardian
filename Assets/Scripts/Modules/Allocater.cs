using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Allocater
{
    static int maxCnt =0;
    static Queue<int> idPool = new Queue<int>();
    public static int GetID(){
        if(idPool.Count==0)
            return maxCnt++;
        else
            return idPool.Dequeue();
    }
    public static void PushID(int id){
        idPool.Enqueue(id);
    }
}
