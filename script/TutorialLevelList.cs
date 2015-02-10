using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[System.Serializable]
public class TutorialLevel
{
	public EnumTutorial id;
	public EnumTutorial preId = EnumTutorial.None;

	public string startIds_mage;
	public string startIds_warrior;

	public string finishIds_mage;
	public string finishIds_warrior;

	public float start_time = 100000;
	public float finish_time = 3.0f;

	public bool only_once;

	public string StartIds
	{
		get
		{
			EnumRole role = PlayerInfo.Instance.Role;
			if (role == EnumRole.Mage)
			{
				return startIds_mage;
			}
			else
			{
				return startIds_warrior;
			}

		}
	}

	public string FinishIds
	{
		get
		{
			EnumRole role = PlayerInfo.Instance.Role;
			if (role == EnumRole.Mage)
			{
				return finishIds_mage;
			}
			else
			{
				return finishIds_warrior;
			}
		}
	}
}


public class TutorialLevelList : ScriptableObject
{
	public List<TutorialLevel> _dataList = new List<TutorialLevel>();
}

