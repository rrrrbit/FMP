using System;
using System.Collections.Generic;
using UnityEngine;

public class Managers : MonoBehaviour
{
    public static Managers Instance { get; private set; }
	Dictionary<Type, object> managers = new();
	private void Awake()
	{
		DontDestroyOnLoad(gameObject);
		Instance = this;

		Register<MGR_gameMaths>(GetComponent<MGR_gameMaths>());
		Register<MGR_graphView>(GetComponent<MGR_graphView>());
		Register<MGR_input>(GetComponent<MGR_input>());
		Register<MGR_levelUI>(GetComponent<MGR_levelUI>());
	}
	
	void Register<T>(T impl)
	{
		managers[typeof(T)] = impl;
	}

	public static T Get<T>() 
	{
		if (Instance.managers.TryGetValue(typeof(T), out var impl))
		{
			return (T)impl;
		}
		else
		{
			throw new Exception($"Manager of type {typeof(T)} not found");

		}

	}
}
