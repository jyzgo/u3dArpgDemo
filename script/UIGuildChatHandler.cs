using UnityEngine;
using System.Collections.Generic;

public class UIGuildChatHandler : MonoBehaviour 
{
	public UITextList _textListUI;
	public UIInput _inputUI;
	public UIScrollBar _uiScrollbar;
	BoxCollider _inputBoxCollider;
	
	
	// Use this for initialization
	void Start () 
	{
	}
	
	void OnEnable()
	{
		_inputBoxCollider = _inputUI.GetComponent<BoxCollider>();
		_inputUI.text = _inputUI.defaultText = Localization.instance.Get("IDS_TAP_TO_CHAT");

		IChatChannel chatChannel = ChatManager.Instance.GetChatChannel(IChatChannel.ENUM_CHAT_CHANNEL.CHANNEL_GUILD);
		if (chatChannel != null)
		{
			chatChannel.SetNewMessageFlag(true);
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		// Check chat service

		{
			_inputUI.defaultText = Localization.instance.Get("IDS_TAP_TO_CHAT");
			_inputBoxCollider.enabled = true;
		}
		
		IChatChannel chatChannel = ChatManager.Instance.GetChatChannel(IChatChannel.ENUM_CHAT_CHANNEL.CHANNEL_GUILD);
		if ((chatChannel!=null) && chatChannel.HasNewMessage())
		{
			_textListUI.Clear();
		
			chatChannel.SortMessagesByStamptime();
			List<ChatMessage> msgList = chatChannel.GetMessages();
			
			for (int i=0; i<msgList.Count; i++)
			{							
				ChatMessage chatMessage = msgList[i];
				string message = chatMessage.messageText;
				
				if (NetworkUtils.IsSystemMessage(message))
				{
					string nickName = Localization.instance.Get("IDS_MESSAGE_TYPE_SYSTEM");
					
					_textListUI.Add(nickName + ": " + message);
				}
				else
				{					
					_textListUI.Add("[00FF00]" + chatMessage.fromPlayerInfo.nickName + "[-]" + ": " + message);
				}		
			}
			
			chatChannel.SetNewMessageFlag(false);
			_uiScrollbar.scrollValue = 1f;
		}
	}
	
	void OnSelectedInputbox(object o)
	{
		bool val = (bool)o;
		
		if (val)
		{
			UIChatHandler._isGuildChat = true;
			
			UIManager.Instance.CloseUI("Guild");
			UIManager.Instance.OpenUI("UITownChat");
			
			GameObject ui = UIManager.Instance.GetUI("UITownChat");
			UIChatHandler chat = ui.GetComponent<UIChatHandler>();
			chat.EnterChatMode(IChatChannel.ENUM_CHAT_CHANNEL.CHANNEL_GUILD);
		}
	}
}
