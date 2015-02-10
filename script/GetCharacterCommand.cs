using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;

using FaustComm;

[NetCmdID(5004, name = "Get characters")]
public class GetCharactersRequest : NetRequest
{
	public short serverId;

	public override void Encode(BinaryWriter writer)
	{
		writer.Write(serverId);
	}	
}

[NetCmdID(10004, name = "Retrive characters from server"),Serializable]
public class GetCharactersResponse : NetResponse
{
	public Dictionary<EnumRole, PlayerInfo> playerInfos;

	public override void Decode(BinaryReader reader)
	{

		InJoyDebugBase.DebugCommandResponse(this,ref reader);


		Debug.Log (" GetCharactersResponse sucess");

		errorCode = reader.ReadInt16();
		
		if (errorCode == 0)
		{
			int playerCount = reader.ReadByte();
            playerInfos = new Dictionary<EnumRole, PlayerInfo>();
			for (int i = 0; i < playerCount; i++)
			{
                PlayerInfo playerInfo = new PlayerInfo();
				playerInfo.Parse(reader);
                playerInfos.Add(playerInfo.Role, playerInfo);
			}
		}
	}

	
}