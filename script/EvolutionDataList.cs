using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

[Serializable]
public class EvolutionUpgradeNeed{
	public string _itemId;
	public int _count;
}

[Serializable]
public class EvolutionData
{	
	public int _level;
	public int _rareLevel;
	public int _evolutionLevel;
	
	public float _outputEffect;
	public float _crit_damage;
	
	public int _costSc;
	public int _costHc;
	public List<EvolutionUpgradeNeed> _needList = new List<EvolutionUpgradeNeed>();
	
	public string GetKey()
	{
		return 	_level.ToString() +"_"+ _rareLevel.ToString() +"_"+ _evolutionLevel.ToString();
	}
}

public class EvolutionDataList : ScriptableObject {
	public List<EvolutionData> _dataList = new List<EvolutionData>();

    /// <summary>
    /// Find the matched evolution data by level, rare level and evolution level. For locating the required matarials.
    /// </summary>
    /// <returns></returns>
    public EvolutionData FindMatch(int level, int rareLevel, int evolutionLevel)
    {
		foreach (EvolutionData evd in _dataList)
		{
			if (evd._level == level && evd._rareLevel == rareLevel && evd._evolutionLevel == evolutionLevel)
			{
				return evd;
			}
		}
		
		Debug.LogError(string.Format("[EvolutionDataList] Failed to find evolution data. Level = {0} rareLevel = {1}  evolution level = {2}", level, rareLevel, evolutionLevel));
		
		return null;
	}
}
