using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TattooBodySlot : MonoBehaviour
{
	public EnumTattooPart part;

	public UITexture icon;

	public GameObject spriteLock;

	private ItemInventory _itemInventory;
	public ItemInventory item { get { return _itemInventory; } }

	private UITattoo _uiTattoo;

	private bool _isLocked;
	public bool IsLocked { get { return _isLocked; } }

	private int _unlockLevel;
	public int UnlockLevel { get { return _unlockLevel; } }

	public void SetData(ItemInventory ii, UITattoo caller)
	{
		_itemInventory = ii;

		if (ii != null)
		{
			icon.gameObject.SetActive(true);

			icon.mainTexture = InJoy.AssetBundles.AssetBundles.Load(ii.ItemData.iconPath) as Texture2D;

			_isLocked = false;
		}
		else
		{
			icon.gameObject.SetActive(false);

			int playerLevel = PlayerInfo.Instance.CurrentLevel;

			TattooUnlockDataList tudList = DataManager.Instance.tattooUnlockDataList;

			if (tudList.IsLocked(this.part, playerLevel, out _unlockLevel))
			{
				_isLocked = true;
			}
			else
			{
				_isLocked = false;
			}
		}
		spriteLock.SetActive(_isLocked);

		_uiTattoo = caller;
	}

	public void OnClick()
	{
		_uiTattoo.OnClickOnSlot(this);
	}
}