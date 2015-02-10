using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[System.Serializable]
public class LootWeight
{
	public int _weight;
	public string _itemId;
	public int _countMin;
	public int _countMax;
}


[System.Serializable]
public class LootData
{
	public string _enemyId;
	public List<LootWeight> _lootList = new List<LootWeight>();
}

