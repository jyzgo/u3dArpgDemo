using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class DailyBonusDataList : ScriptableObject
{
	public List<DailyBonusData> dataList = new List<DailyBonusData>();
}

[Serializable]
public class DailyBonusData
{
	public int day;
	public string reward_item_id;
	public int amount;
}
