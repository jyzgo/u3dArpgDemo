using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class LEVEL_UP_CDI
{
	public float _hpUpValue;
	public float _timeUpValue = -1;
	public int _targetUpLevel;
	
	public bool _canWander;
	
	private float _timeCounter = 0; 
	
	//if true means monster can decrease ai level
	public bool _canGoBackByHP = false;
	public float _hpDownValue;
	public float _timeDownValue = -1;
	public int _targetDownLevel;
	public bool _needNewSkinColor = false;
	
	public float TimeCounter
	{
		get
		{
			return _timeCounter;
		}
		set
		{
			_timeCounter = value;
		}
	}
	

}

[System.Serializable]
public class StateToGoCondition
{
	public int _countsForJump = 1;
	public int _chanceToIdle = 50;
	public float _idleTime = 2f;
	public int _chanceToWander = 50;
	public float _wanderTime = 4f;
	public int _chanceToAttack = 0;
	
	static public int SortByName (StateToGoCondition a, StateToGoCondition b) { return b._countsForJump - a._countsForJump ; }
	
	public bool GetStateToGo(int stateCount,ref AIAgent.STATE ret,ref float timeIdle,ref float timeWander)
	{
		ret = AIAgent.STATE.NONE;
		if(_countsForJump == 0 || (_countsForJump > 0 && stateCount % _countsForJump == 0))
		{
			int c1 = 0;
			int c2 = 0;
			if(_chanceToIdle > 0)
			{
				ret = AIAgent.STATE.IDLE;
				c1 = Random.Range(0,_chanceToIdle);
			}
			if(_chanceToWander > 0)
			{
				if(ret != AIAgent.STATE.NONE)
				{
					c2 = Random.Range(0,_chanceToWander);
					if(c2 > c1)
					{
						ret = AIAgent.STATE.HESITATE;
						c1 = c2;
					}
				}
				else
				{
					ret = AIAgent.STATE.HESITATE;
				}
			}
			if(_chanceToAttack > 0)
			{
				if(ret != AIAgent.STATE.NONE)
				{
					c2 = Random.Range(0,_chanceToAttack);
					if(c2 > c1)
					{
						ret = AIAgent.STATE.ATTACK;
					}
				}
				else
				{
					ret = AIAgent.STATE.ATTACK;
				}
			}
		}
		if(ret == AIAgent.STATE.IDLE)
		{
			timeIdle = _idleTime;
			return true;
		}
		else if(ret == AIAgent.STATE.HESITATE)
		{
			timeWander = _wanderTime;
			return true;
		}
		return false;
	}
}
public class AiNormalBoss : AINormalWarrior {
	
	public LEVEL_UP_CDI[] _levelUpInfo = null;
	
	public string _skillAfterLevelUp = "";
	
	
	public bool _haveWanderAction = false;
	protected bool _haverWanderActionThisTime = false;
	protected float _wanderTimeThisTime = -1;
	public List<StateToGoCondition> _afterSkill;
	public List<StateToGoCondition> _afterHurt;
	
	public float _armorAddAfterRage = 0.25f;
	protected BossMessageReciever _bossCollider = null;
	
	public override void ActiveAI()
	{
		base.ActiveAI();
		if(_afterSkill != null && _afterSkill.Count != 0)
		{
			_afterSkill.Sort(StateToGoCondition.SortByName);
		}
		_bossCollider = _owner.GetComponent<BossMessageReciever>();
	}
	
	
	
	public void ChangeToAILevel(int aiLevel)
	{
		aiLevel = Mathf.Clamp(aiLevel, 0, _levelUpInfo.Length);
		
		if(_aiLevel< _levelUpInfo.Length && _aiLevel >=0)
		{
			_levelUpInfo[_aiLevel].TimeCounter = 0;
			if(aiLevel != _aiLevel)
			{
				_aiLevel = aiLevel;
				if(_levelUpInfo[_aiLevel-1]._needNewSkinColor)
				{
					_owner._avatarController.RageFlashColor(true);
				}
				else
				{
					_owner._avatarController.RageFlashColor(false);
				}
				//clear damage absorb
				_damageAbsorbCurrent = 0;
				//means go to level up action right now
				if(_haveLevelUpAction && _levelUpAtAnyTime)
				{
					_attackCountAgent.AttackLevel = _aiLevel;
					SetNextState(AIAgent.STATE.LEVEL_UP,true);
				}
				// level up action will be done if monster not in attack or in hurt
				// otherwise will do level up action until attack and hurt is over
				else if(_haveLevelUpAction 
					&& _state.CurrentStateID != AIAgent.STATE.ATTACK
					&&  _state.CurrentStateID != AIAgent.STATE.HURT)
				{
					_attackCountAgent.AttackLevel = _aiLevel;
					SetNextState(AIAgent.STATE.LEVEL_UP,true);
				}
				//if we change attack level at attack state, may cause error
				else if(_state.CurrentStateID != AIAgent.STATE.ATTACK)
				{
					_attackCountAgent.AttackLevel = _aiLevel;
				}
				if(_rageSystemIsActive)
				{
					_rageAgent.OnLevelUp();
				}
			}
		}
		
	}
	
	//if mosnter info fit with any condition, the ai level will be changed
	protected override void UpdateLevelUpConditions()
	{
		bool hasLvlChanged = false;
		int aiLevel = _aiLevel;
		if(_aiLevel > _levelUpInfo.Length -1
			|| !_owner.IsAlived)
		{
			//return;
		}
		else
		{
			LEVEL_UP_CDI luc =  _levelUpInfo[_aiLevel];
			
			luc.TimeCounter += Time.deltaTime;
			while(luc._hpUpValue >= _owner.HitPointPercents && aiLevel < _levelUpInfo.Length)
			{
				aiLevel++;
				hasLvlChanged = true;
			}
			if(!hasLvlChanged)
			{
				while(luc._hpDownValue < _owner.HitPointPercents && aiLevel >0)
				{
					aiLevel--;
					hasLvlChanged = true;
				}
			}
			if(!hasLvlChanged)
			{
				if(luc._timeUpValue >0 && luc.TimeCounter > luc._timeUpValue)
				{
					aiLevel++;
					hasLvlChanged = true;
				}
				else if(luc._timeDownValue >0 
					&& luc.TimeCounter > luc._timeDownValue)
				{
					aiLevel--;
					hasLvlChanged = true;
				}
			}
		}
		
		if(hasLvlChanged)
		{
			ChangeToAILevel(aiLevel);
		}
		else
		{
			if(_owner.IsSpawned)
			{
				if( (float)_damageAbsorbCurrent/_owner.Data.TotalHp > _damageValueToAction )
				{
					string skillName = _skillForCounter;
					float disSqrt = _distanceForCounterFar*_distanceForCounterFar;
					float disToTarget = 0;
					if(_targetAC != null)
					{
						disToTarget = (_owner.ThisTransform.localPosition - _targetAC.ThisTransform.localPosition).sqrMagnitude;
					}
					if(_skillsForCounterFar != null  && _skillsForCounterFar.Length > 0
						&& _skillsForCounterFar.Length > _attackCountAgent.AttackLevel
						&& disToTarget >= disSqrt)
					{
						skillName = _skillsForCounterFar[_attackCountAgent.AttackLevel];
					}
					else if(_skillsForCounter != null && _skillsForCounter.Length > _attackCountAgent.AttackLevel)
					{
						skillName = _skillsForCounter[_attackCountAgent.AttackLevel];
					}
					//if true, means damage is enough to make a counter
					if(skillName != "" 
						&& _owner.IsAlived
						&& _attackCountAgent.SkillIsInMap(skillName))
					{
						_damageAbsorbCurrent = 0;
						if(!_hasSuperArmor || (_hasSuperArmor && !_superArmor.ArmorIsBroken(FCConst.SUPER_ARMOR_LVL1)))
						{
							GoToAttackForce(skillName, -1);
						}
					}
					else
					{
						if(_rageSystemIsActive)
						{
							_rageAgent.UpdateTargetDistance(_targetAC);
						}
					}
				}
				else
				{
					if(_rageSystemIsActive)
					{
						_rageAgent.UpdateTargetDistance(_targetAC);
					}
				}
			}
		}
	}
	
	
	public override void LevelUpTaskChange(FCCommand.CMD cmd)
	{
		if(cmd == FCCommand.CMD.STATE_ENTER)
		{
            SetActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_SUPER_SAIYAJIN);
            SetActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_RIGIDBODY2);

			AttackBase ab = _owner.ACGetAttackByName("LevelUp");
			if(ab != null)
			{
				//single attackbase used for other state ,should not have attack conditions
				ab.AttCons = null;
			}
			if(_hasSuperArmor)
			{
				_superArmor.SetArmor(FCConst.SUPER_ARMOR_LVL1, _armorAddAfterRage);
			}
			_currentAttack = ab;
			ab.Init(this);
			ab.AttackEnter();
			AniEventAniIsOver = _currentAttack.AniIsOver;
		}
		else if(cmd == FCCommand.CMD.STATE_UPDATE)
		{
			if(_currentAttack != null)
			{
				_currentAttack.AttackUpdate();
			}
		}
		else if(cmd == FCCommand.CMD.STATE_DONE)
		{
			GoToAttack();
		}
		else if(cmd == FCCommand.CMD.STATE_QUIT)
		{
			_currentAttack.AttackQuit();
			_currentAttack = null;
			AniEventAniIsOver = AniIsOver;
			ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_SUPER_SAIYAJIN);
			ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_RIGIDBODY2);
		}
	}
	

	
	public override void Armor1BrokenTaskChange(FCCommand.CMD cmd)
	{
		if(cmd == FCCommand.CMD.STATE_ENTER)
		{
			_owner.ACStop();
			SetActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_RIGIDBODY2);
			_owner.ACSetMoveMode(MoveAgent.MOVE_MODE.BY_MOVE_MANUAL);
			_owner._avatarController.RimFlashColor(true);
			if(_haveHpBar)
			{
				_timeCounterSuperArmor1Break = 0;
				//Assertion.Check(UIManager.Instance.SetBossShieldInfo != null);
				//UIManager.Instance.SetBossShieldInfo((int)(_timeForSuperArmor1Break-_timeCounterSuperArmor1Break));
			}
		}
		else if(cmd == FCCommand.CMD.STATE_UPDATE)
		{
			_timeCounterSuperArmor1Break+=Time.deltaTime;
			if(_timeCounterSuperArmor1Break > _timeForSuperArmor1Break)
			{
				_timeCounterSuperArmor1Break = _timeForSuperArmor1Break;
			}
			if(_haveHpBar)
			{
				//UIManager.Instance.SetBossShieldInfo((int)(_timeForSuperArmor1Break-_timeCounterSuperArmor1Break));
			}
		}
		else if(cmd == FCCommand.CMD.STATE_QUIT)
		{
			_superArmor.Revive(FCConst.SUPER_ARMOR_LVL1);
			ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_RIGIDBODY2);
			_owner._avatarController.RimFlashColor(false);
			if(_haveHpBar)
			{
				//UIManager.Instance.SetBossShieldInfo(-1);
				//UIManager.Instance.SetBossShield(1);
			}
			_owner.ACSetMoveMode(MoveAgent.MOVE_MODE.BY_ANIMATION);
		}
		else if(cmd == FCCommand.CMD.STATE_DONE)
		{
			string skillName = _skillForAwake;
			if(_skillsForAwake != null && _skillsForAwake.Length > _attackCountAgent.AttackLevel)
			{
				skillName = _skillsForAwake[_attackCountAgent.AttackLevel];
			}
			if(skillName != ""
				&& _owner.IsAlived
				&& _attackCountAgent.SkillIsInMap(skillName))
			{
				GoToAttackForce(skillName, -1);
			}
			else
			{
				SetNextState(AIAgent.STATE.IDLE);
			}
		}
	}
	
	public override void Armor2BrokenTaskChange(FCCommand.CMD cmd)
	{
		if(cmd == FCCommand.CMD.STATE_ENTER)
		{
			_owner.ACStop();
			_owner.ACSetMoveMode(MoveAgent.MOVE_MODE.BY_MOVE_MANUAL);
			
		}
		if(cmd == FCCommand.CMD.STATE_DONE)
		{
			SetNextState(AIAgent.STATE.IDLE);
		}
	}
	

	public override void HpIsChanged(int changeValue)
	{
		base.HpIsChanged(changeValue);
		if(changeValue >0)
		{
			_damageAbsorbCurrent += changeValue;
			/*if(_haveHpBar && UIManager.Instance.SetBossShield != null)
			{
				UIManager.Instance.SetBossShield(_superArmor.GetArmorRemain(FC_CONST.SUPER_ARMOR_LVL1));
			}*/
			
		}
	}
	
	public override void ClearCollider()
	{
		_owner.ACSetColliderAsTrigger(true);
		_owner.ACSetRaduis(0);
		if(_bossCollider != null)
		{
			_bossCollider._beActive = false;
		}
	}
	
	public override void IdleTaskChange(FCCommand.CMD cmd)
	{
		if(cmd == FCCommand.CMD.STATE_QUIT)
		{	
			_owner.ACSetMoveMode(MoveAgent.MOVE_MODE.BY_ANIMATION);
		}
		else if(cmd == FCCommand.CMD.STATE_ENTER)
		{
			_wanderTimeThisTime = -1;
			_timeForIdleThisTime = Random.Range(_timeForIdleMin, _timeForIdleMax);
			//_owner.ACSetMoveMode(MoveAgent.MOVE_MODE.BY_MOVE_MANUAL);
			_haverWanderActionThisTime = _haveWanderAction;
			AIAgent.STATE aas = AIAgent.STATE.IDLE;
			if(_state._preState != null)
			{
				aas = _state._preState.CurrentStateID;
			}
			AIAgent.STATE ret = AIAgent.STATE.NONE;
			if(_state._preState.CurrentStateID == AIAgent.STATE.BORN)
			{
				ret = AIAgent.STATE.ATTACK;
			}
			else if(_skillCount > 0 && aas == AIAgent.STATE.ATTACK && _afterSkill != null && _afterSkill.Count > 0)
			{
				foreach(StateToGoCondition stgc in _afterSkill)
				{
					if(stgc.GetStateToGo(_skillCount, ref ret, ref _timeForIdleThisTime,ref _wanderTimeThisTime))
					{
						break;
					}
				}
			}
			if(ret == AIAgent.STATE.ATTACK)
			{
				GoToAttack();
			}
			else if(ret == AIAgent.STATE.IDLE)
			{
				_haverWanderActionThisTime = false;
			}
			_owner.ACSetMoveMode(MoveAgent.MOVE_MODE.BY_ANIMATION);
			HandleInnerCmd(FCCommand.CMD.DIRECTION_FOLLOW_FORWARD,null);
			if(!_owner.IsPlayer)
			{
				_owner.ACStop();
			}
		}
		else if(cmd == FCCommand.CMD.STATE_FINISH)
		{
		}
		else if(cmd == FCCommand.CMD.STATE_DONE)
		{
			if(_haverWanderActionThisTime)
			{
				SetNextState(AIAgent.STATE.HESITATE);
			}
			else
			{
				GoToAttack();
			}
		}
	}
	
		
	public override void HesitateTaskChange(FCCommand.CMD cmd)
	{
		if(cmd == FCCommand.CMD.STATE_ENTER)
		{
			if(_spActAgent != null && _haveWanderAction)
			{
				_spActAgent.StartToWanderAround(this,SpActionIsEnd,_wanderTimeThisTime);
			}
		}
		else if(cmd == FCCommand.CMD.STATE_UPDATE)
		{
			
		}
		else if(cmd == FCCommand.CMD.STATE_QUIT)
		{
			_spActAgent.StopWanderAround();
			_owner.ACRevertToDefalutMoveParams();
			GoToAttack();
		}
	}
	
	public void SpActionIsEnd(FC_SP_ACTION_LISTS sals,FC_SP_ACTION_CONDITONS sacs)
	{
		GoToAttack();
		_spActAgent._SpActionIsEnd = null;
	}
	
	protected override void UpdateComboTime()
	{
		if(_attackComboLastTime >0)
		{
			_attackComboLastTime -= Time.deltaTime;
			if(_attackComboLastTime <= 0)
			{
				if(_state.CurrentStateID != AIAgent.STATE.ATTACK && _state.CurrentStateID != AIAgent.STATE.HURT)
				{
					if(_owner.IsAlived)
					{
						_attackCountAgent.ContinuePreSkill();
						GoToAttack();
					}
					else
					{
						_attackCountAgent.CurrentSkillID = -1;
					}
				}
			}
		}
	}
	
}
