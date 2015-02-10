using UnityEngine;
using System.Collections;
using InJoy.FCComm;

public class HUDTownPortrait : MonoBehaviour 
{
	public UILabel _nickname;
	public UILabel _playerLevel;
	public UILabel _gSocre;
	public UISprite _portrait;
	public UISlider _xpSlider;
	
    public UISprite portraitFrame;
	
	private bool _initialized;
	private PlayerInfo _playerProfile;

    public Color[] nameColors;

    private string[] portraitFrameNames = new string[]
    {
        "head_frame1",
        "head_frame2",
        "head_frame3",
    };

	// Use this for initialization
	void Start () 
	{
		_initialized = false;	
		_playerProfile = PlayerInfo.Instance;
	}
	
	void Update()
	{
		_nickname.text = NetworkManager.Instance.NickName;
        _nickname.color = nameColors[_playerProfile.difficultyLevel];
		
		_gSocre.text = "";

		_playerLevel.text = _playerProfile.CurrentLevel.ToString();
		_xpSlider.sliderValue = _playerProfile._currentXpPrecent;
				
		if (!_initialized)
		{
			if(TownPlayerManager.Singleton != null && TownPlayerManager.Singleton.HeroInfo != null) {
			
				_portrait.spriteName = GameSettings.Instance.roleSettings[PlayerInfo.Instance.RoleID].portraitPath;

	            portraitFrame.spriteName = portraitFrameNames[_playerProfile.difficultyLevel];
			
				_initialized = true;
			}
		}
	}
	
	void OnPopupPlayerAttributeUI()
	{
		if (!UIManager.Instance.IsUIOpened("TownPlayer"))
		{
			//PlayerInfoManager.Instance.LocalPlayerInfo.UpdateTableInfo();
			//UITownPlayer._activePlayerInfo = PlayerInfoManager.Instance.LocalPlayerInfo;
			//UIManager.Instance.OpenUI("TownPlayer");
		}
	}
}
