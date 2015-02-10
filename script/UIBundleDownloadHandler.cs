using UnityEngine;
using System.Collections;

public class UIBundleDownloadHandler : MonoBehaviour 
{
	public UILabel _tips;
	public UILabel _sizeTip;
	public UISlider _progressBar;
	
	public string Message
	{
		set
		{
			_tips.text = value;
		}
	}
	
	public UISlider _progressBarTips;
	
	public GameObject _downloadSmallIcon;
	public GameObject []_downloadFullInterface;
	public GameObject _downloadErrorIcon;
	
	public string _characterIconPath;
	public UITexture _characterTex;
	
	// Use this for initialization
	void Start () {
		if(FCDownloadManagerView.Instance.IsBundleDownloading()) {
			SetFullInterfaceMode(false);
		}
		Texture tex = InJoy.AssetBundles.AssetBundles.Load(_characterIconPath, typeof(Texture)) as Texture;
		if(tex != null) {
			_characterTex.mainTexture = tex;
			_characterTex.color = Color.white;
		}
		else {
			_characterTex.color = Color.clear;
		}
	}
	
	void OnClickOK()
	{
		SetFullInterfaceMode(false);
	}
	
	void Update() {
		if(!_systemErr && FCDownloadManagerView.Instance.ReadInfoFatalError()) {
			_systemErr = true;
			_downloadErrorIcon.SetActive(true);
			PopupSystemError();
		}
		else
		{
			int p = FCDownloadManagerView.Instance.GetCurrentDownloadProgress();
			if(_fullInterface) {
				string tips = FCDownloadManagerView.Instance.GetCurrentDownloadTips();
				if(tips != "") 
				{
					Message = tips;
				}
                //float totalSize = 0;
                //float progress = 0;
//				if(FCDownloadManager.Instance.CurIndexDownloadingSize(out totalSize, out progress))
//				{
//					float totalSizeM = totalSize / 1024 / 1024;
//					float progressM = progress / 1024 / 1024;
//					_sizeTip.text = string.Format(Localization.Localize("IDS_BUNDLE_DATA_TRAFFIC"), progressM.ToString("f2"), totalSizeM.ToString("f2"));
//				}
//				else
//				{
//					_sizeTip.text = "";
//				}
				//_sizeTip.text = ""+FCDownloadManager.Instance.CurDownloadingSize();
				_progressBarTips.sliderValue = p / 100.0f;
				_sizeTip.text = string.Format("{0}%", p);
				//int p = FCDownloadManagerView.Instance.GetCurrentDownloadProgress();
				//Progress = string.Format("{0}%", p);
			}
			//bool test = FCDownloadManagerView.Instance.IsBundleDownloading();
			_downloadSmallIcon.SetActive(FCDownloadManagerView.Instance.IsBundleDownloading());
			if(FCDownloadManagerView.Instance.IsBundleDownloading())
			{
				_progressBar.sliderValue = p / 100.0f;
			}
		}
	}
	
	bool _fullInterface = false;
	bool _systemErr = false;
	
	public void SetFullInterfaceMode(bool fullInterface)
	{
		_fullInterface = fullInterface;
		if(_fullInterface && _systemErr) {
			PopupSystemError();
			return;
		}
		foreach(GameObject go in _downloadFullInterface) {
			go.SetActive(fullInterface);
		}
	}
	
	void PopupSystemError()
	{
		UIMessageBoxManager.Instance.ShowMessageBox(Localization.instance.Get("IDS_BUNDLE_TIPS_ERROR"), "", MB_TYPE.MB_OKCANCEL, OnNeedReboot);
	}
	
	public void OnNeedReboot(ID_BUTTON buttonID)
	{
		if(buttonID == ID_BUTTON.ID_OK) {
			Application.Quit();
		}
	}
	
	// View download state
	void OnClickDownloadState()
	{
		SetFullInterfaceMode(true);
	}
}
