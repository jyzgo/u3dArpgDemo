using UnityEngine;
using System.Collections.Generic;

public class FCDownloadManagerView : MonoBehaviour
{
	#region Singleton
	private static FCDownloadManagerView _instance = null;
	
	public static FCDownloadManagerView Instance
	{
		get
		{
			return _instance;
		}
	}
	
	void Awake() 
	{
		if (_instance != null)
		{
			Debug.LogError("FCDownloadManagerView: Another FCDownloadManagerView has already been created previously. " + gameObject.name + " is goning to be destroyed.");
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
	
	#endregion
	
	
	// Use this for initialization
	void Start () 
	{	
		//FCDownloadManager.Instance.OnStartNewDownload = OnStartNewDownload;
	}
	
	System.Collections.Generic.List<WorldBundlesData> _downloadBundleSet = new System.Collections.Generic.List<WorldBundlesData>();
	
	public bool IsWorldBundleInQueue(WorldBundlesData world) {
		return _downloadBundleSet.Contains(world);
	}
	
	public void RegisterDownloadEvent(WorldBundlesData bundleSet)
	{
		FCDownloadManager.Instance.OnStartNewDownload = OnStartNewDownload;
		FCDownloadManager.Instance.OnDownloadSuccess = OnDownloadSuccess;
		FCDownloadManager.Instance.OnDownloadFailed = OnDownloadFailed;
		FCDownloadManager.Instance.OnAllDownloadFinished = OnAllDownloadFinished;
		
		if(!_downloadBundleSet.Contains(bundleSet)) {
			_downloadBundleSet.Add(bundleSet);
		}
	}
	
	public bool IsBundleDownloading() {
		return _downloadBundleSet.Count > 0;
	}
	
	void Update()
	{
		
		
	}
	
//	System.DateTime _timer;
	bool _startNewBundleSet = true;
	void OnStartNewDownload(FCDownloadManager.FCIndexDownloadInfo indexInfo)
	{
		if(_downloadBundleSet.Count > 0 && _startNewBundleSet) {
//			_timer = System.DateTime.Now;
			_startNewBundleSet = false;
			Debug.Log("start downloading " + _downloadBundleSet[0]._worldName);
		}
	}
	
	void OnDownloadSuccess(FCDownloadManager.FCIndexDownloadInfo indexInfo)
	{
		int cnt = _downloadBundleSet.Count;
//		System.DateTime now = System.DateTime.Now;
//		int seconds = (int)((now - _timer).TotalSeconds);
		if(cnt > 0) {
			for(int i = cnt - 1;i >= 0;--i) {
				// if current index set downloading is completed, remove it from list.
				if(FCDownloadManager.Instance.CheckIndexState(_downloadBundleSet[i]._bundles) == FCDownloadManager.IndexCheckResult.ICR_AllFinished) {
					_startNewBundleSet = true;
					_downloadBundleSet.RemoveAt(i);
				}
			}
		}
	}
	

	void OnDownloadFailed(FCDownloadManager.FCIndexDownloadInfo indexInfo)
	{	
		if(_downloadBundleSet.Count > 0) {
			WorldBundlesData wbd = _downloadBundleSet[0];
			foreach(string b in wbd._bundles) {
				if(FCDownloadManager.Instance.CheckIndexState(new string[]{b}) == FCDownloadManager.IndexCheckResult.ICR_Downloading) {
//					System.DateTime now = System.DateTime.Now;
//					int minutes = (int)((now - _timer).TotalMinutes);
				}
			}
			Debug.LogError("Failed to download " + _downloadBundleSet[0]._worldName);
		}

		if(FCDownloadManager.Instance.HasFatalError()) {
			if(!_fatalErr) {
				_fatalErr = true;
				FCDownloadManager.Instance.RemoveAllDownloads(FCDownloadManager.DownloadType.DT_Background);
			}
		}
		else {
			FCDownloadManager.Instance.RestartAllDownloads();
		}
	}
	
	bool _fatalErr = false;
	public bool ReadInfoFatalError() {
		return _fatalErr;
	}
	
	void OnAllDownloadFinished()
	{
	}
	
	public void ShowDownloadProgress(bool fullInterface)
	{		
		if(UIManager.Instance.OpenUI("UIBundleDownload")) {
			GameObject go = UIManager.Instance.GetUI("UIBundleDownload");
			UIBundleDownloadHandler ubdh = go.GetComponent<UIBundleDownloadHandler>();
			Assertion.Check(ubdh != null);
			
			ubdh.SetFullInterfaceMode(fullInterface);
		}
	}
	
	public string GetCurrentDownloadTips()
	{
		string tips = "";
		if(_downloadBundleSet.Count > 0) {
			string tipsID = _downloadBundleSet[0]._downloadTipsIDS;
			tips = Localization.instance.Get(tipsID);
		}
		return tips;
	}
	
	public int GetCurrentDownloadProgress() {
		int ret = 100;
		if(_downloadBundleSet.Count > 0) {
			float progress = FCDownloadManager.Instance.UpdateDownloadingProgress(_downloadBundleSet[0]._bundles, false);
			ret = Mathf.Clamp(Mathf.RoundToInt(progress) , 0, 100);
		}
		return ret;
	}
	
	public void ShowDownloadProgress(string tipsIDS, float progress)
	{		
		UIManager.Instance.OpenUI("UIBundleDownload");
		GameObject o = UIManager.Instance.GetUI("UIBundleDownload");
		UIBundleDownloadHandler ui = o.GetComponent<UIBundleDownloadHandler>();
		ui.Message = Localization.instance.Get(tipsIDS);
		//ui.Progress= string.Format("{0}%", progress);
		ui._progressBarTips.sliderValue = progress / 100.0f;
	}
}
