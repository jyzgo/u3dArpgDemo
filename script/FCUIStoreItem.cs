using System;
using System.Collections.Generic;
using UnityEngine;
using FaustComm;

public class FCUIStoreItem : MonoBehaviour
{
    public delegate void BuyButtonClickDelegate(FC_StoreData storeData);
    public BuyButtonClickDelegate OnBuyButtonClickHandler;

    public delegate void ExchangeButtonClickDelegate(StoreSCExchangeData storeSCExchangeData);
    public ExchangeButtonClickDelegate OnSCExchangeClickHandler;

    public delegate void HCBuyButtonClickDelegate();
    public HCBuyButtonClickDelegate OnClickBuyHCButton;

    public UIImageButton buyButton;

    public UILabel buyButtonLabel;

    public UILabel nameLabel;

    public UILabel countLabel;

    public UISprite priceTagIcon;

    public UILabel priceTagLabel;

    public UILabel priceTagLabel1;

    public UILabel priceTagLabel2;

    public UISprite priceTagStrikeout;

    public UITexture texture;

    public UISprite flag;

    public UISprite newIcon;

    public UILabel flagLabel1;

    public UILabel flagLabel2;

    public UILabel remainExchangeTimesLabel;

    private FC_StoreData _storeData;
    public FC_StoreData StoreData
    {
        set
        {
            _storeData = value;
            RefreshStoreData();
        }
        get
        {
            return _storeData;
        }
    }

    void Start()
    {
        UIEventListener.Get(buyButton.gameObject).onClick = OnStoreBuyButtonClick;
    }

    void OnStoreBuyButtonClick(GameObject go)
    {
        if (null != OnBuyButtonClickHandler)
        {
            OnBuyButtonClickHandler(_storeData);
        }
        if (null != OnSCExchangeClickHandler)
        {
            OnSCExchangeClickHandler(_storeSCExchangeData);
        }
        if (null != OnClickBuyHCButton)
        {
            OnClickBuyHCButton();
        }
    }

    public void RefreshStoreData()
    {
        nameLabel.text = Localization.Localize(_storeData.displayNameIds);
        countLabel.text = "x " + _storeData.count.ToString();
        priceTagLabel.text = _storeData.price.ToString();
        priceTagIcon.spriteName = _storeData.IsSC ? "13" : "12";
        priceTagIcon.MakePixelPerfect();
        string iconPath = string.Format("Assets/UI/bundle/Store/{0}.png", _storeData.storeIconName);
        texture.mainTexture = InJoy.AssetBundles.AssetBundles.Load(iconPath) as Texture;
        newIcon.gameObject.SetActive(_storeData.IsNew);
        buyButtonLabel.text = Localization.Localize("IDS_BUTTON_GLOBAL_BUY");
        remainExchangeTimesLabel.text = "";

        if (_storeData.IsDiscount)//limit discount
        {
            float discount = 0;
            if (LocalizationContainer.CurSystemLang == "zh-Hans" ||
                LocalizationContainer.CurSystemLang == "zh-Hant")
            {
                discount = _storeData.discount / 10.0f;
            }
            else
            {
                discount = 100 - _storeData.discount;
            }
            flagLabel1.text = string.Format(Localization.Localize("IDS_MESSAGE_STORE_DISCOUNT"),
                discount);
            flagLabel2.text = Localization.Localize("IDS_MESSAGE_STORE_LIMIT") + _storeData.DisplayDiscountExpireTime;

            flagLabel1.gameObject.SetActive(true);
            flagLabel2.gameObject.SetActive(true);
            flag.gameObject.SetActive(true);

            priceTagLabel.gameObject.SetActive(false);
            priceTagLabel1.gameObject.SetActive(true);
            priceTagLabel2.gameObject.SetActive(true);
            priceTagStrikeout.gameObject.SetActive(true);

            priceTagLabel1.text = _storeData.price.ToString();
            priceTagLabel2.text = _storeData.discountPrice.ToString();
        }
        else if (_storeData.IsLimitDisappear)//limit disppear
        {
            flagLabel1.text = Localization.Localize("IDS_MESSAGE_STORE_LIMIT");
            flagLabel2.text = _storeData.DisplayDisappearTime;

            priceTagLabel.gameObject.SetActive(true);
            flagLabel1.gameObject.SetActive(true);
            flagLabel2.gameObject.SetActive(true);
            flag.gameObject.SetActive(true);

            priceTagLabel1.gameObject.SetActive(false);
            priceTagLabel2.gameObject.SetActive(false);
            priceTagStrikeout.gameObject.SetActive(false);
        }
        else
        {
            flagLabel1.gameObject.SetActive(false);
            flagLabel2.gameObject.SetActive(false);
            flag.gameObject.SetActive(false);

            priceTagLabel.gameObject.SetActive(true);
            priceTagLabel1.gameObject.SetActive(false);
            priceTagLabel2.gameObject.SetActive(false);
            priceTagStrikeout.gameObject.SetActive(false);
        }
    }

    private StoreSCExchangeData _storeSCExchangeData;
    public StoreSCExchangeData StoreSCExchangeData
    {
        set
        {
            _storeSCExchangeData = value;
            RefreshStoreSCExchangeData();
        }
        get
        {
            return _storeSCExchangeData;
        }
    }

    public void RefreshStoreSCExchangeData()
    {
        newIcon.gameObject.SetActive(false);
        priceTagLabel.gameObject.SetActive(true);
        priceTagLabel.text = _storeSCExchangeData.CoseHC.ToString();
        priceTagIcon.spriteName = "12";//hc icon
        countLabel.text = "x " + _storeSCExchangeData.SC;
        nameLabel.text = Localization.Localize("IDS_NAME_PART_SC");
        string iconPath = string.Format("Assets/UI/bundle/Store/SC{0}.png", _storeSCExchangeData.Type);
        texture.mainTexture = InJoy.AssetBundles.AssetBundles.Load(iconPath) as Texture;
        buyButtonLabel.text = Localization.Localize("IDS_BUTTON_GLOBAL_EXCHANGE");

        priceTagLabel1.gameObject.SetActive(false);
        priceTagLabel2.gameObject.SetActive(false);
        priceTagStrikeout.gameObject.SetActive(false);

        flag.gameObject.SetActive(_storeSCExchangeData.ExpireTime != 0);
        flagLabel1.gameObject.SetActive(_storeSCExchangeData.ExpireTime != 0);
        flagLabel2.gameObject.SetActive(_storeSCExchangeData.ExpireTime != 0);
        flagLabel1.text = Localization.Localize("IDS_MESSAGE_STORE_LIMIT");
        flagLabel2.text = _storeSCExchangeData.ExpireDateString;

        int remainExchangeTimes = _storeSCExchangeData.CountMax - _storeSCExchangeData.Count;
        if (remainExchangeTimes > 0)
        {
            remainExchangeTimesLabel.text = string.Format(Localization.Localize("IDS_MESSAGE_STORE_EXCHANGETIME"), remainExchangeTimes);
        }
        else
        {
            buyButton.isEnabled = false;
            remainExchangeTimesLabel.text = Localization.Localize("IDS_MESSAGE_STORE_EXCHANGEFULL");
        }
    }

    public void RefreshHCBuy(int index)
    {
        priceTagLabel1.gameObject.SetActive(false);
        priceTagLabel2.gameObject.SetActive(false);
        flag.gameObject.SetActive(false);
        newIcon.gameObject.SetActive(false);
        flagLabel1.gameObject.SetActive(false);
        flagLabel2.gameObject.SetActive(false);
        priceTagStrikeout.gameObject.SetActive(false);
        remainExchangeTimesLabel.gameObject.SetActive(false);
        priceTagIcon.gameObject.SetActive(false);
        priceTagLabel.gameObject.SetActive(true);

        string iconPath = string.Format("Assets/UI/bundle/Store/HC{0}.png", index + 1);
        texture.mainTexture = InJoy.AssetBundles.AssetBundles.Load(iconPath) as Texture;
        countLabel.text = "x " + (index + 1) * 10;
        priceTagLabel.text = "￥" + (index + 1) + "         ";
        nameLabel.text = Localization.Localize("IDS_NAME_PART_HC");
        name = index.ToString("000") + "(hcSort)";
    }
}
