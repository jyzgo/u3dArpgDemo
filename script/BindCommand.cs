using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;

using FaustComm;

[NetCmdID(5003, name = "Bind Account Cmd Send to Server")]
public class BindRequest : NetRequest
{
    public string account;
    public string password;
    public string newAccount;
    public string newPassword;


    public override void Encode(BinaryWriter writer)
    {
        WriteString(writer, account);  //login request does not have AccountId and SessionId
        WriteString(writer, password);
        WriteString(writer, newAccount); 
        WriteString(writer, newPassword);
    }

    public override string ToString()
    {
        return "account=\"" + account + "\", password=\"" + password + "\"";
    }
}

[NetCmdID(10003, name = "Bind Account Msg Get from Server"),Serializable]
public class BindResponse : NetResponse
{
    public override void Decode(BinaryReader reader)
    {
        errorCode = reader.ReadInt16();
    }

}
