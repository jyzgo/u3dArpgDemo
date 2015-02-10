using UnityEngine;
using System.Collections;

public class AINormalDwarf : AiNormalMonster
{
	public float _changeGravity = 0.5f;
	private bool _hasLvlChanged = false;
	private ENUM_ATTACK_MODE _currentAttackMode = ENUM_ATTACK_MODE.ATTACK_NORMAL_ONE_FRIST;

	protected enum ENUM_ATTACK_MODE
	{
		ATTACK_NORMAL_ONE_FRIST,
		ATTACK_NORMAL_ONE_SECOND,
		ATTACK_NORMAL_TWO,
		ATTACK_NORMAL_THIRD,
		ATTACK_NORMAL_SKILL
	}

	public override void IdleTaskChange(FCCommand.CMD cmd)
	{
		if(cmd == FCCommand.CMD.STATE_QUIT)
		{	
			_owner.ACSetMoveMode(MoveAgent.MOVE_MODE.BY_ANIMATION);
		}
		else if(cmd == FCCommand.CMD.STATE_ENTER)
		{
			_owner.ACSetMoveMode(MoveAgent.MOVE_MODE.BY_MOVE_MANUAL);
			FaceToTarget(_targetAC);
			HandleInnerCmd(FCCommand.CMD.DIRECTION_FOLLOW_FORWARD,null);
			if(_state._preState.CurrentStateID == AIAgent.STATE.ATTACK)
			{
				_awayFromTarget = AWAY_MODE.AWAY_AFTER_ATTACK;
				_timeForIdleThisTime = 0.1f;
			}
			else if(_state._preState.CurrentStateID == AIAgent.STATE.HURT)
			{
				_awayFromTarget = AWAY_MODE.AWAY_AFTER_HURT;
				_timeForIdleThisTime = 0.3f;
			}
			else
			{
				_awayFromTarget = AWAY_MODE.NONE;
				_timeForIdleThisTime = 0;
			}
		}
		else if(cmd == FCCommand.CMD.STATE_UPDATE)
		{
		}
		else if(cmd == FCCommand.CMD.STATE_DONE)
		{
			SetNextState(AIAgent.STATE.HESITATE);
		}
	}

	public override void HesitateTaskChange(FCCommand.CMD cmd)
	{
		if(cmd == FCCommand.CMD.STATE_ENTER)
		{
			_timeTotalForWander = 0;
			_dangerousRaduisSqrt = _dangerousRaduis*_dangerousRaduis;
			_wanderSpeed = _owner.Data.TotalMoveSpeed * _wanderSpeedPercents;
			_hDirection = HES_DIRECTION.NONE;
			_realSafeDistance = _safeDistance+_bodyRadius+_targetAC.BodyRadius;
			_safeDistanceSqrt = _realSafeDistance * _realSafeDistance;
			_owner.ACSetMoveMode(MoveAgent.MOVE_MODE.BY_MOVE_MANUAL);
			_wantToAttack = false;
            _timeForWander = GetHesitateTime();//Random.Range(_timeForWanderMin,_timeForWanderMax);
			//CountTargetToSetWay();
			float ret = Random.Range(0,1f);
			if(_awayFromTarget == AWAY_MODE.AWAY_AFTER_HURT)
			{
				if(RageIsFull)
				{
					if(CanAttackOthers)
					{
						GoToAttack();
					}
					else
					{
						if(!_haveActionTicket)
						{
							FCTicketManager.Instance.ApplyTicket(_owner);
						}
					}
				}
				else if(ret<_chanceToAwayAfterHurt)
				{
					SetNextSeekMode(SEEK_MODE.RUN_AWAY_AFTER_HURT);
				}
				else
				{
					SetNextSeekMode(SEEK_MODE.WANDER_TO_PLAYER);
				}
			}
			else if(_awayFromTarget == AWAY_MODE.AWAY_AFTER_ATTACK)
			{
				//have chance to hit again
				//or run away
				if(ret < _chanceToAwayAfterAttack)
				{
					SetNextSeekMode(SEEK_MODE.RUN_AWAY_AFTER_ATTACK);
				}
				else
				{
					GoToAttack();
				}
			}
			else
			{
				//
				FC_DANGER_LEVEL dl = GetTargetDangerLevel(3,8);
				if(dl == FC_DANGER_LEVEL.SAFE)
				{
					_hesitateJitter = Random.Range(_hesitateJitterMin,_hesitateJitterMax);
					SetNextSeekMode(SEEK_MODE.WANDER_TO_PLAYER);
				}
				else if(dl == FC_DANGER_LEVEL.DANGER)
				{
					SetNextSeekMode(SEEK_MODE.RUN_AWAY_AFTER_ATTACK);
					//SetNextSeekMode(SEEK_MODE.RUN_AWAY_AFTER_ATTACK);
				}
				else
				{
					
					SetNextSeekMode(SEEK_MODE.RUN_AWAY_AFTER_HURT);
				}
				
			}
			if(_usedForFly)
			{
				_owner.MoveFollowForward = true;
				_owner.CurrentAngleSpeed = _angleSpeedForFlyWander;
			}
			ChangeToSeekMode();
		}
		else if(cmd == FCCommand.CMD.STATE_UPDATE)
		{
			UpdateHesitate();
		}
		else if(cmd == FCCommand.CMD.STATE_QUIT)
		{
			StopAllCoroutines();
			_owner.CurrentAngleSpeed = _owner.Data.angleSpeed;
			_owner.ACAniSpeedRecoverToNormal();
			_owner.ACRestoreToDefaultSpeed();
			_owner.SetShipForSpeedAndForward(MoveAgent.ROTATE_FLAG.FOLLOW_SPEED);
			//_owner.MoveFollowForward = false;
		}
	}


    private float GetIdleTime()
    {
        float tempTime = 0;

        if (_hasLvlChanged)
        {
        }
        else
        {
            tempTime = 0.1f;
        }


        return tempTime;
    }


	private float GetHesitateTime()
	{
		float intervalTime = 0;
		if (_hasLvlChanged)
		{
			switch(_currentAttackMode)
			{
			case ENUM_ATTACK_MODE.ATTACK_NORMAL_THIRD:
				_currentAttackMode = ENUM_ATTACK_MODE.ATTACK_NORMAL_SKILL;
				intervalTime = 1.0f;
				break;
			case ENUM_ATTACK_MODE.ATTACK_NORMAL_SKILL:
				_currentAttackMode = ENUM_ATTACK_MODE.ATTACK_NORMAL_THIRD;
				intervalTime = 1.0f;
				break;
			}
		}
		else
		{ 
			switch(_currentAttackMode)
			{
			case ENUM_ATTACK_MODE.ATTACK_NORMAL_ONE_FRIST:
				_currentAttackMode = ENUM_ATTACK_MODE.ATTACK_NORMAL_ONE_SECOND;
				intervalTime = 1.0f;
				break;
			case ENUM_ATTACK_MODE.ATTACK_NORMAL_ONE_SECOND:
				_currentAttackMode = ENUM_ATTACK_MODE.ATTACK_NORMAL_TWO;
				intervalTime = 1.5f;
				break;
			case ENUM_ATTACK_MODE.ATTACK_NORMAL_TWO:
				_currentAttackMode = ENUM_ATTACK_MODE.ATTACK_NORMAL_ONE_FRIST;
				intervalTime = 1.0F;
				break;
			}
		}
		
		return intervalTime;
	}




	public override void HpIsChanged(int changeValue)
	{
		base.HpIsChanged(changeValue);
		if(changeValue >0)
		{
			_damageAbsorbCurrent += changeValue;
		}
	}
	
	
	protected override void UpdateLevelUpConditions()
	{
		base.UpdateLevelUpConditions();
		
//		int aiLevel = _aiLevel;
		if(_aiLevel >=_owner.ACGetAttackAgent()._skillMaps.Length-1
		   || !_owner.IsAlived)
		{
			return;
		}
		
		if (!_hasLvlChanged && (float)_damageAbsorbCurrent / _owner.Data.TotalHp >= _changeGravity)
		{
			_hasLvlChanged = true;
			_currentAttackMode = ENUM_ATTACK_MODE.ATTACK_NORMAL_THIRD;
			ChangeAILevel(1);
			_attackCountAgent.AttackLevel = _aiLevel;
			SetNextState(AIAgent.STATE.IDLE,true);
		}
	}
	
}
