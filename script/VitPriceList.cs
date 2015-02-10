using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


[System.Serializable]
public class VitPrice
{
	public int buyTimes;
	public int hcCost;
}

public class VitPriceList : ScriptableObject
{
	public List<VitPrice> dataList = new List<VitPrice>();

	public VitPrice GetVitPrice(int buyTimes)
	{
		return dataList.Find(delegate(VitPrice vp) { return vp.buyTimes == buyTimes; });
	}
}
