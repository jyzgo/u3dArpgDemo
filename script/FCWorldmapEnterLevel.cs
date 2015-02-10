using UnityEngine;

using System.Collections;
using System.Collections.Generic;

public class FCWorldmapEnterLevel : MonoBehaviour
{
	public GameObject itemPrefab;

	public UIGrid grid;

	public UILabel spendEnergy;

	public UILabel levelNameLabel;

	public GameObject closeButton;

	public GameObject enterLevelButton;

	private LevelData _levelData;

	private bool _buttonClicked;

	private float _lastClickTime;

	private List<GameObject> _lootItems;

	void Awake()
	{
		UIEventListener.Get(closeButton).onClick = OnClickCloseButton;
		UIEventListener.Get(enterLevelButton).onClick = OnEnterLevel;
	}

	void OnInitializeWithCaller(string lvName)
	{
		_levelData = LevelManager.Singleton.LevelsConfig.GetLevelDataByLevelName(lvName);
		levelNameLabel.text = Localization.Localize(_levelData.levelNameIDS);

		//localize
		spendEnergy.text = string.Format(Localization.instance.Get("IDS_MESSAGE_MAPINFO_VITALITYCOST"), _levelData.vitalityCost);
		
		//show possible loots
		FillGrid();

		StartCoroutine(UpdateVitTextColor());

        UIManager.Instance.CloseUI("TownHome");
	}

	private IEnumerator UpdateVitTextColor()
	{
		while (true)
		{
			if (PlayerInfo.Instance.Vitality < _levelData.vitalityCost)
			{
				if (spendEnergy.color != FCConst.k_color_red)
				{
					spendEnergy.color = FCConst.k_color_red;
				}
			}
			else
			{
				if (spendEnergy.color != FCConst.k_color_green)
				{
					spendEnergy.color = FCConst.k_color_green;
					break;
				}
			}
			yield return new WaitForSeconds(1f);
		}
	}

	private void FillGrid()
	{
		_lootItems = new List<GameObject>();

		string[] itemIDs = _levelData.lootList.Split(';');
		foreach (string itemID in itemIDs)
		{
			ItemData itemData = DataManager.Instance.GetItemData(itemID);

			if (itemData != null)
			{
                GameObject t = NGUITools.AddChild(grid.gameObject, itemPrefab);
                t.transform.localPosition = itemPrefab.transform.localPosition;
				//Transform t = Utils.InstantiateGameObjectWithParent(itemPrefab, grid.transform);

				t.GetComponent<UICommonDisplayItem>().SetData(itemData);

				_lootItems.Add(t.gameObject);
			}
		}

		grid.Reposition();
	}

	void OnClickCloseButton(GameObject gameobject)
	{
		UIManager.Instance.CloseUI("FCEnterLevel");
		if (!UIManager.Instance.IsUIOpened("FCWorldmapLevelSelect"))
		{
			UIManager.Instance.OpenUI("TownHome");
			CameraController.Instance.MainCamera.gameObject.SetActive(true);
		}
	}

	void OnDisable()
	{
		foreach(GameObject go in _lootItems)
		{
			Destroy(go);
		}
	}

	void OnEnterLevel(GameObject go)
	{
		if (Time.realtimeSinceStartup - _lastClickTime > 1f)  //at most 1 click/second
		{
			_buttonClicked = false;
		}

		if (!_buttonClicked)
		{
			_lastClickTime = Time.realtimeSinceStartup;

			_buttonClicked = true;

			GameManager.Instance.CurrGameMode = GameManager.FC_GAME_MODE.SINGLE;

			LevelManager.Singleton.LoadLevelWithRandomConfig(_levelData.levelName, WorldMapController.DifficultyLevel);
		}
	}
}
