using UnityEngine;


public class GameSettings : ScriptableObject
{
	public int MaxBagSize = 200;
	#region System Options
    public int DebugConsoleSize = 8;
	public bool EncryptSaveFile = true;
	public bool DebugShader = false;
	public bool AllowDeferredShading = false;
	public bool AllowPostProcess = true;
	public bool AllowParticleEffect = true;
	public string DefaultLoadingTexture = "";
    public float TimeScale = 1;
	#endregion
	
	#region Game Option
	[System.Serializable]
	public class RoleSettings
	{
		public string battleLabel;
		public string townLabel;
		public string townNPCLabel;
		public string previewLabel;
		public string portraitPath;
	}

	public RoleSettings[] roleSettings;
	#endregion
	
	#region Game buffer data
	public float[] _bufferSpeedPercents;
	public float[] _bufferBodySizePercents;
	public float[] _bufferAniSpeedPercents;
	public Color[] _bufferColors;
	public float[] _bufferFinalDamageChangePercents;
	public float[] _bufferFinalDefenseChangePercents;
	
	private string[] _badWordsRules = null;
	private string[] _badCharactersRules = null;
	
	public FCBuffer[] _bufferTable;
	
	public float DamageReduceKeyValue = 1f;
	
	public DeviceLODSetting LODSettings;
	
	public string[] BadWordsRules
	{
		get
		{
			if(_badWordsRules == null || _badWordsRules.Length == 0)
			{
                TextAsset asset = InJoy.AssetBundles.AssetBundles.Load("Assets/Data/NicknameRule/BadWords.txt") as TextAsset;
				_badWordsRules = asset.text.Split(',');
			}
			return _badWordsRules;
		}
	}
	
	public string[] BadCharactersRules
	{
		get
		{
			if(_badCharactersRules == null || _badCharactersRules.Length == 0)
			{
                TextAsset asset = InJoy.AssetBundles.AssetBundles.Load("Assets/Data/NicknameRule/BadCharacteors.txt") as TextAsset;
				_badCharactersRules = asset.text.Split(',');
			}
			return _badCharactersRules;
		}
	}

    public float FovFactor
    {
        get {
            float fovFactor = 0.0f;
#if !UNITY_EDITOR
                string deviceModel = GameSettings.Instance.LODSettings.GetDeviceModel();
				if(deviceModel.StartsWith("iPhone") || deviceModel.StartsWith("iTouch")) {
					fovFactor = 1.0f;
				}
#endif
            return fovFactor;
        }
    }

	#endregion
#if TEMPLATE_PLAYHAVEN
	public string PlayHavenAppToken = "e848346d91254e30a71210d0c5e80916";
	public string PlayHavenSecretKey = "11f0439e482c4306aec3fa24a2d4979d";
#endif

#if TEMPLATE_FLURRY
	public string FlurryApiKey = "VISSQ695L9TB2H74W6ME";
#endif

#if TEMPLATE_TAPJOY
	// circus city id
	// tapJoyID = "bba49f11-b87f-4c0f-9632-21aa810dd6f1";
	// tapJoyKey = "yiQIURFEeKm0zbOggubu";
    //TapjoyInterface.Initialize("bba49f11-b87f-4c0f-9632-21aa810dd6f1", "yiQIURFEeKm0zbOggubu", true);

	// template id
	// tapJoyID = "8a455d73-4346-4423-a916-919ea66ec4a2";
	// tapJoyKey = "xEMa8GB02oc083cvPKEB";
    //TapjoyInterface.Initialize("8a455d73-4346-4423-a916-919ea66ec4a2", "xEMa8GB02oc083cvPKEB", true);

	public string TapjoyGameAppID = "8a455d73-4346-4423-a916-919ea66ec4a2";
	public string TapjoyGameSecretKey = "xEMa8GB02oc083cvPKEB";
#endif

#if TEMPLATE_ADCOLONY
	public string AdColonyZoneID = "z4e541640e51ce";
	public string AdColonyAppID = "app4e541454aa92e";
#endif

#if BURSTLY
    public string BurstlyPublisherID = "fBtwhhxXVEOk5a71l9h0JA";
    public string BurstlyZoneID = "0654106969058294971";
#endif

#if TEMPLATE_ADMANAGER
	public string AdMarvelSiteIDiPhone = "6856";
	public string AdMarvelSiteIDiPad = "6844";
	public string AdMarvelSiteIDInterstitial = "3102";
	public string AdMarvelPartnerID = "a1434e57627654cf";
#endif

#if TEMPLATE_PUSHNOTIFICATIONS
	public string UrbanAirshipAppKey = "XSoTViLIS_6EnCf0gj544Q";
	public string UrbanAirshipAppSecret = "bKniZHE-RvGT6pu_WC_XZw";
#endif

#if FORCED_UPDATE
    public string ForcedUpdateUrl = "http://data.xml";
    public string ForcedUpdateVersion = "1.0.0";
#endif

#if MOBILE_APP_TRACKING
    public string MATAdvertiserId = "2376";
    public string MATAppKey = "2f7f04962bb9ef8bf9c05d740c0c8f0d";
#endif
	
	
	static public GameSettings Instance{
		get {
			if(_inst == null) {
				_inst = InJoy.AssetBundles.AssetBundles.Load(_assetPath, typeof(GameSettings)) as GameSettings; 
			}
			return _inst;
		}
	}
	
	void OnDestroy() {
		if(_inst == this) {
			_inst = null;
		}
	}
	
	public bool IsDeferredShadingActived() {
		int lowResLevel = 2;
        int deviceLevel = LODSettings.GetDeviceLevel();
		bool allowed = false;
		allowed = deviceLevel > lowResLevel;
#if UNITY_EDITOR
		allowed = allowed && SystemInfo.supportsRenderTextures && AllowDeferredShading;
#endif
		return allowed;
	}
	
	public bool IsPostProcessActived() {
		int lowResLevel = 2;
		int deviceLevel = LODSettings.GetDeviceLevel();
		bool allowed = deviceLevel > lowResLevel;
#if UNITY_EDITOR
		allowed = allowed && SystemInfo.supportsRenderTextures && AllowPostProcess && IsDeferredShadingActived();
#endif
		return allowed;
	}
	
	public bool IsDirectionalShadowActived() {
		int allowedShadowLevel = 1;
		int deviceLevel = LODSettings.GetDeviceLevel();
		bool allowed = false;
		allowed = deviceLevel > allowedShadowLevel;
#if UNITY_EDITOR
		allowed = allowed && SystemInfo.supportsRenderTextures;
#endif
		return allowed;
	}
	
	public bool IsPointShadowActived() {
		int allowedShadowLevel = 1;
		int deviceLevel = LODSettings.GetDeviceLevel();
		bool allowed = false;
		allowed = deviceLevel > allowedShadowLevel;
#if UNITY_EDITOR
		allowed = allowed && SystemInfo.supportsRenderTextures;
#endif
		return allowed;
	}
	
	public bool IsFullSceneDisplay() {
		int allowedShadowLevel = 1;
		int deviceLevel = LODSettings.GetDeviceLevel();
		return deviceLevel > allowedShadowLevel;
	}
	
	public bool IsParticleEnabled() {
		int deviceLevel = LODSettings.GetDeviceLevel();
		int allowedLevel = 1;
		return AllowParticleEffect && (deviceLevel > allowedLevel);
	}
	
	static private GameSettings _inst;
	static public string _assetPath = "Assets/Data/GameSettings.asset";
}

