using UnityEngine;
using System.Collections;

public class MessageBoxCamera : MonoBehaviour
{
	public Camera uiCamera;

	private static MessageBoxCamera _instance;

	public static MessageBoxCamera Instance
	{
		get
		{
			return _instance;
		}
	}

	void Awake()
	{
		_instance = this;
		DontDestroyOnLoad(this);
	}

	void OnDestroy()
	{
		_instance = null;
	}
}
