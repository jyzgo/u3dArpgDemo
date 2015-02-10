using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


[System.Serializable]
public class VipPrivilegeData
{
	public int vipLevel;
	public int maxScExchangeCount;	//max times of making sc exchanges within a day
	public int maxVitExchangeCount;	//max times of buying vitality within a day
}

public class VipPrivilegeList : ScriptableObject
{
	public List<VipPrivilegeData> dataList = new List<VipPrivilegeData>();

	public VipPrivilegeData GetVipPrivilege(int vipLevel)
	{
		return dataList.Find(delegate(VipPrivilegeData vd) { return vd.vipLevel == vipLevel; });
	}

	public int GetNextVipLevelForVit(int vipLevel)
	{
		int currentMaxVit = GetVipPrivilege(vipLevel).maxVitExchangeCount;

		//assume the privilege data is sorted acscendingly
		foreach (VipPrivilegeData vd in dataList)
		{
			if (vd.maxVitExchangeCount > currentMaxVit)
			{
				//found!
				return vd.vipLevel;
			}
		}

		Debug.LogError("Next vip level not found!");
		return -1;
	}
}
