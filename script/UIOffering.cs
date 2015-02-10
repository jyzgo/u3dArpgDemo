using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FaustComm;


public class UIOffering : MonoBehaviour
{
    public UITexture backgroundTexture;
    public UITabButtonsGroup tabButtons;
    public OfferingBonusList bonusList;
    public OfferingTributesList tributesList;

    public UIImageButton closeButton;
    public UIImageButton offeringButton;

    public UILabel offeringItemTips;
    public UITexture selectOfferingItem;

    public OfferingCountLabel countLabel1;
    public OfferingCountLabel countLabel2;
    public OfferingCountLabel countLabel3;

    public OfferingShow screenEffect;
    public UIEffectsController effectController;

    public GameObject effectTop;
    public ParticleSystem effectLoop;

    public UISprite hcIcon;
    public UILabel hcLabel;
    public UILabel hcOfferingLabel;

    public const string k_juniorItemId = "tribute_junior_01";
    public const string k_middleItemId = "tribute_middle_01";
    public const string k_seniorItemId = "tribute_senior_01";

    private OfferingData _juniorOfferingData;
    private OfferingData _middleOfferingData;
    private OfferingData _seniorOfferingData;

    private OfferingGroup _juniorGroup;
    private OfferingGroup _middleGroup;
    private OfferingGroup _seniorGroup;

	private OfferingData _currentOfferingData;
    private OfferingItemSlot _currentSelect;
    private OfferingLevel _currentLevel;

    private OfferingResponse _itemResonse;
    private bool _isOffering;

    void Awake()
    {
        UIEventListener.Get(closeButton.gameObject).onClick = OnClickClose;
        UIEventListener.Get(offeringButton.gameObject).onClick = OnClickOffering;
        backgroundTexture.mainTexture = InJoy.AssetBundles.AssetBundles.Load("Assets/UI/bundle/offering/OfferingBackground.png") as Texture;
        tabButtons.OnSelection = OnTabButtonSelection;
        tributesList.OnTributeClickHandler = OnClickOfferingItem;

        int playerLevel = PlayerInfo.Instance.CurrentLevel;
        _juniorOfferingData = DataManager.Instance.GetOfferingData(playerLevel, OfferingLevel.Junior);
        _juniorGroup = DataManager.Instance.GetOfferingGroup(_juniorOfferingData.displayGroup);

        _middleOfferingData = DataManager.Instance.GetOfferingData(playerLevel, OfferingLevel.Middle);
        _middleGroup = DataManager.Instance.GetOfferingGroup(_middleOfferingData.displayGroup);

        _seniorOfferingData = DataManager.Instance.GetOfferingData(playerLevel, OfferingLevel.Senior);
        _seniorGroup = DataManager.Instance.GetOfferingGroup(_seniorOfferingData.displayGroup);

        _currentLevel = OfferingLevel.Junior;
        bonusList.SetOfferingGroup(_juniorGroup, true);
    }

    void OnInitialize()
    {
        OnInitializeWithCaller(null);
    }

    void OnInitializeWithCaller(ItemInventory item)
    {
        int index = 0;
        if (null != item)
        {
            index = GetIndexOfItemData(item.ItemData);
        }
        offeringButton.gameObject.SetActive(true);
        tabButtons.gameObject.SetActive(true);
        tabButtons.ChangeSelection(index);
        if (null != item)
        {
            selectOfferingItem.enabled = true;
            selectOfferingItem.mainTexture = InJoy.AssetBundles.AssetBundles.Load(item.ItemData.iconPath) as Texture2D;
        }
        RefreshTributesCountLabel();
        TownHUD.Instance.TempHide();
    }

    int GetIndexOfItemData(ItemData itemData)
    {
        int index = -1;
        if (_juniorOfferingData.costItemList.IndexOf(itemData.id) != -1)
        {
            index = 0;
        }
        else if (_middleOfferingData.costItemList.IndexOf(itemData.id) != -1)
        {
            index = 1;
        }
        else if (_seniorOfferingData.costItemList.IndexOf(itemData.id) != -1)
        {
            index = 2;
        }
        return index;
    }

    void OnDisable()
    {
        offeringButton.gameObject.SetActive(false);
        tabButtons.gameObject.SetActive(false);
        TownHUD.Instance.ResumeShow();
    }

    void RefreshTributesCountLabel()
    {
        countLabel1.Count = tributesList.GetTributesCount(_juniorOfferingData);
        countLabel2.Count = tributesList.GetTributesCount(_middleOfferingData);
        countLabel3.Count = tributesList.GetTributesCount(_seniorOfferingData);
    }

    void SetOfferingItem(OfferingData offeringData, bool useTweenEffect)
    {
        selectOfferingItem.enabled = false;

        _currentOfferingData = offeringData;
        _currentSelect = null;
        tributesList.Refresh(offeringData, useTweenEffect);
        offeringItemTips.text = string.Format(Localization.instance.Get("IDS_MESSAGE_OFFERING_HITMONEY"), offeringData.hitMoney);
        RefreshHCCost();
    }

    void RefreshHCCost()
    {
        if (tributesList.GetTributesCount(_currentOfferingData) == 0)
        {
            hcIcon.gameObject.SetActive(true);
            hcLabel.gameObject.SetActive(true);
            hcLabel.text = _currentOfferingData.costHC.ToString();
            hcOfferingLabel.gameObject.SetActive(true);
            offeringButton.label.gameObject.SetActive(false);
        }
        else
        {
            offeringButton.label.gameObject.SetActive(true);
            hcOfferingLabel.gameObject.SetActive(false);
            hcIcon.gameObject.SetActive(false);
            hcLabel.gameObject.SetActive(false);
        }
    }

    void OnTabButtonSelection(int index)
    {
        switch (index)
        { 
            case 0:
                _currentLevel = OfferingLevel.Junior;
                SetOfferingItem(_juniorOfferingData, true);
                bonusList.SetOfferingGroup(_juniorGroup);
                break;
            case 1:
                _currentLevel = OfferingLevel.Middle;
                SetOfferingItem(_middleOfferingData, true);
                bonusList.SetOfferingGroup(_middleGroup);
                break;
            case 2:
                _currentLevel = OfferingLevel.Senior;
                SetOfferingItem(_seniorOfferingData, true);
                bonusList.SetOfferingGroup(_seniorGroup);
                break;
        }
    }

    void OnClickClose(GameObject go = null)
    {
        if (!_isOffering)
        {
            UIManager.Instance.CloseUI("FCUIOffering");
        }
    }

    void OnClickOffering(GameObject go = null)
    {
        if (_isOffering)
        {
            return;
        }
        _isOffering = true;
        string itemId = null;
        if (null == _currentSelect)
        {
            if (tributesList.GetTributesCount() > 0)
            {
                //random to select
                byte type = (byte)_currentLevel;
                itemId = tributesList.GetRandomItemId();
                byte useHc = 0;
                ItemData itemData = DataManager.Instance.GetItemData(itemId);
                selectOfferingItem.enabled = true;
                selectOfferingItem.mainTexture = InJoy.AssetBundles.AssetBundles.Load(itemData.iconPath) as Texture2D;
                NetworkManager.Instance.OnSendOfferingRequset(type, itemId, useHc, OnOffering);
            }
            else
            {
                switch(_currentLevel)
                {
                    case OfferingLevel.Junior:
                        itemId = k_juniorItemId;
                        break;
                    case OfferingLevel.Middle:
                        itemId = k_middleItemId;
                        break;
                    case OfferingLevel.Senior:
                        itemId = k_seniorItemId;
                        break;
                }
                ItemData itemData = DataManager.Instance.GetItemData(itemId);
                selectOfferingItem.enabled = true;
                selectOfferingItem.mainTexture = InJoy.AssetBundles.AssetBundles.Load(itemData.iconPath) as Texture2D;
                byte useHc = 1;
                byte type = (byte)_currentLevel;
                NetworkManager.Instance.OnSendOfferingRequset(type, "", useHc, OnOffering);
            }
        }
        else
        {
            byte type = (byte)_currentLevel;
            itemId = _currentSelect.itemId;
            byte useHc = 0;
            NetworkManager.Instance.OnSendOfferingRequset(type, itemId, useHc, OnOffering);
        }
    }

    void OnOffering(NetResponse response)
    {
        selectOfferingItem.enabled = false;
        if (response.Succeeded)
        {
            effectTop.gameObject.SetActive(true);
            effectLoop.gameObject.SetActive(false);
            _itemResonse = (OfferingResponse)response;
            OfferingShow(_itemResonse.itemId, _itemResonse.SCChangeValue());
        }
        else
        {
            _isOffering = false;
            UIMessageBoxManager.Instance.ShowErrorMessageBox(response.errorCode, "FCUIOffering");
        }
    }

    void OnClickOfferingItem(OfferingItemSlot slot)
    {
        if (null != slot && slot.isCanUse)
        {
            _currentSelect = slot;

            selectOfferingItem.enabled = true;
            selectOfferingItem.mainTexture = InJoy.AssetBundles.AssetBundles.Load(slot.itemIcon) as Texture2D;
        }
    }

    void OfferingShow(string itemId, int scGet)
    {
        ItemData itemData = DataManager.Instance.ItemDataManager.GetItemData(itemId);
        screenEffect.Refresh(itemData, scGet);
        effectController.OnPlayEffectCompleteHandler = OnPlayEffectComplete;
        effectController.Play();
    }

    public void OnPlayEffectComplete()
    {
        screenEffect.Play();
        screenEffect.OnComplete = OnPlayItemShowComplete;
        effectTop.gameObject.SetActive(false);
        effectLoop.gameObject.SetActive(true);
    }

    void OnPlayItemShowComplete()
    {
        screenEffect.gameObject.SetActive(false);
        _itemResonse.updateData.Broadcast();
        RefreshTributesCountLabel();
        SetOfferingItem(_currentOfferingData, false);
        _itemResonse = null;
        _isOffering = false;
    }
}


struct OfferingRequestInfo
{
    public byte level;
    public string itemId;
    public byte useHC;
}