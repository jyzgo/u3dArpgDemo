using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using InJoy.FCComm;


public class ChatManager : MonoBehaviour
{
	private Dictionary<IChatChannel.ENUM_CHAT_CHANNEL,IChatChannel> _channelsList = new Dictionary<IChatChannel.ENUM_CHAT_CHANNEL, IChatChannel>();
		
	#region singleton defines	
	static ChatManager _inst;
	static public ChatManager Instance 
	{
		get 
		{
			return _inst;
		}
	}
	
	void Awake() 
	{
		if (_inst != null)
		{
            Debug.LogError("ChatManager: detected singleton instance has existed. Destroy this one " + gameObject.name);
			Destroy(this);
			return;
		}
		
		_inst = this;
	}
	
	void OnDestroy()
	{
		if(_inst == this)
		{
			_inst = null;
		}
	}
	#endregion
	
	
	private IChatChannel.ENUM_CHAT_CHANNEL _currentChannel = IChatChannel.ENUM_CHAT_CHANNEL.CHANNEL_TOWN;
	public IChatChannel.ENUM_CHAT_CHANNEL Channel
	{
		set
		{
			_currentChannel = value;
		}
		get
		{
			return _currentChannel;
		}
	}
	
	private BriefPlayerInfo _currentPeerInfo;
	public BriefPlayerInfo CurrentPeerInfo
	{
		set
		{
			_currentPeerInfo = value;
		}
		get
		{
			return _currentPeerInfo;
		}
	}
	
	public IChatChannel ActiveChatRoom
	{
		get
		{
			if (_channelsList.ContainsKey(_currentChannel))
			{
				return _channelsList[_currentChannel];
			}
			else
			{
				return null;
			}
		}
	}
	
	public void Initialize()
	{
		JoinTownRoom();
		
		JoinWhisperRoom();
	}
	
	public void JoinGuildRoom(string guildName)
	{
		CreateChannel(guildName, IChatChannel.ENUM_CHAT_CHANNEL.CHANNEL_GUILD);
	}
	
	public void JoinWhisperRoom()
	{
		CreateChannel("whisper_chat", IChatChannel.ENUM_CHAT_CHANNEL.CHANNEL_PRIVATE);
	}
	
	// id is guild name or player id.
	public IChatChannel CreateChannel(string name, IChatChannel.ENUM_CHAT_CHANNEL channel)
	{
		IChatChannel chatChannel = null;
		
		//todo_network: chatChannel = new WhisperChat(name, channel);

		chatChannel.Join();
		_channelsList.Add(channel, chatChannel);
		
		return chatChannel;
	}
	
	public void LeaveGuildRoom()
	{
		IChatChannel chatChannel = null;
		if (_channelsList.TryGetValue(IChatChannel.ENUM_CHAT_CHANNEL.CHANNEL_GUILD, out chatChannel))
		{
			chatChannel.Leave();
			_channelsList.Remove(IChatChannel.ENUM_CHAT_CHANNEL.CHANNEL_GUILD);
			
			return;
		}
	}
	
	public IChatChannel GetChatChannel(IChatChannel.ENUM_CHAT_CHANNEL channel)
	{
		IChatChannel chatChannel = null;
		if (_channelsList.TryGetValue(channel, out chatChannel))
		{
			return chatChannel;
		}
		
		return null;
	}

    private void JoinTownRoom()
    {
//        Hashtable payload = new Hashtable();
//        payload.Add("messageType", "get_room_name");
//        payload.Add("language", LocalizationContainer.CurSystemLang);
//        string command = InJoy.Utils.FCJson.jsonEncode(payload);

        DoJoinTownRoom();
    }
	
	void DoJoinTownRoom()
	{
	}

    void OnGetRoomNameResponse(FaustComm.NetResponse response)
    {
    }
	
	public void ReconnectChatRoom()
	{
		foreach (KeyValuePair<IChatChannel.ENUM_CHAT_CHANNEL,IChatChannel> pair in _channelsList)
		{
			if ((pair.Key == IChatChannel.ENUM_CHAT_CHANNEL.CHANNEL_TOWN) ||
				(pair.Key == IChatChannel.ENUM_CHAT_CHANNEL.CHANNEL_GUILD))
			{
				pair.Value.Join();
			}
		}
	}
	
	public bool HasNewMessage(IChatChannel.ENUM_CHAT_CHANNEL channelIndex)
	{
		if (_channelsList.ContainsKey(channelIndex))
		{
			if (_channelsList[channelIndex].HasNewMessage())
			{
				return true;
			}
		}
		
		return false;
	}
	
	public bool HasNewMessage()
	{
		foreach (KeyValuePair<IChatChannel.ENUM_CHAT_CHANNEL,IChatChannel> pair in _channelsList)
		{
			if (pair.Value.HasNewMessage())
			{
				return true;
			}
		}
		
		return false;
	}
	
	void Update()
	{
		foreach (KeyValuePair<IChatChannel.ENUM_CHAT_CHANNEL,IChatChannel> pair in _channelsList)
		{
			pair.Value.Update();
		}
	}
}
