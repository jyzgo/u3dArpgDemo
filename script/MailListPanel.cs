using System;
using System.Collections.Generic;
using UnityEngine;
using FaustComm;

public class MailListPanel : MonoBehaviour
{
    public UIImageButton closeButton;

    public MailListContainer listContainer;

    public MailDetailContainer detailContainer;

    void Awake()
    {
        UIEventListener.Get(closeButton.gameObject).onClick = OnClickCloseButton;
        detailContainer.OnClickBackButtonHandler = OnDetailBackToList;
        listContainer.OnClickMailItemHandler = OnClickMailItem;
        detailContainer.OnRemoveMailHandler = OnRemoveMail;
    }

    void Start()
    {
    }

    void OnEnable()
    {
        SwitchDetailOrList(false);
        listContainer.RequestMore();
    }

    void OnDisable()
    {
        TownHUD.Instance.UpdateMailIndicators();
    }

    void OnClickMailItem(MailVo mailVo)
    {
        if (!mailVo.IsComplete)
        {
            NetworkManager.Instance.MailDetail(mailVo.MailId, OnDetailHandler);
        }
        detailContainer.MailVo = mailVo;
        SwitchDetailOrList(true);
    }

    void OnDetailHandler(NetResponse response)
    {
        if (response.Succeeded)
        {
            MailDetailResponse mdResponse = (MailDetailResponse)response;
            MailVo mv = listContainer.GetItemByMailId(mdResponse.MailId).MailVo;
            mv.Content = mdResponse.Content;
            mv.Attachments = mdResponse.Attachments;
            mv.ReceiveTime = mdResponse.ReceiveTime;
            mv.HasRead = true;
            mv.IsComplete = true;
            if (detailContainer.MailVo.MailId == mdResponse.MailId)
            {
                detailContainer.MailVo = mv;
            }
        }
        else
        {
            UIMessageBoxManager.Instance.ShowErrorMessageBox(response.errorCode, "Mail");
        }
    }

    void OnRemoveMail(int mailId)
    {
        SwitchDetailOrList(false);
        listContainer.RemoveMail(mailId);
    }

    void OnDetailBackToList()
    {
        SwitchDetailOrList(false);
        listContainer.Refresh();
    }

    void OnClickCloseButton(GameObject go)
    {
        UIManager.Instance.CloseUI("Mail");
    }

    void SwitchDetailOrList(bool isDetail)
    {
        listContainer.gameObject.SetActive(!isDetail);
        detailContainer.gameObject.SetActive(isDetail);
    }
}
