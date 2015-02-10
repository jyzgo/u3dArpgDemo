using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;

using InJoy.DynamicContentPipeline;

public class AppManager : MonoBehaviour
{
    private const string LEVEL_TO_LOAD = "GameLaunch";

    public event Action Started;       //delegate to run when started

    private static AppManager _instance = null;	

    public static AppManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType(typeof(AppManager)) as AppManager;
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
	

    IEnumerator Start()
    {
        //save version at first start up
        if (!PlayerPrefs.HasKey("Version String"))
        {
            Debug.Log("Version String: " + AppVersion.GetCurrentVersionString());
            PlayerPrefs.SetString("Version String", AppVersion.GetCurrentVersionString());
        }

        if (!PlayerPrefs.HasKey("Building String"))
        {
            Debug.Log("Building String: " + AppVersion.GetCurrentBuildString());
            PlayerPrefs.SetString("Building String", AppVersion.GetCurrentBuildString());
        }
		
/*		
        Logging.GetLogger("App.Boot").SetLevel(Logging.DEBUG);
		Logging.GetLogger("App.FCDownloadManager").SetLevel(Logging.DEBUG);
        Logging.GetLogger("Package.DynamicContentPipeline").SetLevel(Logging.DEBUG);
        Logging.GetLogger("Package.ForcedUpdate").SetLevel(Logging.DEBUG);
        Logging.GetLogger("Package.ABTesting").SetLevel(Logging.DEBUG);
        Logging.GetLogger("Package.AssetBundles").SetLevel(Logging.DEBUG);
       	Logging.GetLogger("Package.LiveOpsManager").SetLevel(Logging.DEBUG);
*/
		
//        DynamicContent.OnForcedBinariesUpdate = delegate
//        {
//            Debug.Log("ForcedUpdate is required");
//            Application.OpenURL(ForcedUpdate.GetUpdateURL()); // opens page on AppStore
//        };
//		DynamicContent.OnNoNeedBinariesUpdate = delegate
//		{
//			Debug.Log("No Forced update");
//			FCDownloadManager.Instance.UpdateAllIndexDownloaded();
//		};
//		FCDownloadManager.Instance.OnDownloadSuccess = null;
		FCDownloadManager.Instance.OnDownloadSuccess = delegate(FCDownloadManager.FCIndexDownloadInfo indexInfo) 
		{
			Debug.Log(indexInfo.IndexDownloadName+" is load success!");
		};
		FCDownloadManager.Instance.OnStartNewDownload = null;
		
		FCDownloadManager.Instance.OnAllDownloadFinished = delegate 
		{
			Debug.Log("Loaded successfully");

			Assertion.Check(Application.CanStreamedLevelBeLoaded(LEVEL_TO_LOAD));
#if DEVELOPMENT_BUILD || UNITY_EDITOR
			if(!CheatManager.needCheckSplashText)
			{
				AppLoading.Instance.LoadingStep = APP_LOADING_STEP.SCENE_GAMELUNCH;
				//FCDownloadManager.Instance.IsBackgroundMode = true;
        		Application.LoadLevel(LEVEL_TO_LOAD);
			}
#endif
		};
		
		FCDownloadManager.Instance.OnDownloadFailed = delegate(FCDownloadManager.FCIndexDownloadInfo indexInfo) 
		{
			FCDownloadManager.Instance.RemoveAllDownloads(FCDownloadManager.DownloadType.DT_Foreground);
		};
		
		FCDownloadManager.Instance.OnForceUpdate = delegate(string URL) 
		{
		};
		
		FCDownloadManager.Instance.OnServerCheckFailed = delegate
		{
			Debug.LogError("FCDownloadManager.Instance.OnServerCheckFailed");

		};
		
		FCDownloadManager.Instance.OnServerMaintaining = delegate(string maintainMessage)
		{
		};
		
		FCDownloadManager.Instance.OnServerAvailable = delegate 
		{
			Debug.Log("FCDownloadManager.Instance.OnServerAvailable");
			AppLoading.Instance.LoadingStep = APP_LOADING_STEP.BUNDLE_CHECK;
			FCDownloadManager.Instance.Init();
			FCDownloadManager.Instance.CheckDynamicContentInfo();
			FCDownloadManager.Instance.UpdateAllIndexDownloaded();
		};

       if (Started != null)
        {
            Started();
        }

        yield return null;
		if(NetworkManager.isOfflinePlay){
			
			AppLoading.Instance.LoadingStep = APP_LOADING_STEP.SCENE_GAMELUNCH;
			Application.LoadLevel("GameLaunch");
		}else{

			FCDownloadManager.Instance.CheckServerPost();
		}
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Escape)) // e.g. Android "back" key
        {
            Application.Quit();
        }
    }


	public void OnFailDialogCall(string index)
	{
		FCDownloadManager.Instance.CheckDynamicContentInfo();
		FCDownloadManager.Instance.UpdateAllIndexDownloaded();
	}
	
	public void OnServerPostFailedDialog(string index)
	{
		FCDownloadManager.Instance.CheckServerPost();
	}
	
	public void OnServerPostMaintainWebView(string message)
	{
		string commandName = message.Split(new char[]{'='})[0];
		string commandValue = message.Split(new char[]{'='})[1];
		if(commandName == "Close")
		{
			Application.Quit();
		}
		else if(commandName == "OpenURL")
		{
			Application.OpenURL("http://"+commandValue);
		}
	}


	public void OnServerBlockerWebView(string message)
	{
		string commandName = message.Split(new char[]{'='})[0];
		if(commandName == "Close")
		{
			Application.Quit();
		}
	}
}
