using System;
using System.Collections.Generic;
using System.IO;

using FaustComm;

[NetCmdID(5011, name = "fusion")]
public class FusionRequest : NetRequest
{
    public Int64 ItemGUID;
    public byte IsUseHC;

    public override void Encode(BinaryWriter writer)
    {
        writer.Write(ItemGUID);
        writer.Write(IsUseHC);
    }
}


[NetCmdID(10011, name = "fusion")]
public class FusionResponse : NetResponse
{
    public Int64 ItemGUID;

    public Int16 FusionLevel;

    public UpdateInforResponseData updateData; 

    public override void Decode(BinaryReader reader)
    {
        errorCode = reader.ReadInt16();
        if (Succeeded)
        {
            ItemGUID = reader.ReadInt64();
            FusionLevel = reader.ReadInt16();
            updateData = UpdateInforResponseHandler.Instance.UpdateOperation(reader);
        }
    }
}