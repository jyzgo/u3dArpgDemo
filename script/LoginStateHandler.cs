using UnityEngine;
using System.Collections;
using InJoy.FCComm;

public class LoginStateHandler : MonoBehaviour {
	
	public UILabel _loginStatus = null;
	public UILabel _gluonPlayerID = null;
	public UILabel _gluonNickName = null;
	public UILabel _facebookName = null;
	
	public GameObject _continueButton = null;
	public UILabel _continueLabel = null;

	
	
	
	// Use this for initialization
	void Start () {
	
	}
	

	void LateUpdate() {
		
		//change login status
        LOGIN_STEP step = LOGIN_STEP.LOGIN_FAILED;

        switch (step)
		{
		case LOGIN_STEP.NOT_START:
			_loginStatus.text = "login not start";
			break;
				
		case LOGIN_STEP.LOGINING:
			_loginStatus.text = "begin login ...";			
			break;

		case LOGIN_STEP.LOGIN_RECONNECTING:
			_loginStatus.text = "Login failed, reconnect ...";			
			break;			
			
		case LOGIN_STEP.LOGIN_OK:
			_loginStatus.text = "login success!";	
			_continueLabel.text = "online play";
			NGUITools.SetActive(_continueButton, true);
			//OnClickOK(); //auto-enter game
			break;
		
		case LOGIN_STEP.LOGIN_FAILED:
			_loginStatus.text = "login failed!";	
			_continueLabel.text = "offline debug";
			NGUITools.SetActive(_continueButton, true);
			break;
		}
		
	}
	
	void OnClickOK() 
	{

	}
}
