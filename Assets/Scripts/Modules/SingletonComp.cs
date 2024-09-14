using System;
using UnityEngine;

public class SingletonComp<T> : MonoBehaviour where T : SingletonComp<T>
{
	public static T Ins
	{
		get
		{
			return instance;
		}
	}

	// public Singleton() : base()
	// {
	// 	//FORALAND 单例问题
	// 	// if (Singleton<T>.instance == null)
	// 	// {
	// 	Singleton<T>.instance = (this as T);
	// 	return;
	// 	// }
	// 	// if (Singleton<T>.instance != this)
	// 	// {
	// 	// 	UnityEngine.Object.DestroyImmediate(this);
	// 	// }
	// }
	private void Awake()
	{
		instance = this as T;
	}

	private static T instance;
}
