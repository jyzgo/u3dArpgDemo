using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class SkillGrade
{
    public int level = 0;
	public float cost = 0; //all  
	public float coolDownTime = 0;//all 
	public float damageScale = 1;//all 
	public float attackRange = 1; //all 
	public int bulletCount = 0; //all  
	public float dotDamageTime = 0; //all 

	public int effect = 0; //all

	public float godTime = 0; //dodge
	public float attackEffectTime = 0;
	public float chargeTime = 0;//fire spear
	public float skillTime = 0;//storm  
	public int attackNumber = 1; //falsh chain 
	public int specialAttackType = 0; //parry and dash
	public float speed = 0; //dash

	//if for passive skill, 1 means %, 2 means value
	public float attribute1 = 0.0f;//
	public float attribute2 = 0.0f;

    public AIHitParams passiveType1 = AIHitParams.Hp;

    public AIHitParams passiveType2 = AIHitParams.Hp;

	public int costSc = 0;
	public int costHc = 0;
	public int unlockLevel = 0;
	public string preSkillId = string.Empty;
	public int preSkillLevel = 0;
	public string descIDS = string.Empty;
    public string eotId = string.Empty;
}

[Serializable]
public class SkillUpgradeData
{
	public string _enemyId;
	public string _skillId;
	public List<SkillGrade> upgradeDataList = new List<SkillGrade>();

    public SkillGrade GetSkillGradeBySkillLevel(int level)
    {
        SkillGrade skillGrade = upgradeDataList.Find(delegate(SkillGrade sg) { return sg.level == level; });

        return skillGrade;
    }

    public SkillGrade GetSkillGradeByUnlockLevel(int unlocklevel)
    {
        SkillGrade skillGrade = upgradeDataList.Find(delegate(SkillGrade sg) { return sg.unlockLevel == unlocklevel; });

        return skillGrade;
    }
}


public class SkillUpgradeDataList : ScriptableObject 
{
	public List<SkillUpgradeData> dataList = new List<SkillUpgradeData>();
}
