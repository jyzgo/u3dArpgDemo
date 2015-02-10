using UnityEngine;

using System;
using System.Collections.Generic;

public class UITattooChoose : MonoBehaviour
{
	public GameObject itemPrefab;

	public UIGrid ttGrid;

	public UIImageButton buttonOK;

	public UILabel labelTitle;

	private List<UITattooChooseItem> _ttList;

	private UITattooChooseItem _focusItem;

	private UITattoo _uiTattoo;

	private EnumTattooPart _selectedPart;
	public EnumTattooPart SelectedPart { get { return _selectedPart; } }

	public void Initialize(EnumTattooPart part, UITattoo uiTattoo)
	{
		OnDisable();

		_selectedPart = part;

		_uiTattoo = uiTattoo;

		//list all tattoos, including those equipped on body
		_ttList = new List<UITattooChooseItem>();

		//tattoos in bag
		List<ItemInventory> bagTTList = PlayerInfo.Instance.PlayerInventory.itemList.FindAll(
			delegate(ItemInventory ii)
			{
				return ii.ItemData.type == ItemType.tattoo;
			});

		foreach (ItemInventory ii in bagTTList)
		{
			TattooData td = DataManager.Instance.GetTattooData(ii.ItemID);

			if (td.applicableParts.Contains(part))
			{
				GameObject go = NGUITools.AddChild(ttGrid.gameObject, itemPrefab);

				UITattooChooseItem uiItem = go.GetComponent<UITattooChooseItem>();

				uiItem.SetData(td, ii, false, this);

				_ttList.Add(uiItem);
			}
		}

		//tattoos burnt on body
		foreach (KeyValuePair<EnumTattooPart, ItemInventory> kvp in PlayerInfo.Instance.playerTattoos.tattooDict)
		{
			if (kvp.Key != _selectedPart)
			{
				TattooData td = DataManager.Instance.GetTattooData(kvp.Value.ItemID);

				if (td.applicableParts.Contains(part))
				{
					GameObject go = NGUITools.AddChild(ttGrid.gameObject, itemPrefab);

					UITattooChooseItem uiItem = go.GetComponent<UITattooChooseItem>();

					uiItem.SetData(td, kvp.Value, true, this);

					_ttList.Add(uiItem);
				}
			}
		}

		NGUITools.FindInParents<UIDraggablePanel>(ttGrid.gameObject).ResetPosition();

		ttGrid.repositionNow = true;

		labelTitle.text = string.Format("{0} ({1})", Localization.instance.Get("IDS_BUTTON_GLOBAL_EQUIP"),
			Localization.instance.Get(Utils.k_tattoo_part_names[(int)_selectedPart]));

		if (_ttList.Count > 0)
		{
			SetFocus(_ttList[0]);
		}
		else
		{
			buttonOK.gameObject.SetActive(false);
		}
	}

	public void OnDisable()
	{
		if (_ttList == null)
		{
			return;
		}
		//remove grid items
		foreach (UITattooChooseItem item in _ttList)
		{
			Destroy(item.gameObject);
		}

		_ttList.Clear();

		_ttList = null;
	}

	public void SetFocus(UITattooChooseItem item)
	{
		if (_focusItem != null)
		{
			_focusItem.OnFocused(false);
		}

		_focusItem = item;

		item.OnFocused(true);

		if (!buttonOK.gameObject.activeInHierarchy)
		{
			buttonOK.gameObject.SetActive(true);
		}

		buttonOK.isEnabled = item.CanBear;
	}

	private void OnClickOK()
	{
		NetworkManager.Instance.SendCommand(new TattooEquipRequest(_selectedPart, _focusItem.inventoryItem.Item_GUID, 1), OnEquipTattoo);
	}

	private void OnEquipTattoo(FaustComm.NetResponse msg)
	{
		if (msg.Succeeded)
		{
			TattooEquipResponse myMsg = msg as TattooEquipResponse;

			//if current slot is not empty, move the item into inventory
			if (PlayerInfo.Instance.playerTattoos.tattooDict.ContainsKey(SelectedPart))
			{
				ItemInventory oldItem = PlayerInfo.Instance.playerTattoos.tattooDict[SelectedPart];

				PlayerInfo.Instance.PlayerInventory.AddItemInventory(oldItem);
			}

			UpdateInforResponseData updateData = myMsg.updateData;

			updateData.Broadcast();

			//remove the item from inventory. It may not be there, but call removeItem in any case.
			PlayerInfo.Instance.PlayerInventory.RemoveItem(_focusItem.inventoryItem);

			UIMessageBoxManager.Instance.ShowMessageBox(Localization.instance.Get("IDS_TATTOO_ALREADYEQUIPPED"), null, MB_TYPE.MB_OK, null);

			_uiTattoo.RefreshMainPanel();
			this.Initialize(_selectedPart, _uiTattoo);
		}
		else
		{
			UIMessageBoxManager.Instance.ShowMessageBox(Utils.GetErrorIDS(msg.errorCode), null, MB_TYPE.MB_OK, null);
		}
	}
}

