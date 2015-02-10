using UnityEngine;

using System;
using System.Collections.Generic;

public class UITattooMake : MonoBehaviour
{
	public GameObject recipeItemPrefab;

	public GameObject materialItemPrefab;

	public UIGrid recipeGrid;

	public UILabel labelName;
	public UILabel labelPos;
	public UILabel labelBearingPoint;
	public UILabel labelEffect;
	public UILabel labelAmount;

	public UIGrid materialGrid;

	public GameObject buttonLearn;

	public GameObject buttonMakeWithSC;

	public UILabel labelMakeSC;

	public UILabel labelMakeHC;

	public UILabel labelLearnHC;

	private List<UIRecipeItem> _recipeItemList;

	private List<UITattooMaterialItem> _tmList = new List<UITattooMaterialItem>();

	private UIRecipeItem _focusItem;

	private TattooData _selectedTD;

	public void ChooseTattoo(TattooData td)
	{
		_selectedTD = td;

		float x = recipeGrid.cellHeight * (td.ord + td.level - 1);

		UIRecipeItem recipeItem = _recipeItemList.Find(delegate(UIRecipeItem rcp) { return !rcp.IsSummaryLine && rcp.ttData.tattooID == td.tattooID; });

		this.SetFocus(recipeItem);

		//select it!
		UIDraggablePanel panel = recipeGrid.transform.parent.GetComponent<UIDraggablePanel>();
		
		panel.ResetPosition();

		panel.MoveRelative(x * Vector3.up);
	}

	public void Initialize()
	{
		if (_recipeItemList == null)
		{
			FillRecipeGrid();
		}

		_selectedTD = null;
	}

	void FillRecipeGrid()
	{
		_recipeItemList = new List<UIRecipeItem>();

		//get all recipes
		List<TattooData> allRecipes = DataManager.Instance.tattooDataList.dataList;

		int level = -1;

		foreach (TattooData td in allRecipes)
		{
			GameObject go;

			UIRecipeItem uiItem;

			if (td.level > level)	//extra summary item
			{
				go = NGUITools.AddChild(recipeGrid.gameObject, recipeItemPrefab);

				uiItem = go.GetComponent<UIRecipeItem>();

				level = td.level;

				uiItem.name = string.Format("{0}: Summary", td.ord);

				uiItem.IsSummaryLine = true;

				uiItem.SetData(td, this);

				_recipeItemList.Add(uiItem);

				//Debug.LogError(string.Format("Summary -- level {0} created.", level));
			}

			//normal item, always needs to add
			go = NGUITools.AddChild(recipeGrid.gameObject, recipeItemPrefab);

			go.name = string.Format("{0}: {1}", td.ord, td.tattooID);

			uiItem = go.GetComponent<UIRecipeItem>();

			uiItem.SetData(td, this);

			_recipeItemList.Add(uiItem);
		}

		recipeGrid.Reposition();

		SetFocus(_recipeItemList[1]);
	}

	public void ExpandLevel(int level)
	{
		foreach (UIRecipeItem item in _recipeItemList)
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
		foreach (UIRecipeItem item in _recipeItemList)
		{
			if (item.ttData.level == level && !item.IsSummaryLine)
			{
				item.gameObject.SetActive(false);
			}
		}

		recipeGrid.Reposition();
	}

	public void SetFocus(UIRecipeItem item)
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

		if (PlayerInfo.Instance.SoftCurrency >= _focusItem.ttData.scCost)
		{
			labelMakeSC.text = _focusItem.ttData.scCost.ToString();
		}
		else
		{
			labelMakeSC.text = string.Format("[ff0000]{0}[-]", _focusItem.ttData.scCost);
		}
		
		//hide/show buttons
		if (_focusItem.HasLearned)
		{
			buttonLearn.transform.parent.gameObject.SetActive(false);
			buttonMakeWithSC.transform.parent.gameObject.SetActive(true);

			if (PlayerInfo.Instance.PlayerInventory.HasSufficientMaterials(_focusItem.ttData) && 
				PlayerInfo.Instance.SoftCurrency >= _focusItem.ttData.scCost)
			{
				buttonMakeWithSC.GetComponent<UIImageButton>().isEnabled = true;
			}
			else
			{
				buttonMakeWithSC.GetComponent<UIImageButton>().isEnabled = false;
			}

			labelMakeHC.text = _focusItem.ttData.hcCost.ToString();
		}
		else
		{
			buttonLearn.transform.parent.gameObject.SetActive(true);
			buttonMakeWithSC.transform.parent.gameObject.SetActive(false);

			buttonLearn.GetComponent<UIImageButton>().isEnabled = PlayerInfo.Instance.PlayerInventory.GetItemCount(_focusItem.ttData.recipeID) > 0;

			labelLearnHC.text = _focusItem.ttData.learnHC.ToString();
		}

		FillMaterialGrid();
	}


	void FillMaterialGrid()
	{
		TattooData ttd = _focusItem.ttData;

		int index = 0;

		//materials
		foreach (MatAmountMapping mapping in ttd.materials)
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

	public void Clear()
	{
		if (_recipeItemList == null)
		{
			return;
		}
		//remove grid items
		foreach (UIRecipeItem item in _recipeItemList)
		{
			Destroy(item.gameObject);
		}

		_recipeItemList.Clear();

		_recipeItemList = null;

		recipeGrid.Reposition();
	}

	private void OnClickMakeWithSC()
	{
		NetworkManager.Instance.SendCommand(new TattooRecipeRequest(_focusItem.ttData.tattooID, 1, 0), OnMakeTattoo);
	}

	private void OnClickMakeWithHC()
	{
		if (_focusItem.ttData.hcCost > PlayerInfo.Instance.HardCurrency)
		{
			UIMessageBoxManager.Instance.ShowErrorMessageBox(5001, "Tattoo");
		}
		else
		{
			if (_focusItem.ttData.hcCost >= (int)DataManager.Instance.CurGlobalConfig.getConfig("hcWarningAmount"))
			{
				
				UIMessageBoxManager.Instance.ShowMessageBox(
					string.Format(Localization.instance.Get("IDS_MESSAGE_TATTOO_CONFIRM_HCMAKETATTOO"), _focusItem.ttData.hcCost, 
					Localization.instance.Get(DataManager.Instance.GetItemData(_focusItem.ttData.tattooID).nameIds)),
					null, MB_TYPE.MB_OKCANCEL, MakeWithHCCallback);
			}
			else
			{
				NetworkManager.Instance.SendCommand(new TattooRecipeRequest(_focusItem.ttData.tattooID, 1, 1), OnMakeTattoo);
			}
		}
	}

	private void OnClickLearn()
	{
		//player has the scroll (recipe)
		if (PlayerInfo.Instance.PlayerInventory.GetItemCount(_focusItem.ttData.recipeID) > 0)
		{
			if (PlayerInfo.Instance.SoftCurrency < _focusItem.ttData.scCost)
			{
				UIMessageBoxManager.Instance.ShowErrorMessageBox(5002, "Tattoo");
			}
			else
			{
				LearnTattooRecipe(_focusItem.ttData.tattooID, 0);
			}
		}
	}

	private void OnClickLearnWithHC()
	{
		if (PlayerInfo.Instance.HardCurrency < _focusItem.ttData.learnHC)
		{
			UIMessageBoxManager.Instance.ShowErrorMessageBox(5001, "Tattoo");
		}
		else
		{
			if (_focusItem.ttData.learnHC >= (int)DataManager.Instance.CurGlobalConfig.getConfig("hcWarningAmount"))
			{
				UIMessageBoxManager.Instance.ShowMessageBox(string.Format(Localization.instance.Get("IDS_MESSAGE_TATTOO_COSTHCLEARN"), _focusItem.ttData.learnHC, 
					Localization.instance.Get(DataManager.Instance.GetItemData(_focusItem.ttData.tattooID).nameIds)),
					Localization.instance.Get("IDS_TITLE_GLOBAL_NOTICE"), MB_TYPE.MB_OKCANCEL, LearnWithHCCallback);
			}
			else
			{
				LearnTattooRecipe(_focusItem.ttData.tattooID, 1);
			}
		}
	}

	private void MakeWithHCCallback(ID_BUTTON buttonID)
	{
		if (buttonID == ID_BUTTON.ID_OK)
		{
			NetworkManager.Instance.SendCommand(new TattooRecipeRequest(_focusItem.ttData.tattooID, 1, 1), OnMakeTattoo);
		}
	}


	private void LearnWithHCCallback(ID_BUTTON buttonID)
	{
		if (buttonID == ID_BUTTON.ID_OK)
		{
			LearnTattooRecipe(_focusItem.ttData.tattooID, 1);
		}
	}

	private void LearnTattooRecipe(string tattooID, byte useHC)
	{
		NetworkManager.Instance.SendCommand(new TattooRecipeRequest(tattooID, 2, useHC), OnTattooLearned);
	}

	private void OnTattooLearned(FaustComm.NetResponse msg)
	{
		if (msg.Succeeded)
		{
			TattooRecipeResponse myMsg = msg as TattooRecipeResponse;

			UpdateInforResponseData updateData = myMsg.updateData;

			PlayerInfo.Instance.playerMasteredTattoos.Add(myMsg.tattooID);

			updateData.Broadcast();

			DisplayRecipeInfo();

			_focusItem.SetData(_focusItem.ttData, this);

			SetFocus(_focusItem);
			
			UIMessageBoxManager.Instance.ShowMessageBox(string.Format(Localization.instance.Get("IDS_MESSAGE_TATTOO_STUDYTATTOO"), 
				_focusItem.ttData.GetDisplayName()), null, MB_TYPE.MB_OK, null);
		}
		else
		{
			UIMessageBoxManager.Instance.ShowMessageBox(Utils.GetErrorIDS(msg.errorCode), null, MB_TYPE.MB_OK, null);
		}
	}

	private void OnMakeTattoo(FaustComm.NetResponse msg)
	{
		if (msg.Succeeded)
		{
			TattooRecipeResponse myMsg = msg as TattooRecipeResponse;

			UpdateInforResponseData updateData = myMsg.updateData;

			updateData.Broadcast();

			DisplayRecipeInfo();

			UIMessageBoxManager.Instance.ShowMessageBox(Localization.instance.Get("IDS_MESSAGE_TATTOO_FUSIONSUCCESSFUL"), null, MB_TYPE.MB_OK, null); 
		}
		else
		{
			//remember this, to fail this battle
			UIMessageBoxManager.Instance.ShowMessageBox(Utils.GetErrorIDS(msg.errorCode), null, MB_TYPE.MB_OK, null);
		}
	}
}

