using System;
using System.Collections.Generic;
using UnityEngine;


public class FCUIInventoryHeroInfo : MonoBehaviour
{
    public GameObject backButton;

    public UILabel heroLevelValue;

    public UIFightScore heroFSValue;
    public VIPLogo vipLogo;
    public UILabel EXPLabel;
    public UISprite EXPProgressBar;

    public UILabel attributePlayerName;
    public UILabel attributePlayerTitle;
    public UILabel attributeRoleName;
    public GameObject attribute0;
    public GameObject attribute1;
    public GameObject attribute2;
    public GameObject attribute3;
    public GameObject attribute4;
    public GameObject attribute5;
    public GameObject attribute6;
    public GameObject attribute7;

    public UILabel fireRes;
    public UILabel fireDmg;
    public UILabel iceRes;
    public UILabel iceDmg;
    public UILabel lightningRes;
    public UILabel lightningDmg;
    public UILabel poisonRes;
    public UILabel poisonDmg;

    public void OnEnable()
    {
        attribute0.GetComponent<FCPlayerProperty>().RefreshKeyAndValue(PlayerPropKey.Level, PlayerInfo.Instance.CurrentLevel);
        attribute1.GetComponent<FCPlayerProperty>().RefreshKeyAndValue(PlayerPropKey.HP, PlayerInfo.Instance.HP);
        attribute2.GetComponent<FCPlayerProperty>().RefreshKeyAndValue(PlayerPropKey.Critical, PlayerInfo.Instance.Critical);
        attribute3.GetComponent<FCPlayerProperty>().RefreshKeyAndValue(PlayerPropKey.Attack, PlayerInfo.Instance.Attack);
        attribute4.GetComponent<FCPlayerProperty>().RefreshKeyAndValue(PlayerPropKey.CritDamage, PlayerInfo.Instance.CritDamage);
        attribute5.GetComponent<FCPlayerProperty>().RefreshKeyAndValue(PlayerPropKey.Defense, PlayerInfo.Instance.Defense);
        attribute6.GetComponent<FCPlayerProperty>().RefreshKeyAndValue(PlayerPropKey.ReduceEnergy, PlayerInfo.Instance.ReduseEnergy);
        attribute7.GetComponent<FCPlayerProperty>().RefreshKeyAndValue(PlayerPropKey.ItemFind, PlayerInfo.Instance.itemFindPossibility);

        lightningDmg.GetComponent<FCPlayerProperty>().RefreshKeyAndValue(PlayerPropKey.LightningDmg, PlayerInfo.Instance.LightningDmg);
        lightningRes.GetComponent<FCPlayerProperty>().RefreshKeyAndValue(PlayerPropKey.LightningRes, PlayerInfo.Instance.LightningRes);
        fireDmg.GetComponent<FCPlayerProperty>().RefreshKeyAndValue(PlayerPropKey.FireDmg, PlayerInfo.Instance.FireDmg);
        fireRes.GetComponent<FCPlayerProperty>().RefreshKeyAndValue(PlayerPropKey.FireRes, PlayerInfo.Instance.FireRes);
        iceDmg.GetComponent<FCPlayerProperty>().RefreshKeyAndValue(PlayerPropKey.IceDmg, PlayerInfo.Instance.IceDmg);
        iceRes.GetComponent<FCPlayerProperty>().RefreshKeyAndValue(PlayerPropKey.IceRes, PlayerInfo.Instance.IceRes);
        poisonDmg.GetComponent<FCPlayerProperty>().RefreshKeyAndValue(PlayerPropKey.PosisonDmg, PlayerInfo.Instance.PosisonDmg);
        poisonRes.GetComponent<FCPlayerProperty>().RefreshKeyAndValue(PlayerPropKey.PosisonRes, PlayerInfo.Instance.PosisonRes);

        PlayerInfo playerInfo = PlayerInfo.Instance;
        attributeRoleName.text = Localization.Localize("IDS_MESSAGE_GLOBAL_CLASS") + " : "
            + "[BEAA82]" + Localization.Localize(FCConst.ROLE_NAME_KEYS[playerInfo.Role]) + "[-]";
        attributePlayerName.text = playerInfo.Nickname;
        attributePlayerTitle.text = "";

        heroFSValue.FS = playerInfo.FightScore;
        vipLogo.VipLevel = playerInfo.vipLevel;

        PlayerLevelData pld = DataManager.Instance.CurPlayerLevelDataList.GetPlayerLevelDataByLevel(playerInfo.CurrentLevel);
        if (null != pld)
        {
            EXPLabel.text = playerInfo.CurrentXp + "/" + pld._exp;
            decimal percent = (decimal)playerInfo.CurrentXp / pld._exp;
            EXPProgressBar.fillAmount = (float)percent;
        }
        else
        {
            EXPLabel.text = "max";
            EXPProgressBar.fillAmount = 1.0f;
        }
    }

    void Awake()
    {
        UIEventListener.Get(backButton).onClick = OnBackToPreview;
    }

    void Update()
    { 
    }

    void OnBackToPreview(GameObject go)
    {
        FCUIInventory.Instance.OnSwitchInfoToPreview();
    }

    void Start()
    {

    }
}
