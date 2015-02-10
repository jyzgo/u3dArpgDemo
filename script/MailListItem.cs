using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MailListItem : MonoBehaviour
{
    public delegate void ClickDelegate(MailVo mailVo);
    public ClickDelegate OnClicKHandler;

    public Dictionary<MailStatus, string> mailStatusMapping = new Dictionary<MailStatus, string>()
    {
        {MailStatus.unread , "220"},
        {MailStatus.read , "219"}
    };

    public UISprite icon;

    public UILabel titleLabel;

    public UILabel expireTimeLabel;

    public UISprite attachment;

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
    }

    void OnUpdate()
    { 
    }

    public void Refresh()
    {
        titleLabel.text = _mailVo.TitleDisplay;
        MailStatus status = _mailVo.HasRead ? MailStatus.read : MailStatus.unread;
        icon.spriteName = mailStatusMapping[status];
        attachment.gameObject.SetActive(_mailVo.Collectable);
        expireTimeLabel.text = String.Format(Localization.Localize("IDS_MESSAGE_MAIL_LASTDAY"),
            TimeUtils.GetSurplusTimeString(_mailVo.ExpireTime));
    }

    void OnClick()
    {
        if (null != OnClicKHandler)
        {
            OnClicKHandler(_mailVo);
        }
    }
}
