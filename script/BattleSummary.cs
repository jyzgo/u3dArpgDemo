using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using InJoy.FCComm;
using InJoy.Utils;
using InJoy.RuntimeDataProtection;

public class BattleSummary : MonoBehaviour
{
	private LevelConfig _levelConfig;
	private float _startTime;
	private float _consumeTime;

	public float TimeConsumed
	{
		get
		{
			if (!IsFinish)
			{
				return (Time.time - _startTime);
			}
			else
			{
				return _consumeTime;
			}
		}
	}

	public List<string> _chestList = new List<string>();

	public delegate void OnCancelBattle(bool succeeded);

	private OnCancelBattle _abortBattleCallback;
	private int _baseScore;

	public int BaseScore
	{
		get
		{
			return _baseScore;
		}
	}

	private int _completeTimeScore;

	public int CompleteTimeScore
	{
		get
		{
			return _completeTimeScore;
		}
	}

	public int RageUnleashed { get; set; }

	public int _coinsEarned = 0;
	public int _expEarned = 0;

	public int CoinsEarned
	{
		get { return _coinsEarned; }
		set { _coinsEarned = value; }
	}

	public int ExpEarned
	{
		get { return _expEarned; }
		set { _expEarned = value; }
	}

	public int HpCost { get; set; }

	public int MpCost { get; set; }

	public int HcCost { get; set; }

	public int RpCost { get; set; }

	public bool IsFinish { get; set; }

	public int ReviveUsed { get; set; }

	public int VPEarned { get; set; }

	private int _accumulativeHurt = 0;

	public int DamageTaken
	{
		get
		{
			return _accumulativeHurt;
		}
		set
		{
			_accumulativeHurt += value;
		}
	}

	private bool _newDifficultyOpened;

	public bool NewDifficultyOpened { get { return _newDifficultyOpened; } }

	private bool _newCheckpointReached;  //for endless level mode
	public bool NewCheckpointReached { get { return _newCheckpointReached; } }

	public Dictionary<string, int> _skillUses = new Dictionary<string, int>();

	public void UseSkill(string skill)
	{
		if (_skillUses.ContainsKey(skill))
		{
			_skillUses[skill]++;
		}
		else
		{
			_skillUses.Add(skill, 1);
		}
	}

	#region Singleton
	private static BattleSummary _instance = null;

	public static BattleSummary Instance
	{
		get
		{
			return _instance;
		}
	}

	void Awake()
	{
		_completeTimeScore = 0;

		if (_instance != null)
		{
			Debug.LogError("BattleSummary: Another GameLauncher has already been created previously. " + gameObject.name + " is goning to be destroyed.");

			Destroy(this);

			return;
		}

		_instance = this;
	}
	#endregion

	void OnDestroy()
	{
		_instance = null;
		if (LootManager.Instance != null)
		{
			LootManager.Instance.ClearLootObject();
		}
	}

	// Use this for initialization
	void Start()
	{
		if (GameManager.Instance.IsPVPMode)
		{
			this.enabled = false;
			return;
		}
		_levelConfig = LevelManager.Singleton.LevelsConfig;
	}

	public void BeginBattle()
	{
		_startTime = Time.time;

		ReviveUsed = 0;
		RageUnleashed = 0;
		CoinsEarned = 0;
		ExpEarned = 0;
		VPEarned = 0;
		HpCost = 0;
		MpCost = 0;
		HcCost = 0;
		RpCost = 0;
		_skillUses.Clear();

		IsFinish = false;

		_newDifficultyOpened = false;
	}

	public void FinishBattle()
	{
		ReviveUsed = 0;

		// Sub 5 Sec which used to counter.
		_consumeTime = Time.time - _startTime - 5f;

		_consumeTime = (int)_consumeTime;

		SummaryBattle();
		IsFinish = true;

		_newCheckpointReached = false;

		ConnectionManager.Instance.RegisterHandler(SendBattleSummary, true);

		QuestManager.instance.SaveQuestProgress();
	}

	public enum EnumBattleQuitSource
	{
		In_game_quit,
		Loot_screen,
		Died,
		Full_of_pack,       //exceeds bag max
		Out_of_time
	}

	public void AbortBattle(OnCancelBattle callback)
	{
		ReviveUsed = 0;

		IsFinish = false;
		_abortBattleCallback = callback;


		if (NetworkManager.isOfflinePlay)
		{
			callback(true);
		}
		else
		{
			ConnectionManager.Instance.RegisterHandler(SendAbortBattle, true);
		}

	}


	private void SendAbortBattle()
	{
		BattleAbortRequest request = new BattleAbortRequest();

		request.levelID = LevelManager.Singleton.CurrentLevelData.id;

		request.exp = _expEarned;

		NetworkManager.Instance.SendCommand(request, OnBattleAborted);
	}


	private void SendBattleSummary()
	{
		BattleEndRequest request = new BattleEndRequest();

		request.levelID = LevelManager.Singleton.CurrentLevelData.id;

		request.levelState = currentLevelState;

		request.itemList = PlayerInfo.Instance.CombatInventory.itemList;

		request.exp = ExpEarned;

		NetworkManager.Instance.BattleEnd(request, OnSendBattleSummary);
	}

	public void OnSendBattleSummary(FaustComm.NetResponse msg)
	{
		if (msg.Succeeded)
		{
			ConnectionManager.Instance.SendACK(SendBattleSummary, true);

			UpdateInforResponseData updateData = (msg as BattleEndResponse).updateData;

			if (updateData != null)
			{
				PlayerInfo.Instance.ApplyPlayerPropChanges(updateData.playerPropsList.ToArray());
				
				PlayerInfo.Instance.PlayerInventory.ApplyItemCountChanges (updateData.itemCountOps);
			}

			updateData.Broadcast();

			StartBattleSummary();
		}
		else
		{
			//show error only
			UIMessageBoxManager.Instance.ShowErrorMessageBox(msg.errorCode, null);

			Debug.LogError("Error passing server check.");
		}
	}

	private void OnBattleAborted(FaustComm.NetResponse msg)
	{
		ConnectionManager.Instance.SendACK(SendAbortBattle, true);

		if (msg.Succeeded)
		{
			if (!IsFinish && _abortBattleCallback != null)
			{
				_abortBattleCallback(true);
			}	
		}
		else
		{
			//show error only
			UIMessageBoxManager.Instance.ShowMessageBox(Utils.GetErrorIDS(msg.errorCode), null, MB_TYPE.MB_OK, null);
		
			if (!IsFinish && _abortBattleCallback != null)
			{
				_abortBattleCallback(false);
			}
		}
	}


	private void SummaryBattle()
	{
		BattleGrade(_completeTimeScore);

		MessageController.Instance.Reset();

		PlayerInfo.Instance.SaveTutorialProgress();

		PlayerInfo.Instance.ChangeLevelState(LevelManager.Singleton.CurrentLevel, LevelManager.Singleton.CurrentDifficultyLevel, currentLevelState);
	}

	public EnumLevelState currentLevelState;	//of the level that just finished, not written to server yet.

	private void BattleGrade(int score)
	{
		string levelName = LevelManager.Singleton.CurrentLevel;
		
		LevelData ld = _levelConfig.GetLevelData(levelName);
		
		if (_consumeTime <= ld.minTime)
		{
			currentLevelState = EnumLevelState.S;
		}
		else
		if (_consumeTime <= ld.minTime + (ld.maxTime - ld.minTime) / 3)
		{
			currentLevelState = EnumLevelState.A;
		}
		else
		if (_consumeTime <= ld.minTime + (ld.maxTime - ld.minTime) * 2 / 3)
		{
			currentLevelState = EnumLevelState.B;
		}
		else
		if (_consumeTime < ld.maxTime)
		{
			currentLevelState = EnumLevelState.C;
		}
		else
		{
			currentLevelState = EnumLevelState.D;
		}
	}

	[ContextMenu("BattleSummary")]
	public void StartBattleSummary()
	{
		UIManager.Instance.CloseUI("HomeUI");

		UIManager.Instance.OpenUI("UIBattleSummary");
	}

	/// <summary>
	/// Saves the battle loot to inventory. They are in battle inventory during battle.
	/// </summary>
	private void SaveBattleLootToInventory()
	{
		List<ItemInventory> battleItemsList = PlayerInfo.Instance.CombatInventory.itemList;

		PlayerInventory playerInventory = PlayerInfo.Instance.PlayerInventory;

		foreach (ItemInventory item in battleItemsList)
		{
			playerInventory.AddItemInventory(item.ItemData, item.Count);
		}
	}
}

