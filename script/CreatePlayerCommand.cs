using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;

using FaustComm;

[NetCmdID(5005, name = "Create a player")]
public class CreatePlayerRequest : NetRequest
{
	public short serverId;
	public string nickName;
	public byte job;
	
	public override void Encode(BinaryWriter writer)
	{
		writer.Write(serverId);
		WriteString(writer, nickName);
		writer.Write(job);
	}
	
	public override string ToString()
	{ 
		return string.Format("Server id = {0}\\ttNickname = {1}\t\tJob={2}", serverId, nickName, job);
	}
}

[NetCmdID(10005, name = "Create player response")]
public class CreatePlayerResponse : NetResponse
{
	public PlayerInfo playerInfo;

	public override void Decode(BinaryReader reader)
	{
		errorCode = reader.ReadInt16();
		
		if (errorCode == 0)
		{
            playerInfo = new PlayerInfo();
			playerInfo.Parse(reader);
		}
	}
	
}