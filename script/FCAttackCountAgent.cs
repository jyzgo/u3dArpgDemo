using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("FC/Logic/FCObject/PlayAndEnemy/Agent/AI/FCAttackCountAgent")]
public class FCAttackCountAgent : MonoBehaviour , FCAgent{
	
	AIAgent _owner;
	protected bool _inState = false;
	protected bool _inPause = false;
	protected int _currentSkillID = -1;
	protected int _nextSkillID = -1;
	protected int _comboCount;
	private bool _isOver = true;
	
	protected int _attackLevel = 0;
	
	public int AttackLevel
	{
		get
		{
			return _attackLevel;
		}
		set
		{
			_attackLevel = value;
		}
	}
	
	public class AttackComboInfoTable
	{
		public float _distanceMinSqrt;
		public float _distanceMaxSqrt;
		// weight for same kind ,such as we need use this to choose a skill attack from skill attack sets
		public int _weight;
		// weight 
		public FC_COMBO_KIND _kind;
	}
	
	protected AttackAgent _attackAgent = null;
	
	// if >= 0 ,means monster want to use the skill next attack
	protected int _skillIDWantToUse = -1;
	
	// if false, means attack at the origon playce
	protected bool _needDetectDistance = true;
	
	public int SkillIDWantToUse
	{
		get
		{
			return _skillIDWantToUse;
		}
		set
		{
			_skillIDWantToUse = value;
		}
	}
	
	public bool NeedDetectDistance
	{
		get
		{
			return _needDetectDistance;
		}
		set
		{
			_needDetectDistance = value;
		}
	}
	
	public int CurrentSkillID
	{
		get
		{
			return _currentSkillID;
		}
		set
		{
			_currentSkillID = value;
		}
	}
	
	public FCSkillConfig CurrentSkill
	{
		get
		{
			if(_currentSkillID < 0 || _currentSkillID >= _attackAgent._skillMaps[_attackLevel]._skillConfigs.Length)
			{
				return null;
			}
			return _attackAgent._skillMaps[_attackLevel]._skillConfigs[_currentSkillID];
		}
	}
	
	public int NextCurSkillComboID
	{
		get
		{
			if(_currentSkillID < 0 || _currentSkillID >= _attackAgent._skillMaps[_attackLevel]._skillConfigs.Length)
			{
				return -1;
			}
			return _attackAgent._skillMaps[_attackLevel]._skillConfigs[_currentSkillID].NextComboHitValue;
		}

	}
	
	public int NextSkillID
	{
		get
		{
			return _nextSkillID;
		}
		set
		{
			_nextSkillID = value;
		}
	}
	
	public int ComboCount
	{
		get
		{
			return _comboCount;
		}
		set
		{
			_comboCount = value;
		}
	}
	
	public void Init(FCObject owner)
	{
		_owner = owner as AIAgent;
		_inState = false;
		_isOver = true;
		_currentSkillID = -1;
		_nextSkillID = -1;
		_attackAgent = _owner.ACOwner.ACGetAttackAgent();
	}
	
	//next attack of monster ,will use the skill with the name
	public int SetNextSkill(string skillName, int comboValue, bool rightNow)
	{
		int i = 0;
		_nextSkillID = -1;
		_skillIDWantToUse = -1;
		foreach(FCSkillConfig esc in _attackAgent._skillMaps[_attackLevel]._skillConfigs)
		{
			if(esc._skillName == skillName)
			{
				if(!rightNow)
				{
					_skillIDWantToUse = i;
				}
				else
				{
					_nextSkillID = i;
				}
				break;
			}
			i++;
		}
		if(comboValue >0 && (_skillIDWantToUse >=0|| _nextSkillID>=0))
		{
			int sid = _skillIDWantToUse;
			if(sid<0)
			{
				sid = _nextSkillID;
			}
			_attackAgent._skillMaps[_attackLevel]._skillConfigs[sid].ComboHitValue = comboValue;
			_attackAgent._skillMaps[_attackLevel]._skillConfigs[sid].WithSpecCombo = true;
		}
		return _skillIDWantToUse;
	}
	
	public void ContinuePreSkill()
	{
		_nextSkillID = -1;
		_skillIDWantToUse = _currentSkillID;
		RecoverySkill(_skillIDWantToUse);
	}
	//if skillCD > 0 ,should make skill in cd ,if true means disable key press
	public void SetSkillINCoolDown(int skillIdx, bool toEnabled)
	{
		if(!toEnabled)
		{
			_attackAgent._skillMaps[_attackLevel]._skillConfigs[skillIdx].CoolDownTime = _attackAgent._skillMaps[_attackLevel]._skillConfigs[skillIdx].SkillModule._coolDownTimeMax;
		}
		if((!toEnabled && !_attackAgent._skillMaps[_attackLevel]._skillConfigs[skillIdx].SkillIsCD ) || toEnabled)
		{	
			if(_owner.ACOwner.IsPlayerSelf)
			{
				_owner.KeyAgent.SetKeyEnabledBySkillID(skillIdx, toEnabled);
			}
		}

	}
	
	public bool SkillIsInMap(string skillName)
	{
		bool ret = false;
		foreach(FCSkillConfig esc in _attackAgent._skillMaps[_attackLevel]._skillConfigs)
		{
			if(esc._skillName == skillName)
			{
				ret = true;
				break;
			}
		}
		return ret;
	}
	
	public void RecoverySkill(int skillIdx)
	{
		if(skillIdx >= 0)
		{
			_attackAgent._skillMaps[_attackLevel]._skillConfigs[skillIdx].CoolDownTime = 0;
		}
	}
	
	public void GetRageSkills(ref List<FCSkillConfig> skillsList)
	{
		foreach(FCSkillConfig esc in _attackAgent._skillMaps[_attackLevel]._skillConfigs)
		{
			if(esc._isRageSkill)
			{
				skillsList.Add(esc);
			}
		}
	}
	public FCSkillConfig GetSkillAt(Vector3 position)
	{
		FCSkillConfig ret = null;
		if(position.y <= 2)
		{
			foreach(FCSkillConfig esc in _attackAgent._skillMaps[_attackLevel]._skillConfigs)
			{
				if(position.sqrMagnitude > esc._distanceMinSqrt)
				{
					ret = esc;
					break;
				}
			}
		}
		return ret;

	}

	//if true, means some skill of owner is ready to use
	public bool CanUseSkill()
	{
		bool ret = false;
		foreach(FCSkillConfig esc in _attackAgent._skillMaps[_attackLevel]._skillConfigs)
		{
			if(esc.SkillIsCD && esc.EnergyCost <= _owner.ACOwner.Energy)
			{
				ret = true;
				break;
			}
		}
		return ret;
	}
	public FCSkillConfig GetSkillNear(Vector3 position)
	{
		FCSkillConfig ret = _attackAgent._skillMaps[_attackLevel]._skillConfigs[0];
		if(position.y <= 2)
		{
			foreach(FCSkillConfig esc in _attackAgent._skillMaps[_attackLevel]._skillConfigs)
			{
				if(position.sqrMagnitude > esc._distanceMinSqrt)
				{
					ret = esc;
					break;
				}
			}
		}
		return ret;

	}
	/// <summary>
	/// Compares the two attack priority.
	/// </summary>
	/// <returns>
	/// if a1 >= a2 ,return true;
	/// </returns>
	/// <param name='a1'>
	/// </param>
	/// <param name='a2'>
	/// </param>/
	public bool CompareTwoAttackPriority(int a1,int a2)
	{
		return _attackAgent._skillMaps[_attackLevel]._skillConfigs[a1].SkillModule._priority
			< _attackAgent._skillMaps[_attackLevel]._skillConfigs[a2].SkillModule._priority;
	}
	public void Run()
	{
		_inPause = false;
		if(_isOver)
		{
			_inState = true;
			StartCoroutine(STATE());
		}
	}
	
	public int GetRageSkillID(Vector3 v3,List<FCSkillConfig> aslis)
	{
		v3.y -= _owner.ACOwner.BaseFlyHeight;
		if(v3.y<2 && v3.y> -2)
		{
			float disSqrtFromSelfToPlayer = v3.sqrMagnitude;
			v3.y = 0;

			int ret = -1;
            //int weight  = -1;
            //int priority = -1;
			int defaultAttack = -1;

            int attackWeight = 0;//累计权重的总数
            List<int> startWeightList = new List<int>();//每个技能权重的起始数值

            for (int targetID = 0; targetID < aslis.Count; targetID++)
            {
                attackWeight += aslis[targetID]._attackWeight;
                startWeightList.Add(attackWeight - 1);
            }

            List<int> attackIndexList = new List<int>();

			for(int targetID = 0; targetID < aslis.Count; targetID++)				
			{
				float disSqrtMax = aslis[targetID]._distanceMaxSqrt;
				float disSqrtMin = aslis[targetID]._distanceMinSqrt;

				if(aslis[targetID]._attackWeight >= 0 
					&& disSqrtFromSelfToPlayer <= disSqrtMax 
					&& aslis[targetID].SkillIsCD && aslis[targetID].EnergyCost <= _owner.ACOwner.Energy)
				{
					if(disSqrtFromSelfToPlayer >=disSqrtMin)
					{
                        attackIndexList.Add(targetID);

                        //int weightTemp = Random.Range(0, aslis[targetID]._attackWeight);
                        //if(weightTemp > weight && aslis[targetID].SkillModule._priority >= priority)
                        //{
                        //    ret = targetID;
                        //    weight = weightTemp;
                        //    priority = aslis[targetID].SkillModule._priority;
                        //}
					}
					else
					{
						if(aslis[targetID]._isDefaultSkill)
						{
							defaultAttack = targetID;
						}
					}

				}
			}

            if (defaultAttack < 0)
            {
                if (null == attackIndexList)
                {
                    return -1;
                }

                if (attackIndexList.Count > 1)
                {
                    while (ret == -1)
                    {
                        foreach (int index in attackIndexList)
                        {
                            int weightTemp = Random.Range(0, attackWeight);
                            int startWeight = 0;

                            if (index >= 1)
                            {
                                startWeight = startWeightList[index - 1];
                            }

                            if (startWeight < weightTemp && weightTemp  <= startWeightList[index])
                            {
                                ret = index;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    ret = attackIndexList[0];
                }
            }





			if(ret >=0)
			{
				return ret;
			}
			else
			{
				if(defaultAttack >=0)
				{
					return defaultAttack;
				}
			}
		}
		return -1;
	}
	public int GetSkillID(Vector3 v3,FCSkillConfig[] aslis)
	{	
		v3.y -= _owner.ACOwner.BaseFlyHeight;
		if(v3.y<2 && v3.y> -2)
		{
			float disSqrtFromSelfToPlayer = v3.sqrMagnitude;
			v3.y = 0;
			int targetID = _skillIDWantToUse;
			if(targetID != -1 && aslis[targetID].SkillIsCD)
			{
				targetID = _skillIDWantToUse;
				float disSqrtMax = aslis[targetID]._distanceMaxSqrt;
				float disSqrtMin = aslis[targetID]._distanceMinSqrt;
				
				if((disSqrtFromSelfToPlayer <= disSqrtMax && disSqrtFromSelfToPlayer >=disSqrtMin) || !_needDetectDistance)
				{
					_skillIDWantToUse = -1;
					_needDetectDistance = true;
					return targetID;
					
				}
			}
			else
			{
				int ret = -1;
                //int weight  = -1;
				int priority = -1;
				int defaultAttack = -1;

                List<int> beAbleToUseAttIndexList = new List<int>();

				for(targetID = 0; targetID < aslis.Length; targetID++)				
				{
					float disSqrtMax = aslis[targetID]._distanceMaxSqrt;
					float disSqrtMin = aslis[targetID]._distanceMinSqrt;
					if(
						!aslis[targetID]._isRageSkill &&
						aslis[targetID]._attackWeight >= 0 
						&& disSqrtFromSelfToPlayer <= disSqrtMax 
						&& aslis[targetID].SkillIsCD && aslis[targetID].EnergyCost <= _owner.ACOwner.Energy)
					{
						if(disSqrtFromSelfToPlayer >=disSqrtMin)
						{
                            if (aslis[targetID].SkillModule._priority >= priority)
                            {
                                priority = aslis[targetID].SkillModule._priority;
                                beAbleToUseAttIndexList.Add(targetID);
                            }
                                                       
                            //int weightTemp = Random.Range(0, aslis[targetID]._attackWeight);
                            //if(weightTemp > weight && aslis[targetID].SkillModule._priority >= priority)
                            //{
                            //    ret = targetID;
                            //    weight = weightTemp;
                            //    priority = aslis[targetID].SkillModule._priority;
                            //}
						}
						else
						{
							if(aslis[targetID]._isDefaultSkill)
							{
								defaultAttack = targetID;
							}
						}

					}
				}

                if (null != beAbleToUseAttIndexList && beAbleToUseAttIndexList.Count > 0)
                {
                    int allWeight = 0;
                    Dictionary<int, int> attackMaxWeight = new Dictionary<int, int>();

                    for (targetID = 0; targetID < beAbleToUseAttIndexList.Count; targetID++)
                    {
                        int index = beAbleToUseAttIndexList[targetID];

                        allWeight += aslis[index]._attackWeight;
                        attackMaxWeight.Add(index, allWeight - 1);
                    }

                    int weightTemp = Random.Range(0, allWeight);
                    int startWeight = -1;

                    foreach (KeyValuePair<int,int> preWeight in attackMaxWeight)
                    {
                        if (weightTemp > startWeight && weightTemp <= preWeight.Value)
                        {
                            ret = preWeight.Key;
                            startWeight = preWeight.Value;
                        }
                    }

                }
                
                if(ret >=0)
				{
					return ret;
				}
				else
				{
					if(defaultAttack >=0)
					{
						return defaultAttack;
					}
				}
			}
		}
		return -1;
	}
	IEnumerator STATE()
	{
		_isOver = false;
		while(_inState)
		{
			if(!_inPause && _owner.TargetAC != null)
			{
				if(_owner.TargetAC.IsAlived)
				{
					int result = GetSkillID(_owner.ACOwner.ThisTransform.localPosition - _owner.TargetAC.ThisTransform.localPosition,_attackAgent._skillMaps[_attackLevel]._skillConfigs);
					if(result >=0)
					{
						//get right skill , and set current skill to result
						_nextSkillID = result;
						_owner.HandleInnerCmd(FCCommand.CMD.TARGET_IN_ATTACK_DISTANCE,result);
					}
				}

			}
			yield return new WaitForSeconds(0.1f);
		}
		_isOver = true;
	}
	
	public void UpdateSkill()
	{
		int i =0;
		foreach(FCSkillConfig esc in _attackAgent._skillMaps[_attackLevel]._skillConfigs)
		{
            if (!esc.SkillIsCD)
            {
                esc.CoolDownTime -= Time.deltaTime;

                if (esc.SkillIsCD)
                {
                    SetSkillINCoolDown(i, true);
                }
            }
            else
            {
                if (esc._keyBind == FC_KEY_BIND.ATTACK_5)
                {
                    _owner.KeyAgent.SetKeyEnabledBySkillID(i, _owner.ACOwner.SkillGodDownActive ? true : false);
                }
            }
			i++;
		}
	}
	
	public bool SkillIsCD(int skillID)
	{
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        if (CheatManager.noCDTime)
        {
            return true;
        }
#endif
		return _attackAgent._skillMaps[_attackLevel]._skillConfigs[skillID].SkillIsCD;
	}
	
	public bool SkillIsCD(string skillName)
	{
		foreach(FCSkillConfig esc in _attackAgent._skillMaps[_attackLevel]._skillConfigs)
		{
			if(esc._skillName == skillName)
			{
				return esc.SkillIsCD;
			}
		}
		return false;
	}
	
	public float GetSkillCDTimeLast(int skillID)
	{
		return _attackAgent._skillMaps[_attackLevel]._skillConfigs[skillID].CoolDownTime;
	}
	
	public FCSkillConfig GetSkill(int skillID)
	{
		return _attackAgent._skillMaps[_attackLevel]._skillConfigs[skillID];
	}
	public float GetSkillCDTimeLastPercent(int skillID)
	{
		if(_attackAgent._skillMaps[_attackLevel]._skillConfigs[skillID].SkillModule._coolDownTimeMax <= 0.1f)
		{
			return 0;
		}
		else
		{
			return _attackAgent._skillMaps[_attackLevel]._skillConfigs[skillID].CoolDownTime
				/_attackAgent._skillMaps[_attackLevel]._skillConfigs[skillID].SkillModule._coolDownTimeMax;
		}

	}
	public void StopRun()
	{
		_inState = false;
	}
	
	public void Pause(bool ret)
	{
		_inPause = ret;
	}
}
