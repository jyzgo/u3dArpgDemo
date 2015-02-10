using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using InJoy;
using InJoy.FCComm;
using InJoy.Utils;

[System.Serializable]
public class PluginManager : MonoBehaviour
{
	private static PluginManager _instance = null;
	public static PluginManager Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = FindObjectOfType(typeof(PluginManager)) as PluginManager;
			}

			return _instance;
		}
	}
	
	void OnDestroy()
	{
		if(_instance == this)
		{
			_instance = null;
		}
	}	
	
	void Awake()
	{

	}
	
	void Start()
	{
	}
	
	void OnApplicationPause(bool pause)
	{
		if(pause)
		{
		}
		else
		{
		}
	}
	
	void Update()
	{

	}
	
}
