using System;
using UnityEngine;

class FCTopBar : MonoBehaviour
{
	public UILabel hardCurrency;
	public UIImageButton hardCurrencyBuy;

	public UILabel softCurrency;
	public UIImageButton softCurrencyBuy;

	public UILabel labelVitality;
	public UIImageButton vitalityBuy;

	public UILabel labelVitCountdown;	//vit countdown

	private DateTime _vitStartTime = TimeUtils.k_epoch_time;

	private int _vit, _vitMax, _lastSeconds;

	private int _vitRecoveryCD, _vitBuyAmount;

	private static FCTopBar _instance;
	public static FCTopBar Instance
	{
		get { return _instance; }
	}

	void Awake()
	{
		_instance = this;

		UIEventListener.Get(hardCurrencyBuy.gameObject).onClick = OnClickBuyHardCurrency;
		UIEventListener.Get(softCurrencyBuy.gameObject).onClick = OnClickBuySoftCurrency;
		UIEventListener.Get(vitalityBuy.gameObject).onClick = OnClickBuyEnergy;

		_vitRecoveryCD = (int)DataManager.Instance.CurGlobalConfig.getConfig("vitRecoveryCD");
		_vitBuyAmount = (int)DataManager.Instance.CurGlobalConfig.getConfig("vitBuyAmount");
	}

	void Start()
	{
		OnHCChanged(PlayerInfo.Instance.HardCurrency);
		OnSCChanged(PlayerInfo.Instance.SoftCurrency);
		OnVitChanged();

		PlayerInfo.Instance.onHardCurrencyChanged += this.OnHCChanged;
		PlayerInfo.Instance.onSoftCurrencyChanged += this.OnSCChanged;
		PlayerInfo.Instance.onVitalityChanged += this.OnVitChanged;

		//initialize the last calc time
        _vitStartTime = _vitStartTime.AddSeconds(PlayerInfo.Instance.VitalityTime);

        _vitMax = PlayerInfo.Instance.VitalityMax;
	}

	void OnHCChanged(int hc)
	{
		hardCurrency.text = hc.ToString();
	}

	void OnSCChanged(int sc)
	{
		softCurrency.text = sc.ToString();
	}

	void OnVitChanged()
	{
		_vit = PlayerInfo.Instance.Vitality;
		
		labelVitality.text = _vit.ToString() + "/" + PlayerInfo.Instance.VitalityMax.ToString();

		labelVitCountdown.gameObject.SetActive(_vit < _vitMax);
	}

	void Update()
	{
		UpdateVitDisplay();
	}

	void UpdateVitDisplay()
	{
		if (_vit < _vitMax)
		{
			TimeSpan span = _vitStartTime.AddSeconds(_vitRecoveryCD) - NetworkManager.Instance.serverTime;

			int seconds = (int)span.TotalSeconds;

			if (seconds < 0)
			{
				seconds = -seconds;

				int var = seconds / _vitRecoveryCD + 1;

				_vit += var;

				if (_vit < PlayerInfo.Instance.VitalityMax)
				{
					PlayerInfo.Instance.Vitality = _vit;

					_vitStartTime = _vitStartTime.AddSeconds(var * _vitRecoveryCD);
					_lastSeconds = seconds % _vitRecoveryCD;
					span = new TimeSpan(0, 0, _lastSeconds);
					labelVitCountdown.text = TimeUtils.GetStandardTimeString(span);
				}
				else
				{
					PlayerInfo.Instance.Vitality = PlayerInfo.Instance.VitalityMax;

					labelVitCountdown.gameObject.SetActive(false);
				}
			}
			else if (seconds == 0)
			{
				_vit++;

				PlayerInfo.Instance.Vitality = _vit;

				_vitStartTime = NetworkManager.Instance.serverTime;		//simulate the actual time because we do not get this value now

				if (_vit >= _vitMax)
				{
					labelVitCountdown.gameObject.SetActive(false);
				}
			}
			else if (seconds != _lastSeconds) //change of seconds
			{
				if (!labelVitCountdown.gameObject.activeInHierarchy)
				{
					labelVitCountdown.gameObject.SetActive(true);
				}

				labelVitCountdown.text = TimeUtils.GetStandardTimeString(span);

				_lastSeconds = seconds;
			}
		}
	}

	void OnClickBuyHardCurrency(GameObject go)
	{
		UIManager.Instance.CloseAllOpenUI();

        UIManager.Instance.OpenUI("Store", 1);
	}

	void OnClickBuySoftCurrency(GameObject go)
	{
        UIManager.Instance.CloseAllOpenUI();

		UIManager.Instance.OpenUI("Store", 2);
	}

	void OnClickBuyEnergy(GameObject go)
	{
		BuyVitality();
	}

	public void BuyVitality()
	{
		int vipLevel = PlayerInfo.Instance.vipLevel;

		int maxBuyCount = DataManager.Instance.vipPrivilegeList.GetVipPrivilege(vipLevel).maxVitExchangeCount;

		int buyCount = PlayerInfo.Instance.VitalityBuyCount;	//auto refresh in Get method

		////test: fake data
		//vipLevel = 0;
		//buyCount = 1;
		//maxBuyCount = 1;
		////end test

		if (buyCount >= maxBuyCount)
		{
			if (vipLevel == 0)
			{
				//ask player to become vip
				UIMessageBoxManager.Instance.ShowMessageBox(Localization.instance.Get("IDS_MESSAGE_STORE_BUYVITALITY2"), null,
                    "IDS_BUTTON_GLOBAL_CANCEL", "IDS_MESSAGE_VIP_BECOMEVIP",
					MB_TYPE.MB_OKCANCEL, OnAskBecomeVip);
			}
			else
			{
				int nextVipLevel = DataManager.Instance.vipPrivilegeList.GetNextVipLevelForVit(vipLevel);

				if (vipLevel < nextVipLevel)
				{
					//ask player to upgrade vip level
					UIMessageBoxManager.Instance.ShowMessageBox(string.Format(Localization.instance.Get("IDS_MESSAGE_STORE_BUYVITALITY1"), buyCount, nextVipLevel), null,
						"IDS_MESSAGE_VIP_VIPLEVELUP", "IDS_BUTTON_GLOBAL_CANCEL",
						MB_TYPE.MB_OKCANCEL, OnAskUpgradeVipLevel);
				}
				else
				{
					//tell player that no more buying today
					UIMessageBoxManager.Instance.ShowMessageBox(Localization.instance.Get("IDS_MESSAGE_STORE_BUYVITALITY3"), null, MB_TYPE.MB_OK, null);
				}
			}
		}
		else
		{
			string text = Localization.instance.Get("IDS_MESSAGE_STORE_BUYVITALITY");

			int price = DataManager.Instance.vitPriceList.GetVitPrice(buyCount + 1).hcCost;

			text = string.Format(text, price, _vitBuyAmount, buyCount + 1);

			UIMessageBoxManager.Instance.ShowMessageBox(text, null, MB_TYPE.MB_OKCANCEL, OnAskBuyVitalityCallback);
		}
	}

	void OnAskBuyVitalityCallback(ID_BUTTON buttonID)
	{
		if (buttonID == ID_BUTTON.ID_OK)
		{
			NetworkManager.Instance.SendCommand(new BuyVitalityRequest(), OnBuyVitCallback);
		}
	}

	private void OnAskBecomeVip(ID_BUTTON buttonID)
	{
		if (buttonID == ID_BUTTON.ID_OK)
		{
            UIManager.Instance.CloseAllOpenUI();
			UIManager.Instance.OpenUI("Store", 1);
		}
	}

	void OnAskUpgradeVipLevel(ID_BUTTON buttonID)
	{
		if (buttonID == ID_BUTTON.ID_OK)
		{
			//go to upgrade vip level
			UIManager.Instance.OpenUI("Store", 1);
		}
	}

	void OnBuyVitCallback(FaustComm.NetResponse msg)
	{
		if (msg.Succeeded)
		{
			(msg as BuyVitalityResponse).updateData.Broadcast();
			UIMessageBoxManager.Instance.ShowMessageBox(Localization.instance.Get("IDS_MESSAGE_INVENROEY_INCREASESPACESUCCESSFUL"), null, MB_TYPE.MB_OK, null);
		}
		else
		{
			UIMessageBoxManager.Instance.ShowMessageBox(Utils.GetErrorIDS(msg.errorCode), null, MB_TYPE.MB_OK, null);
		}
	}

	void OnDestroy()
	{
		PlayerInfo.Instance.onHardCurrencyChanged -= this.OnHCChanged;
		PlayerInfo.Instance.onSoftCurrencyChanged -= this.OnSCChanged;
		PlayerInfo.Instance.onVitalityChanged -= this.OnVitChanged;

		_instance = null;
	}
}
