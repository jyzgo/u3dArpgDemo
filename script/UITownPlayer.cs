using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using InJoy.FCComm;

public class UITownPlayer : MonoBehaviour {
		
	public GameObject _itemPanel;
	public GameObject _playerInfoPanel;
	public GameObject _preview;
	
	public GameObject[] _items = new GameObject[10];
	public GameObject[] _itemIcons = new GameObject[4];
	public GameObject _playerInfoButton;
	public GameObject _previewButton;
	public GameObject _closeButton;
	public GameObject _chatButton;
	public GameObject _friendButton;
	
	public GameObject _leftButton;
	public GameObject _rightButton;
	
	public GameObject _upPanel;
	
	
	public UILabel _nickName;
	
	
	
	public PreviewGestureProcessor _previewGestureProcessor;
	private UIPlayerPreview _playerPreview;
	private Transform _equipmentRoot;
	
	private UIInventoryPlayerInfo _uIInventoryPlayerInfo; // fir show player information
	
	public static PlayerInfo _activePlayerInfo = null;  
	
	
	public GameObject _selectedEffectPrefab;
	private GameObject _selectedEffect;
	private Transform _selectedTransform;
	
	private UITownPlayerItem _currentItem = null;
	private Transform _currentItemTransform = null;
	
	public static bool _openFromFriendList = false;
	
	public static int _openShowPlayerClass = -1;
	
	
	void Awake()
	{
		_playerPreview = GetComponent<UIPlayerPreview>();
		
		_uIInventoryPlayerInfo = _playerInfoPanel.GetComponent<UIInventoryPlayerInfo>();
		
		_playerInfoButton.GetComponent<UIButtonMessage>().target = gameObject;
		_playerInfoButton.GetComponent<UIButtonMessage>().functionName = "OnClickPlayerInfoButton";
		
		_previewButton.GetComponent<UIButtonMessage>().target = gameObject;
		_previewButton.GetComponent<UIButtonMessage>().functionName = "OnClickPreviewButton";
		
		_closeButton.GetComponent<UIButtonMessage>().target = gameObject;
		_closeButton.GetComponent<UIButtonMessage>().functionName = "OnClose";
		
		
		_friendButton.GetComponent<UIButtonMessage>().target = gameObject;
		_friendButton.GetComponent<UIButtonMessage>().functionName = "OnClickFriendButton";
		
		_chatButton.GetComponent<UIButtonMessage>().target = gameObject;
		_chatButton.GetComponent<UIButtonMessage>().functionName = "OnClickChatButton";
		
		_leftButton.GetComponent<UIButtonMessage>().target = gameObject;
		_leftButton.GetComponent<UIButtonMessage>().functionName = "OnClickLeft";
		
		_rightButton.GetComponent<UIButtonMessage>().target = gameObject;
		_rightButton.GetComponent<UIButtonMessage>().functionName = "OnClickRight";
		
		
		//ChangeButtonState(_chatButton, false);
		
		_canSwitch = false;
	}
	
	
	
	private bool _canSwitch = true;
	public void OnClickLeft()
	{
		//if(_canSwitch)
		//{
		//    _canSwitch = false;
		//    _activePlayerInfo._selectedClass--;
		//    if(_activePlayerInfo._selectedClass <0)
		//    {
		//        _activePlayerInfo._selectedClass = 2;
		//    }
		//    _activePlayerInfo._selectedClass %= 3;
		//    _activePlayerInfo.UpdateSelectClassData();
		//    OnClear();
		//    OnInitialize();
		//}
	}
	
	public void OnClickRight()
	{
		//if(_canSwitch)
		//{
		//    _canSwitch = false;
		//    _activePlayerInfo._selectedClass++;
		//    _activePlayerInfo._selectedClass %= 3;
		//    _activePlayerInfo.UpdateSelectClassData();
		//    OnClear();
		//    OnInitialize();
		//}
	}
	
	
	public void ChangeButtonState(GameObject button, bool active)
	{
		button.GetComponent<UIImageButton>().isEnabled = active;
	}
	
	
	public void OnClickFriendButton()
	{
	}
	
	public void OnClickChatButton()
	{
		OnClickChatWithFriend();
        OnClear();

        //call functions similiar to OnClose()
        _activePlayerInfo = null;
        UIManager.Instance.CloseUI("TownPlayer");

        if (_openFromFriendList)
        {
            _openFromFriendList = false;
            UIManager.Instance.CloseUI("SocialFriend");
        }
	}
	
	
	public void OnClickChatWithFriend()
	{
	
		
//		PlayerInfo playerInfo = new PlayerInfo(_activePlayerInfo._id,_activePlayerInfo.Nickname);
//		
//		GameObject o = UIManager.Instance.GetUI("UITownChat");
//		UIChatHandler chat = o.GetComponent<UIChatHandler>();
//		chat.SelectPrivateChannel(playerInfo);
	}
	
	
		//select effect on current selected item
	private void CreateSelectEffect()
	{
		if(_selectedEffect == null)
		{
			_selectedEffect = GameObject.Instantiate(_selectedEffectPrefab,Vector3.zero,Quaternion.identity) as GameObject;	
			_selectedTransform = _selectedEffect.transform;
		}
	}
	
	private void DestroySelectEffect()
	{
		if(_selectedEffect != null)
		{
			Destroy(_selectedEffect);
			_selectedEffect = null;
			_selectedTransform = null;
		}
	}
	
	private void UpdateSelectEffectPosition()
	{
		if(_selectedEffect != null)
		{	
			_selectedTransform.position = _currentItemTransform.position;
		}
	}
	

	public void OnClear()
	{
		DestroySelectEffect();
		
		_playerPreview.Clean();

		for(int i = 0; i< 10; i++)
		{
			UITownPlayerItem uiTownPlayerItem = _items[i].GetComponent<UITownPlayerItem>();
			uiTownPlayerItem.UnloadImage();
		} 
	}
	
	
	public void OnClose()
	{
		OnClear();
		_activePlayerInfo = null;
		UIManager.Instance.CloseUI("TownPlayer");
		SoundManager.Instance.PlaySoundEffect("button_cancel");
		
		if (_openFromFriendList)
		{
			_openFromFriendList = false;
		}
	}
	

	public void OnInitialize()
	{
		_preview.SetActive(true);
		_itemPanel.SetActive(false);
		_playerInfoPanel.SetActive(false);
		_previewButton.SetActive(false);
		_playerInfoButton.SetActive(true);
		
		StartCoroutine(ShowPanel());
	}
	
	
	public void OnInit()
	{
	
		_nickName.text = _activePlayerInfo.DisplayNickname;
		
		
		// _playerPreview.Initialize();
		int role = 1; // _openShowPlayerClass == -1 ? (int)_activePlayerInfo.CurPlayerProfile.Role : _openShowPlayerClass;
		_playerPreview.Clean();
		 _playerPreview.InitializePreview();
		 _equipmentRoot = _playerPreview.PreviewModal.transform.Find("EquipmentRoot");
		
		//_activePlayerInfo._selectedClass = role;
		//_activePlayerInfo.UpdateSelectClassData();
		UpdatePreviewModel(_activePlayerInfo.equipIds,false, role);	
		
		
		//this controll the drag of player model
		_previewGestureProcessor.TargetTransform = _playerPreview.PreviewModal.transform;
		
		ResetPreviewModelAngle();// set player model face us.
		
		InitItems();
		_openShowPlayerClass = -1;
		StartCoroutine(DelaySwitch());
	}
	
	
	IEnumerator DelaySwitch()
	{
		yield return new WaitForSeconds(0.5f);
		_canSwitch = true;
	}
	
	
	IEnumerator	ShowPanel()
	{
		if(!_activePlayerInfo.haveDonwloadedData)
		{
			//if the player info is not downloaded, get it
			//_activePlayerInfo.GetSelfInformation();
		}
		
		while(true)
		{
			if(_activePlayerInfo.haveDonwloadedData)
			{
				break;
			}
			yield return null;
		}
		OnInit();
	}
	
	
	
	public void SelectItem(UITownPlayerItem uiTownPlayerItem)
	{
		if(_currentItem != null)
		{
			_currentItem.Selected = false;
		}
		
		_currentItem  = uiTownPlayerItem;
		_currentItem.Selected = true;
		_currentItemTransform = uiTownPlayerItem.gameObject.transform;
		CreateSelectEffect();
		UpdateSelectEffectPosition();
		
	
		_previewButton.SetActive(true);
		_playerInfoButton.SetActive(false);
		
		_itemPanel.SetActive(true);
		StartCoroutine(FadeColor(_itemPanel));

		
		_preview.SetActive(false);
		_playerInfoPanel.SetActive(false);
	}

	
	
	private  int GetPartPosition(ItemSubType part)
	{
		int result = 0;
		
		if(part ==  ItemSubType.weapon)
		{
			result = 2;	
		}
		else if(part ==  ItemSubType.helmet)
		{
			result = 1;
		}
		else if(part ==  ItemSubType.shoulder)
		{
			result = 4;
		}
		else if(part ==  ItemSubType.belt)
		{
			result = 3;
		}
		else if(part ==  ItemSubType.armpiece)
		{
			result = 5;
		}
		else if(part ==  ItemSubType.leggings)
		{
			result =6;
		}
		else if(part ==  ItemSubType.necklace)
		{
			result = 7;
		}
		else if(part ==  ItemSubType.ring)
		{
			result =8;
		}
		else
		{
			result =0;
		}
		return result;
	}
	
	
	public void InitItems()
	{
		
		_itemIcons[0].SetActive(true);
		_itemIcons[1].SetActive(true);
		_itemIcons[2].SetActive(true);
		_itemIcons[3].SetActive(true);

		int role = 1;// _openShowPlayerClass == -1 ? (int)_activePlayerInfo.CurPlayerProfile.Role : _openShowPlayerClass;
		for(int i = 0; i< 10; i++)
		{
			UITownPlayerItem uiTownPlayerItem = _items[i].GetComponent<UITownPlayerItem>();
			uiTownPlayerItem.MyUITownPlayer = this;
		
			uiTownPlayerItem.SetItemData(null, role);
			uiTownPlayerItem.ObservedItem = null;
		}
		
		
		for(int i = 0; i< _activePlayerInfo.equipIds.Count; i++)
		{
			string itemId = _activePlayerInfo.equipIds[i]._id;
			ItemData itemData = DataManager.Instance.GetItemData(itemId);
			
			int itemPos = GetPartPosition(itemData.subType) ;
			if(itemPos != 0)
			{
				UITownPlayerItem uiTownPlayerItem = _items[itemPos-1].GetComponent<UITownPlayerItem>();
				
				uiTownPlayerItem.SetItemData(itemData, role);
				//uiTownPlayerItem.ObservedItem = _activePlayerInfo._tablePlayerProfile[role]._equipedInventory.GetItem(itemId);
				
				if(itemData.subType == ItemSubType.necklace)
				{
					_itemIcons[0].SetActive(false);
				}else if(itemData.subType == ItemSubType.ring)
				{
					_itemIcons[1].SetActive(false);
				}
			}
			
		}
		
		
		for(int i = 0; i< 10; i++)
		{
			UITownPlayerItem uiTownPlayerItem = _items[i].GetComponent<UITownPlayerItem>();
			uiTownPlayerItem.InitImage();
		}
		
	
	}
	
	
	private void ResetPreviewModelAngle()
	{
		_playerPreview.PreviewModal.transform.localEulerAngles = new Vector3(0,150f,0);
	}
	

		
	
	IEnumerator FadeColor(GameObject obj)
	{
		UIPanel[] panels = obj.GetComponents<UIPanel>();
		
		float alpha = 1;
		float time = 0;
		
		while(true)
		{
			time += Time.deltaTime;
			alpha = time/0.5f;
			
			if(alpha >= 1)
			{
				alpha  = 1;
			}
			
			foreach(UIPanel panel in panels)
			{
				panel.alpha = alpha;
			}
			
			if(alpha >= 1)
			{
				yield break;
			}else{
				yield return null;
			}
		}
	}

	
	public void OnClickPreviewButton()
	{
		
		_previewButton.SetActive(false);
		_playerInfoButton.SetActive(true);
		
		if(_currentItem !=  null)
		{
			_currentItem = null;
			DestroySelectEffect();
			
			_preview.SetActive(true);
			_itemPanel.SetActive(false);
			_playerInfoPanel.SetActive(false);
						
		}else{				

			_preview.SetActive(true);
			_itemPanel.SetActive(false);
			_playerInfoPanel.SetActive(false);
			
			_previewButton.SetActive(false);
			_playerInfoButton.SetActive(true);
		}
	}
	

	public void OnClickPlayerInfoButton()
	{
		
		_previewButton.SetActive(true);
		_playerInfoButton.SetActive(false);
		
		_preview.SetActive(false);
		_itemPanel.SetActive(false);
		_playerInfoPanel.SetActive(true);
		
		_uIInventoryPlayerInfo.UpdatePlayerInfo(_activePlayerInfo);	
		StartCoroutine(FadeColor(_playerInfoPanel));
	}

	
		
	#region UpdateModel
	//change equipments of player, if "isEquip == true"  we will update player in preview and player in town,
	// if "isEquip == false" we only update player in preview.
	private void UpdatePreviewModel(List<EquipmentIdx> ids, bool isEquip, int role)
	{
		ClearOldEquipment();
		
		List<GameObject> EquipInstances = new List<GameObject>();
		PlayerInfo.GetOtherEquipmentInstanceWithIds(EquipInstances,ids);
        foreach (GameObject go in EquipInstances)
        {
            go.transform.parent = _equipmentRoot;
        }
		
       	_playerPreview.PreviewModal.GetComponent<AvatarController>().RefreshEquipments(_equipmentRoot);
      	if(isEquip)
		{
			TownPlayerManager.Singleton.HeroInfo._avatarController.RefreshEquipments(_equipmentRoot);
		}
		
        Renderer[] renderers = _playerPreview.PreviewModal.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            r.gameObject.layer = LayerMask.NameToLayer("3DUI");
        }
	}
	
	
	//remove current equipments of player
	private void ClearOldEquipment()
	{
	 	List<GameObject> oldEquipmentList = new List<GameObject>();
		
        foreach (Transform t in _equipmentRoot)
        {
            oldEquipmentList.Add(t.gameObject);
        }

        foreach (GameObject go in oldEquipmentList)
        {
            go.transform.parent = null;
            Destroy(go);
        }
		
		oldEquipmentList.Clear();
	}
	#endregion
	
}
