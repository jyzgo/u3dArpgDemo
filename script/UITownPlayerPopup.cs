using UnityEngine;
using System.Collections;
using InJoy.FCComm;

public class UITownPlayerPopup : MonoBehaviour {
	
	public static GameObject _selectedPlayer;
		
	public Transform _offset;
	
	public GameObject _equipmentButton;
	public GameObject _chatButton;
	public GameObject _beFriendButton;
	public GameObject _closeButton;
	
	public UIImageButton _guildInvite;
	
	
	void Awake()
	{	
		_equipmentButton.GetComponent<UIButtonMessage>().target = gameObject;
		_equipmentButton.GetComponent<UIButtonMessage>().functionName = "OnClickEquipment";
		
		_beFriendButton.GetComponent<UIButtonMessage>().target = gameObject;
		_beFriendButton.GetComponent<UIButtonMessage>().functionName = "OnClickFriendButton";
		
		_chatButton.GetComponent<UIButtonMessage>().target = gameObject;
		_chatButton.GetComponent<UIButtonMessage>().functionName = "OnClickChat";
		
		_closeButton.GetComponent<UIButtonMessage>().target = gameObject;
		_closeButton.GetComponent<UIButtonMessage>().functionName = "OnClose";
		
		//ChangeButtonState(_chatButton,false);
		
	}
	
	
	public void ChangeButtonState(GameObject button, bool active)
	{
		button.GetComponent<UIImageButton>().isEnabled = active;
	}
	
	public void OnClickEquipment()
	{
		
		
		if (!UIManager.Instance.IsUIOpened("TownPlayer"))
		{
			OnClose();
			UIManager.Instance.OpenUI("TownPlayer");
		}
	}
	
	public void OnClickChat()
	{
		OnClose();

		OnClickChatWithFriend();
	}
	
	public void OnClickGuild()
	{
		OnClose();
//		GuildManager.Instance.invite(UITownPlayer._activePlayerInfo.Nickname, GuildManager.Instance);
	}
	
	public void OnClickChatWithFriend()
	{
//		PlayerInfo playerInfo = new PlayerInfo(UITownPlayer._activePlayerInfo._id,UITownPlayer._activePlayerInfo.Nickname);
		
//		GameObject o = UIManager.Instance.GetUI("UITownChat");
//		UIChatHandler chat = o.GetComponent<UIChatHandler>();
//		chat.SelectPrivateChannel(playerInfo);
	}
	
	
	public void OnClickFriendButton()
	{
		OnClose();
	}
	
	public void OnClose()
	{
		_selectedPlayer = null;
		UIManager.Instance.CloseUI("TownPlayerPopup");
		SoundManager.Instance.PlaySoundEffect("button_cancel");
	}
	
	
	public void OnInitialize()
	{
//		ChangeButtonState(_beFriendButton, !isFriend);
		
		Camera _modelCamera = CameraController.Instance.MainCamera;
		Vector3 screenPoint  = _modelCamera.WorldToScreenPoint(_selectedPlayer.transform.position);
		Vector3 finalPoint = InScreen(screenPoint);
		
		_offset.localPosition = InScreen(finalPoint);
		
//		if(!CheatManager._clientCheck || (GuildManager.Instance.HasGuild && (int)GuildManager.Instance.SelfMember._attrs[GuildManager.ATTR_RANK] >= GuildManager.RANKS[GuildManager.RANK_MANAGER] 
//			&& string.IsNullOrEmpty(UITownPlayer._activePlayerInfo._guildName)))
//		{
//			_guildInvite.isEnabled = true;
//		}
//		else
//		{
//			_guildInvite.isEnabled = false;
//		}
	}
	
	
	Vector3 InScreen(Vector3 screenPoint)
	{
		int w = 284;
		int h = 200;
		int border = 30;
		
		float screenW = Screen.width * 1.0f/ Screen.height * 640;
		float screenH = 640; 
		
		float x = screenPoint.x;
		float y = screenPoint.y;
		
		if(x  < border + w/2)
		{
			x = border + w/2;	
		}
		if(x  > screenW - w/2 - border)
		{
			x = screenW - w/2 - border;
		}
		
		if(y  < border + h/2) 
		{
			y = border + h/2;	
		}
		if(y  > screenH - h/2 - border)
		{
			y = screenH - h/2 - border;
		}

		return new Vector3(x, y, 0);
	}
	
}
