using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class SkillData
{
	public string skillID;
	public string enemyID;
	public int roleID;			//mage = 0, warrior = 1
	public int ord;
	public string nameIDS;
	public string descIDS;
	public string iconPath;
	public bool isPassive;
	public int level;

	public void InitSkillUpgradeData()
	{
		_skillUpgradeData = DataManager.Instance.GetSkillUpgradeData(enemyID, skillID);
	}

	private SkillGrade _currentLevelData = null;
	public SkillGrade CurrentLevelData
	{
		get
		{
			_currentLevelData = SkillUpgradeData.upgradeDataList[level];

			return _currentLevelData;
		}
	}

	private SkillUpgradeData _skillUpgradeData = null;
	public SkillUpgradeData SkillUpgradeData
	{
		get
		{
			return _skillUpgradeData;
		}
	}
}

public class SkillDataList : ScriptableObject {

	public List<SkillData> _dataList = new List<SkillData>();
}
