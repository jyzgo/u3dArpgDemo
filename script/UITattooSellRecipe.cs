using UnityEngine;

using System;
using System.Collections.Generic;

public class UITattooSellRecipe : MonoBehaviour
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

	public UIGrid materialGrid;

	public GameObject buttonTrade;

	private List<UIRecipeItem3> _recipeItemList;

	private List<UITattooMaterialItem> _tmList = new List<UITattooMaterialItem>();

	private UIRecipeItem3 _focusItem;

	void OnEnable()
	{
		FillRecipeGrid();
	}

	void FillRecipeGrid()
	{
		if (_recipeItemList == null)
		{
			_recipeItemList = new List<UIRecipeItem3>();
		}

		int level = -1;

		List<ItemInventory> recipeItemList = PlayerInfo.Instance.PlayerInventory.GetItemLisyByType(ItemType.recipe);

		//change to recipe list, some items need to be merged

		List<string> recipeNameList = new List<string>();

		List<TattooData> tdList = new List<TattooData>();

		foreach (ItemInventory item in recipeItemList)
		{
			if (!recipeNameList.Contains(item.ItemID))
			{
				recipeNameList.Add(item.ItemID);

				tdList.Add(DataManager.Instance.GetTattooDataByRecipeID(item.ItemID));
			}
		}

		tdList.Sort(new TattooDataList.TattooDataComparer());

		foreach (TattooData td in tdList)
		{
			GameObject go;

			UIRecipeItem3 uiItem;

			if (td.level > level)	//extra summary item
			{
				go = NGUITools.AddChild(recipeGrid.gameObject, recipeItemPrefab);

				uiItem = go.GetComponent<UIRecipeItem3>();

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

			uiItem = go.GetComponent<UIRecipeItem3>();

			uiItem.SetData(td, PlayerInfo.Instance.PlayerInventory.GetItemCount(td.recipeID), this);

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
		foreach (UIRecipeItem3 item in _recipeItemList)
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
		foreach (UIRecipeItem3 item in _recipeItemList)
		{
			if (item.ttData.level == level && !item.IsSummaryLine)
			{
				item.gameObject.SetActive(false);
			}
		}

		recipeGrid.Reposition();
	}
	public void SetFocus(UIRecipeItem3 item)
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
		ItemData itemData = DataManager.Instance.GetItemData(_focusItem.ttData.recipeID);

		labelName.text = Localization.instance.Get(itemData.nameIds);

		labelName.color = FCConst.RareColorMapping[(EnumRareLevel)itemData.rareLevel];

		labelPos.text = Utils.GetTattooApplicablePositions(_focusItem.ttData.applicableParts);

		labelBearingPoint.text = _focusItem.ttData.bearingPoint.ToString();

		itemData = DataManager.Instance.GetItemData(_focusItem.ttData.tattooID);

		labelEffect.text = Localization.instance.Get(itemData.descriptionIds);

		labelAmount.text = PlayerInfo.Instance.PlayerInventory.GetItemCount(_focusItem.ttData.tattooID).ToString();

		FillTradeItemGrid();
	}


	void FillTradeItemGrid()
	{
		string recipeID = _focusItem.ttData.recipeID;

		TattooExchangeData exchangeData = DataManager.Instance.tattooExchangeDataList.dataList.Find(delegate(TattooExchangeData ted)
		{
			return ted.id == recipeID;
		});

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

		materialGrid.Reposition();
	}

	public void OnDisable()
	{
		if (_recipeItemList == null)
		{
			return;
		}
		//remove grid items
		foreach (UIRecipeItem3 item in _recipeItemList)
		{
			Destroy(item.gameObject);
		}

		_recipeItemList.Clear();

		_recipeItemList = null;

		recipeGrid.Reposition();
	}

	private void OnClickTradeButton()
	{
		//check inventory space, make sure we have at least 4 empty slots
		if (PlayerInfo.Instance.PlayerInventory.itemList.Count <= PlayerInfo.Instance.InventoryCount - 4)
		{
			NetworkManager.Instance.SendCommand(new TattooRecipeRequest(_focusItem.ttData.tattooID, 3, 0), OnTradeRecipe);
		}
		else
		{
			UIMessageBoxManager.Instance.ShowErrorMessageBox(5003, "Tattoo");
		}
	}

	private void OnMessageBoxNotEnoughSpaceCallback(ID_BUTTON buttonID)
	{
		FCUIInventoryItemList.OnClickIncrementButton();
	}

	private void TradeRecipe(string tattooID, byte useHC)
	{
		NetworkManager.Instance.SendCommand(new TattooRecipeRequest(tattooID, 2, useHC), OnTradeRecipe);
	}

	private void OnTradeRecipe(FaustComm.NetResponse msg)
	{
		if (msg.Succeeded)
		{
			TattooRecipeResponse myMsg = msg as TattooRecipeResponse;

			UpdateInforResponseData updateData = myMsg.updateData;

			updateData.Broadcast();

			this.OnDisable();

			this.OnEnable();

			UIMessageBoxManager.Instance.ShowMessageBox(Localization.instance.Get("IDS_BUTTON_TATTOO_RECIPETRADESUCCESSFUL"),
				Localization.instance.Get("IDS_BUTTON_TATTOO_RECIPETRADE"), MB_TYPE.MB_OK_WITH_ITEMS, null, updateData.itemUpdateList);
		}
		else
		{
			UIMessageBoxManager.Instance.ShowErrorMessageBox(msg.errorCode, "Tattoo");
		}
	}
}

