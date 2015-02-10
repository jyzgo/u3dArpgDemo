using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FaustComm;

public class UIFusion : MonoBehaviour
{
    public UIEffectsController effectController1;
    public UIEffectsController effectController2;

    public UIFusionSupplementPanel supplementPanel;

    public UIImageButton closeButton;
    public UIImageButton backButton;
    public UIImageButton startFusionButton;
    public UIImageButton supplementButton;

    public UIFusionEquipment equipment;
    public UIFusionMaterial material1;
    public UIFusionMaterial material2;
    public UIFusionMaterial material3;

    public UILabel beforeFusionLevel;
    public UILabel beforeAttribute1;
    public UILabel beforeAttribute2;
    public UILabel beforeAttribute3;

    public UILabel afterFusionLevel;
    public UILabel afterAttribute1;
    public UILabel afterAttribute2;
    public UILabel afterAttribute3;
    public UILabel afterTitle;

    public UILabel fusionCostSC;
    public UISprite scIcon;

    public UILabel equipmentNameLabel;
    private ItemInventory _equipment;

    private bool _isFusioning = false;

    void Awake()
    {
        UIEventListener.Get(closeButton.gameObject).onClick = OnCloseUIFusion;
        UIEventListener.Get(startFusionButton.gameObject).onClick = OnStartFusion;
        UIEventListener.Get(supplementButton.gameObject).onClick = OnSupplementButton;
        UIEventListener.Get(backButton.gameObject).onClick = OnCloseUIFusion;
        supplementPanel.OnBuyCompleteHandler = Refresh;
    }

    void Start()
    {
        supplementPanel.gameObject.SetActive(false);
        effectController2.OnPlayEffectCompleteHandler = Refresh;
    }

    void OnInitializeWithCaller(ItemInventory item)
    {
        _equipment = item;
        TownHUD.Instance.TempHide();

        equipmentNameLabel.text = item.ItemData.DisplayName;
        equipmentNameLabel.color = FCConst.RareColorMapping[(EnumRareLevel)item.ItemData.rareLevel];
        equipment.Item = item;

		Refresh();
    }

    void Refresh()
    {
        _isFusioning = false;
        RefreshBeforeAndAfterAttribute();
        RefreshMaterialCount();
        RefreshFusionCostSC();
    }

    void RefreshBeforeAndAfterAttribute()
    {
        beforeFusionLevel.text = Localization.Localize("IDS_MESSAGE_ITEMINFO_ITEMFUSIONLEVEL") + " : " + _equipment.CurrentFusionLevel();

        if (null != _equipment.NextFusionData)
        {
            afterFusionLevel.text = Localization.Localize("IDS_MESSAGE_ITEMINFO_ITEMFUSIONLEVEL") + " : " + (int)(_equipment.CurrentFusionLevel() + 1);
            afterTitle.text = Localization.Localize("IDS_MESSAGE_FUSION_AFTER");

            afterAttribute1.text = _equipment.GetFusionAttribute0(false);
            afterAttribute2.text = _equipment.GetFusionAttribute1(false);
            afterAttribute3.text = _equipment.GetFusionAttribute2(false);

            afterAttribute1.gameObject.SetActive(true);
            afterAttribute2.gameObject.SetActive(true);
            afterAttribute3.gameObject.SetActive(true);
            afterFusionLevel.gameObject.SetActive(true);
            supplementButton.gameObject.SetActive(true);
            scIcon.gameObject.SetActive(true);
            fusionCostSC.gameObject.SetActive(true);
            startFusionButton.gameObject.SetActive(true);
        }
        else 
        {
            afterTitle.text = Localization.Localize("IDS_MESSAGE_FUSION_MAXFUSIONLEVEL");
            afterFusionLevel.gameObject.SetActive(false);
            afterAttribute1.gameObject.SetActive(false);
            afterAttribute2.gameObject.SetActive(false);
            afterAttribute3.gameObject.SetActive(false);
            supplementButton.gameObject.SetActive(false);
            scIcon.gameObject.SetActive(false);
            fusionCostSC.gameObject.SetActive(false);
            startFusionButton.gameObject.SetActive(false);
        }
        beforeAttribute1.text = _equipment.GetFusionAttribute0(true);
        beforeAttribute2.text = _equipment.GetFusionAttribute1(true);
        beforeAttribute3.text = _equipment.GetFusionAttribute2(true);
    }

    void RefreshMaterialCount()
    {
        if (null != _equipment.NextFusionData)
        {
            material1.ItemId = _equipment.NextFusionData.material1;
            material2.ItemId = _equipment.NextFusionData.material2;
            material3.ItemId = _equipment.NextFusionData.material3;

            material1.NeedCount = _equipment.NextFusionData.materialCount1;
            material2.NeedCount = _equipment.NextFusionData.materialCount2;
            material3.NeedCount = _equipment.NextFusionData.materialCount3;

            material1.CurrentCount = PlayerInfo.Instance.PlayerInventory.GetItemCount(_equipment.NextFusionData.material1);
            material2.CurrentCount = PlayerInfo.Instance.PlayerInventory.GetItemCount(_equipment.NextFusionData.material2);
            material3.CurrentCount = PlayerInfo.Instance.PlayerInventory.GetItemCount(_equipment.NextFusionData.material3);

            supplementButton.gameObject.SetActive(!CheckNeedMaterialEnough());

            material1.gameObject.SetActive(true);
            material2.gameObject.SetActive(true);
            material3.gameObject.SetActive(true);
        }
        else
        {
            material1.gameObject.SetActive(false);
            material2.gameObject.SetActive(false);
            material3.gameObject.SetActive(false);

            supplementButton.gameObject.SetActive(false);
        }
    }

    #region fusion effect test
    #if UNITY_EDITOR
    void OnGUI()
    {
        if(GUI.Button(new Rect(Screen.width - 200, 0, 100, 30),new GUIContent("effect", "")))
        {
            effectController2.Play();
        }
    }
    #endif
    #endregion

    void OnEnable()
    {
        
    }

    void OnDisable()
    {
        supplementPanel.gameObject.SetActive(false);
    }

    void Update()
    {

    }

    int ShortageOfMat1 
    { 
        get
        {
            return _equipment.NextFusionData.materialCount1 - PlayerInfo.Instance.PlayerInventory.GetItemCount(_equipment.NextFusionData.material1);
        }
    }

    int ShortageOfMat2
    {
        get
        {
            return _equipment.NextFusionData.materialCount2 - PlayerInfo.Instance.PlayerInventory.GetItemCount(_equipment.NextFusionData.material2);
        }
    }

    int ShortageOfMat3
    {
        get 
        {
            return _equipment.NextFusionData.materialCount3 - PlayerInfo.Instance.PlayerInventory.GetItemCount(_equipment.NextFusionData.material3);
        }
    }

    bool CheckNeedMaterialEnough()
    {
        bool isEnough = true;
        if (ShortageOfMat1 > 0 || ShortageOfMat2 > 0 || ShortageOfMat3 > 0)
            isEnough = false;
        return isEnough;
    }

    void RefreshFusionCostSC()
    {
        if (null != _equipment.NextFusionData)
        {
            fusionCostSC.text = _equipment.NextFusionData.cost.ToString();
        }
        else
        {
            fusionCostSC.text = "";
        }
    }

    void OnStartFusion(GameObject go)
    {
        if (!_isFusioning)
        {
            if (!IsSCEnoughToFusion)
            {
                string hint = Localization.Localize("IDS_MESSAGE_GLOBAL_NOTENOUGHSC");
                UIMessageBoxManager.Instance.ShowMessageBox(hint, "", MB_TYPE.MB_OK, null);
                return;
            }
            if (!CheckNeedMaterialEnough())
            {
                string hint = Localization.Localize("IDS_MESSAGE_FUSION_MATERIALNOTENOUGH");
                UIMessageBoxManager.Instance.ShowMessageBox(hint, "", MB_TYPE.MB_OK, null);
                return;
            }
            NetworkManager.Instance.FusionEquipment(_equipment.Item_GUID, OnFusionCallback);
            _isFusioning = true;
            effectController1.Play();
        }
    }

    void OnSupplementButton(GameObject go)
    {
        List<InventoryHCWorth> items = new List<InventoryHCWorth>();
        
        if (ShortageOfMat1 > 0)
        {
            InventoryHCWorth hcBuy = new InventoryHCWorth();
            hcBuy.ItemId = _equipment.NextFusionData.material1;
            hcBuy.Count = ShortageOfMat1;
            items.Add(hcBuy);
        }
        if (ShortageOfMat2 > 0)
        {
            InventoryHCWorth hcBuy = new InventoryHCWorth();
            hcBuy.ItemId = _equipment.NextFusionData.material2;
            hcBuy.Count = ShortageOfMat2;
            items.Add(hcBuy);
        }
        if (ShortageOfMat3 > 0)
        {
            InventoryHCWorth hcBuy = new InventoryHCWorth();
            hcBuy.ItemId = _equipment.NextFusionData.material3;
            hcBuy.Count = ShortageOfMat3;
            items.Add(hcBuy);
        }
        supplementPanel.WantBuyItems = items;
        supplementPanel.gameObject.SetActive(true);
    }

    bool IsSCEnoughToFusion
    {
        get
        {
            return PlayerInfo.Instance.SoftCurrency >= _equipment.NextFusionData.cost;
        }
    }

    void OnFusionCallback(NetResponse response)
    {
        effectController1.Stop();
        if (response.Succeeded)
        {
            effectController2.Play();
            FusionResponse fusionResponse = (FusionResponse)response;
            fusionResponse.updateData.Broadcast();
        }
        else
        {
            _isFusioning = false;
            UIMessageBoxManager.Instance.ShowErrorMessageBox(response.errorCode, "UIFusion");
        }
    }

    void OnCloseUIFusion(GameObject go)
    {
        if (!_isFusioning)
        {
            UIManager.Instance.CloseUI("UIFusion");
            UIManager.Instance.OpenUI("FCUIInventory");    
        }
    }
}
