using UnityEngine;
using System.Collections;

public class UIKingdomBadgeHander : MonoBehaviour 
{
	public string _groupName;
	public GameObject _UI;
	
	public UISlider _downloadProgress;
	public UILabel  _downloadTips;
	
	bool _levelButtonsUpdated;
	FCDownloadManager.IndexCheckResult _downloadResult;
	
	void Awake()
	{
		_levelButtonsUpdated = false;
	}
	
	// Use this for initialization
	void Start ()
	{
		UpdateKingdomState();
	}
	
	// Update is called once per frame
	void Update () 
	{
		UpdateKingdomState();
	}
	
	void OnClickDownload()
	{
		WorldBundlesData wbd = LevelManager.Singleton.BundleConfig.GetBundlesByWorld(_groupName);
		Assertion.Check(wbd != null);
		if (FCDownloadManagerView.Instance.IsWorldBundleInQueue(wbd))
		{
			FCDownloadManagerView.Instance.ShowDownloadProgress(true);
		}
		else {
			LevelManager.Singleton.CheckDownloadByGroup(wbd);
		}
	}
	
	// Switch state by download status.
	void UpdateKingdomState()
	{
		WorldBundlesData wbd = LevelManager.Singleton.BundleConfig.GetBundlesByWorld(_groupName);
		_downloadResult = FCDownloadManager.Instance.CheckIndexState(wbd._bundles);
		
		if (_downloadResult == FCDownloadManager.IndexCheckResult.ICR_AllFinished)
		{
			//_UI.SetActive(false);
			gameObject.SetActive(false);
			_downloadTips.text = Localization.instance.Get("IDS_BUNDLE_DOWNLOAD_COMPLETE");
			_downloadProgress.gameObject.SetActive(false);
			
			if (!_levelButtonsUpdated)
			{
				WorldMapController.Instance.UpdateLevelButtonsState();
				_levelButtonsUpdated = true;
			}
		}
		else// if (FCDownloadManagerView.Instance.IsWorldBundleInQueue(wbd))
		{
			//_UI.SetActive(true);
			_downloadTips.text = Localization.instance.Get("IDS_BUNDLE_DOWNLOADING");
			_downloadProgress.gameObject.SetActive(true);
			
			float progress = FCDownloadManager.Instance.UpdateDownloadingProgress(wbd._bundles, false);		
			_downloadProgress.sliderValue = progress/100f;
		}
		/*else
		{
			//_UI.SetActive(true);
			_downloadTips.text = Localization.instance.Get("IDS_BUNDLE_CLICK_TO_DOWNLOAD");
			_downloadProgress.gameObject.SetActive(false);
		}*/
	}
}
