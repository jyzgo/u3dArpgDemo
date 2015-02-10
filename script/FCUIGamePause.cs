using UnityEngine;
using System.Collections;

public class FCUIGamePause : MonoBehaviour
{
    public UILabel _labelSoundFX;
    public UILabel _labelMusic;
	
	private bool _isConnecting = false;

    // Use this for initialization
    void Start()
    {
        if (SoundManager.Instance.EnableSFX)
        {
            _labelSoundFX.text = Localization.instance.Get("IDS_BUTTON_GAMEPAUSE_SOUNDFX_ON");
        }
        else
        {
            _labelSoundFX.text = Localization.instance.Get("IDS_BUTTON_GAMEPAUSE_SOUNDFX_OFF");
        }

        if (SoundManager.Instance.EnableBGM)
        {
            _labelMusic.text = Localization.instance.Get("IDS_BUTTON_GAMEPAUSE_MUSIC_ON");
        }
        else
        {
            _labelMusic.text = Localization.instance.Get("IDS_BUTTON_GAMEPAUSE_MUSIC_OFF");
        }
    }

    void OnInitialize()
    {
		 SoundManager.Instance.Pause();
		if(!_isConnecting)
		{
        	GameManager.Instance.GamePaused = true;
		}
    }

    void OnClickResumeGame()
    {
        GameManager.Instance.GamePaused = false;

        UIManager.Instance.CloseUI("UIGamePaused");
		
		LevelManager.Singleton.StopActorMoving();

        if (SoundManager.Instance.EnableSFX)
        {
            SoundManager.Instance.Resume();
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
            SoundManager.Instance.StopAllSoundEffect();
            _labelSoundFX.text = Localization.instance.Get("IDS_BUTTON_GAMEPAUSE_SOUNDFX_OFF");
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

    void OnClickBackToTown()
    {
        string caption = Localization.instance.Get("IDS_TITLE_GLOBAL_WARNING");
        
        string content;

        {
            content = Localization.instance.Get("IDS_MESSAGE_GAMEPAUSE_QUIT");
        }

        UIMessageBoxManager.Instance.ShowMessageBox(content, caption, MB_TYPE.MB_OKCANCEL, OnClickBackToTownCallback, MBButtonColorScheme.green_red);
    }

    void OnClickBackToTownCallback(ID_BUTTON buttonID)
    {
        if (buttonID == ID_BUTTON.ID_OK)
        {
            AudioListener.pause = false;
            
			UIManager.Instance.CloseUI("UIGamePaused");
            
            _isConnecting = true;
            if(GameManager.Instance.CurrGameMode == GameManager.FC_GAME_MODE.SINGLE)
            {
                BattleSummary.Instance.AbortBattle(OnSaveOK);
            }
//            else if(GameManager.Instance.IsPVPMode)
//            {
//                //PvPBattleSummary.Instance.CancelBattle(OnSaveOK);
//                OnSaveOK();
//            }
        }
    }

    void OnSaveOK(bool succeeded)
    {
		GameManager.Instance.GamePaused = false;

		if (succeeded)
		{
			_isConnecting = false;
			ActionController ac = ObjectManager.Instance.GetMyActionController();
			ac.AIUse.ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_SUPER_SAIYAJIN);
			LevelManager.Singleton.ExitLevel();
		}
    }
}
