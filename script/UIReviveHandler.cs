using UnityEngine;
using System.Collections.Generic;

public class UIReviveHandler : MonoBehaviour
{
	public GameObject itemPrefab; 
	public UIGrid grid;
	public UILabel labelAmount;
	public GameObject hcIcon;
	public GameObject revivePotionIcon;
	public GameObject reviveArrow;

	private List<ItemInventory> _itemList = new List<ItemInventory>();
	private bool _hasQuitLevelClicked = false; //prevent from click quit twice

	// Use this for initialization
	void Start()
	{
	}

	//called by OpenUI
	void OnInitialize()
	{
		_itemList = PlayerInfo.Instance.CombatInventory.itemList;

		// Set revive button state.
		PlayerInventory inventory = PlayerInfo.Instance.PlayerInventory;
		int count = inventory.GetItemCount(FCConst.k_potion_revive);

		int playerLevel = PlayerInfo.Instance.CurrentLevel;
		PLevelData playerData = DataManager.Instance.GetCurrentClassLevelData(playerLevel);

		revivePotionIcon.SetActive(count > 0);

		hcIcon.SetActive(count <= 0);

		labelAmount.text = count > 0 ? string.Format("{0}/1", count) : playerData._reviveHc.ToString();

		InitTutorialEffect();

		//todo: BeginTutorial(TutorialManager._level_revive_100);

		FillGrid();
	}

	void FillGrid()
	{
		foreach (ItemInventory item in _itemList)
		{
			Transform t = Utils.InstantiateGameObjectWithParent(itemPrefab, grid.transform);

			t.GetComponent<UICommonDisplayItem>().SetData(item);
		}

		grid.repositionNow = true;
	}

	// Quit level
	void OnClickQuitLevel()
	{
		if (_hasQuitLevelClicked)
			return;

		LevelManager.Singleton.ExitLevel();	//using simple way to exitLevel.
	}

	//callback when save OK
	private void OnSaveOK(bool succeeded)
	{
		UIManager.Instance.CloseUI("BattleReviveUI");
		//exit to level
		LevelManager.Singleton.ExitLevel();
	}

	void OnQuitCallback(ID_BUTTON buttonID)
	{
		if (buttonID == ID_BUTTON.ID_OK)
		{
			_hasQuitLevelClicked = true;

			Debug.Log("OnClickQuitLevel " + GameManager.Instance.GameState.ToString());

			GiveUpRevive();

			PlayerInfo.Instance.CombatInventory.Clear();

			//save game data, exit to town when save success
			BattleSummary.Instance.AbortBattle(OnSaveOK);
		}
	}

	// Revive: Free daily, Potion or HC
	void OnClickRevive()
	{
		FinishTutorial();

		bool canRevive = false;
		PlayerInventory inventory = PlayerInfo.Instance.PlayerInventory;
		int count = inventory.GetItemCount(FCConst.k_potion_revive);

		int playerLevel = PlayerInfo.Instance.CurrentLevel;
		PLevelData playerData = DataManager.Instance.GetCurrentClassLevelData(playerLevel);


		if (count > 0)
		{
			inventory.UseItem(FCConst.k_potion_revive);
			canRevive = true;
			++BattleSummary.Instance.RpCost;
			NetworkManager.Instance.ReviveInBattle(LevelManager.Singleton.CurrentLevelData.id, FCConst.k_potion_revive, 1, 0, OnTakePotion);
		}
		else if (PlayerInfo.Instance.HardCurrency >= playerData._reviveHc)
		{
			PlayerInfo.Instance.ReduceHardCurrency(playerData._reviveHc);

			BattleSummary.Instance.ReviveUsed++;

			canRevive = true;

			NetworkManager.Instance.ReviveInBattle(LevelManager.Singleton.CurrentLevelData.id, FCConst.k_potion_revive, 1, 1, OnTakePotion);
		}
		else
		{
			UIMessageBoxManager.Instance.ShowErrorMessageBox(5001, string.Empty);
		}

		if (canRevive)
		{
			UIManager.Instance.CloseUI("BattleReviveUI");
			if (GameManager.Instance.CurrGameMode == GameManager.FC_GAME_MODE.SINGLE)
			{
				UIManager.Instance.OpenUI("HomeUI");
				//ObjectManager.Instance.GetMyActionController().GoToRevive();

				CommandManager.Instance.Send(FCCommand.CMD.REVIVE,
					null, FC_PARAM_TYPE.NONE,
					null, FC_PARAM_TYPE.NONE,
					null, FC_PARAM_TYPE.NONE,
					ObjectManager.Instance.GetMyActionController().ObjectID,
					FCCommand.STATE.RIGHTNOW, true);
			}
			else if (GameManager.Instance.IsPVPMode)
			{
				PvPBattleSummary.Instance.ReviveMySelf();
			}

		}
	}


	public void InitTutorialEffect()
	{
		reviveArrow.SetActive(false);
	}


	public void BeginTutorial(string tutorialId)
	{
		if (LevelManager.Singleton.IsTutorialLevel())
		{
			return; //don't active this tutorial in "tutorial level".
		}

		if (PlayerInfo.Instance.GetTutorialState(EnumTutorial.Battle_Revive) != EnumTutorialState.Finished)
		{
			reviveArrow.SetActive(true);
		}
	}

	public void FinishTutorial()
	{
		reviveArrow.SetActive(false);
		TutorialManager.Instance.TryFinishTutorialLevel(EnumTutorial.Battle_Revive);
	}

	public void GiveUpRevive()
	{
		if (PlayerInfo.Instance.GetTutorialState(EnumTutorial.Battle_Revive) != EnumTutorialState.Finished)
		{
			reviveArrow.SetActive(false);

			EnumTutorialState count = PlayerInfo.Instance.GetTutorialState(EnumTutorial.Battle_Revive);
			count++;
			PlayerInfo.Instance.ChangeTutorialState(EnumTutorial.Battle_Revive, count);
			if (count > EnumTutorialState.Finished)
			{
				TutorialManager.Instance.TryFinishTutorialLevel(EnumTutorial.Battle_Revive);
			}
			else
			{
				PlayerInfo.Instance.ChangeTutorialState(EnumTutorial.Battle_Revive, EnumTutorialState.Inactive); //inactive
			}
		}
	}

	void OnTakePotion(FaustComm.NetResponse msg)
	{
		if (msg.Succeeded)
		{
			UpdateInforResponseData updateData = (msg as BattleUseReviveResponse).updateData;

			if (updateData != null)
			{
				PlayerInfo.Instance.PlayerInventory.ApplyItemCountChanges(updateData.itemCountOps);
			}
		}
		else
		{
			//remember this, to fail this battle
			LevelManager.Singleton.IsCheat = true;

			Debug.LogError("Cheat on taking potions! Error: " + msg.errorCode);
		}
	}
}
