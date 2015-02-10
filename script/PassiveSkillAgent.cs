using UnityEngine;
using System.Collections;

public class PassiveSkillAgent : MonoBehaviour ,FCAgent {

	public FCPassiveSkill[] passiveSkills;
	public FCPassiveSkillMap[] passiveSkillMaps;

    public GameObject passiveSkillAgent = null;

	public bool forceRefresh = false;
	public bool enablePassiveSkill = false;

	protected ActionController _owner;
    protected PassiveSkillBase[] _passiveSkillHandles = null;
	
	public static string GetTypeName()
	{
		return "PassiveSkillAgent";
	}
	
	public void OnHpChanged()
	{
		
	}
	public FCPassiveSkill GetSkill(string skillName)
	{
		FCPassiveSkill skill = null;
		foreach(FCPassiveSkill es in passiveSkills)
		{
			if(es.skillName == skillName)
			{
				skill = es;
			}
		}
		return skill;
	}
	
	public void Init(FCObject owner)
	{
		_owner = owner as ActionController;
		forceRefresh = false;

		for(int i =0;i<passiveSkills.Length;i++)
		{
			passiveSkills[i].beActive = false;
		}

		for(int i =0;i<passiveSkillMaps.Length;i++)
		{
			foreach(FCPassiveSkillConfig psc in passiveSkillMaps[i]._skillConfigs)
			{
				psc.SkillModule = GetSkill( psc._skillName );
				psc.SkillModule.beActive = true;
			}
		}
		
	}
	
	public void InitSkillData()
	{
        if (null != passiveSkillAgent)
        {
            _passiveSkillHandles = passiveSkillAgent.GetComponents<PassiveSkillBase>();
        }


		if(enablePassiveSkill)
		{
			if(_owner.IsPlayer)
			{
				InitPlayerSkillData(_owner.IsPlayerSelf);
			}
			else
			{
				foreach(FCPassiveSkill passiveSkill in passiveSkills)
				{
                    passiveSkill.InitSkillData(null, _owner, _passiveSkillHandles);
				}
			}
		}
		
	}
	
	void InitPlayerSkillData(bool isPlayerSelf)
	{
		MatchPlayerProfile otherInfo = null;
		if(!isPlayerSelf) otherInfo = MatchPlayerManager.Instance.GetMatchPlayerProfile(_owner._instanceID);
		if(!isPlayerSelf && otherInfo == null ) {
            Debug.LogError("otherInfo == null !");
			return ;
		}

        string roleName = GameSettings.Instance.roleSettings[PlayerInfo.Instance.RoleID].battleLabel;

		foreach(FCPassiveSkill passiveSkill in passiveSkills)
		{
			string skillName = passiveSkill.skillName;
			SkillData skillData = null;
			if(passiveSkill.needDataFromDatabase)
			{
                skillData = DataManager.Instance.GetSkillData(roleName, skillName, true);
				//if(!isPlayerSelf){
				//    skillData.CurLevel = otherInfo._playerInfo.GetSkillLevel(skillName);
				//}
			}

            passiveSkill.InitSkillData(skillData, _owner, _passiveSkillHandles);
		}
	}
	
#if UNITY_EDITOR
	void Update()
	{
		if(forceRefresh)
		{
			forceRefresh = false;

			 Init(_owner);
			InitSkillData();

            _owner.UpdateData();
		}
	}
	
#endif
	//protected ActionController 
    public void PassiveEffect(int aiLevel, AcData acd)
    {
        if (enablePassiveSkill)
        {
            foreach (FCPassiveSkillConfig psc in passiveSkillMaps[aiLevel]._skillConfigs)
            {
                if (psc.SkillModule.beActive)
                {
                    foreach (FCPassiveSkillAttribute skill in psc.SkillModule.skillAttributes)
                    {
                        switch (skill.skillType)
                        {
                            case AIHitParams.Hp:
                                acd.AddHitParamsData(AIHitParams.Hp, skill.attributeValue);
                                break;
                            case AIHitParams.HpPercent:
                                acd.AddHitParamsData(AIHitParams.HpPercent, skill.attributeValue);
                                break;
                            case AIHitParams.Attack:
                                acd.AddHitParamsData(AIHitParams.Attack, skill.attributeValue);
                                break;
                            case AIHitParams.AttackPercent:
                                acd.AddHitParamsData(AIHitParams.AttackPercent, skill.attributeValue);
                                break;
                            case AIHitParams.Defence:
                                acd.AddHitParamsData(AIHitParams.Defence, skill.attributeValue);
                                break;
                            case AIHitParams.DefencePercent:
                                acd.AddHitParamsData(AIHitParams.DefencePercent, skill.attributeValue);
                                break;
                            case AIHitParams.RunSpeed:
                                //acd.passiveSpeed = acd.TotalMoveSpeed * skill.attributePercents + skill.attributeValue;
                                acd.AddHitParamsData(AIHitParams.RunSpeed, skill.attributeValue);
                                break;
                            case AIHitParams.RunSpeedPercent:
                                acd.AddHitParamsData(AIHitParams.RunSpeedPercent, skill.attributeValue);
                                break;
                            case AIHitParams.CriticalDamage:
                                acd.AddHitParamsData(AIHitParams.CriticalDamage, skill.attributeValue);
                                break;
                            case AIHitParams.CriticalDamagePercent:
                                acd.AddHitParamsData(AIHitParams.CriticalDamagePercent, skill.attributeValue);
                                break;
                            case AIHitParams.CriticalChance:
                                acd.AddHitParamsData(AIHitParams.CriticalChance, skill.attributeValue);
                                break;
                            case AIHitParams.CriticalChancePercent:
                                acd.AddHitParamsData(AIHitParams.CriticalChancePercent, skill.attributeValue);
                                break;
                            case AIHitParams.PassiveSkillGodDownCriticalToHp:
                                acd.AddHitParamsData(AIHitParams.PassiveSkillGodDownCriticalToHp, skill.attributeValue);
                                break;
                        }
                    }
                }
            }
        }
    }
}
