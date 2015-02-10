using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LootAgent : FCObject ,FCAgent {
	
	public List<Vector3> lootOffest = new List<Vector3>();
	public LOOTTYPE lootType = LOOTTYPE.RANDOM;
	
	private List<LootObjData> _dataList = new List<LootObjData>();
	
	ActionController _owner;
	public static string GetTypeName()
	{
		return "LootAgent";
	}
	
	public void Init(FCObject ewb)
	{
		_owner = ewb as ActionController;
	}
	
	//calculate loot in my hero active	
	public void StartCalculateAtOnce(List<LootObjData> list)
	{
		_dataList = list;
		Assertion.Check(_dataList != null);
		Debug.Log("Preparing " + _owner.gameObject.name + "'s loot, length is " + _dataList.Count); 
		LootManager.Instance.PrepareLootPrefabs(_dataList);
	}
	
	
	public void Loot()
	{
		Debug.LogWarning("Loot from " + _owner.gameObject.name + ", length is " + _dataList.Count);
		LootManager.Instance.Loot(_dataList,_owner.ThisTransform.position,_owner.ThisTransform.forward,lootType,lootOffest);
		
		if(ObjectManager.Instance != null)
		{
			ActionController ac = ObjectManager.Instance.GetMyActionController();
			if(ac.IsAlived)
			{

				float exp_add = ObjectManager.Instance.GetMyActionController().Data.TotalExpAddition;
				
				int xp = (int)(_owner.Data.LootXp * (1+ exp_add));
				//string textFormat = Localization.instance.Get("IDS_GET_XP") ;
				//string message  = string.Format(textFormat ,xp);

				PlayerInfo.Instance.AddXP(xp);
				
				BattleSummary.Instance.ExpEarned+=xp;
			}
		}
	}
}
