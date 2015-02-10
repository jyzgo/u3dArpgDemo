using System;
using UnityEngine;

public class FCUIPortrait : MonoBehaviour
{
    public UIFightScore fightScore;

    public UILabel nickNameLabel;

    public UILabel levelLabel;

    public VIPLogo vip;

    public GameObject daily;

    public GameObject activity;

    public GameObject mail;

    void Awake()
    {
        UIEventListener.Get(gameObject).onClick = OnPopupPlayerAttributeUI;
        UIEventListener.Get(daily).onClick = OnDailyClicked;
        UIEventListener.Get(activity).onClick = OnActivityClicked;
        UIEventListener.Get(mail).onClick = OnMailClicked;
    }

    void Start()
    {
        PlayerInfo.Instance.OnLevelUp = OnRefreshLevel;
        OnRefreshLevel();
        OnRefreshNickName();
        OnRefreshFS();
        OnRefreshVip();
    }

    void Update()
    { 

    }

    void OnRefreshLevel(int level = 0)
    {
        levelLabel.text = PlayerInfo.Instance.CurrentLevel.ToString();
    }

    void OnRefreshNickName()
    {
        nickNameLabel.text = PlayerInfo.Instance.Nickname;
    }

    void OnRefreshVip()
    {
        vip.VipLevel = PlayerInfo.Instance.vipLevel;
    }

    void OnRefreshFS()
    {
        fightScore.FS = PlayerInfo.Instance.FightScore;
    }

    void OnMailClicked(GameObject go)
    {
        UIManager.Instance.OpenUI("Mail");
    }

    void OnPopupPlayerAttributeUI(GameObject go = null)
    {
        UIManager.Instance.OpenUI("FCUIInventory", InventoryDisplayLayoutPolicy.LeftHeroInfoRightItemsList);
    }

    void OnDailyClicked(GameObject go)
    {
        
    }

    void OnActivityClicked(GameObject go)
    {
        Debug.Log("Activity click");
    }
}
