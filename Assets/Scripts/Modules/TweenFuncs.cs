using System;
using UnityEngine;
public static class TweenFuncs
{
    public static float SineIn(float x)
    {
        return 1f - Mathf.Cos(x * Mathf.PI / 2f);
    }
    public static float SineOut(float x)
    {
        return Mathf.Sin(x * Mathf.PI / 2);
    }
    public static float QuadOut(float x)
    {
        return 1 - (1 - x) * (1 - x);
    }
}