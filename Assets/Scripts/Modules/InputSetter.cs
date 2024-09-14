using System.Collections;
using System.Collections.Generic;
using UnityEngine;
///<summary> Input Setter </summary>
// 控件库，将要用到的控件KeyCode作为初始化参数构造KeyCtrl即可
public static class IS  
{
    private static KeyCtrl[] move = new KeyCtrl[4]{
        new KeyCtrl(KeyCode.W),
        new KeyCtrl(KeyCode.D),
        new KeyCtrl(KeyCode.S),
        new KeyCtrl(KeyCode.A)
    };
    private static KeyCtrl jump=new KeyCtrl(KeyCode.Space);
    public static KeyCtrl W{
        get=>move[0];
    }
    public static KeyCtrl A{
        get=>move[1];
    }
    public static KeyCtrl S{
        get=>move[2];
    }
    public static KeyCtrl D{
        get=>move[3];
    }
    public static KeyCtrl Jump{
        get=>jump;
    }
}
public enum KeyState{
    Key,
    Up,
    Down
}
public class KeyCtrl{
    public KeyCtrl(KeyCode k)
    {
        code = k;
    }
    private KeyCode code;
    public bool up
    {
        get
        {
            if(Time.time>validTime)
                getUp=false;
            bool res = Input.GetKeyUp(code) || getUp;
            // getUp = false;
            return res;
        }
    }
    public bool down
    {
        get { 
            if(Time.time>validTime)
                getDown=false;
            bool res =Input.GetKeyDown(code) || getDown; 
            // getDown=false;
            return res;
        }
    }
    public bool key
    {
        get
        {
            if (keyTimer && Time.time > keyValidTime)
            {
                getk = false;
                keyTimer = false;
            }
            bool res = Input.GetKey(code) || getk;
            return res;
        }
    }
    private bool getk;
    private bool getDown;
    private bool getUp;
    private float validTime;
    private float keyValidTime;
    private bool keyTimer;
    public float intervalTime = 0.2f;
    public void Press()
    {
        getDown = getk = true;
        validTime = Time.time + intervalTime;
    }
    public void Release()
    {
        getDown = getk = false;
        getUp=true;
        validTime = Time.time + intervalTime;
    }
    public void Click(float interval=0.2f)
    {
        getDown = getk = true;
        keyTimer=true;
        keyValidTime=Time.time+interval;
        validTime=Time.time+intervalTime;
    }
    public static implicit operator bool(KeyCtrl c){
        return c.key||c.down;
    }
}
