using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIInventoryPlayerInfo : MonoBehaviour {
	
	public UILabel  _nickName;
	public UILabel  _calssName;
	public UILabel  _level;
	public UILabel  _xpTitle;
	public UISlider _xpBar;
	
	public UILabel _scoreValue;
	public UILabel _maxScoreValue;
	
	public UILabel[] _attributes = new UILabel[9];
	
	public void UpdatePlayerInfoLocal()
	{
		//PlayerInfoManager.Instance.PlayerBaseInfo.UpdateTableInfo();
		UpdatePlayerInfo(PlayerInfo.Instance);
	}
	
	public void UpdatePlayerInfo(PlayerInfo playerInfo)
	{
		//string roleName = "";
		//if(playerInfo.CurPlayerProfile.Role == EnumRole.Mage)
		//{
		//    roleName = Localization.instance.Get("IDS_NAME_CLASS_MAGE");
		//}
		//else if (playerInfo.CurPlayerProfile.Role == EnumRole.Warrior)
		//{
		//    roleName = Localization.instance.Get("IDS_NAME_CLASS_WARRIOR");
		//}
		
		//_nickName.text = playerInfo.DisplayNickname;
		//_calssName.text = Localization.instance.Get("IDS_CLASS_TITLE") + roleName;
		//_xpTitle.text = Localization.instance.Get("IDS_XP_TITLE"); 
		//_level.text =    Localization.instance.Get("IDS_LEVEL_TITLE") + "[ffffff]" +playerInfo.CurPlayerProfile.CurrentLevel.ToString() +"[-]";
		//_xpBar.sliderValue = playerInfo.CurPlayerProfile._currentXpPrecent; 

		//List<string> attributes = playerInfo._playerInformations;
		//UIUtils.DrawText(_attributes,attributes);
		
		//_scoreValue.text = ((int)(playerInfo._gScore)).ToString();
		//_maxScoreValue.text = Localization.instance.Get("IDS_MAX") + ":" +((int)(playerInfo._gScoreMax)).ToString();
	}
}
