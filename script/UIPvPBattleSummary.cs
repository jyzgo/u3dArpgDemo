using UnityEngine;
using System.Collections;

public class UIPvPBattleSummary : MonoBehaviour {
	
	public GameObject _winnerTitle;
	public GameObject _defeatedTitle;
	
	public UILabel _totalScore;
	public UILabel[] _detialLabelList;
	public UISummaryPlayerCard[] _playerCards;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnInitialize()
	{
		PvPSummaryBattle();
	}
	
	void OnDisable()
	{
	}
	
	void OnBack()
	{
		LevelManager.Singleton.ExitLevel();
	}
	
	void OnPlayAgain()
	{
	}

	public void PvPSummaryBattle()
	{
		PvPPlayerSummaryData sd = PvPBattleSummary.Instance.PlayerResult[MatchPlayerManager.Instance.GetPlayerIndex()];
		bool wt = sd._result == "win";
		bool dt = sd._result == "lose";
		_winnerTitle.SetActive(wt);
		_defeatedTitle.SetActive(dt);
		_totalScore.text = Localization.instance.Get("IDS_TOTAL_SCORE") + sd._TotalLP;
		
		for (int i = 0; i < _playerCards.Length; i ++)
		{
			//OLD_PlayerInfo pi = MatchPlayerManager.Instance.GetMatchPlayerProfile(i)._playerInfo;
			//_playerCards[i]._portrait.spriteName = rolIcons[pi._class];
			//_playerCards[i]._playerName.text = pi.DisplayNickname;
			//sd = PvPBattleSummary.Instance.PlayerResult[i];
			//_playerCards[i]._result.text = Localization.instance.Get("IDS_" + sd._result.ToUpper());
			//bool isMyselft = i == MatchPlayerManager.Instance.GetPlayerIndex();
			//_playerCards[i]._winnerBG.gameObject.SetActive(isMyselft);
		}

		for(int i = 0; i < _detialLabelList.Length; i ++)
		{
			if(i == 0)
			{
				_detialLabelList[i].gameObject.SetActive(true);
				_detialLabelList[i].text = Localization.instance.Get("IDS_KILLS")
					+ MultiplayerDataManager.Instance.PvPStatisticsList[MatchPlayerManager.Instance.GetPlayerIndex()].Kills.ToString();
			}
			else if(i == 1)
			{
				_detialLabelList[i].gameObject.SetActive(true);
				_detialLabelList[i].text = Localization.instance.Get("IDS_DEATHS")
					+ MultiplayerDataManager.Instance.PvPStatisticsList[MatchPlayerManager.Instance.GetPlayerIndex()].Deaths.ToString();
			}
			else
			{
				_detialLabelList[i].gameObject.SetActive(false);
			}
		}
	}
}
