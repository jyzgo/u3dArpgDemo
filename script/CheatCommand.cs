using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;

using FaustComm;

[NetCmdID(5017, name = "Cheat Cmd Send to Server")]
public class CheatRequest : NetRequest
{
	public string command;

	public override void Encode(BinaryWriter writer)
	{
		WriteString(writer, command);  //login request does not have AccountId and SessionId
	}
	
	public override string ToString()
	{ 
		return "command =\"" + command + "\"";
	}
}

[NetCmdID(10017, name = "Cheat Msg Get from Server")]
public class CheatResponse : NetResponse
{
	public string response;

    public UpdateInforResponseData UpdateData;

	public override void Decode(BinaryReader reader)
	{
		errorCode = reader.ReadInt16();
		
		if (errorCode == 0)
		{
			response = ReadString(reader);
            UpdateData = UpdateInforResponseHandler.Instance.UpdateOperation(reader);
		}
	}
}
