using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class PlayerLevelData
{
	public int _level = 1;
	public int _exp = 100;
	public int bearPoint;
	public int _reviveHc = 3;
	
	public int _mage_hp = 100;
	public int _mage_attack = 0;
	public int _mage_defense = 0;
	public float _mage_crit_damage = 0;
	public float _mage_crit = 0;
	
	public int _warrior_hp = 100;
	public int _warrior_attack = 0;
	public int _warrior_defense = 0;
	public float _warrior_crit_damage = 0;
	public float _warrior_crit = 0;
}


public class PLevelData
{
	public int _level = 0;
	public int _xp = 0;
	public int _reviveHc = 0;
	
	public int _hp = 0;
	public int _attack = 0;
	public int _defense = 0;
	public float _crit_damage = 0;
	public float _crit_rate = 0;
}

public class PlayerLevelDataList : ScriptableObject
{
    public List<PlayerLevelData> _dataList = new List<PlayerLevelData>();

    public PlayerLevelData GetPlayerLevelDataByLevel(int level)
    {
        return _dataList.Find(delegate(PlayerLevelData pld) { return pld._level == level; });
    }
}




