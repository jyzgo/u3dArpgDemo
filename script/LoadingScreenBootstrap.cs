using UnityEngine;
using System.Collections;

public class LoadingScreenBootstrap : MonoBehaviour 
{
	public GameObject _loadingScreenPrefab;
	GameObject _loadingScreen;
	
	LoadingScreen _loadingScreenScript;
	public LoadingScreen LoadingScreen
	{
		get
		{
			return _loadingScreenScript;
		}
	}
	
	#region Singleton
	private static LoadingScreenBootstrap _instance = null;
	public static LoadingScreenBootstrap Instance
	{
		get
		{	
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
	
	#endregion
	
	void Awake() 
	{
		_instance = this;
	}
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public void Initialize()
	{
		_loadingScreen = NGUITools.AddChild(gameObject, _loadingScreenPrefab);
		_loadingScreenScript = _loadingScreen.GetComponent<LoadingScreen>();
		
		_loadingScreenScript.StartProgress();
	}
	
	public void Clean()
	{		
		NGUITools.Destroy(_loadingScreen);
	}
}
