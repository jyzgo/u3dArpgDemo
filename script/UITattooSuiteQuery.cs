using UnityEngine;

using System;
using System.Collections.Generic;

public class UITattooSuiteQuery : MonoBehaviour
{
	public GameObject suiteItemPrefab;

	public GameObject recipeItem2Prefab;

	public UIGrid suiteGrid;

	public UILabel labelName;
	public UILabel labelPos;
	public UILabel labelBearingPoint;
	public UILabel labelAmount;
	public UILabel labelSuiteEffect;

	public UIGrid recipeListGrid;

	private List<UITattooSuiteItem> _suiteItemList;

	private List<UIRecipeItem2> _tmList = new List<UIRecipeItem2>();

	private UITattooSuiteItem _focusItem;


	void OnEnable()
	{
		if (_suiteItemList == null)
		{
			FillSuiteGrid();
		}
	}

	void FillSuiteGrid()
	{
		_suiteItemList = new List<UITattooSuiteItem>();

		List<TattooSuiteData> allSuites = DataManager.Instance.tattooSuiteDataList.dataList;

		int level = -1;

		foreach (TattooSuiteData tsd in allSuites)
		{
			UITattooSuiteItem uiItem;
			
			GameObject go;

			if (tsd.level > level)
			{
				go = NGUITools.AddChild(suiteGrid.gameObject, suiteItemPrefab);

				uiItem = go.GetComponent<UITattooSuiteItem>();

				level = tsd.level;

				uiItem.name = string.Format("{0}: Summary", tsd.ord);

				uiItem.IsSummaryLine = true;

				uiItem.SetData(tsd, this);

				_suiteItemList.Add(uiItem);

				//Debug.LogError(string.Format("Summary -- level {0} created.", level));
			}

			go = NGUITools.AddChild(suiteGrid.gameObject, suiteItemPrefab);

			go.name = string.Format("{0}: {1}", tsd.ord, tsd.suiteID);

			uiItem = go.GetComponent<UITattooSuiteItem>();

			uiItem.SetData(tsd, this);

			_suiteItemList.Add(uiItem);
		}

		suiteGrid.Reposition();

		SetFocus(_suiteItemList[1]);
	}

	public void SetFocus(UITattooSuiteItem item)
	{
		if (_focusItem != null)
		{
			_focusItem.OnFocused(false);
		}

		_focusItem = item;

		_focusItem.OnFocused(true);

		DisplaySuiteInfo();
	}

	void DisplaySuiteInfo()
	{
		labelName.text = Localization.instance.Get(_focusItem.suiteData.nameIDS);

		labelName.color = FCConst.RareColorMapping[(EnumRareLevel)_focusItem.suiteData.rareLevel];

		labelPos.text = Utils.GetTattooApplicablePositions(DataManager.Instance.GetTattooSuiteApplicablePositions(_focusItem.suiteData));

		labelBearingPoint.text = DataManager.Instance.GetTattooSuiteBearPoints(_focusItem.suiteData).ToString();

		labelAmount.text = _focusItem.suiteData.tdList.Count.ToString();

		labelSuiteEffect.text = Localization.instance.Get(_focusItem.suiteData.descIDS);

		FillRecipeListGrid();
	}

	
	void FillRecipeListGrid()
	{
		TattooSuiteData tsd = _focusItem.suiteData;

		int index = 0;

		//tattoo suites
		foreach(string tattooID in tsd.tdList)
		{
			UIRecipeItem2 uiItem;

			if (index < _tmList.Count)
			{
				uiItem = _tmList[index];
			}
			else
			{
				GameObject go = NGUITools.AddChild(recipeListGrid.gameObject, recipeItem2Prefab);

				uiItem = go.GetComponent<UIRecipeItem2>();

				_tmList.Add(uiItem);
			}

			int amount = PlayerInfo.Instance.PlayerInventory.GetItemCount(tattooID) + PlayerInfo.Instance.playerTattoos.GetTattooCount(tattooID);

			uiItem.SetData(tattooID, amount);

			uiItem.gameObject.SetActive(true);

			index++;
		}

		while (index < _tmList.Count)
		{
			_tmList[index++].gameObject.SetActive(false);
		}

		recipeListGrid.Reposition();
	}

	public void ExpandLevel(int level)
	{
		foreach (UITattooSuiteItem item in _suiteItemList)
		{
			if (item.suiteData.rareLevel == level && !item.IsSummaryLine)
			{
				item.gameObject.SetActive(true);
			}
		}

		suiteGrid.Reposition();
	}

	public void CollapseLevel(int level)
	{
		foreach (UITattooSuiteItem item in _suiteItemList)
		{
			if (item.suiteData.rareLevel == level && !item.IsSummaryLine)
			{
				item.gameObject.SetActive(false);
			}
		}

		suiteGrid.Reposition();
	}


	public void Clear()
	{
		if (_suiteItemList == null)
		{
			return;
		}
		//remove grid items
		foreach (UITattooSuiteItem item in _suiteItemList)
		{
			Destroy(item.gameObject);
		}

		_suiteItemList.Clear();

		_suiteItemList = null;

		suiteGrid.Reposition();
	}
}

