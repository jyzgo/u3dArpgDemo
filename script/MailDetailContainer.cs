using System;
using System.Collections.Generic;
using UnityEngine;
using FaustComm;

public class MailDetailContainer : MonoBehaviour
{
    public delegate void RemoveMailDelegate(int mailId);
    public RemoveMailDelegate OnRemoveMailHandler;

    public delegate void ClickBackButtonDelegate();
    public ClickBackButtonDelegate OnClickBackButtonHandler;

    public UILabel titleLabel;

    public UILabel contentLabel;

    public UILabel receiveTimeLabel;

    public UILabel expireTimeLabel;

    public UIImageButton backButton;

    public UIImageButton removeButton;

    public UIImageButton collectButton;

    public UILabel hasbeenCollected;

    public FCSlotForItemExhibition[] slots;

    private MailVo _mailVo;
    public MailVo MailVo
    {
        set
        {
            _mailVo = value;
            Refresh();
        }
        get { return _mailVo; }
    }

    void Start()
    {
        UIEventListener.Get(backButton.gameObject).onClick = OnBackButtonClicked;
        UIEventListener.Get(removeButton.gameObject).onClick = OnRemoveButtonClicked;
        UIEventListener.Get(collectButton.gameObject).onClick = OnCollectButtonClicked;
    }

    void OnBackButtonClicked(GameObject go)
    {
        if (null != OnClickBackButtonHandler)
        {
            OnClickBackButtonHandler();
        }
    }

    void Refresh()
    {
        titleLabel.text = _mailVo.Title;
        contentLabel.text = _mailVo.Content;
        receiveTimeLabel.text = _mailVo.ReceiveTime.ToLocalTime().ToString();
        expireTimeLabel.text = string.Format(Localization.Localize("IDS_MESSAGE_MAIL_LASTDAY"), TimeUtils.GetSurplusTimeString(_mailVo.ExpireTime));
        int index = 0;
        foreach (FCSlotForItemExhibition slot in slots)
        {
            if (null != _mailVo.Attachments && _mailVo.Attachments.Count > index)
            {
                slot.gameObject.SetActive(true);
                UIEventListener.Get(slot.gameObject).onPress = OnClickViewItemDetail;
                slot.Refresh(_mailVo.Attachments[index].itemId, _mailVo.Attachments[index].amount);
            }
            else
            {
                slot.gameObject.SetActive(false);
            }
            ++index;
        }
        collectButton.gameObject.SetActive(_mailVo.Collectable);
        hasbeenCollected.gameObject.SetActive(_mailVo.IsContainsAttachment && !_mailVo.Collectable);
    }

    void OnClickViewItemDetail(GameObject go, bool press)
    {
        if (press)
        {
            FCSlotForItemExhibition exhibition = go.GetComponent<FCSlotForItemExhibition>();
            UIMessageBoxManager.Instance.ShowMessageBox(null, null, MB_TYPE.MB_FLOATING, null, exhibition.ItemData);
        }
        else
        {
            UIMessageBoxManager.Instance.HideMessageBox(MB_TYPE.MB_FLOATING);
        }
    }


    void OnRemoveButtonClicked(GameObject go)
    {
        if (_mailVo.Collectable)
        {
            string content = Localization.Localize("IDS_MESSAGE_MAIL_CANNOTDELETE");
            UIMessageBoxManager.Instance.ShowMessageBox(content, "", MB_TYPE.MB_OK, null);
        }
        else
        {
            NetworkManager.Instance.MailOperate(_mailVo.MailId, MailOperation.RemoveOne, OnOperateOneMail);
        }
    }

    void OnOperateOneMail(NetResponse response)
    {
        if (response.Succeeded)
        {
            MailOperationResponse moResponse = (MailOperationResponse)response;
            moResponse.UpdateData.Broadcast();
            if (moResponse.Operation == MailOperation.RemoveOne)
            {
                if (null != OnRemoveMailHandler)
                {
                    OnRemoveMailHandler(moResponse.MailId);
                }
            }
            else if (moResponse.Operation == MailOperation.CollectOne)
            {
                _mailVo.Collectable = false;
                Refresh();
            }
        }
        else
        {
            UIMessageBoxManager.Instance.ShowErrorMessageBox(response.errorCode, "Mail");
        }
    }

    void OnCollectButtonClicked(GameObject go)
    {
        NetworkManager.Instance.MailOperate(_mailVo.MailId, MailOperation.CollectOne, OnOperateOneMail);
    }
}
