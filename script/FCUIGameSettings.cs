using UnityEngine;
using System.Collections;

public class FCUIGameSettings : MonoBehaviour
{
    public UILabel _labelSoundFX;
    public UILabel _labelMusic;

    public GameObject buttonSwitchServer;
    public GameObject buttonSwitchHero;
    public GameObject buttonChangeName;
    public GameObject oneChanceIconForName;

    public static bool _UIIsActive = false;


    private bool _needShowSwitchServerButton = false;

    void OnInitialize()
    {
        _needShowSwitchServerButton = LocalizationContainer.CurSystemLang == "zh-Hans";

        _UIIsActive = true;

        LevelManager.Singleton.StopActorMoving();
        string[] languageList = DataManager.Instance.CurGlobalConfig.getConfig("language_list_for_change_nickname").ToString().Split(new char[] { ',' });
        bool isInList = false;
        foreach (string str in languageList)
        {
            if (str == LocalizationContainer.CurSystemLang)
            {
                isInList = true;
                break;
            }
        }

        bool canChangeNickname = isInList && PlayerInfo.Instance.have_chance_change_nickname;
#if !ENABLE_OTHER_LETTER_FOR_NAME
        canChangeNickname = false;
#endif
#if DEVELOPMENT_BUILD || UNITY_EDITOR
		if(CheatManager.changeNickname)
		{
			canChangeNickname = true;
		}
#endif
        oneChanceIconForName.SetActive(canChangeNickname);
        buttonChangeName.SetActive(canChangeNickname);
        buttonSwitchHero.transform.localPosition =
            new Vector3((_needShowSwitchServerButton || canChangeNickname) ? 108 : 0, -86.1f, -1);

        if (LocalizationContainer.CurSystemLang == "zh-Hans")
        {
            buttonSwitchServer.SetActive(true);
            if (canChangeNickname)
            {
                buttonSwitchServer.transform.localPosition = new Vector3(0, -169, -1);
            }
            else
            {
                buttonSwitchServer.transform.localPosition = new Vector3(-103, -86.1f, -1);
            }
        }
        else
        {
            buttonSwitchServer.SetActive(false);
        }
    }

    // Use this for initialization
    void Start()
    {
        _needShowSwitchServerButton = LocalizationContainer.CurSystemLang == "zh-Hans";

        if (SoundManager.Instance.EnableSFX)
        {
            _labelSoundFX.text = Localization.instance.Get("IDS_BUTTON_GAMEPAUSE_SOUNDFX_ON");
        }
        else
        {
            _labelSoundFX.text = Localization.instance.Get("IDS_BUTTON_GAMEPAUSE_SOUNDFX_OFF");
        }

        if (_needShowSwitchServerButton)
        {
            _labelMusic.text = Localization.instance.Get("IDS_BUTTON_GAMEPAUSE_MUSIC_ON");
        }
        else
        {
            _labelMusic.text = Localization.instance.Get("IDS_BUTTON_GAMEPAUSE_MUSIC_OFF");
        }
    }

    void OnClickSoundFX()
    {
        SoundManager.Instance.EnableSFX = !SoundManager.Instance.EnableSFX;

        if (SoundManager.Instance.EnableSFX)
        {
            _labelSoundFX.text = Localization.instance.Get("IDS_BUTTON_GAMEPAUSE_SOUNDFX_ON");
            SoundManager.Instance.PlayOpenSoundEffect();
        }
        else
        {
            _labelSoundFX.text = Localization.instance.Get("IDS_BUTTON_GAMEPAUSE_SOUNDFX_OFF");
            SoundManager.Instance.StopAllSoundEffect();
        }
    }

    void OnClickMusic()
    {
        SoundManager.Instance.EnableBGM = !SoundManager.Instance.EnableBGM;

        if (SoundManager.Instance.EnableBGM)
        {
            _labelMusic.text = Localization.instance.Get("IDS_BUTTON_GAMEPAUSE_MUSIC_ON");
            SoundManager.Instance.PlayOpenSoundEffect();
        }
        else
        {
            _labelMusic.text = Localization.instance.Get("IDS_BUTTON_GAMEPAUSE_MUSIC_OFF");
        }
    }

    void OnClickCredits()
    {
        UIManager.Instance.CloseUI("UIGameSettings");
        UIManager.Instance.OpenUI("Panel(Credits)");
        _UIIsActive = false;
    }

    void OnClickTermAndUse()
    {
        Application.OpenURL("");
    }

    void OnClickAccountSetting()
    {
        UIManager.Instance.OpenUI("Nickname");
        UIManager.Instance.CloseUI("UIGameSettings");

        _UIIsActive = false;
    }

    void OnClickChangeCharacter()
    {
        GameManager.Instance.GameState = EnumGameState.Launch;

        //go back to enter point scene
        Application.LoadLevel("character_selection");

        //use current role
        NetworkManager.Instance.SelectedRole = EnumRole.NotSelected;

        _UIIsActive = false;
    }

    void OnClickChangeServer()
    {
		UIMessageBoxManager.Instance.ShowMessageBox(Localization.instance.Get("IDS_MESSAGE_GAMESETTING_CHANGESERVER"), Localization.instance.Get("IDS_TITLE_GLOBAL_CHANGESERVER"), MB_TYPE.MB_OKCANCEL, OnChangeServerCallback);
    }

    void OnChangeServerCallback(ID_BUTTON buttonID)
    {
        if (buttonID == ID_BUTTON.ID_OK)
        {
            FCDownloadManager.SaveServerSelected(false);

            CharacterSelector.IsForceToSelectServer = true;
            Application.LoadLevel("character_selection");

            _UIIsActive = false;
        }
    }

    void OnClickBackToTown()
    {
        UIManager.Instance.CloseUI("UIGameSettings");

        _UIIsActive = false;
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
	private string _buildTag;
	void OnGUI()
	{
		if (string.IsNullOrEmpty(_buildTag))
		{
			_buildTag = (Resources.Load("build_tag") as TextAsset).text;
		}
		GUI.Label(new Rect(5, Screen.height - 20, 100, 20), _buildTag);
	}
#endif
}
