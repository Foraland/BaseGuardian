using System;
using UnityEngine;

public class Singleton<T> where T : new()
{
    private static T instance;
    public static T Ins
    {
        get
        {
            if (instance == null)
                instance = new T();
            return instance;
        }
    }

}
