//#define test_ui

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class UIDailyBonus : MonoBehaviour
{
    public GameObject dailyBonusPrefab;

    public GameObject gridGO;

    private UIGrid _grid;

    public UILabel labelTitle;

    public UILabel labelVIP;

    public UILabel labelClaim;

    private bool _isBonusReady;

	private TownHUD _uiTownHome;

    private List<UIDailyBonusItem> _itemList = new List<UIDailyBonusItem>();

    private int _activeDays;
    public int ActiveDays
    {
        get
        {
            return _activeDays;
        }
    }

    private int _vipLevel;
    public int VIPLevel { get { return _vipLevel; } }

    private int _vipTime;
    public int VIPTime { get { return _vipTime; } }

    void Awake()
    {
		_uiTownHome = transform.parent.FindChild("TownHome").GetComponent<TownHUD>();

        _grid = gridGO.GetComponent<UIGrid>();
    }

    //called automatically when openUI
    public void OnInitialize()
    {
        PutTownHomeUIAt(100);

        _activeDays = PlayerInfo.Instance.activeDays;

        _vipLevel = PlayerInfo.Instance.vipLevel;

        _vipTime = PlayerInfo.Instance.vipTime;

        _isBonusReady = DailyBonusManager.instance.isBonusReady;

        RefreshDisplay();
    }

    private void PutTownHomeUIAt(float z)
    {
        Vector3 pos = _uiTownHome.transform.localPosition;

        pos.z = z;

        _uiTownHome.transform.localPosition = pos;
    }

    private void RefreshDisplay()
    {
        if (DailyBonusManager.instance.CurDailyBonusDataList.dataList.Count == 0)
        {
            labelTitle.text = "Server error";
            labelClaim.text = Localization.instance.Get("IDS_BUTTON_BACK");
            labelVIP.text = "";
            _isBonusReady = false;
            return;
        }
		
		//GD should fill the list with 31 days
        List<DailyBonusData> list = DailyBonusManager.instance.CurDailyBonusDataList.dataList;
		Assertion.Check(list.Count == 31);		

		if (_itemList.Count == 0)
        {
            for (int i = 0; i < list.Count; i++)
            {
                AddDailyBonusItem();
            }

            _grid.repositionNow = true;
        }

        //update data
        for (int i = 0; i < list.Count; i++)
        {
            _itemList[i].Init(list[i], this);
        }

        labelClaim.text = _isBonusReady ? Localization.instance.Get("IDS_CLAIM") : Localization.instance.Get("IDS_BUTTON_BACK");

//        DateTime dt = NetworkManager.Instance.serverTime;

        labelTitle.text = Localization.instance.Get("IDS_DAILY_BONUS");

        //vip
        if (_vipLevel < 1)
        {
            labelVIP.gameObject.SetActive(true);
            labelVIP.text = Localization.instance.Get("IDS_VIP_1");
        }
        else if (_vipLevel < 4)
        {
            labelVIP.gameObject.SetActive(true);
            labelVIP.text = Localization.instance.Get("IDS_VIP_2");
        }
        else
        {
            labelVIP.gameObject.SetActive(false);
        }
    }

    public void AddDailyBonusItem()
    {
        GameObject go = GameObject.Instantiate(dailyBonusPrefab) as GameObject;

        go.name = "Day " + (_itemList.Count + 1).ToString();

        go.transform.parent = gridGO.transform;

        go.transform.localPosition = Vector3.zero;

        go.transform.localScale = Vector3.one;

        UIDailyBonusItem item = go.GetComponent<UIDailyBonusItem>();

		_itemList.Add(item);
		
		//hard code, although we have up to 31 days, we only show 20 days in UI
		if (_itemList.Count > 20)
			go.SetActive(false);	
    }

    void OnBackgroundClick()
    {
    }

    private void OnClaimButtonClick()
    {  
        if (_isBonusReady)
        {
            DailyBonusManager.instance.ClaimBonus();

            _itemList[_activeDays].tick.gameObject.SetActive(true);

            _isBonusReady = false;

            labelClaim.text = Localization.instance.Get("IDS_BUTTON_BACK");
        }
        else
        {
            this.gameObject.SetActive(false);

            PutTownHomeUIAt(0);
        }
    }
}