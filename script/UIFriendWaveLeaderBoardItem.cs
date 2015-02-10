using UnityEngine;
using System;
using System.Collections;
using InJoy.FCComm;
using System.Reflection;
using InJoy.Utils;

public class UIFriendWaveLeaderBoardItem : MonoBehaviour
{
	public UILabel _rankLbl;
	public UILabel _nameLbl;
	public UILabel _warriorLbl;
	public UILabel _mageLbl;
	public UILabel _monkLbl;
	public UILabel _totalLbl;
	public GameObject _background;
	public Color _selectedItemColor = Color.magenta;

    //private string _friendID;
	
	public void SetData(FriendData entry, string towerID, int rankIndex)
	{
        //_friendID = entry.friendName;

		gameObject.name = string.Format("{0,16:X}", rankIndex + 1);

        _rankLbl.text = string.Format("#{0}", rankIndex + 1);
        _nameLbl.text = entry.friendName;
		//_mageLbl.text = entry.GSList[FCConst.ROLE_MAGE].ToString();
		//_warriorLbl.text = entry.GSList[FCConst.ROLE_WARRIOR].ToString();
		//_monkLbl.text = entry.GSList[FCConst.ROLE_MONK].ToString();
        
        _totalLbl.text = entry.towerFarestPosition[towerID].ToString();
		
		if((rankIndex & 1) == 0)
		{
			_background.SetActive(false);
		}
	}
}
