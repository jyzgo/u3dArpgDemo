using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;

using FaustComm;

[NetCmdID(5007, name = "ItemOperationRequest")]
public class ItemOperationRequest : NetRequest
{
    public long ItemGuid;
    public InventoryMenuItemOperationType OprationType;

    public override void Encode(BinaryWriter writer)
    {
        writer.Write((Int64)ItemGuid);
        writer.Write((byte)OprationType);
    }
}

[NetCmdID(10007, name = "ItemOperationResonse")]
public class ItemOperationResponse : NetResponse
{
    public InventoryMenuItemOperationType Operation;

    public UpdateInforResponseData updateData;

    public override void Decode(BinaryReader reader)
    {
        errorCode = reader.ReadInt16();
        if (Succeeded)
        {
            Operation = (InventoryMenuItemOperationType)reader.ReadByte();
            updateData = UpdateInforResponseHandler.Instance.UpdateOperation(reader);
        }
    }
}

[NetCmdID(5023, name = "C2S_INCREMENT_INVENTORY")]
public class IncrementInventoryRequest : NetRequest
{
    public override void Encode(BinaryWriter writer)
    {
    }
}

[NetCmdID(10023, name = "S2C_INCREMENT_INVENTORY")]
public class IncrementInventoryResponse : NetResponse
{
    public UpdateInforResponseData data;

    public override void Decode(BinaryReader reader)
    {
        errorCode = reader.ReadInt16();
        if(Succeeded)
            data = UpdateInforResponseHandler.Instance.UpdateOperation(reader);
    }
}
