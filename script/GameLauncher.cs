using UnityEngine;

using System;
using System.Collections;

// http://www.cnblogs.com/kuangwu/archive/2013/06/14/3135994.html
[System.Serializable]
public class SingletonManager
{
	public string prefabName;
	public string prefabPath;
	[HideInInspector]
	public GameObject instance;
}

public class GameLauncher : MonoBehaviour
{
	public GameObject debugManagerPrefab;

	private static GameLauncher _inst;

	void Awake()
	{
		if (_inst != null)
		{
			Debug.LogError("GameLauncher: Another GameLauncher has already been created previously. " + gameObject.name + " is goning to be destroyed.");
			Destroy(this);
			return;
		}
		_inst = this;
//		当加载一个新关卡时，所有场景中所有的物体被销毁，然后新关卡中的物体被加载进来。为了保持在加载新关卡时物体不被销毁，使用DontDestroyOnLoad保持，如果物体是一个组件或游戏物体，它的整个transform层次将不会被销毁，全部保留下来
		DontDestroyOnLoad(this.gameObject);
	}

	public SingletonManager[] managers;

	private int _managerCount = 0;

	// Use this for initialization
	void Start()
	{
		if (managers.Length == 0)
		{
			Debug.LogWarning("GameLauncher: no managers to create");
		}
#if DEVELOPMENT_BUILD || UNITY_EDITOR
		Utils.InstantiateGameObjectWithParent(debugManagerPrefab, this.transform);
#endif

		Localization.instance.transform.parent = this.transform;
		FCDownloadManager.Instance.transform.parent = this.transform;

	}

	// Update is called once per frame
	void Update()
	{
		if (_managerCount < managers.Length)
		{
			GameObject go = InJoy.AssetBundles.AssetBundles.Load(managers[_managerCount].prefabPath, typeof(GameObject)) as GameObject;
			Transform t = Utils.InstantiateGameObjectWithParent(go, this.transform);
			t.name = managers[_managerCount].prefabName;
			_managerCount++;
		}
		else
		{
			Localization.instance.Mode = Localization.EnumResourcePath.language_bundle;
			Localization.instance.Reset();
			AppLoading.Instance.LoadingStep = APP_LOADING_STEP.SCENE_SERVER;
			Application.LoadLevel("login");
			this.enabled = false;
		}
	}

	void OnStartNewDownload(FCDownloadManager.FCIndexDownloadInfo indexInfo)
	{
		Debug.Log("index start download:" + indexInfo.IndexDownloadName);
	}

	void OnDownloadSuccess(FCDownloadManager.FCIndexDownloadInfo indexInfo)
	{
		Debug.Log("index succeeded to download:" + indexInfo.IndexDownloadName);
	}

	void OnDownloadFail(FCDownloadManager.FCIndexDownloadInfo indexInfo)
	{
		Debug.Log("index failed to download:" + indexInfo.IndexDownloadName);
	}

	void OnAllDownloadFinished()
	{
		Debug.Log("All download finished.");
	}

	void OnDestroy()
	{
		if (_inst == this)
		{
			_inst = null;
		}
	}
}
