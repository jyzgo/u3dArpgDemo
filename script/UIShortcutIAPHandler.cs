using UnityEngine;
using System.Collections;

public class UIShortcutIAPHandler : MonoBehaviour 
{
	public delegate void OnIAPFinish();
	public OnIAPFinish OnBuyFinishCallback;
	
	public UILabel _HC1_desc;
	public UILabel _HC6_desc;
	
	public UILabel _HC1_buy;
	public UILabel _HC6_buy;
	
	public UILabel _HC1_bonus;
	public UILabel _HC6_bonus;
	
	public BoxCollider _uiBox;
	
	
	// Use this for initialization
	void Start () 
	{
		string ids;
		StoreData sd;
		
		// HC 1
		sd = StoreDataManager.Instance.GetStoreData("hc_2");
		ids = Localization.instance.Get("IDS_BUTTON_BUY");
		ids = Localization.instance.Get("IDS_FILTER_GEMS");
		_HC1_desc.text = sd._iapInfo._hcCount + " " + ids;
		
		if (sd._iapInfo._bonusCount > 0)
		{
			ids = Localization.instance.Get("IDS_BUY_BONUS");
			_HC1_bonus.text = string.Format(ids, sd._iapInfo._bonusCount);
		}
		else
		{
			_HC1_bonus.text = "";
		}
		
		// HC 6
		sd = StoreDataManager.Instance.GetStoreData("hc_3");
		ids = Localization.instance.Get("IDS_BUTTON_BUY");
		ids = Localization.instance.Get("IDS_FILTER_GEMS");
		_HC6_desc.text = sd._iapInfo._hcCount + " " + ids;
		
		if (sd._iapInfo._bonusCount > 0)
		{
			ids = Localization.instance.Get("IDS_BUY_BONUS");
			_HC6_bonus.text = string.Format(ids, sd._iapInfo._bonusCount);
		}
		else
		{
			_HC6_bonus.text = "";
		}
		
	}
	
	void OnEnable()
	{
		GameManager.Instance.GamePaused = true;
	}
	
	// IAP1
	void OnPurchaseHC1()
	{
//		StoreData sd = StoreDataManager.Instance.GetStoreData("hc_2");
	}
	
	// IAP6
	void OnPurchaseHC6()
	{
//		StoreData sd = StoreDataManager.Instance.GetStoreData("hc_3");
	}
	
	void Close()
	{
		GameManager.Instance.GamePaused = false;
		UIMessageBoxManager.Instance.CloseMessageBox(gameObject);
	}
	
	void OnTapToClose()
	{
		if (UICamera.lastHit.collider.gameObject != _uiBox.gameObject)
		{
			Close();
		}
	}
	
	void OnTapToCloseByMB()
	{
		if (UICamera.lastHit.collider.gameObject != _uiBox.gameObject)
		{
			UIMessageBoxManager.Instance.CloseMessageBox(gameObject);
			GameManager.Instance.GamePaused = false;
		}
	}
}
