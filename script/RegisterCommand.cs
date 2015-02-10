using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;

using FaustComm;

[NetCmdID(5002, name = "Register with account and password")]
public class RegisterRequest : NetRequest
{
	public string account;
	public string password;
	public string os = string.Empty;
	
	public override void Encode(BinaryWriter writer)
	{
		WriteString(writer, account);  //login request does not have AccountId and SessionId
		WriteString(writer, password);
		WriteString(writer, os);
	}
	
	public override string ToString()
	{ 
		return "account=\"" + account + "\", password=\"" + password + "\"";
	}
}

[NetCmdID(10002, name = "Register info process")]
public class RegisterResponse : NetResponse
{
	public int accountId;
	public int session;
	
	public override void Decode(BinaryReader reader)
	{
		errorCode = reader.ReadInt16();
		
		if (errorCode == 0)
		{
			accountId = reader.ReadInt32();
			session = reader.ReadInt32();
		}
	}
	
}