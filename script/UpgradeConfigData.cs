using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


[System.Serializable]
public class PropertyScore
{
	public AIHitParams _type;
	public float _score;
	public string _nameIds = "IDS_XXX";
	public float _value = 1.0f;
}


[System.Serializable]
public class FuserData
{
	public int _costHc;
	public float _bonus;
}

[System.Serializable]
public class VipBonus{
	public float _rate;
	public float _bonus;
}




public class UpgradeConfigData : ScriptableObject {
	
	
	public float _firstAttMin = 0.3f;
	public float _firstAttMax = 0.8f;
	public float _firstAttDiscard = 0.2f;
	
	public float _secondAttMin = 0.3f;
	public float _secondAttMax = 0.8f;
	public float _secondAttDiscard = 0.2f;
	
	

	
	public AttributeDataList _attributeDataList;
	public FusionConfigDataList _fusionConfigDataList;
	
	public float GetFusionFactor(ItemSubType part)
	{	
		foreach(FusionConfigData configData in _fusionConfigDataList._dataList)
		{
			if(configData._part == part)
			{
				return configData._factor;
			}
		}
		return 1.0f;
	}
}








