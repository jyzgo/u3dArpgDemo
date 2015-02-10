using UnityEngine;
using System.Collections;


public class UILoginHandler : MonoBehaviour {
	
	public GameObject _selectPanel = null;
	public GameObject _loginPanel = null;
	public GameObject _RegisterPanel = null;
	public GameObject _anonymousLoginPanel = null;
	
	
	public UIInput _inputLoginEmail = null;
	public UIInput _inputLoginPassword = null;
	public UIInput _inputRegisterEmail = null;
	public UIInput _inputRegisterPassword = null;
	public UIInput _inputRegisterPasswordConfirm = null;	

	const string Key_AnonymousLogin = "AnonymousLogin";
	const string Key_LoginID = "LoginID";

	
	// Use this for initialization
	void Start () {
		_loginPanel.SetActive(false);
		_selectPanel.SetActive(false);
		_RegisterPanel.SetActive(false);
		_anonymousLoginPanel.SetActive(false);		
		
		_inputLoginEmail.defaultText = Localization.instance.Get("IDS_LOGIN_EMAIL_DEFAULT");
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	
	//popup the page
	public void BeginSelect() {
		_selectPanel.SetActive(true);
		_RegisterPanel.SetActive(false);
		_anonymousLoginPanel.SetActive(false);		
		_loginPanel.SetActive(false);
	}
	
	//popup the page if never enter before
	public void BeginSelectIfFirstTime() {
		if (PlayerPrefs.HasKey(Key_AnonymousLogin) || PlayerPrefs.HasKey(Key_LoginID))
			return;
		
		_selectPanel.SetActive(true);
		_RegisterPanel.SetActive(false);
		_anonymousLoginPanel.SetActive(false);		
		_loginPanel.SetActive(false);
	}
	
	public void Exit() {
		_selectPanel.SetActive(false);
		_RegisterPanel.SetActive(false);
		_anonymousLoginPanel.SetActive(false);		
		_loginPanel.SetActive(false);		
	}
	
	public void ClearKeys() {
		PlayerPrefs.DeleteKey(Key_AnonymousLogin);
		PlayerPrefs.DeleteKey(Key_LoginID);
		PlayerPrefs.Save();
	}

	
    #region Select Page
	
	void OnAnonymousClick()
	{
		Debug.Log("OnAnonymousClick");	
		_selectPanel.SetActive(false);
		_anonymousLoginPanel.SetActive(true);		
	}
	
	
	void OnRegisterClick()
	{
		Debug.Log("OnRegisterClick");	
		_selectPanel.SetActive(false);
		_RegisterPanel.SetActive(true);	
	}
	
	void OnLoginClick()
	{
		Debug.Log("OnLoginClick");	
		_selectPanel.SetActive(false);
		_loginPanel.SetActive(true);
	}
    #endregion	
	
	
	#region Login Page
	void OnLoginPageLoginClick()
	{
		Debug.Log("OnLoginPageLoginClick");	
	}
	
	void OnLoginPageCancelClick()
	{
		Debug.Log("OnLoginPageCancelClick");
		BeginSelect();		
	}
    #endregion	
	
	#region Register Page
	void OnRegisterPageRegisterClick()
	{
		Debug.Log("OnRegisterPageRegisterClick");	
	}
	
	void OnRegisterPageCancelClick()
	{
		Debug.Log("OnRegisterPageCancelClick");
		BeginSelect();			
	}
    #endregion		
	
	#region AnonymouseLogin Page
	void OnAnonymousLoginClick()
	{
		Debug.Log("OnAnonymousLoginClick");	
		
		//save locally, I am using anonymous
		PlayerPrefs.SetInt(Key_AnonymousLogin, 1);
		PlayerPrefs.SetString(Key_LoginID, "");
		PlayerPrefs.Save();
		
		//TODO, GloOn anonymous login
		
		
		
		//close UI
		Exit();
	}
	
	void OnAnonymousCancelClick()
	{
		Debug.Log("OnAnonymousCancelClick");
		BeginSelect();		
	}
	
	
	
    #endregion		
		
	
	
}
