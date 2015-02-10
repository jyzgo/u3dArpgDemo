using UnityEngine;

using System;
using System.Collections.Generic;

public class UICommonDisplayItem : MonoBehaviour
{
	public UITexture icon;
	public UISprite frame;
	public UILabel amount;

	protected ItemData _itemData;
	protected int _amount;

	public void SetData(ItemInventory itemInventory)
	{
		_itemData = itemInventory.ItemData;

		SetData(_itemData, itemInventory.Count);
	}

	public void SetData(string itemID, int amount)
	{
		_itemData = DataManager.Instance.GetItemData(itemID);

		_amount = amount;

		UpdateDisplay();
	}

	public void SetData(ItemData itemData, int amount = 1)
	{
		_itemData = itemData;

		_amount = 1;

		UpdateDisplay();
	}

	public virtual void UpdateDisplay()
	{
		icon.mainTexture = InJoy.AssetBundles.AssetBundles.Load(_itemData.iconPath) as Texture2D;

		frame.spriteName = Utils.GetRaritySpriteName(_itemData.rareLevel);

		this.amount.text = "x " + _amount.ToString();
	}

	void OnPress(bool pressed)
	{
		if (pressed)
		{
			UIMessageBoxManager.Instance.ShowMessageBox(null, null, MB_TYPE.MB_FLOATING, null, _itemData);
		}
		else
		{
			UIMessageBoxManager.Instance.HideMessageBox(MB_TYPE.MB_FLOATING);
		}
	}
}
