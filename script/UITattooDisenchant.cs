using UnityEngine;

using System;
using System.Collections.Generic;

public class UITattooDisenchant : MonoBehaviour
{
	public GameObject recipeItemPrefab;

	public GameObject materialItemPrefab;

	public UIGrid recipeGrid;

	public GameObject labelNoRecipe;
	public GameObject labelNoRecipe2;
	
	public UILabel labelName;
	public UILabel labelPos;
	public UILabel labelBearingPoint;
	public UILabel labelEffect;
	public UILabel labelAmount;
	public UILabel labelNeedSC;

	public UIGrid materialGrid;

	public GameObject buttonTrade;

	private List<UIRecipeItem4> _recipeItemList;

	private List<UITattooMaterialItem> _tmList = new List<UITattooMaterialItem>();

	private UIRecipeItem4 _focusItem;

	void OnEnable()
	{
		FillRecipeGrid();
	}

	void FillRecipeGrid()
	{
		if (_recipeItemList == null)
		{
			_recipeItemList = new List<UIRecipeItem4>();
		}

		int level = -1;

		List<ItemInventory> recipeItemList = PlayerInfo.Instance.PlayerInventory.GetItemLisyByType(ItemType.tattoo);

		//change to recipe list, some items need to be merged

		List<string> tattooNameList = new List<string>();

		List<TattooData> tdList = new List<TattooData>();

		foreach (ItemInventory item in recipeItemList)
		{
			if (!tattooNameList.Contains(item.ItemID))
			{
				tattooNameList.Add(item.ItemID);

				tdList.Add(DataManager.Instance.GetTattooData(item.ItemID));
			}
		}

		tdList.Sort(new TattooDataList.TattooDataComparer());

		foreach (TattooData td in tdList)
		{
			GameObject go;

			UIRecipeItem4 uiItem;

			if (td.level > level)	//extra summary item
			{
				go = NGUITools.AddChild(recipeGrid.gameObject, recipeItemPrefab);

				uiItem = go.GetComponent<UIRecipeItem4>();

				level = td.level;

				uiItem.name = string.Format("{0}: Summary", td.ord);

				uiItem.IsSummaryLine = true;

				uiItem.SetData(td, 0, this);

				_recipeItemList.Add(uiItem);

				//Debug.LogError(string.Format("Summary -- level {0} created.", level));
			}

			//normal item, always needs to add
			go = NGUITools.AddChild(recipeGrid.gameObject, recipeItemPrefab);

			go.name = string.Format("{0}: {1}", td.ord, td.tattooID);

			uiItem = go.GetComponent<UIRecipeItem4>();

			uiItem.SetData(td, PlayerInfo.Instance.PlayerInventory.GetItemCount(td.tattooID), this);

			_recipeItemList.Add(uiItem);
		}

		recipeGrid.repositionNow = true;

		if (_recipeItemList.Count > 0)
		{
			buttonTrade.GetComponent<UIImageButton>().isEnabled = true;
			labelNoRecipe.SetActive(false);
			labelNoRecipe2.SetActive(false);

			labelName.transform.parent.gameObject.SetActive(true);

			SetFocus(_recipeItemList[1]);
		}
		else
		{
			buttonTrade.GetComponent<UIImageButton>().isEnabled = false;
			labelNoRecipe.SetActive(true);
			labelNoRecipe2.SetActive(true);

			labelName.transform.parent.gameObject.SetActive(false);
		}
	}

	public void ExpandLevel(int level)
	{
		foreach (UIRecipeItem4 item in _recipeItemList)
		{
			if (item.ttData.level == level && !item.IsSummaryLine)
			{
				item.gameObject.SetActive(true);
			}
		}

		recipeGrid.Reposition();
	}

	public void CollapseLevel(int level)
	{
		foreach (UIRecipeItem4 item in _recipeItemList)
		{
			if (item.ttData.level == level && !item.IsSummaryLine)
			{
				item.gameObject.SetActive(false);
			}
		}

		recipeGrid.Reposition();
	}

	public void SetFocus(UIRecipeItem4 item)
	{
		if (_focusItem != null)
		{
			_focusItem.OnFocused(false);
		}

		_focusItem = item;

		_focusItem.OnFocused(true);

		DisplayRecipeInfo();
	}

	void DisplayRecipeInfo()
	{
		ItemData itemData = DataManager.Instance.GetItemData(_focusItem.ttData.tattooID);

		labelName.text = _focusItem.ttData.GetDisplayName();

		labelPos.text = Utils.GetTattooApplicablePositions(_focusItem.ttData.applicableParts);

		labelBearingPoint.text = _focusItem.ttData.bearingPoint.ToString();

		labelEffect.text = Localization.instance.Get(itemData.descriptionIds);

		labelAmount.text = PlayerInfo.Instance.PlayerInventory.GetItemCount(_focusItem.ttData.tattooID).ToString();

		FillTradeItemGrid();
	}


	void FillTradeItemGrid()
	{
		string recipeID = _focusItem.ttData.tattooID;

		TattooExchangeData exchangeData = DataManager.Instance.tattooExchangeDataList.dataList.Find(delegate(TattooExchangeData ted)
		{
			return ted.id == recipeID;
		});

		if (PlayerInfo.Instance.SoftCurrency >= exchangeData.costSC)
		{
			labelNeedSC.text = exchangeData.costSC.ToString();
		}
		else
		{
			labelNeedSC.text = string.Format("[ff0000]{0}", exchangeData.costSC);
		}

		int index = 0;

		foreach (MatAmountMapping mapping in exchangeData.materials)
		{
			UITattooMaterialItem uiItem;

			if (index < _tmList.Count)
			{
				uiItem = _tmList[index];
			}
			else
			{
				GameObject go = NGUITools.AddChild(materialGrid.gameObject, materialItemPrefab);

				uiItem = go.GetComponent<UITattooMaterialItem>();

				_tmList.Add(uiItem);
			}

			uiItem.SetData(mapping.materialName, mapping.amount);

			uiItem.gameObject.SetActive(true);

			index++;
		}

		while (index < _tmList.Count)
		{
			_tmList[index++].gameObject.SetActive(false);
		}

		materialGrid.repositionNow = true;
	}

	public void OnDisable()
	{
		if (_recipeItemList == null)
		{
			return;
		}
		//remove grid items
		foreach (UIRecipeItem4 item in _recipeItemList)
		{
			Destroy(item.gameObject);
		}

		_recipeItemList.Clear();

		_recipeItemList = null;

		recipeGrid.Reposition();
	}

	private void OnClickDisenchant()
	{
		//check inventory space, make sure we have at least 4 empty slots
		if (PlayerInfo.Instance.PlayerInventory.itemList.Count <= PlayerInfo.Instance.InventoryCount - 4)
		{
			NetworkManager.Instance.SendCommand(new TattooRecipeRequest(_focusItem.ttData.tattooID, 4, 0), OnDisenchant);
		}
		else
		{
			UIMessageBoxManager.Instance.ShowMessageBox(Localization.instance.Get("IDS_NOT_ENOUGH_BAG_SPACE"), null, MB_TYPE.MB_OKCANCEL, OnMessageBoxNotEnoughSpaceCallback);
		}
	}

	private void OnMessageBoxNotEnoughSpaceCallback(ID_BUTTON buttonID)
	{
		FCUIInventoryItemList.OnClickIncrementButton();
	}


	private void OnDisenchant(FaustComm.NetResponse msg)
	{
		if (msg.Succeeded)
		{
			TattooRecipeResponse myMsg = msg as TattooRecipeResponse;

			UpdateInforResponseData updateData = myMsg.updateData;

			updateData.Broadcast();

			this.OnDisable();

			this.OnEnable();

			UIMessageBoxManager.Instance.ShowMessageBox(Localization.instance.Get("IDS_BUTTON_TATTOO_DISENCHANTITEMLIST"), null, MB_TYPE.MB_OK_WITH_ITEMS, null, updateData.itemUpdateList);
		}
		else
		{
			UIMessageBoxManager.Instance.ShowErrorMessageBox(msg.errorCode, "Tattoo");
		}
	}
}

