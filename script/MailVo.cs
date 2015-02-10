using System;
using System.Collections.Generic;

public class MailVo : IComparable
{
    public int MailId;

    public DateTime ReceiveTime;

    public string Title;

    public string TitleDisplay
    {
        get
        {
            if (Title.Length > 17)
                return Title.Substring(0, 14) + "...";
            else
                return Title;
        }
    }

    public string Content;

    public bool HasRead;

    public bool Collectable;

    public DateTime ExpireTime;

    public List<MailAttachmentVo> Attachments;

    public bool IsContainsAttachment
    {
        get
        {
            return Attachments != null && Attachments.Count > 0;
        }
    }

    public bool IsComplete;

    public int CompareTo(object obj)
    {
        MailVo test = (MailVo)obj;
        if (!this.HasRead && test.HasRead)
        {
            return -1;
        }
        else if (this.HasRead && !test.HasRead)
        {
            return 1;
        }
        else if (MailId > test.MailId)
        {
            return 1;
        }
        else if (MailId < test.MailId)
        {
            return -1;
        }
        else
        {
            return 0;
        }
    }
}

public enum MailStatus
{
    unread = 0,
    read
}

public struct MailAttachmentVo
{
    public MailAttachmentVo(string itemId, int amount)
    {
        this.itemId = itemId;
        this.amount = amount;
    }

    public string itemId;

    public int amount;
}
