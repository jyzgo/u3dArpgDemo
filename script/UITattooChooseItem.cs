using UnityEngine;

using System;
using System.Collections.Generic;

public class UITattooChooseItem : MonoBehaviour
{
	public UILabel labelName;
	public UILabel labelUsed;
	public UILabel labelBearPoint;
	public UILabel labelEffect;
	public UITexture icon;

	public GameObject selectionEffect;

	public TattooData ttData { get; set; }

	private bool _isEquipped;

	private UITattooChoose _caller;

	private bool _canBear;
	public bool CanBear { get { return _canBear; } }

	public ItemInventory inventoryItem { get; set; }

	public void SetData(TattooData data, ItemInventory ii, bool isEquipped, UITattooChoose caller)
	{
		ttData = data;

		inventoryItem = ii;

		_isEquipped = isEquipped;

		_caller = caller;

		DisplayInfo();

		CheckBearPoint();
	}

	void CheckBearPoint()
	{
		ItemInventory ii = null;
		if (PlayerInfo.Instance.playerTattoos.tattooDict.ContainsKey(_caller.SelectedPart))
		{
			ii = PlayerInfo.Instance.playerTattoos.tattooDict[_caller.SelectedPart];
		}

		TattooData oldTD = null;

		if (ii != null)
		{
			oldTD = DataManager.Instance.GetTattooData(ii.ItemID);
		}

		if (PlayerInfo.Instance.CanBearTattoo(this.ttData, oldTD))
		{
			labelBearPoint.color = Color.white;
			_canBear = true;
		}
		else
		{
			labelBearPoint.color = Color.red;
			_canBear = false;
		}
	}

	void DisplayInfo()
	{
		labelName.text = ttData.GetDisplayName();

		if (_isEquipped)
		{
			labelUsed.text = Localization.instance.Get("IDS_TATTOO_ALREADYEQUIPPED");
		}
		else
		{
			labelUsed.text = string.Empty;
		}

		ItemData itemData =DataManager.Instance.GetItemData(ttData.tattooID);

		labelBearPoint.text = ttData.bearingPoint.ToString();

		labelEffect.text = Localization.instance.Get(itemData.descriptionIds);

		icon.mainTexture = InJoy.AssetBundles.AssetBundles.Load(itemData.iconPath) as Texture2D;

		selectionEffect.SetActive(false);
	}

	void OnClick()
	{
		if (_canBear)
		{
			_caller.SetFocus(this);
		}
	}

	public void OnFocused(bool focused)
	{
		selectionEffect.SetActive(focused);
	}
}
