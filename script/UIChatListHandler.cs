using UnityEngine;
using System.Collections;

public class UIChatListHandler : MonoBehaviour 
{
	public UIInput _inputBox;

	
	// Use this for initialization
	void Start () 
	{
	
	}
	
	void OnEnable()
	{
		_inputBox.defaultText = Localization.instance.Get("IDS_TAP_TO_CHAT");
	}
	
	void OnDisable()
	{
		if (ChatManager.Instance == null)
		{
			return;
		}
		
		IChatChannel chatRoom = ChatManager.Instance.ActiveChatRoom;
			
		if (chatRoom != null)
		{
			UIChatHandler.DisplayLatestMessageInInputbox(chatRoom, _inputBox);
		}
	}
}
