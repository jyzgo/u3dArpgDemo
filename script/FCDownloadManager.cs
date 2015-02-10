using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using InJoy.DynamicContentPipeline;
using InJoy.AssetBundles;


public class ServerInfo
{
    public short id;
    public string name;
    public ServerState state;

    public static int CompareServerFromMaxToMin(ServerInfo info1, ServerInfo info2)
    {
        if (null == info1)
        {
            if (null == info2)
            {
                return 0;
            }

            return -1;
        }

        if (null == info2)
        {
            return 1;
        }

        int value = info2.id.CompareTo(info1.id);

        return value;
    }
}


public class FCDownloadManager : MonoBehaviour
{
	public const string URL_SERVER_ROOT_DEV = "http://192.168.1.201/fc";
	public const string URL_SERVER_ROOT_TEST = "http://115.28.244.246/fc";		//for test
	public const string URL_SERVER_ROOT_LIVE = "http://not_ready_live";		//for product
	public const string URL_BUNDLE_BASE_ADDRESS = "<baseurl>";

    public static string URL_SERVER_ROOT = null;
	public static string URL_DOWNLOAD_SERVER = null;
	public static string URL_ZONE_SERVER = null;

    //public static string[] GameServers;	//game servers available, filled after reading server post
    public List<ServerInfo> GameServers = new List<ServerInfo>();

    public ServerInfo GetServerInfoById(int serverID)
    {
        return GameServers.Find(delegate(ServerInfo info) { return info.id == serverID; });
    }

	public enum ServerPostType
	{
		SPT_DEV,
		SPT_TEST,
		SPT_PRODUCT,
	}

	public static ServerPostType CurServerPostType
	{set;get;}

	#region UploadAddress

	public static string UploadServerpostName
	{
		get
		{
			return string.Format("serverpost_{0}.json", InJoy.UnityBuildSystem.BuildInfo.ServerPostTag);
		}
	}

	public static string UploadServerPlatform
	{
		get
		{
#if UNITY_IPHONE
			return "ios";
#elif UNITY_ANDROID
			return "android";
#else
			return "none";
#endif
		}
	}

	public static string UploadLocalS3BuildJSONAddress
	{
		get
		{
			return URL_SERVER_ROOT + "/" + UploadServerPlatform + "/" + DynamicContent.Impl.Config + "/bundles" + "/" + DynamicContent.Impl.BuildTag;
		}
	}

	public static string UploadLocalS3ServerpostJSONAddress
	{
		get
		{
			return URL_SERVER_ROOT + "/" + UploadServerPlatform + "/" + DynamicContent.Impl.Config + "/";
		}
	}
	
	public static string UploadLocalS3BuildBundleTagAddress
	{
		get
		{
			return URL_SERVER_ROOT + "/" + UploadServerPlatform + "/" + DynamicContent.Impl.Config + "/bundles" + "/" + DynamicContent.Impl.BuildTag + "/AssetBundles/"+DynamicContent.Impl.AssetBundleUploadTag;
		}
	}

	public static string UploadLocalDynamicInfoURLPrefix
	{
		get
		{
			return FCDownloadManager.URL_BUNDLE_BASE_ADDRESS + "/" + UploadServerPlatform + "/" + DynamicContent.Impl.Config + "/bundles" + "/" + DynamicContent.Impl.BuildTag + "/AssetBundles/"+DynamicContent.Impl.AssetBundleUploadTag;
		}
	}
	#endregion

	public static string DynamicContentInfoAddress
	{
		get
		{
			return URL_SERVER_ROOT+"/"+ServerPlatform+"/"+DynamicContent.Impl.Config+"/bundles/"+DynamicContent.Impl.BuildTag+"/DynamicContentInfo.json";
		}
	}

	public string ServerPostAddressTest
	{
		get
		{
			return URL_SERVER_ROOT_TEST + "/" + ServerPlatform + "/" + DynamicContent.Impl.Config + "/serverpost_" + InJoy.UnityBuildSystem.BuildInfo.ServerPostTag + ".json";
		}
	}

	public string ServerPostAddressLive
	{
		get
		{
			return URL_SERVER_ROOT_LIVE + "/" + ServerPlatform + "/" + DynamicContent.Impl.Config + "/serverpost_live_" + DynamicContent.Impl.BuildTag +".json";
		}
	}
	
	public string ServerPostAddressLocal
	{
		get
		{
			return URL_SERVER_ROOT_DEV + "/" + ServerPlatform + "/" + DynamicContent.Impl.Config + "/serverpost_" + InJoy.UnityBuildSystem.BuildInfo.ServerPostTag + ".json";
		}
	}
	public static string ServerWebTextureAddress
	{
		get
		{
			return URL_SERVER_ROOT + "/" + DynamicContent.Impl.Platform + "/" + DynamicContent.Impl.Config + "/html/shopBanner";
		}
	}

	public static string CheaterHtmlAddress
	{
		get
		{
			return URL_SERVER_ROOT + "/" + DynamicContent.Impl.Platform + "/" + DynamicContent.Impl.Config + "/html/cheater";
		}
	}

	
	public static string ServerMusicAddress
    {
        get
        {
            return URL_SERVER_ROOT + "/music";
            //return URL_SERVER_ROOT + "/" + DynamicContent.Impl.Platform + "/" + DynamicContent.Impl.Config + "/music";
        }
    }

	private const string BackgroundStorageKey = "BACKGROUND_INDEX_NAMES";
	private const string ServerSelectionKey = "ServerSelection";
	private const string IsServerSelectedKey = "IsServerSelected";
	private const string IsNeedTestServerBlockKey = "IsFirstBlood";
	private const float ServerPostTimeOut = 5f;//in seconds
	private const float MaxPercentage = 100;
	
	private List<FCIndexDownloadInfo> downloadedIndexList = new List<FCIndexDownloadInfo>();
	
	private DynamicContentInfo DCInfoCache = null;

	public enum IndexCheckResult
	{
		ICR_AllFinished,
		ICR_Downloading,
		ICR_WaitingForDownload,
		ICR_NotInList,
	};
	
	private string[] baseIndexList = 
	{
		"index.version",
	};
	
	
	#region Singleton
	private static FCDownloadManager _instance = null;
	public static FCDownloadManager Instance
	{
		get
		{
			return _instance;
		}
	}
	
	void Awake() {
		if(_instance != null)
		{
            Debug.LogError("FCDownloadManager: Another FCDownloadManager has already been created previously. " + gameObject.name + " is goning to be destroyed.");
			Destroy(this);
			return;
		}
		_instance = this;
		DontDestroyOnLoad(this);
	}
	
	void OnDestroy()
	{
		if(_instance == this)
		{
			_instance = null;
		}
	}
	
//	public bool IsBackgroundMode
//	{set;get;}
	
	public bool IsServerBlock
	{set;get;}
	
	public static void SaveServerSelected(bool isSelected)
	{
		PlayerPrefs.SetString(IsServerSelectedKey, isSelected ? "yes":"no");
		PlayerPrefs.Save();
	}

	public static string ServerPlatform
	{
		get
		{
			return "ios";
		}
	}

	public static bool IsServerSelected
	{
		get
		{
			if(!PlayerPrefs.HasKey(IsServerSelectedKey))
			{
				return false;
			}
			return PlayerPrefs.GetString(IsServerSelectedKey) == "yes";
		}
	}
	#endregion
	
	public enum DownloadType
	{
		DT_Foreground,
		DT_Background,
	}
	
	public class FCIndexDownloadInfo
	{
		public FCIndexDownloadInfo(string name, DownloadType type)
		{
			IndexDownloadType = type;
			IndexDownloadName = name;
			IndexDownloadInfo = null;
		}
		
		public DownloadType IndexDownloadType
		{set;get;}
		
		public string IndexDownloadName
		{set;get;}
		
		public IndexInfo IndexDownloadInfo
		{set;get;}
	}
	
	public int CheckUpdateListCount
	{
		get
		{
			RefreshCheckIndexList();
			return checkUpdateList.Count;
		}
	}
	
	public List<string> ChecKUpdatekList
	{
		get
		{
			RefreshCheckIndexList();
			return checkUpdateList;
		}
	}
	
	public int DownloadingListCount
	{
		get
		{
			return downloadList.Count;
		}
	}
	
	public bool IsDownloadManagerInited
	{set;get;}
	
	public List<FCIndexDownloadInfo> DownloadedIndexList
	{
		get
		{
			return downloadedIndexList;
		}
	}
	
//	public string CurrrentAssetBundleTag
//	{
//		get
//		{
//			return DynamicContent.Impl.CurrentAssetBundleTag;
//		}
//	}
	
	public FCIndexDownloadInfo CurDownloadingInfo
	{
		get
		{return curDownloadingIndex;}
		set
		{curDownloadingIndex = value;}
	}
	
	public bool IsCurIndexDownloading
	{
		get
		{
			return DynamicContent.AssetBundlesIndexInfo != null && DynamicContent.AssetBundlesIndexInfo.assetBundleInfo.Length > 0;
		}
	}
	
	public int TotalAssetBundlesCountInCurIndex
	{
		get
		{
			return DynamicContent.AssetBundlesIndexInfo.assetBundleInfo.Length;	
		}
	}
	
	public string CurrentAssetBundleTag
	{
		get
		{
			if(DCInfoCache != null)
			{
				return DCInfoCache.AssetBundleTag;
			}
			return "";
		}
	}
	
/*	
	public int GetRestAssetBundlesCountInCurIndex
	{
		get
		{
			return DynamicContent.AssetBundlesIndexInfo.
		}
	}
*/
	public delegate void OnDownloadSuccessDelegate(FCIndexDownloadInfo indexInfo);
	public delegate void OnDownloadFailedDelegate(FCIndexDownloadInfo indexInfo);
	public delegate void OnStartNewDownloadDelegate(FCIndexDownloadInfo indexInfo);
	public delegate void OnAllDownloadFinishedDelegate();
	
	public delegate void OnForceUpdateDelegate(string URL);
	public delegate void OnServerMaintainingDelegate(string URL);
	public delegate void OnServerAvailableDelegate();
	public delegate void OnServerCheckFailedDelegate();
	public delegate void OnNoNeedForceUpdateDelegate();
	
	public OnDownloadSuccessDelegate OnDownloadSuccess = null;
	public OnDownloadFailedDelegate OnDownloadFailed = null;
	public OnStartNewDownloadDelegate OnStartNewDownload = null;
	public OnAllDownloadFinishedDelegate OnAllDownloadFinished = null;
	
	public OnForceUpdateDelegate OnForceUpdate = null;
	public OnServerMaintainingDelegate OnServerMaintaining = null;
	public OnServerAvailableDelegate OnServerAvailable = null;
	public OnServerCheckFailedDelegate OnServerCheckFailed = null;
	public OnNoNeedForceUpdateDelegate OnNoNeedForceUpdate = null;
	
	List<FCIndexDownloadInfo> downloadList = new List<FCIndexDownloadInfo>();
	List<string> checkUpdateList = new List<string>();
	FCIndexDownloadInfo curDownloadingIndex = null;
	
	public void Init()
	{
		DynamicContent.Init(URL_SERVER_ROOT);
		//DynamicContent.UseCloudFrontAddress(URL_BUNDLE_BASE_ADDRESS, URL_SERVER_ROOT);
		//IsBackgroundMode = false;
		
		downloadList.Clear();
		checkUpdateList.Clear();
		
		if(PlayerPrefs.HasKey("ForceCleanCache"))
		{
			CleanIndexStorageList();
			Caching.CleanCache();
			PlayerPrefs.DeleteKey("ForceCleanCache");
			PlayerPrefs.Save();
		}

		IsDownloadManagerInited = true;
	}
	
	public bool HasFatalError()
	{
		return PlayerPrefs.HasKey("ForceCleanCache");
	}
	
	// Use this for initialization
	void Start () 
	{
#if UNITY_EDITOR
		//filled with dev servers, for starting from Boot scene
        //GameServers = new string[]
        //{
        //    "1", "Default server", "idle"
        //};
#endif
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(!IsDownloadManagerInited || DCInfoCache == null || !DCInfoCache.Ready)
		{
			return;
		}
		
		if(curDownloadingIndex == null && downloadList.Count > 0)
		{
			curDownloadingIndex = downloadList[0];
			StartDownloadIndex(curDownloadingIndex);
		}
	}
	
	public void RefreshCheckIndexList()
	{
		checkUpdateList.Clear();
		checkUpdateList.AddRange(baseIndexList);
		string[] expandIndexList = GetIndexFromStorageList();
		if(expandIndexList != null )
		{
			checkUpdateList.AddRange(expandIndexList);
		}
	}
	
	public void CheckDynamicContentInfo()
	{	
		if(DCInfoCache == null)
		{
			Debug.Log("Check Dynamic Content Info URL is " + DynamicContentInfoAddress);
			DCInfoCache = new DynamicContentInfo(DynamicContentInfoAddress);
			DCInfoCache.OnDCInfoFailed  = delegate() 
			{
				if(OnServerCheckFailed != null) OnServerCheckFailed();
			};
		}
		StartCoroutine(DCInfoCache.StartDownloadContent());
	}
	
	public void UpdateAllIndexDownloaded()
	{
		RefreshCheckIndexList();
		AddDownloadIndex(checkUpdateList.ToArray(), DownloadType.DT_Foreground);
	}
	
	public void AddDownloadIndex(string indexNames, DownloadType type)
	{
		string[] newIndexNames = indexNames.Split( new char[]{','});
		AddDownloadIndex(newIndexNames, type);
	}
	
	public void RemoveDownloadIndex(string indexNames, DownloadType type)
	{
		FCIndexDownloadInfo targetDownloadInfo = downloadList.Find(delegate(FCIndexDownloadInfo obj) 
		{
			if(indexNames == obj.IndexDownloadName && type == obj.IndexDownloadType)
			{
				return true;
			}
			else
			{
				return false;
			}
		});
		
		if(targetDownloadInfo != null)
		{
			downloadList.Remove(targetDownloadInfo);
		}
		
		if (curDownloadingIndex != null 
		&& curDownloadingIndex.IndexDownloadName == indexNames 
		&& curDownloadingIndex.IndexDownloadType == type)
		{
			curDownloadingIndex = null;
		}
	}
	
	public void AddDownloadIndex(string[] indexNames, DownloadType type)
	{
		foreach(string targetIndexName in indexNames)
		{
			if(!downloadList.Exists(delegate(FCIndexDownloadInfo obj) {
				return obj.IndexDownloadName == targetIndexName;
			}))
			{
				FCIndexDownloadInfo newInfo = new FCIndexDownloadInfo(targetIndexName, type);
				downloadList.Add(newInfo);
				Debug.Log("FCdonwload mananger add new index: " + newInfo.IndexDownloadName);
			}
		}
	}
	
	public bool IsIndexDownloaded(string indexName)
	{
		RefreshCheckIndexList();
		return checkUpdateList.Contains(indexName);
	}
	
	public bool IsIndexDownloading(string indexName)
	{
		return downloadList.Exists(delegate(FCIndexDownloadInfo obj) 
		{
			return obj.IndexDownloadName == indexName;
		});
	}
	
	private void StartDownloadIndex(FCIndexDownloadInfo info)
	{
		DynamicContent.OnSuccess = delegate 
		{
			if(info.IndexDownloadType == DownloadType.DT_Background)
			{
				AddIndexIntoStorageList(info.IndexDownloadName);
			}
			if(OnDownloadSuccess != null)
			{
				OnDownloadSuccess(curDownloadingIndex);
			}
			if(!downloadedIndexList.Contains(info))
			{
				downloadedIndexList.Add(info);
			}
			curDownloadingIndex = null;
			downloadList.RemoveAt(0);
			
			if(downloadList.Count == 0 && OnAllDownloadFinished != null)
			{
				
				OnAllDownloadFinished();
			}
		};
		
		DynamicContent.OnFail = delegate {
			if(info.IndexDownloadType == DownloadType.DT_Background)
			{
			}
			if(OnDownloadFailed != null)
			{
				OnDownloadFailed(curDownloadingIndex);
			}
		};
		if(OnStartNewDownload != null)
		{
			OnStartNewDownload(curDownloadingIndex);
		}
		//DynamicContent.Init(URL_TO_AMAZON_S3);
		DynamicContentParam param = new DynamicContentParam();
		param.SpecialIndexName = curDownloadingIndex.IndexDownloadName;
		param.IsAddonContent = false;
		param.DCInfoCache = DCInfoCache;
		param.TargetIndexDownloadInfo = curDownloadingIndex;
        DynamicContent.StartContentUpdate(param);
	}
	
	public void RemoveAllDownloads(DownloadType type)
	{
		downloadList.RemoveAll(delegate(FCIndexDownloadInfo obj) 
		{
			return obj.IndexDownloadType == type;	
		});
		curDownloadingIndex = null;
	}
	
	public void RestartAllDownloads()
	{
		curDownloadingIndex = null;
	}
	
	public void CleanIndexStorageList()
	{
		checkUpdateList.Clear();
		PlayerPrefs.DeleteKey(BackgroundStorageKey);
		PlayerPrefs.Save();
	}
	
	public void AddIndexIntoStorageList(string indexName)
	{
		RefreshCheckIndexList();
		if(checkUpdateList.Contains(indexName))
		{
			return;
		}

		if(PlayerPrefs.HasKey(BackgroundStorageKey))
		{
			PlayerPrefs.SetString(BackgroundStorageKey, PlayerPrefs.GetString(BackgroundStorageKey) + ","+indexName);
		}
		else
		{
			PlayerPrefs.SetString(BackgroundStorageKey, indexName);
		}
		PlayerPrefs.Save();
	}
	
	private string[] GetIndexFromStorageList()
	{
		if(PlayerPrefs.HasKey(BackgroundStorageKey))
		{
			return PlayerPrefs.GetString(BackgroundStorageKey).Split(new char[]{','});
		}
		else
		{
			return null;
		}
	}
	
	public bool CurIndexDownloadingSize(out float totalSize, out float receivingProgress)
	{
		totalSize = 0;
		receivingProgress = 0;
		if (CurDownloadingInfo == null || CurDownloadingInfo.IndexDownloadInfo == null)
		{
			return false;
		}
		foreach(AssetBundleInfo curInfo in CurDownloadingInfo.IndexDownloadInfo.assetBundleInfo)
		{
			totalSize += curInfo.Size;
			receivingProgress += curInfo.downloadInfo.ReceivingProgress * curInfo.Size;
		}
		return true;
	}
	
	public bool CurBundleDownloadingSize(out float totalSize, out float receivingProgress)
	{
		totalSize = -1;
		receivingProgress = -1;
		if (CurDownloadingInfo == null || CurDownloadingInfo.IndexDownloadInfo == null)
		{
			return false;
		}
		foreach( AssetBundleInfo curInfo in CurDownloadingInfo.IndexDownloadInfo.assetBundleInfo)
		{
			if(curInfo.downloadInfo.state == DownloadManager.Info.State.Receiving 
			&& curInfo.downloadInfo.CurDownloadSource == DownloadManager.Download.DownloadSource.Online)
			{
				totalSize = curInfo.Size;
				receivingProgress = curInfo.downloadInfo.ReceivingProgress * curInfo.Size;
				return true;
			}
		}
		return false;
	}
	
	public float UpdateDownloadingProgress(string[] indexNames, bool isCheckCacheLoad)
	{
		List<FCIndexDownloadInfo> targetList = new List<FCIndexDownloadInfo>();
		List<FCIndexDownloadInfo> downloadedList = new List<FCIndexDownloadInfo>();
		
		foreach(string curIndexName in indexNames)
		{
			FCIndexDownloadInfo info = downloadList.Find(delegate(FCIndexDownloadInfo obj) 
			{
				return obj.IndexDownloadName == curIndexName;
			});
			if(info != null)
			{
				targetList.Add(info);
				continue;
			}
			
			info = downloadedIndexList.Find(delegate(FCIndexDownloadInfo obj) 
			{
				return obj.IndexDownloadName == curIndexName;
			});
			if(info != null)
			{
				downloadedList.Add(info);
				continue;
			}
		}
		
		int indexFinished = downloadedList.Count;//indexNames.Length - targetList.Count;
		
		float percentagePerIndex = MaxPercentage / indexNames.Length;
		float totalPercentage = indexFinished * percentagePerIndex;
		foreach(FCIndexDownloadInfo curIndexInfo in targetList)
		{
			float totalAssetBundleFinished = 0.0f;
			if(curIndexInfo.IndexDownloadInfo == null || curIndexInfo.IndexDownloadInfo.assetBundleInfo.Length == 0)
			{
				continue;
			}
			foreach(InJoy.AssetBundles.AssetBundleInfo curBundleInfo in curIndexInfo.IndexDownloadInfo.assetBundleInfo)
			{
				if(!isCheckCacheLoad)
				{
					if(curBundleInfo.downloadInfo.CurDownloadSource == DownloadManager.Download.DownloadSource.Online)
					{
						totalAssetBundleFinished += curBundleInfo.downloadInfo.ReceivingProgress;
					}
				}
				else
				{
					totalAssetBundleFinished += curBundleInfo.downloadInfo.ReceivingProgress;
				}
			}
			float indexFinishedPercentage = totalAssetBundleFinished / curIndexInfo.IndexDownloadInfo.assetBundleInfo.Length;
			totalPercentage += percentagePerIndex * indexFinishedPercentage;
		}
		//Debug.Log("Download Index Count : " + indexNames.Length);
		//Debug.Log("Download Percentage : " + totalPercentage);
		return totalPercentage;
	}
	
	public IndexCheckResult CheckIndexState(string[] indexNames)
	{
		bool isAllDownloaded = true;
		bool isAnyDownloading = false;
		bool isCurDownloading = false;
		foreach(string curIndexName in indexNames)
		{
			if(!IsIndexDownloaded(curIndexName))
			{
				isAllDownloaded = false;
			}
			
			if(IsIndexDownloading(curIndexName))
			{
				isAnyDownloading = true;
			}
			
			if(curDownloadingIndex != null && curDownloadingIndex.IndexDownloadName == curIndexName)
			{
				isCurDownloading = true;
			}
		}
		
		if(isAllDownloaded)
		{
			return IndexCheckResult.ICR_AllFinished;
		}
		
		if(isAnyDownloading && isCurDownloading)
		{
			return IndexCheckResult.ICR_Downloading;
		}
		else if(isAnyDownloading && !isCurDownloading)
		{
			return IndexCheckResult.ICR_WaitingForDownload;
		}
		
		return IndexCheckResult.ICR_NotInList;
	}
	
	#region Check is server available
	public void CheckServerPost()
	{
		StartCoroutine(DoGetServerPost());
	}
	
	private string ServerPostAddress()
	{
		Debug.Log("Current Server Post Type is " + CurServerPostType);
		switch(CurServerPostType)
		{
		case ServerPostType.SPT_PRODUCT:
			return ServerPostAddressLive;

		case ServerPostType.SPT_DEV:
			return ServerPostAddressLocal;

		case ServerPostType.SPT_TEST:
			return ServerPostAddressTest;
		}
		Assertion.Check(false, "Get ServerPostAddress Failed!");
		return "";
	}
	
	private IEnumerator DoGetServerPost()
	{
		//Debug.Log("Begin to check server post");

		string serverPostUrl = ServerPostAddress();
		Debug.Log("Server Post Address is " + serverPostUrl);

		WWW getPostWWW = new WWW(serverPostUrl);
		float time = 0;
		while(!getPostWWW.isDone && string.IsNullOrEmpty(getPostWWW.error))
		{
			time += Time.deltaTime;
			if(time > ServerPostTimeOut)
			{
				if(OnServerCheckFailed != null)
				{
					OnServerCheckFailed();
					yield break;
				}
			}
			else
			{
				yield return null;
			}
		}
		
		if(!string.IsNullOrEmpty(getPostWWW.error))
		{
			if(OnServerCheckFailed != null)
			{
				Debug.LogWarning(getPostWWW.error);
				OnServerCheckFailed();
			}
			yield break;
		}
		Debug.Log("Server post retrieved: " + getPostWWW.text);

		Hashtable serverPost = InJoy.Utils.FCJson.jsonDecode(getPostWWW.text) as Hashtable;

		bool isBlock = CheckBlockPolicy(serverPost["BlockerPolicy"] as string, serverPost["BlockerURLGroup"] as Hashtable);
		if(isBlock) yield break;
		
		CheckServerConfig(serverPost["ServerConfig"] as Hashtable);
		CheckSpecialOptions(serverPost["SpecialOptions"] as Hashtable);
		Check3rdKeys(serverPost["3rdKeys"] as Hashtable);

		if(CheckNeedForceUpdate(serverPost["ForceUpdate"] as Hashtable))
		{
			yield break;
		}
		
		CheckServerMaintain(serverPost["ServerMaintain"] as Hashtable);
	}

	private bool CheckBlockPolicy(string blockerPolicy, Hashtable blockerURLGroup)
	{
		if(!IsServerSelected)
		{
			//string blockerPolicy = (localConfig[countryCode] as Hashtable)["BlockerPolicy"] as string;
			if(blockerPolicy == "BlockJB" && Integrity.IsJailbroken())
			{
				Debug.Log(" Using Policy Block JB");
				SaveServerSelected(false);
				return true;
			}
			else if(blockerPolicy == "BlockAll")
			{
				Debug.Log(" Using Policy Block JB");
				SaveServerSelected(false);
				return true;
			}
			else
			{
				SaveServerSelected(true);
			}
		}
		return false;
	}

	private void CheckServerConfig(Hashtable serverConfig)
	{
		URL_DOWNLOAD_SERVER = serverConfig["DownloadServer"] as String;

		URL_ZONE_SERVER = serverConfig["ZoneServer"] as String;

		Hashtable servers = serverConfig["GameServers"] as Hashtable;

        GameServers = new List<ServerInfo>();
        //GameServers = new string[servers.Count * 3];

        //int index = 0;

		foreach (System.Object key in servers.Keys)
		{
            //string serverID = key as String;

			Hashtable server = servers[key] as Hashtable;

            //string serverName = server["name"] as String;

            //string serverStatus = server["status"] as String;

            ServerInfo si = new ServerInfo();
            si.id = System.Int16.Parse(key as String);
            si.name = server["name"] as String;
            si.state = (ServerState)System.Enum.Parse(typeof(ServerState), server["status"] as String);
            GameServers.Add(si);

            //GameServers[index++] = serverID;

            //GameServers[index++] = serverName;

            //GameServers[index++] = serverStatus;

			//Debug.Log(string.Format("Server id = {0}, name = \"{1}\", status = \"{2}\"", serverID, serverName, serverStatus));
		}
	}

	private void Check3rdKeys(Hashtable keysHash)
	{

	}

	private void CheckSpecialOptions(Hashtable specialOptions)
	{

	}
	
	private bool CheckNeedForceUpdate(Hashtable forceUpdateData)
	{
		//string forceUpdateTargetTag = forceUpdateData["ForceUpdateTargetTag"] as string;
		//ArrayList ForceUpdateIgnoreTags = forceUpdateData["ForceUpdateIgnoreTags"] as ArrayList;
		string forceUpdateURL = ProcessURL((forceUpdateData["ForceUpdateURL"] as string));//(forceUpdateData["ForceUpdateURL"] as string).Replace("<Language>", LocalizationContainer.CurSystemLang);
		if(forceUpdateData["ForceUpdateEnable"] as string == "yes" && OnForceUpdate != null)
		{
			OnForceUpdate(forceUpdateURL);
			return true;
		}
		
		if(forceUpdateData["ForceUpdateEnable"] as string == "no" && OnNoNeedForceUpdate != null)
		{
			OnNoNeedForceUpdate();
			return false;
		}
		
		return false;
	}
	
	public static string ProcessURL(string orgURL)
	{
		string result = orgURL;
		result = result.Replace("<Language>", LocalizationContainer.CurSystemLang);
		result = result.Replace("<baseurl>", URL_SERVER_ROOT);
		return result;
	}
	
	private void CheckServerMaintain(Hashtable serverMaintainData)
	{
		Debug.Log("CheckServerMaintain");
		//bool isTargetTag = (serverMaintainData["ServerMaintainTags"] as ArrayList).Contains(buildTag);
		if((serverMaintainData["ServerMaintainEnable"] as string == "yes") && OnServerMaintaining != null)
		{
			OnServerMaintaining(ProcessURL(serverMaintainData["ServerMaintainURL"] as string));//((serverMaintainData["ServerMaintainURL"] as string).Replace("<Language>", LocalizationContainer.CurSystemLang));
			return;
		}
#if !UNITY_EDITOR || FORCE_ASSET_BUNDLES_IN_EDITOR
		if((serverMaintainData["ServerMaintainEnable"] as string == "no") && OnServerAvailable != null)
		{
			OnServerAvailable();
			return;
		}
#else
		AppLoading.Instance.LoadingStep = APP_LOADING_STEP.SCENE_GAMELUNCH;
		Application.LoadLevel("GameLaunch");
#endif
	}
	#endregion
}
