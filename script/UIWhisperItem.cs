using UnityEngine;
using System.Collections;

public class UIWhisperItem : MonoBehaviour 
{
	private UIChatHandler _chatHandler;
	public UIChatHandler ChatHandler
	{
		set
		{
			_chatHandler = value;
			
			UICheckbox checkbox = gameObject.GetComponent<UICheckbox>();
			checkbox.eventReceiver = _chatHandler.gameObject;
		}
	}
	
	public UILabel _nickname;
	
	private InJoy.FCComm.BriefPlayerInfo _peerInfo;
	public InJoy.FCComm.BriefPlayerInfo PeerInfo
	{
		set
		{
			_peerInfo = value;
		}
		get
		{
			return _peerInfo;
		}
	}
	
	private Transform _radioButtonRoot;
	public Transform RadioButtonRoot
	{
		set
		{
			_radioButtonRoot = value;
			
			UICheckbox checkbox = gameObject.GetComponent<UICheckbox>();
			checkbox.radioButtonRoot = _radioButtonRoot;
		}
	}
	
	public GameObject _closeButton;
	
	private TweenScale _tweener;

	
	void Awake()
	{
		_closeButton.SetActive(false);
		_tweener = gameObject.GetComponent<TweenScale>();
		_tweener.enabled = false;
	}
	
	// Use this for initialization
	void Start () 
	{
		if (_peerInfo.nickName.Length > 5)
		{
			_nickname.text = _peerInfo.nickName.Substring(0, 5) + "...";
		}
		else
		{		
			_nickname.text = _peerInfo.nickName;	
		}
		
		UICheckbox checkbox = gameObject.GetComponent<UICheckbox>();
		checkbox.radioButtonRoot = _radioButtonRoot;
		
//		_whisperRoom = ChatManager.Instance.GetWhisperRoom();
	}
	
	void Update()
	{
		UpdateTweenState();
	}
	
	void OnClick()
	{
		ChatManager.Instance.CurrentPeerInfo = _peerInfo;
		
		_chatHandler.SelectPrivateChannel();
		
		_chatHandler.SelectWhisperItem(transform);
	}
	
	void OnClosePeer()
	{
//		WhisperChat whisperRoom = ChatManager.Instance.GetWhisperRoom();
//		whisperRoom.KillPeer(_peerInfo);
		
		_chatHandler.RemoveWhisperItem(gameObject);
	}
	
	public void Select(bool val)
	{
		_closeButton.SetActive(val);
	}
	
	void UpdateTweenState()
	{
//		if (_whisperRoom.HasNewMessage(PeerInfo))
//		{
//			
//			_tweener.enabled = true;
//		}
//		else
		{
			_tweener.enabled = false;
		}
	}
}
