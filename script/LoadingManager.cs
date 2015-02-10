using UnityEngine;
using System.Collections;

public class LoadingManager : MonoBehaviour
{

	#region Singleton
	private static LoadingManager _instance = null;
	public static LoadingManager Instance
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
			Debug.LogError("LoadingManager: Another GameLauncher has already been created previously. " + gameObject.name + " is goning to be destroyed.");
			Destroy(this);
			return;
		}
		_instance = this;
	}

	void OnDestroy()
	{
		if (_instance == this)
		{
			_instance = null;
		}
	}
	#endregion


	public UILabel _name;
	public UILabel _tip;

	public GameObject _loadingUI = null; //the UI root, off at start-up

	public UITexture[] _loadingBGs; //the BG for loading
	public UITexture[] _loadingBG1s; // loading texture pair.
	public UITexture[] _loadingBG2s;
	public GameObject _largeScreenLayout;
	public GameObject _standardScreenLayout;

	public UILabel[] _playerName = null; //player name and status, for multi-loading
	public UILabel[] _playerStatus = null;


	public GameObject _loadingJuhua = null; //the loading Juhua, rotate every frame	
	private float _loadingJuhuaSpeed = 0.1f;
	private float _loadingJuhuaTick = 0.0f;


	void SetLoadingTextActive(bool actived)
	{
		_name.gameObject.SetActive(actived);
		_tip.gameObject.SetActive(actived);
	}

	public void UpdateLoadingIcon()
	{
		//rotate the loading icon
		_loadingJuhuaTick += Time.deltaTime;
		if (_loadingJuhuaTick > _loadingJuhuaSpeed)
		{
			_loadingJuhuaTick = 0;
			_loadingJuhua.transform.Rotate(Vector3.forward, -45);
		}
	}


	public void SetLoadingScreenActive(bool active)
	{
		_loadingUI.SetActive(active);
		SetLoadingTextActive(active);
	}

	public void SetLoadingBG(string loadingBGTex)
	{
		Texture2D tex = InJoy.AssetBundles.AssetBundles.Load(loadingBGTex) as Texture2D;
		Texture2D tex1 = InJoy.AssetBundles.AssetBundles.Load(loadingBGTex.Replace(".", "_1.")) as Texture2D;
		Texture2D tex2 = InJoy.AssetBundles.AssetBundles.Load(loadingBGTex.Replace(".", "_2.")) as Texture2D;
		if (tex == null || tex1 == null || tex2 == null)
		{
			tex = InJoy.AssetBundles.AssetBundles.Load(GameSettings.Instance.DefaultLoadingTexture) as Texture2D;
			tex1 = InJoy.AssetBundles.AssetBundles.Load(GameSettings.Instance.DefaultLoadingTexture.Replace(".", "_1.")) as Texture2D;
			tex2 = InJoy.AssetBundles.AssetBundles.Load(GameSettings.Instance.DefaultLoadingTexture.Replace(".", "_2.")) as Texture2D;
		}
		Assertion.Check(tex != null);
		foreach (UITexture t in _loadingBGs)
		{
			t.mainTexture = tex;
		}
		Assertion.Check(tex1 != null);
		foreach (UITexture t in _loadingBG1s)
		{
			t.mainTexture = tex1;
		}
		Assertion.Check(tex2 != null);
		foreach (UITexture t in _loadingBG2s)
		{
			t.mainTexture = tex2;
		}

		// switch layout for large-screen & standard-screen.
		string model = GameSettings.Instance.LODSettings.GetDeviceModel();
		_largeScreenLayout.SetActive(model.Contains("iPad"));
		_standardScreenLayout.SetActive(!model.Contains("iPad"));
	}

	//fill the player names
	public void SetPlayerName(int playerIndex, string playerName)
	{
		Assertion.Check((playerIndex >= 0) && (playerIndex < FCConst.MAX_PLAYERS));
		_playerName[playerIndex].text = playerName;
	}

	//fill status
	public void SetPlayerStatus(int playerIndex, FC_MULTIPLAY_STATUS status)
	{
		Assertion.Check((playerIndex >= 0) && (playerIndex < FCConst.MAX_PLAYERS));

		switch (status)
		{
			case FC_MULTIPLAY_STATUS.LOADING:
				_playerStatus[playerIndex].text = Localization.instance.Get("IDS_MULTIPLAY_STATUS_LOADING");
				break;
			case FC_MULTIPLAY_STATUS.DISCONNECTED:
				_playerStatus[playerIndex].text = Localization.instance.Get("IDS_MULTIPLAY_STATUS_DISCONNECT");
				break;
			case FC_MULTIPLAY_STATUS.READY:
				_playerStatus[playerIndex].text = Localization.instance.Get("IDS_MULTIPLAY_STATUS_READY");
				break;
			default:
				Debug.LogError("invalid loading status for player" + playerIndex.ToString());
				break;
		}

	}



}