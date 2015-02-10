using UnityEngine;

using System;
using System.Collections;

public class DebugManager : MonoBehaviour
{
	private static DebugManager _instance;

	public static DebugManager Instance
	{
		get { return _instance; }
	}

	private FpsCounter _fpsCounter;

	public FpsCounter fpsCounter
	{
		get
		{
			if (_fpsCounter == null)
			{
				_fpsCounter = this.GetComponent<FpsCounter>();
			}
			return _fpsCounter;
		}
	}

	void Awake()
	{
		_instance = this;
	}

	void OnDestroy()
	{
		_instance = null;
	}
}
