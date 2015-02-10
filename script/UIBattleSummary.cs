using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using InJoy.FCComm;

public class UIBattleSummary : GestureProcessor
{
	public GameObject itemPrefab;

	public GameObject backButton;

	public GameObject againButton;

	public UILabel labelLevelName;

	public UILabel labelCompleteTime;

	public UILabel labelScore;		//some kind of score

	private List<ItemInventory> _itemList;

	public GameObject[] gradeSprites;	//in reverse order: DCBAS

	public UIGrid grid;

	private GestureController _gestureController;

	void OnInitialize()
	{
		MessageController.Instance.Reset();

		if (InputManager.Instance == null)
		{
			return;
		}

		List<FCObject> clients = InputManager.Instance.Clients;

		foreach (FCObject client in clients)
		{
			CommandManager.Instance.Send(FCCommand.CMD.INPUT_KEY_RELEASE,
					FCConst.FC_KEY_DIRECTION, FC_PARAM_TYPE.INT,
					client.ObjectID, FCCommand.STATE.RIGHTNOW, true);
		}

		labelCompleteTime.text = UIUtils.GetBattleTimerString(BattleSummary.Instance.TimeConsumed);

		LevelData ld = LevelManager.Singleton.CurrentLevelData;

		labelLevelName.text = Localization.instance.Get(ld.levelNameIDS);

		_itemList = PlayerInfo.Instance.CombatInventory.itemList;

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


	// Use this for initialization
	void Start()
	{
		UIEventListener.Get(againButton).onClick = OnClickAgainButton;
		UIEventListener.Get(backButton).onClick = OnClickBackButton;

		SoundManager.Instance.PlaySoundEffect("victory");

		int index = (int)BattleSummary.Instance.currentLevelState - (int)EnumLevelState.D;

		for (int i = 0; i < 5; i++)
		{
			gradeSprites[i].SetActive(i == index);
		}
	}

	void OnClosePanel(GameObject button = null)
	{
		UIManager.Instance.CloseUI("FCUIBattleSummary");
	}

	void OnClickBackButton(GameObject button)
	{
		LevelManager.Singleton.ExitLevel();
		OnClosePanel();
	}

	void OnClickAgainButton(GameObject button)
	{
		UIManager.Instance.CloseUI("FCUIBattleSummary");
		LevelManager.Singleton.LoadLevelWithRandomConfig(LevelManager.Singleton.CurrentLevel, LevelManager.Singleton.CurrentDifficultyLevel);
	}
}