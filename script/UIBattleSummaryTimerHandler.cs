using UnityEngine;
using System.Collections;

public class UIBattleSummaryTimerHandler : MonoBehaviour 
{
	public UILabel _timerTip;
	
	private int _times = 5;
	private float _elapsedTime = 0f;
	
	// Use this for initialization
	void Start () 
	{
        string ids = Localization.instance.Get("IDS_MESSAGE_COMBAT_FINISH_TIME");
		_timerTip.text = string.Format(ids, _times);
		
		GameObject o = UIManager.Instance.GetUI("HomeUI");
		UILevelHome ui = o.GetComponent<UILevelHome>();
		ui.DisableButtons();
		
		SoundManager.Instance.PlaySoundEffect("summary_rank");
		
		UIManager.Instance.CloseUI("UIGamePaused");
	}
	
	void Update()
	{		
		//return;
		_elapsedTime += Time.deltaTime;
		if (_elapsedTime >= 1f)
		{
			string ids = Localization.instance.Get("IDS_MESSAGE_COMBAT_FINISH_TIME");
			_timerTip.text = string.Format(ids, _times);
			
			_elapsedTime = 0f;
			_times--;
		}
		
		if (_times < 0)
		{
			UIManager.Instance.CloseUI("BattleSummeryTimerUI");
			if(GameManager.Instance.CurrGameMode == GameManager.FC_GAME_MODE.SINGLE)
			{
				GotoSummary();
			}
			else if(GameManager.Instance.IsPVPMode)
			{
				GotoPvPSummary();
			}
		}
	}
	
	void GotoSummary()
	{
		LevelData levelData = LevelManager.Singleton.CurrentLevelData;
		
        if (levelData.worldName == "Activity")
        {
        }
		
		BattleSummary.Instance.FinishBattle();

		LevelManager.Singleton.DestroyTriggerManager();
    }
	
	void GotoPvPSummary()
	{
		PvPBattleSummary.Instance.NeedOpenSummaryForTime = true;
	}
}
