using UnityEngine;
using System.Collections;

public class UIEnterActivityLevel : MonoBehaviour {
	
	public string _levelName;
	
	void OnClickClose()
	{
		UIManager.Instance.CloseUI("UIActivityLevel");
	}
	
	void OnEnterLevel()
	{
		UIManager.Instance.CloseUI("UIActivityLevel");
		
        GameManager.Instance.CurrGameMode = GameManager.FC_GAME_MODE.SINGLE;
        LevelManager.Singleton.LoadLevelWithRandomConfig(_levelName, PlayerInfo.Instance.difficultyLevel);
	}
}
