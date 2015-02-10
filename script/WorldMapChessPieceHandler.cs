using UnityEngine;
using System.Collections;

public class WorldMapChessPieceHandler : MonoBehaviour 
{
	public string _levelName;
	public PathLoft _pathLoft = null;
	public GameObject _locked = null;
	public GameObject _highlight = null;
	public GameObject _arrow = null;
	public Texture[] _scoreTextures;
	public string _clickEvent = "DefaultClick";
    public string _iconBGSetting = "UseNormalLevelBG";       //method name
    public string _scoreDisplaySetting = "DisplayNormalScore";      //method name
	
	private UIImageButton _imageButton = null;
	private UITexture _levelScore;
	
	private string[] bossButtonBGNames = new string[]
	{
		"Worldmap_UI_boss_bg",
		"Worldmap_UI_boss_bg_1",
		"Worldmap_UI_boss_bg_2",
	};
	
	private string[] levelButtonBGNames = new string[]
	{
		"Worldmap_UI_level_bg",
		"Worldmap_UI_level_bg_1",
		"Worldmap_UI_level_bg_2",
	};

#if switch_boss_icon
    private string[] bossIconNames = new string[]
    {
		"Worldmap_UI_boss_icon",
		"Worldmap_UI_boss_icon_1",
		"Worldmap_UI_boss_icon_2",
    };

    private string[] bossIconNamesHit = new string[]
    {
		"Worldmap_UI_boss_icon_hit",
		"Worldmap_UI_boss_icon_hit_1",
		"Worldmap_UI_boss_icon_hit_2",
    };
    private UISprite _imageButtonBG;
#endif

    private UISprite _buttonBG;

	void Awake()
	{
		_imageButton = gameObject.GetComponent<UIImageButton>();
		
		if (_locked != null)
		{
			_levelScore  = _locked.GetComponent<UITexture>();
		}
		
		_buttonBG = transform.parent.FindChild("Sprite (Worldmap_UI_Btn)").GetComponent<UISprite>();
	}
	

	
	void OnClick()
	{
		SendMessage(_clickEvent);
	}
	
	void DefaultClick()
	{
		SoundManager.Instance.PlaySoundEffect("button_normal");
        if(LevelManager.Singleton.CheckDownloadAndEnterLevel(_levelName, PlayerInfo.Instance.difficultyLevel)) {
			UIManager.Instance.OpenUI("FCEnterLevel");
		}
		
		TryFinishTutorial();
	}
	
	void ActivityLevelClick()
	{
		SoundManager.Instance.PlaySoundEffect("button_normal");
		if(LevelManager.Singleton.CheckDownloadAndEnterLevel(_levelName, PlayerInfo.Instance.difficultyLevel)) {
			UIManager.Instance.OpenUI("UIActivityLevel");		
		}
	}

	public void UpdateState()
	{
		if(_levelName != null)
		{
			PlayerInfo profile = PlayerInfo.Instance;
			
			LevelData levelData = LevelManager.Singleton.LevelsConfig.GetLevelData(_levelName);
			
			if(levelData == null || !levelData.available)
			{
				transform.parent.gameObject.SetActive(false);
				return;
			}
			
			if(TryStartTutorial())
			{
				if(_levelName != "village2")
				{
					transform.parent.gameObject.SetActive(false);
					return;
				}	
			}

            SendMessage(_iconBGSetting);
		
			int unlockLevel = levelData.unlockPlayerLevel;

			int prvLevelID =  levelData.id;
			
			EnumLevelState preLevelState =  EnumLevelState.D;
			
			if(prvLevelID > 0)
			{
				preLevelState = profile.GetLevelState(levelData.levelName);	
			}
			
			if (preLevelState >= EnumLevelState.D && profile.CurrentLevel >= unlockLevel)
			{
                profile.ChangeLevelState(_levelName, profile.difficultyLevel, EnumLevelState.NEW_UNLOCK);
			}	
			
			EnumLevelState state = profile.GetLevelState(_levelName);
			
			bool isLocked = state == EnumLevelState.LOCKED;
			
            SendMessage(_scoreDisplaySetting, preLevelState);
			
			if(isLocked && preLevelState == EnumLevelState.LOCKED && profile.difficultyLevel == 0)
			{
				transform.parent.gameObject.SetActive(false);
				return;
			}
						
            if (_pathLoft != null)
            {
                if (state >= EnumLevelState.D)
                {
                    _pathLoft.UnlockEffect = false;
                    _pathLoft.UpdatePath();
                }
                else if (state == EnumLevelState.NEW_UNLOCK)
                {
                    _pathLoft.UnlockEffect = true;
                    _pathLoft.UpdatePath();
                }
            }
				
			if(_highlight != null)
			{
				bool highLight = (state == EnumLevelState.NEW_UNLOCK);
				_highlight.SetActive(highLight);
			}
			
		}
	}

    //to be called by send message
    private void UseNormalLevelBG()
    {
        int difficultyLevel = PlayerInfo.Instance.difficultyLevel;

        _buttonBG.spriteName = levelButtonBGNames[difficultyLevel];
    }

    private void UseBossLevelBG()
    {
        int difficultyLevel = PlayerInfo.Instance.difficultyLevel;

        _buttonBG.spriteName = bossButtonBGNames[difficultyLevel];
    }

    private void UseFestivalLevelBG()
    {
        //no change, use current sprite
    }

    private void DisplayNormalScore(EnumLevelState preLevelState)
    {
        PlayerInfo profile = PlayerInfo.Instance;

        EnumLevelState state = profile.GetLevelState(_levelName);

        bool isLocked = state == EnumLevelState.LOCKED;

        if (isLocked && preLevelState == EnumLevelState.LOCKED && profile.difficultyLevel == 0)
        {
            transform.parent.gameObject.SetActive(false);
            return;
        }

        _imageButton.isEnabled = !(isLocked && profile.difficultyLevel == 0);

        if (_locked != null)
        {
            if (profile.difficultyLevel > 0)
            {
                _locked.SetActive(state > EnumLevelState.NEW_UNLOCK);
            }
            else
            {
                _locked.SetActive(state != EnumLevelState.NEW_UNLOCK);
            }
        }

        if (_levelScore != null)
        {
            if (PlayerInfo.Instance.difficultyLevel > 0)   //always enabled
            {
                if (state > EnumLevelState.NEW_UNLOCK) //passed, we have a score
                {
                    _levelScore.mainTexture = _scoreTextures[(int)state];
                }
                else //no score, make it available
                {
                    _levelScore.mainTexture = _scoreTextures[(int)EnumLevelState.NEW_UNLOCK];
                }
            }
            else
            {
                _levelScore.mainTexture = _scoreTextures[(int)state];
            }

            if (state >= EnumLevelState.D)
            {
                UISprite sprite = _imageButton.GetComponentInChildren<UISprite>();
                if (sprite != null)
                {
                    sprite.gameObject.SetActive(false);
                }
            }
        }
    }

    private void DisplayNoScore(EnumLevelState preLevelState)
    {
        //do nothing
        _locked.SetActive(false);
    }
	
	void SetLevelScore()
	{
		//_levelScore.spriteName = "";
	}

	
	private int _tutorialStep = 0;
	public bool TryStartTutorial()
	{
		CloseTutorial();
		EnumTutorialState state = PlayerInfo.Instance.GetTutorialState(EnumTutorial.Town_Map);
		if (state == EnumTutorialState.Active)
		{
			BeginStep1();
			return true;
		}
		return false;
	}
	
	public void BeginStep1()
	{
		_tutorialStep = 1;
		if(_arrow != null)
		{
			_arrow.SetActive(true);
		}
	}

	public void TryFinishTutorial()
	{
		if(_tutorialStep != 0)
		{
			CloseTutorial();
		}
	}
	
	public void CloseTutorial()
	{
		_tutorialStep = 0;
		if(_arrow != null)
		{
			_arrow.SetActive(false);
		}
	}
	
	
}
