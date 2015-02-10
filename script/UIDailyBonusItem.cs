using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class UIDailyBonusItem : MonoBehaviour
{
    public GameObject X2;
    public GameObject X3;

    public UITexture icon;
    public UISprite iconBG1;
    public UISprite iconBG2;
    public GameObject blinkEffect;
    public UILabel dayLabel;
    public UILabel amountLabel;

    public GameObject tick;

    private DailyBonusData _dbData;

    private UIDailyBonus _uiDailyBonus;

    public void Init(DailyBonusData data, UIDailyBonus uiDailyBonus)
    {
        _dbData = data;

        _uiDailyBonus = uiDailyBonus;

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        DailyBonusData bonusData = _dbData;

        dayLabel.text = string.Format(Localization.instance.Get("IDS_DAILY_BONUS_DAY"), bonusData.day);

        amountLabel.text = "x" + _dbData.amount.ToString();

        if (bonusData.day % 5 == 0)
        {
            iconBG1.gameObject.SetActive(false);
            iconBG2.gameObject.SetActive(true);
        }
        else
        {
            iconBG1.gameObject.SetActive(true);
            iconBG2.gameObject.SetActive(false);
        }

        ItemData itemData = DataManager.Instance.GetItemData(bonusData.reward_item_id);

        Texture2D tex = InJoy.AssetBundles.AssetBundles.Load(itemData.iconPath) as Texture2D;

        icon.mainTexture = tex;

        tick.SetActive(_uiDailyBonus.ActiveDays >= _dbData.day);

        blinkEffect.SetActive(_uiDailyBonus.ActiveDays + 1 == _dbData.day);

        //x2, x3
        if (_uiDailyBonus.VIPLevel >= 4)    //vip level 4: 3x
        {
            X3.SetActive(true);
            X2.SetActive(false);
        }
        else if (_uiDailyBonus.VIPLevel >= 1)
        {
            X2.SetActive(true);
            X3.SetActive(false);
        }
        else
        {
            X2.SetActive(false);
            X3.SetActive(false);
        }
    }
}
