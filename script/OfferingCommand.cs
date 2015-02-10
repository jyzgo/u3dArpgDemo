using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;

using FaustComm;

[NetCmdID(5010, name = "Offering Cmd Send to Server")]
public class OfferingRequest : NetRequest
{
    public byte type;
    public string itemId;
    public byte useHc;

    
    public override void Encode(BinaryWriter writer)
    {
        writer.Write(type);
        WriteString(writer, itemId);
        writer.Write(useHc);
    }

    public override string ToString()
    {
        return "type=\"" + type + "\", itemId=\"" + itemId + "\",useHc=\""+useHc+ "\"";
    }

}


[NetCmdID(10010, name = "Offering Msg Get from Server")]
public class OfferingResponse : NetResponse
{
    public string itemId;
    public UpdateInforResponseData updateData;

    public override void Decode(BinaryReader reader)
    {
        errorCode = reader.ReadInt16();

        if (Succeeded)
        {
            itemId = NetResponse.ReadString(reader);
            updateData = UpdateInforResponseHandler.Instance.UpdateOperation(reader);
        }
    }

    public int SCChangeValue()
    {
        int changeSC = 0;
        int oldPlayerSC = PlayerInfo.Instance.SoftCurrency;
        foreach(PlayerProp prop in updateData.playerPropsList)
        {
            if (prop.Key == PlayerPropKey.SoftCurrency)
            {
                changeSC = prop.Value - oldPlayerSC;
            }
        }
        return changeSC;
    }
}