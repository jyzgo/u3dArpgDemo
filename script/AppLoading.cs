using UnityEngine;
using System.Collections;


public enum APP_LOADING_STEP
{
	START = 0,
	SERVER_INFO,
	BUNDLE_CHECK,
	SCENE_GAMELUNCH,
	SCENE_SERVER,
	SERVER_NEWS,

	FINAL_STEP
};

public class AppLoading : MonoBehaviour {
	
	
	#region Singleton
	private static AppLoading _instance = null;
	
	public UISlider loadingBar = null;
	public UILabel assetBundleDownloadTip = null;
	
	private static string[] LoadingTipStrings=
	{
		"IDS_TIPS_LOADING_GENERAL_01",
		"IDS_TIPS_LOADING_GENERAL_02",
		"IDS_TIPS_LOADING_GENERAL_03",
		"IDS_TIPS_LOADING_GENERAL_04",
		"IDS_TIPS_LOADING_GENERAL_05",
		"IDS_TIPS_LOADING_GENERAL_06",
		"IDS_TIPS_LOADING_GENERAL_07",
		"IDS_TIPS_LOADING_GENERAL_08",
		"IDS_TIPS_LOADING_GENERAL_09",
		"IDS_TIPS_LOADING_GENERAL_10",
		"IDS_TIPS_LOADING_GENERAL_11",
		"IDS_TIPS_LOADING_GENERAL_12"
	};
    
    private const float LoadingTip = 7;//second
	private System.DateTime curLoadingTipTimer = System.DateTime.MinValue;
	private int curLoadingTipIndex = 0;

    private const int k_max_valid_tip_index = 14;       //for Simplified Chinese only
	private ArrayList curLoadingTipStrings = new ArrayList(LoadingTipStrings);
	
	public static AppLoading Instance
	{
		get
		{
			return _instance;
		}
	}
	
	void Awake() {
		if(_instance != null)
		{
			Debug.LogError("AppLoading: Another GameLauncher has already been created previously. " + gameObject.name + " is goning to be destroyed.");
			Destroy(this);
			return;
		}
		_instance = this;
		DontDestroyOnLoad(this);

        ChangeLogoOnLanguage();
	}
	#endregion
	
	public UILabel _loadingLabel = null;
	public GameObject _loadingJuhua = null; //the loading Juhua, rotate every frame	
    public GameObject _logo;

	private float _loadingJuhuaTick = 0.0f;
	
	private APP_LOADING_STEP _loadingStep = APP_LOADING_STEP.START;
	public APP_LOADING_STEP LoadingStep
	{
		get
		{
			return _loadingStep;
		}
		set
		{
			_loadingStep = value;
		}
	}	
	
	
	
	// Use this for initialization
	void Start () {
		_loadingStep = APP_LOADING_STEP.SERVER_INFO;

        if (LocalizationContainer.CurSystemLang != "zh-Hans")
        {
            while (curLoadingTipStrings.Count > k_max_valid_tip_index)
            {
                curLoadingTipStrings.RemoveAt(k_max_valid_tip_index);
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
		
		UpdateLabel();
		UpdateLoadingIcon();
	}
	
	//update label depends on the loading step
	private void UpdateLabel()
	{
		
		if (_loadingStep == APP_LOADING_STEP.BUNDLE_CHECK
			|| _loadingStep == APP_LOADING_STEP.SCENE_SERVER
			|| _loadingStep == APP_LOADING_STEP.SERVER_INFO)
		{
			float percentage = FCDownloadManager.Instance.UpdateDownloadingProgress(FCDownloadManager.Instance.ChecKUpdatekList.ToArray(), true);
			loadingBar.sliderValue = percentage / 100.0f;
            if ((System.DateTime.UtcNow - curLoadingTipTimer).TotalSeconds >= LoadingTip && curLoadingTipStrings.Count > 0)
            {
                curLoadingTipIndex = Random.Range(0, curLoadingTipStrings.Count);
                _loadingLabel.text = Localization.Localize(curLoadingTipStrings[curLoadingTipIndex] as string);
                curLoadingTipStrings.RemoveAt(curLoadingTipIndex);
                curLoadingTipTimer = System.DateTime.UtcNow;
            }
			
			float totalSize = 0;
			float progress = 0;
			if(FCDownloadManager.Instance.CurBundleDownloadingSize(out totalSize, out progress))
			{
				float totalSizeM = totalSize / 1024 / 1024;
				float progressM = progress / 1024 / 1024;
				assetBundleDownloadTip.text = string.Format("{0} / {1} MB", progressM.ToString("f2"), totalSizeM.ToString("f2"));
			}
			else
			{
				assetBundleDownloadTip.text = "";
			}
			//string curTipText = Localization.Localize("IDS_LOADING_TIP_"+curLoadingTipIndex);
			//_loadingLabel.text = curTipText;//Localization.Localize("IDS_SERVER_UPDATE")+" " + (int)percentage+"%";
		}
	}

    private void ChangeLogoOnLanguage()
    {
        string logoName = "Logo " + Localization.instance.currentLanguage;

        Transform transEN = null;
        bool found = false;

        foreach (Transform t in _logo.transform)
        {
            if (t.name == logoName)
            {
                t.gameObject.SetActive(true);
                found = true;
            }
            else
            {
                t.gameObject.SetActive(false);
            }

            if (t.name == "Logo en")
            {
                transEN = t;
            }
        }

        if (!found)
        {
            transEN.gameObject.SetActive(true);
        }
    }
	
	
	//update juhua
	private void UpdateLoadingIcon()	
	{
		//rotate juhua?
		_loadingJuhuaTick += Time.deltaTime;
		if (_loadingJuhuaTick > FCConst.k_juhua_speed)
		{
			_loadingJuhuaTick = 0;
			_loadingJuhua.transform.Rotate(Vector3.forward, -45);
		}
	}
	

	
	public void FinishLoading()
	{
		DestroyObject(gameObject);
	}
}
