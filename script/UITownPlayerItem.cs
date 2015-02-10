using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using InJoy.FCComm;

public class UITownPlayerItem : MonoBehaviour 
{
	public UITexture _icon;
	public UISprite[] _backgrounds = new UISprite[5];
	public UISprite _border;
	public UISprite _defaultBorder;
    public GameObject _blinkingStar;
		
	private UITownPlayer _myUITownPlayer;
	public UITownPlayer MyUITownPlayer
	{
		get{return _myUITownPlayer;}
		set{_myUITownPlayer = value;}
	}
	
	private ItemData _itemData;
	private int _role;
	
	private ItemInventory _observedItem;
	public ItemInventory ObservedItem
	{
		set
		{
			_observedItem = value;
		}
		get{
			return _observedItem;
		}
	}
	
	public void SetItemData(ItemData itemData,int role)
	{
		_role = role;
		_itemData = itemData;
		
	}
	
	public void InitImage()
	{
		_selected = false;
		if(_observedItem == null || _itemData == null)
		{
			UIUtils.UnloadTexture(_icon);
		}else{
			UIUtils.UnloadTexture(_icon);
			UIUtils.LoadTexture(_icon, _observedItem.ItemData, _role);
		}
		RefreshIcon();
	}
	

	private bool _selected = false;
	public bool Selected
	{
		get { return _selected; }
		set { 
			_selected = value;
		}
	}
	

	private int GetDisplayRareLevel()
	{
		if(_observedItem != null)
		{
			return 	_observedItem.DisplayRareLevel;
		}else if(_itemData != null)
		{
			return _itemData.rareLevel;
		}else{
			return 0;	
		}
	}
	
	
	public void RefreshIcon()
	{		

		int displayRareLevel = GetDisplayRareLevel();
		for(int i = 0 ; i< 5; i++)
		{
			_backgrounds[i].gameObject.SetActive(i == displayRareLevel);
		}
		if(_defaultBorder != null)
		{
			_defaultBorder.gameObject.SetActive(_observedItem == null);
		}
		_border.gameObject.SetActive(_observedItem != null);
		if(_observedItem != null)
		{
			UIUtils.SetBorder(_border, 0);
		}

		////show blinking star for myself only
		//if (UITownPlayer._activePlayerInfo._id == NetworkManager.Instance.AccountName &&
		//    _observedItem != null &&
		//    _observedItem.IsCanEvolution())
		//{
		//    //-9.21  42.02  1.26   size 12
		//    _blinkingStar.transform.localPosition = new Vector3(-8.49f, 42.42f, -6) + Vector3.right * _observedItem._evolutionLevel * 9.5f;
		//    _blinkingStar.SetActive(true);
		//}
		//else
		//{
		//    _blinkingStar.SetActive(false);
		//}
	}
	
	
	
	public void UnloadImage()
	{
		UIUtils.UnloadTexture(_icon);
	}
	
	private void OnSelectItem()
	{
		if(_itemData != null)
		{
			if(_observedItem != null)
			{
				MyUITownPlayer.SelectItem(this);
			}
		}
	}
	
}
