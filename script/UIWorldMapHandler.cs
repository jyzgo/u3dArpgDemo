using UnityEngine;
using System.Collections;

/// <summary>
/// User interface world map handler.
/// 
/// The main user interface for world map.
/// </summary>
public class UIWorldMapHandler : MonoBehaviour 
{
	public GameObject _buttonMultiplay = null;
	public GameObject _buttonSingleplay = null;	
	
	public GameObject _backButton = null;
	
	// Use this for initialization
	void Start () 
	{
		_buttonMultiplay.SetActive(true);
		_buttonSingleplay.SetActive(false);
	}
	
	void OnInitialize()
	{
		LevelManager.Singleton.StopActorMoving();

		EnumTutorialState state = PlayerInfo.Instance.GetTutorialState(EnumTutorial.Town_Map);
		if (state == EnumTutorialState.Active)
		{
			_backButton.GetComponent<UIImageButton>().isEnabled = false;
		}else{
			_backButton.GetComponent<UIImageButton>().isEnabled = true;
		}
		
	}
	
	// Return to town.
	void OnCloseWorldMap()
	{
		WorldMapController.Instance.DestroyWorldMap();
		
		UIManager.Instance.CloseUI("WorldMapHome");
		UIManager.Instance.OpenUI("TownHome");
	}
	
	// Find a game in the game lobby.
	void OnFindAGame()
	{
		UIManager.Instance.OpenUI("ListHost");
	}
/*	
	void OnEnterMultiplayGame()
	{
		//skip enter multiplay twice
		if (_enteringMultiplay)
			return;
		else
			_enteringMultiplay = true;

		//if not connect to photon, connect to it.
		if (PhotonNetwork.connectionState == ConnectionState.Disconnected)
		{
			//not connect, connect to photon
			PhotonManager.Instance.OnPhotonConnectedComplete = OnEnterMultiplayGameCompelete;
			PhotonManager.Instance.ConnectToPhoton();
		}
		else
		{
			//already coneected
			PhotonManager.Instance.OnPhotonConnectedComplete = null;
			OnEnterMultiplayGameCompelete(true);
		}
	}
	
	void OnEnterMultiplayGameCompelete(bool success)
	{
		_enteringMultiplay = false;
		
		if (success)
		{
			GameManager.Instance.isMultiplay = true;	
			_buttonMultiplay.SetActive(false);
			_buttonSingleplay.SetActive(true);
		}

	}
	
	void OnEnterSingleplayGame()
	{
		PhotonNetwork.Disconnect();
		GameManager.Instance.isMultiplay = false;
		_buttonMultiplay.SetActive(true);
		_buttonSingleplay.SetActive(false);
	}
*/
}
