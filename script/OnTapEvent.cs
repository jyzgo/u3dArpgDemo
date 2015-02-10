using UnityEngine;
using System.Collections;

public class OnTapEvent : MonoBehaviour {

	private string _playerId;
	public string PlayerID {
		set {_playerId = value;}
	}
	
	
	void OnClick() {

		//if(_playerId == PlayerInfoManager.Instance.LocalPlayerInfo._id)
		//{
		//    return;
		//}
		
		TownPlayerManager.Singleton.ActivedPlayerID = _playerId;
		
		if (!UIManager.Instance.IsUIOpened("TownPlayerPopup"))
		{
			UITownPlayerPopup._selectedPlayer = gameObject.transform.parent.gameObject;
			UITownPlayer._activePlayerInfo = TownPlayerManager.Singleton.ActivedPlayerInfo;
			
			UIManager.Instance.OpenUI("TownPlayerPopup");
			SoundManager.Instance.PlaySoundEffect("button_normal");
		}
	}
}
