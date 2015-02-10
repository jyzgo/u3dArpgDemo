using UnityEngine;
using System.Collections;

public class HUDBattleTopBar : MonoBehaviour 
{
	public UILabel _hc;
	public UILabel _sc;
	
	public Transform _hcPlus;
	public Transform _scPlus;
	
	
	public Transform _vpIcon;
	public UILabel _vpValue;
	public GameObject _vpButton;
	

	// Use this for initialization
	void Start () 
	{
		PlayerInfo.Instance.onHardCurrencyChanged += OnHardCurrencyChanged;
		PlayerInfo.Instance.onSoftCurrencyChanged += OnSoftCurrencyChanged;
		
		
		if(_vpButton != null)
		{
			_vpButton.GetComponent<UIButtonMessage>().target = gameObject;
			_vpButton.GetComponent<UIButtonMessage>().functionName = "OnVpButton";
		}
	}
	
	void OnEnable()
	{
        DisplayCurrency(PlayerInfo.Instance.HardCurrency, _hc, _hcPlus);
        DisplayCurrency(PlayerInfo.Instance.SoftCurrency, _sc, _scPlus);
	}
	
	void OnDestroy()
	{
		PlayerInfo.Instance.onHardCurrencyChanged -= OnHardCurrencyChanged;
		PlayerInfo.Instance.onSoftCurrencyChanged -= OnSoftCurrencyChanged;
	}
	
	public void OnHardCurrencyChanged(int hc)
	{
		DisplayCurrency(hc, _hc, _hcPlus);
	}
	
	public void OnSoftCurrencyChanged(int sc)
	{
		DisplayCurrency(sc, _sc, _scPlus);
	}
	
	private void DisplayCurrency(int currency, UILabel label, Transform t)
	{
		if (label != null)
		{
			currency = (currency>FCConst.MAX_CURRENCY_VALUE_FOR_DISPLAY) ? FCConst.MAX_CURRENCY_VALUE_FOR_DISPLAY : currency;
			
			label.text = Utils.GetSplitNumberString(currency);
		}
	}
	

	void OnClickBuySC()
	{
		if (GameManager.Instance.GameState == EnumGameState.InTown)
		{
			//UIMessageBoxManager.Instance.ShowShortcutIAP(null, false);
		}
	}
	
	void OnClickBuyHC()
	{
		if (GameManager.Instance.GameState == EnumGameState.InTown)
		{
			//UIMessageBoxManager.Instance.ShowShortcutIAP(null, true);
		}
		else
		{
			//UIMessageBoxManager.Instance.ShowShortcutIAP(null, true);
		}
	}
	
	void OnClickGamePaused()
	{
		if(!TutorialTownManager.Instance.isInTutorial)
		{
			UIManager.Instance.OpenUI("UIGamePaused");
		}
	}
}
