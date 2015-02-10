using UnityEngine;

using System;
using System.Collections.Generic;

public class UITattooRemove : MonoBehaviour
{
	public GameObject buttonGotoMake;
	public GameObject buttonReplace;
	public GameObject buttonRemove;

	public UITexture icon;

	public UILabel labelName;
	public UILabel labelBearPoint;
	public UILabel labelPos;
	public UILabel labelEffect;
	public UILabel labelSuiteEffect;

	private TattooBodySlot _slot;

	private TattooData _tattooData;

	private UITattoo _uiTattooMain;

	public void Initialize(TattooBodySlot slot, string tattooID, UITattoo uiTattoo)
	{
		_uiTattooMain = uiTattoo;

		_slot = slot;

		ItemData itemData;

		TattooData td;

		if (slot == null)	//called from recipe clicking
		{
			itemData = DataManager.Instance.GetItemData(tattooID);

			//show goto button
			buttonGotoMake.SetActive(true);
			buttonReplace.SetActive(false);
			buttonRemove.SetActive(false);
		}
		else
		{
			itemData = slot.item.ItemData;

			buttonGotoMake.SetActive(false);
			buttonReplace.SetActive(true);
			buttonRemove.SetActive(true);
		}
		
		td = DataManager.Instance.GetTattooData(itemData.id);

		_tattooData = td;

		icon.mainTexture = InJoy.AssetBundles.AssetBundles.Load(itemData.iconPath) as Texture2D;

		labelName.text = Localization.instance.Get(itemData.nameIds);

		labelBearPoint.text = td.bearingPoint.ToString();

		labelPos.text = Utils.GetTattooApplicablePositions(td.applicableParts);

		labelEffect.text = Localization.instance.Get(itemData.descriptionIds);

		if (string.IsNullOrEmpty(td.suiteID))
		{
			labelSuiteEffect.text = string.Empty;
		}
		else
		{
			labelSuiteEffect.text = Localization.instance.Get(DataManager.Instance.GetTattooSuiteData(td.suiteID).descIDS);
		}
	}

	//button message, called by name
	private void OnClickRepalceButton()
	{
		gameObject.SetActive(false);

		_uiTattooMain.OpenTattooSelectionWindow(_slot);
	}

	private void OnClickRemoveButton()
	{
		NetworkManager.Instance.SendCommand(new TattooEquipRequest(_slot.part, _slot.item.Item_GUID, 2), OnRemoveTattoo);
	}

	private void OnClickGotoMakeButton()
	{
		this.gameObject.SetActive(false);
		
		_uiTattooMain.ResetWindows();
		
		_uiTattooMain.GotoMake(_tattooData);
	}

	//button message
	private void OnClickCloseButton()
	{
		gameObject.SetActive(false);
	}

	private void OnReplaceTattoo(FaustComm.NetResponse msg)
	{
		if (msg.Succeeded)
		{
			TattooEquipResponse myMsg = msg as TattooEquipResponse;

			UpdateInforResponseData updateData = myMsg.updateData;

			updateData.Broadcast();

			UIMessageBoxManager.Instance.ShowMessageBox(Localization.instance.Get("IDS_TATTOO_ALREADYEQUIPPED"), null, MB_TYPE.MB_OK, null);

			_uiTattooMain.RefreshMainPanel();
		}
		else
		{
			UIMessageBoxManager.Instance.ShowMessageBox(Utils.GetErrorIDS(msg.errorCode), null, MB_TYPE.MB_OK, null);
		}
	}

	private void OnRemoveTattoo(FaustComm.NetResponse msg)
	{
		if (msg.Succeeded)
		{
			TattooEquipResponse myMsg = msg as TattooEquipResponse;

			UpdateInforResponseData updateData = myMsg.updateData;

			updateData.Broadcast();

			//manually add to inventory
			PlayerInfo.Instance.PlayerInventory.AddItemInventory(_slot.item);

			UIMessageBoxManager.Instance.ShowMessageBox(Localization.instance.Get("IDS_BUTTON_TATTOO_REMOVE"), null, MB_TYPE.MB_OK, OnMsgboxCallback);
		}
		else
		{
			//remember this, to fail this battle
			UIMessageBoxManager.Instance.ShowMessageBox(Utils.GetErrorIDS(msg.errorCode), null, MB_TYPE.MB_OK, null);
		}
	}

	private void OnMsgboxCallback(ID_BUTTON button)
	{
		this.gameObject.SetActive(false);

		_uiTattooMain.RefreshMainPanel();
	}
}

