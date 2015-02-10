using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("FC/Logic/FCObject/PlayAndEnemy/Agent/AC/Attack/AttackAgent")]
public class AttackAgent : MonoBehaviour ,FCAgent
{
	
	#region data
	//must not be changed in inspector ,only should changed by click force refresh
	//[HideInInspector]
	public List<AttackBase> _attackList = null;
	public bool _forceRefresh = false;
	public GameObject _attacks;
	public FCSkill[] _skills;
	public FCSkillMap[] _skillMaps;
	
	protected ActionController _owner = null;
	//public
	

	#endregion
	
	
	public static string GetTypeName()
	{
		return "AttackAgent";
	}
	public void Init(FCObject owner)
	{
		_owner = owner as ActionController;
		foreach(FCSkill es in _skills)
		{
			foreach(FCAttackConfig eac in es._attackConfigs)
			{
				eac.AttackModule = GetAttack(eac._attackModuleName);
				//bool isdefy = false;
				foreach(string ds in es._defyModule)
				{
					if(ds == eac.AttackModule._name)
					{
						eac.AttackModule.IsInDefy = true;
						break;
					}
				}
			}
		}
		
		for(int i =0;i<_skillMaps.Length;i++)
		{
			FCSkillConfig[] sc = _skillMaps[i]._skillConfigs;
			for(int j =0; j< sc.Length; j++)
			{
				sc[j].SkillModule = GetSkill(sc[j]._skillName);
				sc[j]._distanceMinSqrt = sc[j].SkillModule._attackConfigs[0].AttackModule._distanceMin
					*sc[j].SkillModule._attackConfigs[0].AttackModule._distanceMin;
				sc[j]._distanceMaxSqrt = sc[j].SkillModule._attackConfigs[0].AttackModule._distanceMax
					*sc[j].SkillModule._attackConfigs[0].AttackModule._distanceMax;
			}
			
		}
		WriteInfoToFirstAttack();
	}
	
	
	private FC_KEY_BIND GetSkillKeyBind(string skillName)
	{
        FC_KEY_BIND key = FC_KEY_BIND.NONE;

		for(int i =0;i<_skillMaps.Length;i++)
		{
			FCSkillConfig[] sc = _skillMaps[i]._skillConfigs;

			for(int j =0; j< sc.Length; j++)
			{
                if(sc[j]._skillName == skillName)
                {
                    key = sc[j]._keyBind;
                }
            }
        }

        return key;

        //List<string>  usedSkill = PlayerInfo.Instance.activeSkillList;
		
        //for(int i = 0; i< usedSkill.Count; i++)
        //{
        //    if (usedSkill[i] == skillName)
        //    {
        //        if(i == 0)
        //        {
        //            return FC_KEY_BIND.ATTACK_2;
        //        }
        //        else if( i== 1)
        //        {
        //            return FC_KEY_BIND.ATTACK_3;
        //        }
        //        else if( i== 2)
        //        {
        //            return FC_KEY_BIND.ATTACK_4;
        //        }
        //        else if( i== 3)
        //        {
        //            return FC_KEY_BIND.ATTACK_5;
        //        }
        //    }
        //}
        //return key;
	}
	
	private void BindSkillKey()
	{
		int keyCantSee = FCConst.FC_KEY_FOR_TOUCH;
		foreach(FCSkillMap skillMap in _skillMaps){
			foreach(FCSkillConfig config in skillMap._skillConfigs)
			{
				if(IsSkill(config._skillName))
				{
					FC_KEY_BIND key  = GetSkillKeyBind(config._skillName);
					if(key == FC_KEY_BIND.NONE && _owner.IsPlayerSelf)
					{
						key = (FC_KEY_BIND)keyCantSee;
						keyCantSee++;
						if(keyCantSee > FCConst.FC_KEY_MAX-1)
						{
							break;
						}
					}
					config._keyBind = key;
				}
			}
		}
	}
	
	private bool IsSkill(string skillName)
	{
		if(skillName != "Slashx4"
				&& skillName != "MagicBall")
		{
			return true;	
		}
		return false;
	}
	
	public void InitSkillData(AIAgent aiOwner)
	{
		if(_owner.IsPlayer)
		{
			if(_owner.IsPlayerSelf)
			{
				BindSkillKey();
			}
			
			InitAIAgentSkillData(_owner.IsPlayerSelf , aiOwner);
		}
		else
		{
			foreach(FCSkill eWSkill in _skills)
			{
				string skillName = eWSkill._skillName;
					
				if(IsSkill(skillName))
				{
					eWSkill.InitSkillData(null, aiOwner);
				}
			}
		}
	}
	
	void InitAIAgentSkillData(bool isPlayerSelf , AIAgent aiOwner)
	{
		MatchPlayerProfile otherInfo = null;
		if(!isPlayerSelf) otherInfo = MatchPlayerManager.Instance.GetMatchPlayerProfile(_owner._instanceID);
		if(!isPlayerSelf && otherInfo == null ) {
            Debug.LogError("otherInfo == null !");
			return ;
		}
		
		foreach(FCSkill eWSkill in _skills)
		{
			string skillName = eWSkill._skillName;
				
			if(IsSkill(skillName))
			{
				SkillData skillData = null;
				if(eWSkill._needDataFromDatabase)
				{
					skillData =  DataManager.Instance.GetSkillData(_owner.Data.id, skillName ,true);
					//if(!isPlayerSelf){
					//    skillData.CurLevel = otherInfo._playerInfo.GetSkillLevel(skillName);
					//}
				}
				eWSkill.InitSkillData(skillData, aiOwner);
			}
		}
	}
	
	/*
	public void InitAllSkillData(List<SkillMap> skillMapList, List<SkillData> skillDataList, List<AttackModuleData> attackModuleList)
	{
		//init attack
		Assertion.Check(attackModuleList.Count == _attackList.Count);
		if(attackModuleList.Count == _attackList.Count)
		{
			for(int i =0;i<attackModuleList.Count;i++)
			{
				_attackList[i]._damageScale =  attackModuleList[i]._scale;
				_attackList[i]._hitType =  attackModuleList[i]._hitType;
				_attackList[i]._energyCost =  attackModuleList[i]._costEnergy;
				//Todo: need Gd write true data
				//_attackList[i]._distanceMin =  attackModuleList[i]._distanceMin;
				//_attackList[i]._distanceMax=  attackModuleList[i]._distanceMax;
				
			}
		}
		//init skills
		{
			_skills = new FCSkill[skillDataList.Count];
			for(int i =0;i<skillDataList.Count;i++)
			{
				_skills[i] = new FCSkill();
				_skills[i]._comboHitMax =  skillDataList[i]._comboHitMax;
				_skills[i]._priority =  skillDataList[i]._priority;
				_skills[i]._coolDownTimeMax =  skillDataList[i]._coolDownTime;
				_skills[i]._attackConfigs = new FCAttackConfig[skillDataList[i]._attackModuleConfigList.Count];
				for(int j = 0; j < skillDataList[i]._attackModuleConfigList.Count; j++)
				{
					_skills[i]._attackConfigs[j] = new FCAttackConfig();
					_skills[i]._attackConfigs[j]._attackModuleName = skillDataList[i]._attackModuleConfigList[j]._attackModuleId;
					_skills[i]._attackConfigs[j].AttackModule = GetAttack(_skills[i]._attackConfigs[j]._attackModuleName);
					_skills[i]._attackConfigs[j]._attackConditions = skillDataList[i]._attackModuleConfigList[j]._attackModuleConditionList.ToArray();
				}
				_skills[i]._skillName = skillDataList[i]._skillId;
				if(_owner.IsPlayer 
					&& (_skills[i]._skillName == "Slashx4" || _skills[i]._skillName == "MagicBall"))
				{
					_skills[i]._isNormalSkill = false;
					_skills[i]._canCacheCombo = true;
				}
				else
				{
					_skills[i]._isNormalSkill = true;
					_skills[i]._canCacheCombo = false;
				}
				if(_owner.IsPlayer)
				{
					_skills[i]._isInLoop = true;
				}
				else
				{
					_skills[i]._isInLoop = false;
				}
			}
		}
		//init skill map
		{
			_skillMaps = new FCSkillMap[skillMapList.Count];
			for(int i =0;i<skillMapList.Count;i++)
			{
				_skillMaps[i] = new FCSkillMap();
				_skillMaps[i]._skillConfigs = new FCSkillConfig[skillMapList[i]._skillConfigList.Count];
				for(int j = 0; j < _skillMaps[i]._skillConfigs.Length; j++)
				{
					_skillMaps[i]._skillConfigs[j] = new FCSkillConfig();
					FCSkillConfig esc = _skillMaps[i]._skillConfigs[j];
					SkillConfig sc = skillMapList[i]._skillConfigList[j];
					esc._attackWeight = sc._skillWeight;
					esc.SkillModule = GetSkill(sc._skillId);
					esc._distanceMinSqrt = esc.SkillModule._attackConfigs[0].AttackModule._distanceMin
					*esc.SkillModule._attackConfigs[0].AttackModule._distanceMin;
					esc._distanceMaxSqrt = esc.SkillModule._attackConfigs[0].AttackModule._distanceMax
						*esc.SkillModule._attackConfigs[0].AttackModule._distanceMax;
					if(_owner.IsPlayerSelf)
					{
						esc._keyBind = FC_KEY_BIND.ATTACK_1+j;
					}
					esc._skillName = sc._skillId;
				}
			}
		}

	}
	*/
	
	#region logic function
	public void StartAttack()
	{
		
	}
	
	public FCSkill GetSkill(string skillName)
	{
		FCSkill skill = null;
		foreach(FCSkill es in _skills)
		{
			if(es._skillName == skillName)
			{
				skill = es;
			}
		}
		return skill;
	}
	
	//for editor
	
	void ClearFirstAttack()
	{
		foreach(AttackBase ab in _attackList)
		{
			ab.IsFirstAttack = false;
		}
	}
	public void WriteInfoToFirstAttack()
	{
		ClearFirstAttack();
		if(_skillMaps != null && _skillMaps.Length >0)
		{
			for(int i = 0;i<_skillMaps.Length;i++)
			{
				FCSkillMap asm = _skillMaps[i];
				if(asm != null)
				{
					foreach(FCSkillConfig esc in asm._skillConfigs)
					{
						int counter = 0;
						foreach( FCSkill aslt in _skills )
						{
							if(aslt != null 
								&& aslt._attackConfigs != null 
								&& aslt._attackConfigs.Length >0 
								&& esc._skillName == aslt._skillName)
							{
								for(int z = 0;z < aslt._attackConfigs.Length;z++)
								{
									FCAttackConfig eac = aslt._attackConfigs[z];
									if(eac._attackConditions == null || eac._attackConditions.Length == 0)
									{
										eac._attackConditions = null;
									}
									int k = 0;
									foreach(AttackBase ab in _attackList)
									{
										if(eac._attackModuleName == ab._name)
										{
											if(z == 0)
											{
												ab.IsFirstAttack = true;
											}
											eac.AttackModule = ab;
											break;
										}
										k++;
									}
								}
							}
							counter++;
						}
					}
				}
			}
		}
		
		
		return;
	}

	public AttackBase GetAttack(int id,int aiLevel,int comboIdx)
	{
		if(_skillMaps == null || id<0 || id>= _skillMaps[aiLevel]._skillConfigs.Length)
		{
			return null;
		}
		return _skillMaps[aiLevel]._skillConfigs[id].SkillModule._attackConfigs[comboIdx].AttackModule;
	}
	
	public FCSkillConfig GetSkillConfig(int id,int aiLevel)
	{
		if(_skillMaps == null || id<0 || id>= _skillMaps[aiLevel]._skillConfigs.Length)
		{
			return null;
		}
		return _skillMaps[aiLevel]._skillConfigs[id];
	}
	
	public AttackBase GetAttack(string attName)
	{
		int hc = attName.GetHashCode();
		AttackBase ret = null;
		foreach(AttackBase ab in _attackList)
		{
			if(hc == ab.NameHashCode)
			{
				ret = ab;
				break;
			}
		}
		return ret;
	}
	
	public void SetAttackCombo(int attackID,int aiLevel,int comboIdx)
	{
		if(_skillMaps == null || attackID<0 || attackID>= _skillMaps[aiLevel]._skillConfigs.Length)
		{
			return;
		}
		Assertion.Check(comboIdx<_skillMaps[aiLevel]._skillConfigs[attackID].ComboHitMax);
		
		_skillMaps[aiLevel]._skillConfigs[attackID].ComboHitValue = comboIdx;
	}
	
	public AttackConditions[] GetAttackConditions(int attackID,int aiLevel,int comboIdx)
	{
		return _skillMaps[aiLevel]._skillConfigs[attackID].SkillModule._attackConfigs[comboIdx]._attackConditions;
	}
	
	public bool IsFinalAttack(int attackID, int aiLevel)
	{
		if(_skillMaps == null || attackID<0 || attackID>= _skillMaps[aiLevel]._skillConfigs.Length)
		{
			return true;
		}
		if(_skillMaps[aiLevel]._skillConfigs[attackID].ComboHitValue 
			< _skillMaps[aiLevel]._skillConfigs[attackID].ComboHitMax-1)
		{
			return false;
		}
		else
		{
			return true;
		}
	}
	public int IncreaseAttackCombo(int attackID,int aiLevel)
	{
		if(_skillMaps == null || attackID<0 || attackID>= _skillMaps[aiLevel]._skillConfigs.Length)
		{
			return -1;
		}

		if(_skillMaps[aiLevel]._skillConfigs[attackID].ComboHitValue 
			< _skillMaps[aiLevel]._skillConfigs[attackID].ComboHitMax-1)
		{
			_skillMaps[aiLevel]._skillConfigs[attackID].ComboHitValue++;
		}
		else
		{
			_skillMaps[aiLevel]._skillConfigs[attackID].ComboHitValue = 0;
		}
		if(_skillMaps[aiLevel]._skillConfigs[attackID].IsInLoop || _owner.IsPlayerSelf)
		{
			return _skillMaps[aiLevel]._skillConfigs[attackID].ComboHitValue;
		}
		else if(_skillMaps[aiLevel]._skillConfigs[attackID].ComboHitValue >0)
		{
			return _skillMaps[aiLevel]._skillConfigs[attackID].ComboHitValue;
		}
		//if( _skillMaps[id])
		return -1;
	}
	
	public int GetAttackCombo(int attackID,int aiLevel)
	{
		if(_skillMaps == null || attackID<0 || attackID>= _skillMaps[aiLevel]._skillConfigs.Length)
		{
			return -1;
		}
		return _skillMaps[aiLevel]._skillConfigs[attackID].ComboHitValue;
	}
	
	public void ClearAttackCombo(int attackID,int aiLevel)
	{
		_skillMaps[aiLevel]._skillConfigs[attackID].ComboHitValue = 0;
	}
	
	public void StopAttack()
	{
		
	}
	
	
	#endregion
}
