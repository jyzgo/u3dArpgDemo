using System;

using System.IO;

//base class for all server attributes. These classes always get values from server.
public abstract class ServerMessage
{
	public abstract void Parse(BinaryReader reader);
}
