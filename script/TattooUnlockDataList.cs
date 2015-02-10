using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class TattooUnlockDataList : ScriptableObject
{
	public List<TattooUnlockData> dataList = new List<TattooUnlockData>();

	public bool IsLocked(EnumTattooPart part, int playerLevel, out int unlockLevel)
	{
		TattooUnlockData data = dataList.Find(delegate(TattooUnlockData tud) { return tud.part == part; });
		
		unlockLevel = data.playerLevel;
		
		return unlockLevel > playerLevel;
	}
}

[System.Serializable]
public class TattooUnlockData
{
	public int playerLevel;

	public EnumTattooPart part;
}
