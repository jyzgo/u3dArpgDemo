using UnityEngine;

using System;
using System.Collections.Generic;

public class UITattoo : MonoBehaviour
{
	public UILabel labelTitle;

	public GameObject buttonClose;

	//windows
	public GameObject main;
	public GameObject popupSelection;
	public GameObject popupSuiteQuery;
	public GameObject popupSellRecipe;
	public GameObject popupDisenchant;
	public GameObject popupMake;
	public GameObject popupRemove;

	private PlayerTattoos _playerTattoos;

	void Awake()
	{
		ResetWindows();
	}

	void OnInitialize()
	{
		UIManager.Instance.CloseUI("TownHome");
		CameraController.Instance.MainCamera.gameObject.SetActive(false);

		_playerTattoos = PlayerInfo.Instance.playerTattoos;

		ResetWindows();

		PlayerPrefs.SetInt(PrefsKey.PreviousPlayerLevel, PlayerInfo.Instance.CurrentLevel);
	}

	void OnInitializeWithCaller(System.Object[] objs)
	{
		OnInitialize();

		ItemData itemData = objs[0] as ItemData;

		if (itemData.type == ItemType.tattoo)
		{
			string op = objs[1] as string;
			if (op == "disenchant")
			{
				//go to disenchant
				OnClickDisenchantButton();
			}
			else if (op == "equip")
			{
				//find the body slot and open it
			}
		}
		else if (itemData.type == ItemType.recipe)
		{
			OnClickSellRecipeButton();
		}
	}

	//also called from child nodes
	public void ResetWindows()
	{
		//windows active
		main.SetActive(true);

		labelTitle.text = Localization.instance.Get("IDS_BUTTON_HUD_TATTOO");

		popupSelection.SetActive(false);
		popupSuiteQuery.SetActive(false);
		popupSellRecipe.SetActive(false);
		popupDisenchant.SetActive(false);
		popupMake.SetActive(false);
		popupRemove.SetActive(false);
	}

	public void RefreshMainPanel()
	{
		main.GetComponent<UITattooMain>().RefreshDisplay();
	}


	void OnClickCloseButton(GameObject go)
	{
		UIManager.Instance.OpenUI("TownHome");

		UIManager.Instance.CloseUI("Tattoo");

		CameraController.Instance.MainCamera.gameObject.SetActive(true);
	}

	void OnClickBackButton(GameObject go)
	{
		ResetWindows();
	}

	void OnClickSuiteQueryButton()
	{
		ResetWindows();
		main.SetActive(false);

		labelTitle.text = Localization.instance.Get("IDS_BUTTON_TATTOO_SUITEQUERY");

		popupSuiteQuery.SetActive(true);
	}

	void OnClickDisenchantButton()
	{
		main.SetActive(false);

		labelTitle.text = Localization.instance.Get("IDS_BUTTON_TATTOO_TATTOODISENCHANT");

		popupDisenchant.SetActive(true);
	}

	void OnClickSellRecipeButton()
	{
		main.SetActive(false);

		labelTitle.text = Localization.instance.Get("IDS_BUTTON_TATTOO_RECIPETRADE");

		popupSellRecipe.SetActive(true);
	}
	
	void OnClickMakeButton()
	{
		main.SetActive(false);

		labelTitle.text = Localization.instance.Get("IDS_BUTTON_TATTOO_TATTOOMAKE");

		popupMake.SetActive(true);

		popupMake.GetComponent<UITattooMake>().Initialize();
	}

	void OnClickCancelPopupSelection()
	{
		popupSelection.SetActive(false);
		buttonClose.SetActive(true);
		ResetWindows();
	}

	private EnumTattooPart _selectedPart;
	public EnumTattooPart SelectedTattooPart { get { return _selectedPart; } }

	//called when clicking on a slot
	public void OnClickOnSlot(TattooBodySlot slot)
	{
		_selectedPart = slot.part;

		if (slot.IsLocked)
		{
			UIMessageBoxManager.Instance.ShowMessageBox(string.Format(Localization.instance.Get("IDS_TATTOO_WILL_UNLOCK_AT_LEVEL"),
				Localization.instance.Get(Utils.k_tattoo_part_names[(int)slot.part]), slot.UnlockLevel), null,  MB_TYPE.MB_OK, null);
		}
		else if (slot.item == null) //empty slot
		{
			OpenTattooSelectionWindow(slot);
		}
		else //occupied
		{
			ShowTattooDetailWindow(slot, null);
		}
	}

	public void OpenTattooSelectionWindow(TattooBodySlot slot)
	{
		main.SetActive(false);

		popupSelection.SetActive(true);

		buttonClose.SetActive(false);

		popupSelection.GetComponent<UITattooChoose>().Initialize(slot.part, this);
	}

	void OnDisable()
	{
		popupMake.GetComponent<UITattooMake>().Clear();
	}

	public void GotoMake(TattooData td)
	{
		main.SetActive(false);
		popupMake.SetActive(true);

		popupMake.GetComponent<UITattooMake>().ChooseTattoo(td);
	}

	public void ShowTattooDetailWindow(TattooBodySlot slot, string tattooID)
	{
		popupRemove.SetActive(true);
		popupRemove.GetComponent<UITattooRemove>().Initialize(slot, tattooID, this);
	}
}

