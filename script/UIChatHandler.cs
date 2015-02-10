using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIChatHandler : MonoBehaviour 
{
	public UIInput _inputUI;
	public UITextList _textListUI;
	public UIScrollBar _scrollbar;
	
	public UIAnchor _chatList;
	public UIAnchor _inputAnchor;
	public GameObject _chatHistory;
	public Transform _radioButtonRoot;
	public UISprite _fullScreenBG;
	
	public UICheckbox _displayCheck;
	public UICheckbox _townCheck;
	public UICheckbox _guildCheck;
	
	public UISprite _newMsgSprite;
	
	bool _ignoreNextEnter = false;
	TweenScale _displayCheckTweener;
	BoxCollider _inputBoxCollider;
	
	public static bool _isGuildChat = false;
	
	
	void Awake()
	{
		_chatList.uiCamera = UIManager.Instance.MainCamera;
		_inputAnchor.uiCamera = UIManager.Instance.MainCamera;
		
		_textListUI.Clear();
		
		_inputUI.text = _inputUI.defaultText = Localization.instance.Get("IDS_TAP_TO_CHAT");
		_inputBoxCollider = _inputUI.GetComponent<BoxCollider>();
		
		_displayCheckTweener = _displayCheck.gameObject.GetComponent<TweenScale>();
		_displayCheckTweener.enabled = false;
	}
	
	void OnInitialize()
	{
		_displayCheck.isChecked = false;
		
		ChatManager.Instance.Channel = IChatChannel.ENUM_CHAT_CHANNEL.CHANNEL_TOWN;
		IChatChannel chatRoom = ChatManager.Instance.ActiveChatRoom;
		if (chatRoom != null)
		{
			DisplayLatestMessageInInputbox(chatRoom, _inputUI);
		}
		
		RefreshMessageList();
	}
	
	// Use this for initialization
	void Start () 
	{			
//		WhisperChat whisperRoom = ChatManager.Instance.GetWhisperRoom();

//		if(null != whisperRoom)
//		{
//			whisperRoom._onReceivePrivateMessage = OnReceivePrivateMessage;
//		}

		_inputUI.text = "";
	}
	
	void OnDestroy()
	{
		if (ChatManager.Instance != null)
		{
			//todo_network
//			WhisperChat whisperRoom = ChatManager.Instance.GetWhisperRoom();
//
//			if(null != whisperRoom)
//			{
//				whisperRoom._onReceivePrivateMessage = null;
//			}
		}
	}
	
	// There is a new private message coming.
	public void OnReceivePrivateMessage(InJoy.FCComm.BriefPlayerInfo playerInfo)
	{		
		AddWhisperItem(playerInfo);
		
		SoundManager.Instance.PlaySoundEffect("Evolve_finish");
	}
	
	public static void DisplayLatestMessageInInputbox(IChatChannel room, UIInput inputbox)
	{
		ChatMessage message = room.GetLatestMessage();
		
		if (message == null)
		{
			return;
		}
		
		string lastetMsg = "";
		string nickName = "";
		
		if (NetworkUtils.IsSystemMessage(message.messageText))
		{
			nickName = Localization.instance.Get("IDS_MESSAGE_TYPE_SYSTEM");		
		}
		else
		{			
			nickName = message.fromPlayerInfo.nickName;
			lastetMsg = message.messageText;
		}
		
		inputbox.defaultText = nickName + ": " + lastetMsg;
	}
	
	// Update is called once per frame
	void Update () 	
	{		
		// Check chat service
		// if chat enabled
        //{
        //    _inputUI.text = _inputUI.defaultText = Localization.instance.Get("IDS_SERVER_DOWN_CHAT");
        //    _inputBoxCollider.enabled = false;
			
        //    return;
        //}
		
        if (_displayCheck.isChecked)
		{
			_inputUI.defaultText = Localization.instance.Get("IDS_TAP_TO_CHAT");
			_inputBoxCollider.enabled = true;
		}
		
		IChatChannel chatRoom = ChatManager.Instance.ActiveChatRoom;
		if (chatRoom != null)
		{
			if (chatRoom.IsCooldownEnable && chatRoom.IsCooldowning)
			{
				string ids = Localization.instance.Get("IDS_CHAT_COOLDOWN_TIME");
				int cooldown_time = System.Convert.ToInt32(DataManager.Instance.CurGlobalConfig.getConfig("worldchat_cooldown_time").ToString());
				_inputUI.defaultText = string.Format(ids, cooldown_time);
				_inputBoxCollider.enabled = false;
			}
			else
			{
				UpdateNewMessageState();	
				
				_inputBoxCollider.enabled = true;
			}
		}	
		
		// Chat list is hided.
		if (!IsEnableChatlist())
		{				
			if ((chatRoom!=null) && chatRoom.HasNewMessage())
			{
				chatRoom.SortMessagesByStamptime();
				
				DisplayLatestMessageInInputbox(chatRoom, _inputUI);
			}
		}
		else
		{		
			DisplayMessageList();
		}
		
		if (Input.GetKeyUp(KeyCode.Return))
		{
			if (!_ignoreNextEnter && !_inputUI.selected)
			{
				_inputUI.selected = true;
			}
			
			_ignoreNextEnter = false;
		}
	}
	
	void DisplayMessageList()
	{
		IChatChannel chatRoom = ChatManager.Instance.ActiveChatRoom;
		
		if ((chatRoom!=null) && chatRoom.HasNewMessage())
		{
			RefreshMessageList();
		}
	}
	
	void RefreshMessageList()
	{		
		_textListUI.Clear();
	
		IChatChannel chatRoom = ChatManager.Instance.ActiveChatRoom;
		if (chatRoom == null)
		{
			return;
		}
		
		chatRoom.SortMessagesByStamptime();
		
		List<ChatMessage> msgList = chatRoom.GetMessages();		
		if (msgList != null)
		{
			bool hasTimeSpliter = false;
			
			int timeSpliterOld = 0;
			int timeSpliter = 0;
			
			string ids = Localization.instance.Get("IDS_CHAT_TIMELINE_TIPS");
			
			for (int i=0; i<msgList.Count; i++)
			{						
				timeSpliterOld = timeSpliter;
								
				ChatMessage chatMessage = msgList[i];
				string message = chatMessage.messageText;
				
				if (NetworkUtils.IsSystemMessage(message))
				{
					string nickName = Localization.instance.Get("IDS_MESSAGE_TYPE_SYSTEM");
					_textListUI.Add(nickName + ": " + message);
				}
				else
				{					
					if ((timeSpliterOld!=0) && (timeSpliter!=timeSpliterOld))
					{						
						string text = string.Format(ids, timeSpliterOld);
						_textListUI.Add(text);
						
						hasTimeSpliter = true;
					}
					
					_textListUI.Add("[00FF00]" + chatMessage.fromPlayerInfo.nickName + "[-]" + ": " + chatMessage.messageText);
				}
			}
			
			if (!hasTimeSpliter && (timeSpliterOld!=0))
			{
				string text = string.Format(ids, timeSpliterOld);
				_textListUI.Add(text);
			}
			else if (timeSpliter != 0)
			{
				string text = string.Format(ids, timeSpliter);
				_textListUI.Add(text);
			}
				
			chatRoom.SetNewMessageFlag(false);
			
			_scrollbar.scrollValue = 1f;
		}
	}
	
	public void SelectPrivateChannel(InJoy.FCComm.BriefPlayerInfo toUser)
	{
		UIChatHandler._isGuildChat = false;
		//SetFullScreenBGTransparent();
		
		EnableChatlist(true);
		
		_displayCheck.isChecked = true;
				
		ChatManager.Instance.CurrentPeerInfo = toUser;
		
		GameObject item = AddWhisperItem(toUser);
		if (item != null)
		{
			item.GetComponent<UICheckbox>().isChecked = true;
			_townCheck.isChecked = false;
		}
		
		SelectWhisperItem(item.transform);
		
		SelectPrivateChannel();
	}
	
	void OnSubmit()
	{		
		if(!Utils.FilterWords(_inputUI.text))
		{
			UIMessageBoxManager.Instance.ShowMessageBox(Localization.instance.Get("IDS_MESSAGE_CHAT_ILLEGAL"), Localization.instance.Get("IDS_MESSAGE_TITLE_GLOBAL"), MB_TYPE.MB_OK, null);
			return;
		}
		SendChatMessage();
	}
	
	void SendChatMessage()
	{
		if (_inputUI.text.Length <= 0 )
		{
			return;
		}
		
		string text = NGUITools.StripSymbols(_inputUI.text);
		
		IChatChannel chatRoom = ChatManager.Instance.ActiveChatRoom;
				
		if (chatRoom != null)
		{
			chatRoom.SendMessage(text);
		}
		
		_ignoreNextEnter = true;
		_inputUI.text = "";
	}
	
	// Show chat message list
	void ShowChatMessages()
	{
		bool active = IsEnableChatlist();
		
		if (!active)
		{
			_newMsgSprite.spriteName = "dialogue_icon";
			SetButtonTweenState(_displayCheck, false);
			
			_fullScreenBG.gameObject.SetActive(false);
		}
		
		EnableChatlist(_displayCheck.isChecked);
	}
	
	void EnableChatlist(bool setting)
	{
		NGUITools.SetActive(_chatHistory, setting);
		
		if (setting)
		{
			RefreshWhisperItems();
		}
	}
		
	bool IsEnableChatlist()
	{
		return NGUITools.GetActive(_chatHistory);
	}
	
	// Active town channel
	void OnClickTownChannel(bool setting)
	{
		if (setting)
		{
			ChatManager.Instance.Channel = IChatChannel.ENUM_CHAT_CHANNEL.CHANNEL_TOWN;
			IChatChannel chatRoom = ChatManager.Instance.ActiveChatRoom;
			
			if (chatRoom != null)
			{
				chatRoom.SetNewMessageFlag(true);
				
				_textListUI.Clear();
				
				SetButtonTweenState(_townCheck, false);
				
				DeselectAllWhisperItems();
			}
		}
	}
	
	// Active Guild Channel
	void OnClickGuildChannel(bool setting)
	{
		if (setting)
		{						
			_textListUI.Clear();
			
			SetButtonTweenState(_guildCheck, false);
			
			if (PlayerInfo.Instance.GuildName == "")
			{
				string tips = Localization.instance.Get("IDS_MESSAGE_NO_GUILD");
				UIMessageBoxManager.Instance.ShowMessageBox(tips, "", MB_TYPE.MB_OK, OnClickGotoGuildCallback);
				
				_townCheck.isChecked = true;
			}
			else
			{
				ChatManager.Instance.Channel = IChatChannel.ENUM_CHAT_CHANNEL.CHANNEL_GUILD;
				IChatChannel chatRoom = ChatManager.Instance.ActiveChatRoom;
				chatRoom.SetNewMessageFlag(true);
			}
			
			DeselectAllWhisperItems();
					
		}
	}
	
	// Active Private Channel
	void OnClickPrivateChannel(bool setting)
	{
		if (setting)
		{
			//_chatController.Channel = ChatRoom.ENUM_CHAT_CHANNEL.CHANNEL_PRIVATE;
			
			//_chatController.RefleshMessage(true);
			
			//_textListUI.Clear();			
		}
	}
	
	void OnClickGotoGuildCallback(ID_BUTTON buttonID)
	{
		if (buttonID == ID_BUTTON.ID_OK)
		{
//			GuildManager.Instance.tryToOpenGuildUI();
		}
	}
	
	// Active Private Channel
	public void SelectPrivateChannel()
	{
		ChatManager.Instance.Channel = IChatChannel.ENUM_CHAT_CHANNEL.CHANNEL_PRIVATE;
		//IChatChannel chatRoom = ChatManager.Instance.ActiveChatRoom;
		//chatRoom.SetNewMessageFlag(true);
		
		_fullScreenBG.gameObject.SetActive(false);
		
		RefreshMessageList();
		
		_scrollbar.scrollValue = 1f;
	}

	
	void UpdateNewMessageState()
	{
		if (!_displayCheck.isChecked)
		{		
			if (ChatManager.Instance.HasNewMessage())
			{
				_newMsgSprite.spriteName = "dialogue_icon_2";
				
				SetButtonTweenState(_displayCheck, true);
			}
		}
		else
		{
			IChatChannel chatRoom = null;
			if (IChatChannel.ENUM_CHAT_CHANNEL.CHANNEL_TOWN == ChatManager.Instance.Channel)
			{
				chatRoom = ChatManager.Instance.GetChatChannel(IChatChannel.ENUM_CHAT_CHANNEL.CHANNEL_GUILD);
				if ((chatRoom!=null) && chatRoom.HasNewMessage())
				{
					SetButtonTweenState(_guildCheck, true);
				}
				else
				{
					SetButtonTweenState(_guildCheck, false);
				}
			}
			else if (IChatChannel.ENUM_CHAT_CHANNEL.CHANNEL_GUILD == ChatManager.Instance.Channel)
			{
				chatRoom = ChatManager.Instance.GetChatChannel(IChatChannel.ENUM_CHAT_CHANNEL.CHANNEL_TOWN);				
				if ((chatRoom!=null) && chatRoom.HasNewMessage())
				{
					SetButtonTweenState(_townCheck, true);
				}
				else
				{
					SetButtonTweenState(_townCheck, false);
				}
			}		
			else
			{
				chatRoom = ChatManager.Instance.GetChatChannel(IChatChannel.ENUM_CHAT_CHANNEL.CHANNEL_GUILD);
				if ((chatRoom!=null) && chatRoom.HasNewMessage())
				{
					SetButtonTweenState(_guildCheck, true);
				}
				else
				{
					SetButtonTweenState(_guildCheck, false);
				}
				
				chatRoom = ChatManager.Instance.GetChatChannel(IChatChannel.ENUM_CHAT_CHANNEL.CHANNEL_TOWN);				
				if ((chatRoom!=null) && chatRoom.HasNewMessage())
				{
					SetButtonTweenState(_townCheck, true);
				}
				else
				{
					SetButtonTweenState(_townCheck, false);
				}
			}
		}
	}
	
	void SetButtonTweenState(UICheckbox checkbox, bool setting)
	{
		TweenScale tween = checkbox.GetComponent<TweenScale>();
		tween.enabled = setting;
	}
	
	void OnSelectedInputbox(object o)
	{
		bool val = (bool)o;
		
		if (val && !_displayCheck.isChecked)
		{			
		}
		else
		{
			_chatList.relativeOffset = new Vector2(0f,0f);
			_fullScreenBG.gameObject.SetActive(false);
			_radioButtonRoot.gameObject.SetActive(true);
		}
		
		if (val)
		{
			PupupChatUI();
			
			SetFullScreenBGTransparent();
		}
		else if (UIChatHandler._isGuildChat)
		{
//			GuildManager.Instance.tryToOpenGuildUI(false);
			UIManager.Instance.CloseUI("UITownChat");
		}
	}
	
	void PupupChatUI()
	{
		EnableChatlist(true);
			
		_newMsgSprite.spriteName = "dialogue_icon";
		SetButtonTweenState(_displayCheck, false);
		
		_displayCheck.isChecked = true;
		
		_chatList.relativeOffset = new Vector2(0f,0.6f);
		_radioButtonRoot.gameObject.SetActive(false);
	}
	
	public void EnterChatMode(IChatChannel.ENUM_CHAT_CHANNEL channel)
	{
		ChatManager.Instance.Channel = IChatChannel.ENUM_CHAT_CHANNEL.CHANNEL_GUILD;
		
		PupupChatUI();
		
		SetFullScreenBGTransparent();
		
		_guildCheck.isChecked = true;
		_inputUI.selected = true;
	}
	
	//update the info by chatroom status
	void UpdateInputboxInfo()
	{
		//get town chat room
		IChatChannel room = ChatManager.Instance.ActiveChatRoom;
		
		if (room == null)
		{
			//room is null
			_inputUI.text = _inputUI.defaultText = Localization.instance.Get("IDS_CHAT_NOT_READY");
		}
		else
		{
			_inputUI.text = _inputUI.defaultText = Localization.instance.Get("IDS_TAP_TO_CHAT");
		}
	}
	
	
	
	//////////////////////////////////////////////////////////////////////////////////////////////////////////
	public GameObject _whisperItem;
	public UINGUIGrid _whisperGrid;
	
	public GameObject AddWhisperItem(InJoy.FCComm.BriefPlayerInfo peerInfo)
	{
		GameObject item = GetWhisperItem(peerInfo);
		if (item != null)
		{
			return item;
		}
		
		GameObject o = _whisperGrid.InsertItem(0, _whisperItem);
		UIWhisperItem uiItem = o.GetComponent<UIWhisperItem>();
		uiItem.PeerInfo = peerInfo;
		uiItem.RadioButtonRoot = _radioButtonRoot;
		uiItem.ChatHandler = this;
		
		//_whisperGrid.repositionNow = true;
		
		return uiItem.gameObject;
	}
	
	GameObject GetWhisperItem(InJoy.FCComm.BriefPlayerInfo peerInfo)
	{
		foreach (Transform t in _whisperGrid.transform)
		{
			UIWhisperItem item = t.gameObject.GetComponent<UIWhisperItem>();
			if (item.PeerInfo.playerId == peerInfo.playerId)
			{
				return t.gameObject;
			}
		}
		
		return null;
	}
	
	public void RemoveWhisperItem(GameObject o)
	{
		NGUITools.Destroy(o);
		
		_whisperGrid.repositionNow = true;
		
		_townCheck.isChecked = true;
	}
	
	void ClearWhisperItemList()
	{
		List<Transform> itemlist = new List<Transform>();
		
		foreach (Transform t in _whisperGrid.transform)
		{
			itemlist.Add(t);
		}
		
		foreach (Transform t in itemlist)
		{
			t.parent = null;
			NGUITools.Destroy(t.gameObject);
		}
		
		itemlist.Clear();
	}
	
	public void RefreshWhisperItems()
	{
	}
	
	void SetFullScreenBGTransparent()
	{
		_fullScreenBG.gameObject.SetActive(true);
		
		if (!UIChatHandler._isGuildChat)
		{
			_fullScreenBG.color = new Color(1.0f,1.0f,1.0f,0.4f);
		}
		else
		{
			_fullScreenBG.color = new Color(1.0f,1.0f,1.0f,1.0f);
		}
	}
	
	public void SelectWhisperItem(Transform item)
	{
		foreach (Transform t in _whisperGrid.transform)
		{
			UIWhisperItem uiItem = t.gameObject.GetComponent<UIWhisperItem>();
			uiItem.Select(t==item);
		}
	}
	
	void DeselectAllWhisperItems()
	{
		foreach (Transform t in _whisperGrid.transform)
		{
			UIWhisperItem uiItem = t.gameObject.GetComponent<UIWhisperItem>();
			uiItem.Select(false);
		}
	}
	
	void OnApplicationPause(bool pauseStatus)
	{		
		if (!pauseStatus)
		{
			_inputUI.selected = false;
		}
	}
}
