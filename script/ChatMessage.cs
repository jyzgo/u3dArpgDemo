using UnityEngine;
using System.Collections;
using InJoy.FCComm;

/// <summary>
/// Encapsulates a room message or a private message into a single class to simplify message handling. In the demo app, private messages are displayed
/// in a different color.
/// </summary>
public class ChatMessage
{
	/// <summary>
	/// Chat message type.
	/// </summary>
	public enum ChatMessageType
	{
		ChatMessageTown,	/**< Global chat message */
		ChatMessageGuild,	/**< Room chat message */
		ChatMessagePrivate,	/**< Private chat message */
		ChatMessageSystem	/**< System chat message (currently not supported) */
	};
	
    public InJoy.FCComm.BriefPlayerInfo fromPlayerInfo { set; get; }
    public string room { set; get; }
	public System.DateTime timestamp { set; get; }
    public string messageText { set; get; }
    public ChatMessageType messageType { set; get; }
	
	// ------------------------------------------------------------------------------------
	/// <summary>
	/// Initializes a new instance of the <see cref="ChatMessage"/> class.
	/// </summary>
	/// <param name='roomMessage'>
	/// Room message.
	/// </param>
	// ------------------------------------------------------------------------------------
    public ChatMessage(FaustComm.NetResponse response)
	{

	}
	
	// ------------------------------------------------------------------------------------
	/// <summary>
	/// Initializes a new instance of the <see cref="ChatMessage"/> class.
	/// </summary>
	/// <param name='privateMessage'>
	/// Private message.
	/// </param>
	/// <param name='room'>
	/// Room.
	/// </param>
	// ------------------------------------------------------------------------------------
    public ChatMessage(FaustComm.NetResponse response, string room)
	{

	}
	
	public ChatMessage(string text)
	{
		fromPlayerInfo = null;
		room = "";
		messageText = text;
		messageType = ChatMessageType.ChatMessageSystem;
		
		timestamp = System.DateTime.Now;
	}
}
