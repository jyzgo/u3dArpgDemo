using UnityEngine;
using System.Collections;

public class UIPhotonServerSelect : MonoBehaviour
{
	public UILabel _title;
	public UIPopupList _popupList;
	
	void OnInitialize()
	{
		_popupList.items.Clear();
		foreach(string strID in MultiplayerDataManager.Instance.PvPData._photonServerNames)
		{
			_popupList.items.Add(Localization.instance.Get(strID));
		}
		_popupList.selection = _popupList.items[MultiplayerDataManager.Instance.PhotonServerSelectIndex];
	}
	
	void OnClose()
	{
		UIManager.Instance.CloseUI("PhotonServerSelect");
		UIManager.Instance.OpenUI("UIPvPEnter");
	}
	
	void OnSelectionChange(string item)
	{
		int length = MultiplayerDataManager.Instance.PvPData._photonServerNames.Length;
		for(int i = 0; i < length; i ++)
		{
			if(item == Localization.instance.Get(MultiplayerDataManager.Instance.PvPData._photonServerNames[i]))
			{
				MultiplayerDataManager.Instance.PhotonServerSelectIndex = i;
				break;
			}
		}
	}
}
