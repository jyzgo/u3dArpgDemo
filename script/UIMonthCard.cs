using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using InJoy.Utils;
using InJoy.FCComm;

public class UIMonthCard : MonoBehaviour {

	public GameObject _claimButton;
	public UILabel _remainingDays;
		
	// Use this for initialization
	void Start () {
		
		
		_claimButton.GetComponent<UIButtonMessage>().target = gameObject;
		_claimButton.GetComponent<UIButtonMessage>().functionName = "OnClaim";
		
	}
	
	void OnInitialize()
	{
		_remainingDays.text = string.Format(Localization.instance.Get("IDS_MONTH_CARD_LEFT_TIME_POPUP"), UIMonthCard._remaining);   	
	}
	

	void OnClaim()
	{
		PreClaimBonus(OnClaimSuccess);	
	}
		
	void OnClaimSuccess()
	{
        PlayerInfo.Instance.haveClaimMonthCardToday = true;
        PlayerInfo.Instance.lastMonthCardTime = TimeUtils.ConvertToUnixTimestamp(TimeUtils.GetPSTDateTime());
		//todo: save
		OnClose();
	}
	
	
	void OnClose()
	{
		UIManager.Instance.CloseUI("UIMonthCard");

		TownHUD townHome = transform.parent.FindChild("TownHome").GetComponent<TownHUD>();
		if(townHome != null)
		{
			//townHome.SetNewBgAndNewNumber();
		}
	}
	

	
	//claim cgs
	public delegate void OnResult();
	public static OnResult _callBack = null; 
	public static bool  _canClaimFlag = true;
	
	public static bool PreClaimBonus(OnResult callBack)
	{
		if(_canClaimFlag)
		{
			_canClaimFlag = false;
			_callBack = callBack;
			ConnectionManager.Instance.RegisterHandler(DoClaimBonus, true);
			return true;
		}
		return false;
	}
	
	public static void DoClaimBonus()
	{
		SendClaimBonus();
	}
	
	public static void SendClaimBonus()
	{
        Utils.CustomGameServerMessage(null, null);
	}
	
	static void OnClaimBonus(FaustComm.NetResponse response)
	{
	}
	
	
	
	public static void CheckMonthCard()
	{	
	    DateTime lastTime = TimeUtils.ConvertFromUnixTimestamp(PlayerInfo.Instance.lastMonthCardTime);
	   
	    DateTime BeginDay = new DateTime(lastTime.Year,lastTime.Month,lastTime.Day);
	    TimeSpan span = TimeUtils.GetPSTDateTime() - BeginDay;
	   	
		if(span.Days >=1)
		{
			PlayerInfo.Instance.haveClaimMonthCardToday = false;
			PreMonthCard();	
		}else{
			//if(!PlayerInfoManager.Singleton.AccountProfile._haveClaimMonthCardToday)
			{
				PreMonthCard();		
			}
		}
	}
	
	
	
	
	
	
	
	//request month card infomation
	public static OnResult _monthCardCallBack = null; 
	public static bool  _canMonthCardFlag = true;
	
	public static  bool _claimMonthCard = false;
	public static  int _remaining = 0;
	public static  bool _isOpen = false;
	//public static List<LootObjData> _bonusList = new List<LootObjData>();
	
	public static bool PreMonthCard()
	{
        return false;
	}
	
	public static void DoMonthCard()
	{
		SendMonthCard();
	}
	
	public static void SendMonthCard()
	{
        Utils.CustomGameServerMessage(null, OnMonthCard);
	}

    static void OnMonthCard(FaustComm.NetResponse response)
    {
    }
	
	
	
	//just for cheat
	public static int _cheatType = 0;
	public static bool PreBuyMonthCard()
	{
		if(_cheatType == 0)
		{
			_claimMonthCard = true;
			_remaining = 30;
			return false;
		}
		
		ConnectionManager.Instance.RegisterHandler(DoBuyMonthCard, true);
		return false;
	}
	
	public static void DoBuyMonthCard()
	{
		SendBuyMonthCard();
	}
	
	public static void SendBuyMonthCard()
	{
        Utils.CustomGameServerMessage(null, OnBuyMonthCard);
	}

    static void OnBuyMonthCard(FaustComm.NetResponse response)
    {
    }
}
