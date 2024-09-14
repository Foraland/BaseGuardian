//Event Center

using System;
using System.Collections.Generic;
public delegate void ECListener(string args);

public static class EC
{
	//======================= Event Key Const ===========================//
	/// <summary>
	/// param:float energy
	/// </summary>
	public const string PREVIEW_COST = "PreviewCost";
	public const string CANCEL_PREVIEW = "CancelPreview";
	public const string ERROR_COST = "ErrorCost";
	/// <summary>
	/// param:float energy
	/// </summary>
	public const string COST = "COST";
	public const string REFRESH = "REFRESH";


	//===================================================================//


	private static Object agent = new Object();

	private static Dictionary<string, Dictionary<object, List<ECListener>>> msgMap = new Dictionary<string, Dictionary<object, List<ECListener>>>();
	private static Dictionary<string, Dictionary<object, List<Action>>> actMap = new Dictionary<string, Dictionary<object, List<Action>>>();

	public static void On(this object obj, string key, ECListener evt)
	{
		if (msgMap.ContainsKey(key))
		{
			if (!msgMap[key].ContainsKey(obj))
			{
				msgMap[key].Add(obj, new List<ECListener> { evt });
				return;
			}
			if (!msgMap[key][obj].Contains(evt))
			{
				msgMap[key][obj].Add(evt);
				return;
			}
		}
		else
		{
			msgMap.Add(key, new Dictionary<object, List<ECListener>>
			{
				{obj,new List<ECListener>{evt}}
			});
		}
	}
	public static void On(string key, Action act)
	{
		agent.On(key, act);
	}
	public static void On(string key, ECListener evt)
	{
		agent.On(key, evt);
	}
	public static void On(this object obj, string key, Action act)
	{
		if (actMap.ContainsKey(key))
		{
			if (!actMap[key].ContainsKey(obj))
			{
				actMap[key].Add(obj, new List<Action> { act });
				return;
			}
			if (!actMap[key][obj].Contains(act))
			{
				actMap[key][obj].Add(act);
				return;
			}
		}
		else
		{
			actMap.Add(key, new Dictionary<object, List<Action>>
			{
				{obj,new List<Action>{act}}
			});
		}
	}

	public static void Off(this object obj, string key, ECListener evt)
	{
		if (msgMap.ContainsKey(key) && msgMap[key].ContainsKey(obj))
		{
			msgMap[key][obj].Remove(evt);
		}
	}
	public static void Off(this object obj, string key, Action act)
	{
		if (actMap.ContainsKey(key) && actMap[key].ContainsKey(obj))
		{
			actMap[key][obj].Remove(act);
		}
	}
	public static void Off(string key, ECListener evt)
	{
		agent.Off(key, evt);
	}
	public static void Off(string key, Action act)
	{
		agent.Off(key, act);
	}

	public static void Off(this object obj, string key)
	{
		if (msgMap.ContainsKey(key))
		{
			msgMap[key].Remove(obj);
		}
		if (actMap.ContainsKey(key))
		{
			actMap[key].Remove(obj);
		}
	}

	public static void Send(string key, string args = null)
	{
		if (args == null)
		{
			if (actMap.ContainsKey(key))
			{
				foreach (List<Action> list in actMap[key].Values)
				{
					list.ForEach(e => e.Invoke());
				}
			}
		}
		else
		{
			UnityEngine.Debug.Log("send");
			if (msgMap.ContainsKey(key))
			{
				foreach (List<ECListener> list in msgMap[key].Values)
				{
					list.ForEach(e => e.Invoke(args));
				}
			}
		}
	}

	public static void Send(this object obj, string key, string args = null)
	{
		if (msgMap.ContainsKey(key) && msgMap[key].ContainsKey(obj))
		{
			msgMap[key][obj].ForEach(e => e.Invoke(args));
		}
		if (actMap.ContainsKey(key) && args == null)
		{
			foreach (List<Action> list in actMap[key].Values)
			{
				list.ForEach(e => e.Invoke());
			}
		}
	}
}