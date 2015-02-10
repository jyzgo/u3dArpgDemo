using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class FCUIInventoryHeroPreview : MonoBehaviour
{
    public UILabel roleNameLabel;
    public UILabel levelLabel;
    public UIFightScore fightScore;
    public VIPLogo vipLogo;

    #region equipments on body
    public GameObject slotWeapon;
    public GameObject slotHelm;
    public GameObject slotShoulder;
    public GameObject slotArmpiece;
    public GameObject slotChest;
    public GameObject slotBelt;
    public GameObject slotLeggings;
    public GameObject slotRing;
    public GameObject slotNecklace;
    public GameObject slotVanity;
    #endregion

    #region 3d preview
    public GameObject plyaerPreviewCamera;
    public GameObject playerPreviewRoot;
    public GameObject playerPreviewContainer;
    #endregion

    public GameObject switchButton;

    private Transform _equipmentRoot;
    private UIPlayerPreview _playerPreview;
    private List<EquipmentIdx> _currentEquipmentsIds = new List<EquipmentIdx>();
    private FCUIInventorySlot _lastSelectSlot;
    private bool _needToUpdateTown;

    public void RefreshPreview(bool needToUpdateTown)
    {   
        PlayerInfo.Instance.GetSelfEquipmentIds(_currentEquipmentsIds);
        UpdatePreviewModel(_currentEquipmentsIds);
        if (needToUpdateTown)
            _needToUpdateTown = needToUpdateTown;
    }

    public void RefreshEquipedItems()
    {
        PlayerInventory pi = PlayerInfo.Instance.EquippedInventory;
        slotWeapon.GetComponent<FCUIInventorySlot>().Item = pi.GetItem(ItemType.weapon, ItemSubType.weapon);
        slotHelm.GetComponent<FCUIInventorySlot>().Item = pi.GetItem(ItemType.armor, ItemSubType.helmet);
        slotShoulder.GetComponent<FCUIInventorySlot>().Item = pi.GetItem(ItemType.armor, ItemSubType.shoulder);
        slotArmpiece.GetComponent<FCUIInventorySlot>().Item = pi.GetItem(ItemType.armor, ItemSubType.armpiece);
        slotChest.GetComponent<FCUIInventorySlot>().Item = pi.GetItem(ItemType.armor, ItemSubType.chest);
        slotBelt.GetComponent<FCUIInventorySlot>().Item = pi.GetItem(ItemType.armor, ItemSubType.belt);
        slotLeggings.GetComponent<FCUIInventorySlot>().Item = pi.GetItem(ItemType.armor, ItemSubType.leggings);
        slotRing.GetComponent<FCUIInventorySlot>().Item = pi.GetItem(ItemType.ornament, ItemSubType.ring);
        slotNecklace.GetComponent<FCUIInventorySlot>().Item = pi.GetItem(ItemType.ornament, ItemSubType.necklace);
        slotVanity.GetComponent<FCUIInventorySlot>().Item = pi.GetItem(ItemType.vanity, ItemSubType.vanity);
    }

    void Awake()
    {
        UIEventListener.Get(switchButton).onClick = OnClickSwitchButton;
        _playerPreview = GetComponent<UIPlayerPreview>();
        _playerPreview._root = playerPreviewContainer;
        _playerPreview.InitializePreview(1);
        _equipmentRoot = _playerPreview.PreviewModal.transform.Find("EquipmentRoot");
    }

    void Start()
    {
        UIEventListener.Get(slotWeapon).onClick = OnClickEquipmentOnBody;
        UIEventListener.Get(slotHelm).onClick = OnClickEquipmentOnBody;
        UIEventListener.Get(slotShoulder).onClick = OnClickEquipmentOnBody;
        UIEventListener.Get(slotArmpiece).onClick = OnClickEquipmentOnBody;
        UIEventListener.Get(slotChest).onClick = OnClickEquipmentOnBody;
        UIEventListener.Get(slotBelt).onClick = OnClickEquipmentOnBody;
        UIEventListener.Get(slotLeggings).onClick = OnClickEquipmentOnBody;
        UIEventListener.Get(slotRing).onClick = OnClickEquipmentOnBody;
        UIEventListener.Get(slotNecklace).onClick = OnClickEquipmentOnBody;
        UIEventListener.Get(slotVanity).onClick = OnClickEquipmentOnBody;


        RefreshEquipedItems();
        RefreshPreview(false);
        RefreshLabels();

        RepositionPreview();
    }

    void RepositionPreview()
    {
        float basePointX = -0.19f;
        float factor = 2000;
        UIRoot root = NGUITools.FindInParents<UIRoot>(gameObject);
        float designWidth = Screen.width * root.GetPixelSizeAdjustment(Screen.height);
        float offseX = (designWidth - 854) / factor;
        plyaerPreviewCamera.transform.localPosition = new Vector3(basePointX + offseX,
            plyaerPreviewCamera.transform.localPosition.y, plyaerPreviewCamera.transform.localPosition.z);
    }

    void Update()
    {
        
    }

    void OnClickSwitchButton(GameObject go)
    {
        FCUIInventory.Instance.OnSwitchPreviewToInfo();
    }

    void OnClickEquipmentOnBody(GameObject go)
    {
        FCUIInventorySlot slot = go.GetComponent<FCUIInventorySlot>();
        ItemInventory item = slot.Item;
        if (null != item)
        {
            if (FCUIInventory.Instance.CurrentSelectionItem != item)
            {
                FCUIInventory.Instance.CurrentSelectionItem = item;
                FCUIInventory.Instance.OnSwithToItemInfo();
                if (null != _lastSelectSlot) _lastSelectSlot.GodIgnoreMe();
                slot.GodSelectedMe();
            }
            else
            {
                FCUIInventory.Instance.CurrentSelectionItem = null;
                FCUIInventory.Instance.OnResumeDefaultLayout();
                if (null != _lastSelectSlot) _lastSelectSlot.GodIgnoreMe();
            }
        }
        else
        {
            FCUIInventory.Instance.CurrentSelectionItem = null;
            FCUIInventory.Instance.OnResumeDefaultLayout();
            if (null != _lastSelectSlot) _lastSelectSlot.GodIgnoreMe();
        }
        _lastSelectSlot = slot;
    }

    public void ClearSelection()
    {
        if (null != _lastSelectSlot) _lastSelectSlot.GodIgnoreMe();
        _lastSelectSlot = null;
    }

    void OnEnable()
    {
        playerPreviewContainer.SetActive(true);
        playerPreviewRoot.SetActive(true);
        RefreshEquipedItems();
        RefreshLabels();
    }

    public void RefreshLabels()
    {
        levelLabel.text = "Lv." + PlayerInfo.Instance.CurrentLevel.ToString();
        roleNameLabel.text = Localization.Localize(FCConst.ROLE_NAME_KEYS[PlayerInfo.Instance.Role]);
        fightScore.FS = PlayerInfo.Instance.FightScore;
        vipLogo.VipLevel = PlayerInfo.Instance.vipLevel;
    }

    void OnDisable()
    {
        if(null != playerPreviewRoot)
            playerPreviewRoot.SetActive(false);
        if(null != playerPreviewContainer)
            playerPreviewContainer.SetActive(false);
    }

    void UpdatePreviewModel(List<EquipmentIdx> ids)
    {
        Utils.ClearChildrenGameobjects(_equipmentRoot);
        FillRootTransform(_equipmentRoot, ids);
        _playerPreview.PreviewModal.GetComponent<AvatarController>().RefreshEquipments(_equipmentRoot);

        Renderer[] renderers = _playerPreview.PreviewModal.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            if (r.gameObject.layer != LayerMask.NameToLayer("TransparentFX"))
            {
                r.gameObject.layer = LayerMask.NameToLayer("3DUI");
            }
        }
    }

    public void UpdateTownModel()
    {
        if (_needToUpdateTown)
        {
            Transform tran = TownPlayerManager.Singleton.HeroInfo._avatarController.transform.Find("agent_playerForTown(Clone)/EquipmentsAgent");
            Utils.ClearChildrenGameobjects(tran);
            FillRootTransform(tran, _currentEquipmentsIds);
            TownPlayerManager.Singleton.HeroInfo._avatarController.RefreshEquipments(tran);
            _needToUpdateTown = false;
        }
    }

    void FillRootTransform(Transform tran, List<EquipmentIdx> ids)
    {
        List<GameObject> EquipInstances = new List<GameObject>();
        PlayerInfo.GetOtherEquipmentInstanceWithIds(EquipInstances, ids);
        foreach (GameObject go in EquipInstances)
        {
            go.transform.parent = tran;
        }
    }
}
