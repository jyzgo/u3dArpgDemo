using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using InJoy.FCComm;
using InJoy.Utils;
using System.Reflection;

public class DailyBonusManager : MonoBehaviour
{
    public string dailyBonusDataPath;

    private static DailyBonusManager _instance;

    public static DailyBonusManager instance { get { return _instance; } }

    public DailyBonusDataList _dailyBonusDataList;
    private DailyBonusDataList _curDailyBonusDataList;

    public DailyBonusDataList CurDailyBonusDataList
    {
        get
        {
            if (_curDailyBonusDataList == null)
            {
                _curDailyBonusDataList = _dailyBonusDataList;
            }
            return this._curDailyBonusDataList;
        }
    }

    private bool _isBonusReady = true;     //true when bonus is ready for claim

    public bool isBonusReady { get { return _isBonusReady; } }



    void Awake()
    {
		if(_instance != null)
		{
			Debug.LogError("DailyBonusManager: detected singleton instance has existed. Destroy this one " + gameObject.name);
			Destroy(this);
			return;
		}		
		
        _instance = this;
    }
	
	void OnDestroy() {
		if(_instance == this)
		{
			_instance = null;
		}
	}	
	
	
    public void Init()
    {
        StartCoroutine(CheckBonusState());
    }

    private IEnumerator CheckBonusState()
    {
        while (true)
        {
            GetServerBonusState();

            Debug.Log("Server time: " + NetworkManager.Instance.serverTime);

            yield return new WaitForSeconds(3600 * 24);      //check every hour
        }
    }

    public void GetServerBonusState()
    {
        Utils.CustomGameServerMessage(null, OnGetServerBonusState);
    }

    private void OnGetServerBonusState(FaustComm.NetResponse response)
    {
    }


    //claim today's bonus, and update active days.
    public void ClaimBonus()
    {
        ConnectionManager.Instance.RegisterHandler(ServerClaimBonus, true);
    }

    private void ServerClaimBonus()
    {
        Utils.CustomGameServerMessage(null, OnClaimBonus);
    }

    private void OnClaimBonus(FaustComm.NetResponse response)
    {
    }
}