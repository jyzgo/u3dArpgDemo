using System;
using System.Collections.Generic;
using UnityEngine;
using FaustComm;

public class MailListContainer : MonoBehaviour
{
    public delegate void OnClickMailItemDelegate(MailVo mv);
    public OnClickMailItemDelegate OnClickMailItemHandler;

    public UIPanel mailListScrollPanel;

    public UIGrid mailListScrollPanelGrid;

    public UIDraggablePanel mailListDraggablePanel;

    public MailListItem templateMailItem;

    public UIImageButton receiveALLAttachmentsButton;

    public UIImageButton removeAllMailsButton;

    public UILabel noMailsLabel;

    public UILabel unreadCountLabel;

    private List<MailListItem> _mailItemsList = new List<MailListItem>();

    private int _maxMailId = 0;

    void Start()
    {
        receiveALLAttachmentsButton.gameObject.SetActive(false);
        removeAllMailsButton.gameObject.SetActive(false);
        templateMailItem.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        mailListScrollPanel.gameObject.SetActive(true);
    }

    void OnDisable()
    {
        mailListScrollPanel.gameObject.SetActive(false);
    }

    void Test()
    {
        MailVo[] mailvos = new MailVo[10];
        for (int i = 0; i < 10; i++)
        {
            MailVo mv = new MailVo();
            mv.MailId = UnityEngine.Random.Range(1, int.MaxValue);
            mv.Title = string.Format("obama send a mail to you {0}", i);
            mv.Content = "Barack H. Obama is the 44th President of the United States. \n His story is the American story — values from the heartland, a middle-class upbringing in a strong family, hard work and education as the means of getting ahead, and the conviction that a life so blessed should be lived in service to others.";
            mv.ReceiveTime = System.DateTime.Now;
            mv.ExpireTime = System.DateTime.Now.Add(new System.TimeSpan(3, 0, 0, 0));
            mv.HasRead = i % 3 == 0;
            mv.Attachments = new List<MailAttachmentVo>();
            mv.Attachments.Add(new MailAttachmentVo("all_ring_1_white_1", 2));
            mailvos[i] = mv;
        }
        AddMails(mailvos);
        ShowOrHideNoMailsLabel(_mailItemsList.Count == 0);
    }

    public void RequestMore()
    {
        NetworkManager.Instance.MailsList(_maxMailId, OnMailListHandler);
    }

    public void Refresh()
    {
        SortMails();
        foreach (MailListItem item in _mailItemsList)
        {
            item.Refresh();
        }
        ShowOrHideNoMailsLabel(_mailItemsList.Count == 0);
        RefreshUnReadMailsCount();
    }

    void OnMailListHandler(NetResponse response)
    {
        if (response.Succeeded)
        {
            MailListResponse mlResponse = (MailListResponse)response;
            _maxMailId = mlResponse.MaxMailId;
            AddMails(mlResponse.MailVos);
            ShowOrHideNoMailsLabel(_mailItemsList.Count == 0);
            RefreshUnReadMailsCount();
            Refresh();
        }
        ShowOrHideNoMailsLabel(_mailItemsList.Count == 0);
    }

    void ShowOrHideNoMailsLabel(bool isShow)
    {
        noMailsLabel.gameObject.SetActive(isShow);
    }

    public void RemoveMail(int mailId)
    {
        MailListItem item = GetItemByMailId(mailId);
        _mailItemsList.Remove(item);
        item.transform.parent = null;
        Destroy(item.gameObject);
        RefreshUnReadMailsCount();
        SortMails();
    }

    void RemoveAllMails()
    {
        //to do
    }

    void AddMails(MailVo[] mails)
    {
        foreach (MailVo mailVo in mails)
        {
            MailListItem item = MakeMailItem(mailVo);
            item.OnClicKHandler = OnClickMailItem;
        }
        SortMails();
    }

    void SortMails()
    {
        float originaly = mailListScrollPanel.clipRange.w / 2 - mailListScrollPanelGrid.cellHeight / 2 - 10;
        mailListScrollPanel.clipRange = new Vector4(mailListScrollPanel.clipRange.x,
            -originaly,
            mailListScrollPanel.clipRange.z,
            mailListScrollPanel.clipRange.w);
        mailListScrollPanel.transform.localPosition = new Vector3(mailListScrollPanel.clipRange.x,
            originaly,
            mailListScrollPanel.transform.localPosition.z);
        mailListDraggablePanel.disableDragIfFits = _mailItemsList.Count < 3;

        _mailItemsList.Sort(delegate(MailListItem left, MailListItem right)
        {
            return left.MailVo.CompareTo(right.MailVo);
        });

        int index = 0;
        foreach (MailListItem item in _mailItemsList)
        {
            item.name = index.ToString("000") + "(sort)";
            ++index;
        }
        mailListScrollPanelGrid.repositionNow = true;
    }

    public MailListItem GetItemByMailId(int mailId)
    {
        MailListItem item = _mailItemsList.Find(delegate(MailListItem test)
        {
            return test.MailVo.MailId == mailId;
        });
        return item;
    }

    MailListItem MakeMailItem(MailVo mailVo)
    {
        GameObject go = GameObject.Instantiate(templateMailItem.gameObject) as GameObject;
        go.transform.parent = mailListScrollPanelGrid.transform;
        go.transform.localScale = Vector3.one;
        go.transform.localPosition = templateMailItem.transform.localPosition;
        go.SetActive(true);
        MailListItem mailItem = go.GetComponent<MailListItem>();
        mailItem.MailVo = mailVo;
        _mailItemsList.Add(mailItem);
        return mailItem;
    }

    void RefreshUnReadMailsCount()
    {
        int unreadCount = 0;
        foreach (MailListItem item in _mailItemsList)
        {
            if (!item.MailVo.HasRead)
                ++unreadCount;
        }
        unreadCountLabel.text = string.Format(Localization.Localize("IDS_MESSAGE_MAIL_UNREAD"), unreadCount);
    }

    void OnClickMailItem(MailVo mv)
    { 
        if(null != OnClickMailItemHandler)
        {
            OnClickMailItemHandler(mv);
        }
    }

}
