using UnityEngine;
using System.Collections.Generic;

public abstract class IChatChannel
{
	protected static int MAX_MESSAGE_COUNT = 50;
	
	public enum ENUM_CHAT_CHANNEL
	{
		CHANNEL_TOWN,
		CHANNEL_GUILD,
		CHANNEL_PRIVATE,
		
		CHANNEL_COUNT
	}
	
	// Guild Name or Player Id
	protected string _channelId;
	
	protected ENUM_CHAT_CHANNEL _type;
	public ENUM_CHAT_CHANNEL Type
	{
		get
		{
			return _type;
		}
	}
	
	public bool IsCooldownEnable {set; get;}
	public bool IsCooldowning {set; get;}
	
	
	public abstract void SendMessage(string message);	
	public abstract void Join();
	public abstract void Leave();
	public abstract void Reset();
	
	public abstract void SortMessagesByStamptime();	
	public abstract ChatMessage GetLatestMessage();
	public abstract List<ChatMessage> GetMessages();
	
	public abstract void SetNewMessageFlag(bool hasNew);
	public abstract bool HasNewMessage();
	public abstract void Update();
}
