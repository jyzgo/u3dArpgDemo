using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using FaustComm;
using System;

[NetCmdID(5006, name = "Enter game")]
public class EnterGameRequest : NetRequest
{
    public short ServerId;
    public int UID;

    public override void Encode(BinaryWriter writer)
    {
        writer.Write(ServerId);
        writer.Write(UID);
    }
}

[NetCmdID(10006, name = "Enter Game response")]
public class EnterGameResponse : NetResponse
{
	private PlayerInventory _playerInventory;

	public PlayerInventory playerInventory
	{
		get { return _playerInventory; }
	}

    public override void Decode(BinaryReader reader)
    {
		InJoyDebugBase.DebugCommandResponse(this,ref reader);
        errorCode = reader.ReadInt16();

		if (errorCode == 0)
		{
			//get inventory
			_playerInventory = new PlayerInventory();

			_playerInventory.Parse(reader);
		}
    }
}
