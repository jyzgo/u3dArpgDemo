using System;
using System.Collections.Generic;
using UnityEngine;
using FaustComm;
using System.IO;

public enum MailOperation
{ 
    RemoveOne = 1,
    CollectOne = 2,
    RemoveAll = 3,
    CollectAll = 4
}

[NetCmdID(5027, name = "get mails list")]
public class MailListRequest : NetRequest
{
    public int StartMailId;

    public override void Encode(BinaryWriter writer)
    {
        writer.Write(StartMailId);
    }
}

[NetCmdID(5028, name = "get mail detail")]
public class MailDetailRequest : NetRequest
{
    public int MailId;

    public override void Encode(BinaryWriter writer)
    {
        writer.Write(MailId);
    }
}

[NetCmdID(5029, name = "mail operation")]
public class MailOperationRequest : NetRequest
{
    public MailOperation Operation;

    public int MailId;

    public override void Encode(BinaryWriter writer)
    {
        writer.Write((byte)Operation);
        writer.Write(MailId);
    }
}

[NetCmdID(10027, name = "mails list")]
public class MailListResponse : NetResponse
{
    public int MaxMailId;

    public MailVo[] MailVos;

    public override void Decode(BinaryReader reader)
    {
        errorCode = reader.ReadInt16();
        if (Succeeded)
        {
            MaxMailId = reader.ReadInt32();
            int num = reader.ReadInt16();
            MailVos = new MailVo[num];
            for (int i = 0; i < num; i++)
            {
                MailVo mv = new MailVo();
                mv.MailId = reader.ReadInt32();
                mv.Title = ReadString(reader);
                int status = reader.ReadByte();
                mv.HasRead = (status & (1 << 0)) != 0;
                mv.Collectable = (status & (1 << 1)) != 0;
                int expireTime = reader.ReadInt32();
                mv.ExpireTime = TimeUtils.ConvertFromUnixTimestamp(expireTime);
                mv.IsComplete = false;
                MailVos[i] = mv;
            }
        }
    }

    public int UnReadMailCount()
    {
        int sum = 0;
        foreach(MailVo mailVo in MailVos)
        {
            sum += mailVo.HasRead ? 0 : 1;
        }
        return sum;
    }
}

[NetCmdID(10028, name = "mail detail")]
public class MailDetailResponse : NetResponse
{
    public int MailId;

    public DateTime ReceiveTime;

    public string Content;

    public List<MailAttachmentVo> Attachments;

    public override void Decode(BinaryReader reader)
    {
        errorCode = reader.ReadInt16();
        if (Succeeded)
        {
            MailId = reader.ReadInt32();
            int time = reader.ReadInt32();
            ReceiveTime = TimeUtils.ConvertFromUnixTimestamp(time);
            Content = ReadString(reader);
            int count = reader.ReadByte();
            Attachments = new List<MailAttachmentVo>();
            for (int i = 0; i < count; i++)
            {
                MailAttachmentVo mav = new MailAttachmentVo();
                mav.itemId = ReadString(reader);
                mav.amount = reader.ReadInt32();
                Attachments.Add(mav);
            }
        }
    }
}

[NetCmdID(10029, name = "mails operation")]
public class MailOperationResponse : NetResponse
{
    public int MailId;

    public MailOperation Operation;

    public UpdateInforResponseData UpdateData;

    public override void Decode(BinaryReader reader)
    {
        errorCode = reader.ReadInt16();
        if (Succeeded)
        {
            Operation = (MailOperation)reader.ReadByte();
            MailId = reader.ReadInt32();
            UpdateData = UpdateInforResponseHandler.Instance.UpdateOperation(reader);
        }
    }
}