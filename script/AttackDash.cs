#define FAUSTCITY

using UnityEngine;
using System.Collections;

[AddComponentMenu("FC/Logic/FCObject/PlayAndEnemy/Agent/AC/Attack/Dash")]
public class AttackDash : AttackBase
{

    private float _dashDistance;
    public int _angleSpeed = 200;
    public float _accBeginPercent = 0.5f;
    public float _accSpeed = -100;
    public float _slowTimeSpeed = 0.3f;
    public float _slowRecoverSpeed = 1.5f;
    public int _dashTotalLength = 10;//use dash skill need displacement
    public float _timeToStopWhenHit = 0.05f;
    public float _timeToStopWhenHitCD = 0.08f;
    public float _dashSpeed = 50f;
    protected uint _startPosition = 0;
    protected float _currentSlowSpeed = 0;
    private float _speed = 0;

    public FC_CHARACTER_EFFECT _partcileEffect = FC_CHARACTER_EFFECT.DashEffect;


    protected float _addDashTime = 0;

    protected bool _endWhenHitTarget = false;

    public override void Init(FCObject owner)
    {
        _owner = owner as AIAgent;
    }

    protected override void AniOver()
    {
        base.AniOver();

        if (_isFirstAttack)
        {
            _attackCanSwitch = true;
        }
        else if (_isFinalAttack)
        {
            _shouldGotoNextHit = false;
            AttackEnd();
        }
        else
        {
            _attackCanSwitch = true;
        }
    }

    public override void AttackEnter()
    {
        base.AttackEnter();
        if (GameManager.Instance.IsPVPMode)
        {
            _owner.EotAgentSelf.ClearEot(Eot.EOT_TYPE.EOT_SPEED);
        }
        _endWhenHitTarget = false;
        if (_owner.ACOwner.IsClientPlayer)
        {
            _owner._updateAttackRotation = true;
        }
        //Time.timeScale = _slowTimeSpeed;
        //_currentSlowSpeed = _slowTimeSpeed;
        //_partcileEffect = _partcileEffect0;
        //_modleEffect = _modleEffect0;
        //if (SkillData.CurrentLevelData.effect == 1)
        //{
        //    _partcileEffect = _partcileEffect1;
        //    _modleEffect = _modleEffect1;
        //}
        //else if (SkillData.CurrentLevelData.effect == 2)
        //{
        //    _partcileEffect = _partcileEffect2;
        //    _modleEffect = _modleEffect2;
        //}
        if (_owner.ACOwner.IsClientPlayer)
        {
            _owner._updateAttackRotation = true;
        }
#if FAUSTCITY

        _startPosition = _owner.ACOwner.SelfMoveAgent._currentMoveLength;
        _speed = SkillData.CurrentLevelData.speed;
        _speed = _dashSpeed;

        _owner.ACOwner.ACSetMoveMode(MoveAgent.MOVE_MODE.BY_MOVE_MANUAL);

        Vector3 direction = _owner.ACOwner.ThisTransform.forward;
        direction.y = 0;

        if (_owner.TargetAC != null && !_owner.TargetAC.IsPlayerSelf)
        {
            direction = _owner.TargetAC.ThisTransform.localPosition - _owner.ACOwner.ThisTransform.localPosition;
            direction.y = 0;
            direction.Normalize();
        }

        _owner.MoveByDirection(direction, _speed, 10);
        _owner.HandleInnerCmd(FCCommand.CMD.DIRECTION_FOLLOW_FORWARD, null);
#else
		_startPosition = _owner.ACOwner.SelfMoveAgent._currentMoveLength;
		_speed = SkillData.CurrentLevelData._speed;
		_speed = _dashSpeed;
	
		_owner.ACOwner.ACSetMoveMode(MoveAgent.MOVE_MODE.BY_MOVE_MANUAL);
		Vector3 direction =  _owner.ACOwner.ThisTransform.forward;
		if(_owner.TargetAC != null && !_owner.TargetAC.IsPlayerSelf)
		{
			direction = _owner.TargetAC.ThisTransform.localPosition - _owner.ACOwner.ThisTransform.localPosition;
			direction.y =0;
			direction.Normalize();
		}
		direction.y = 0;
		_owner.MoveByDirection(direction,_speed,10);
		_owner.HandleInnerCmd(FCCommand.CMD.DIRECTION_FOLLOW_FORWARD,null);
#endif
        _currentState = AttackBase.ATTACK_STATE.STEP_1;

        CharacterEffectManager.Instance.PlayEffect(_partcileEffect, _owner.ACOwner._avatarController, -1);
        //CharacterEffectManager.Instance.PlayEffect(_modleEffect, _owner.ACOwner._avatarController, -1);

    }

    public override void AttackUpdate()
    {
        if (_currentState == AttackBase.ATTACK_STATE.STEP_1)
        {
            base.AttackUpdate();

            /*if(_currentSlowSpeed <1)
            {
                _currentSlowSpeed += Time.deltaTime * _slowRecoverSpeed;
                if(_currentSlowSpeed >=1)
                {
                    _currentSlowSpeed = 1;
                }
                //Time.timeScale = _currentSlowSpeed;
            }*/
            float animationPercent = _owner.ACOwner.AniGetAnimationNormalizedTime();

            /*if(animationPercent > _accBeginPercent)
            {
                _speed += _accSpeed * Time.deltaTime;
                if(_speed <0)
                {
                    _speed = 0;
                }
                _owner.ACOwner.CurrentSpeed = _speed;
            }*/

#if FAUSTCITY
            //if( animationPercent > 0.8f)
            //{
            //    AttackEnd();
            //}

            //Debug.Log("_owner.ACOwner.CurrentSpeed:"+ _owner.ACOwner.CurrentSpeed);
            if ((_owner.ACOwner.SelfMoveAgent._currentMoveLength - _startPosition) >= _dashTotalLength)
            {
                AttackEnd();
            }
#else
            //Debug.Log("_owner.ACOwner.CurrentSpeed:"+ _owner.ACOwner.CurrentSpeed);
            if((_owner.ACOwner.SelfMoveAgent._currentMoveLength - _startPosition) >= _dashTotalLength)
            {
                AttackEnd();
            }
#endif


        }

    }


    public override void IsHitTarget(ActionController ac, int sharpness)
    {
        if (_addDashTime <= 0)
        {
            _owner.ACOwner.SelfMoveAgent.PauseMove(true, _timeToStopWhenHit);
            _addDashTime = _timeToStopWhenHitCD;
            //_speed = 0;
            //_currentSlowSpeed = 0.95f;
        }
        base.IsHitTarget(ac, sharpness);
        if (_endWhenHitTarget)
        {
            _attackCanSwitch = true;
            _shouldGotoNextHit = true;
        }
    }


    protected override bool AKEvent(FC_KEY_BIND ekb, bool isPress)
    {
        return true;
    }


    public override void ClearEffect()
    {
        StopEffect();
    }


    private void StopEffect()
    {
        CharacterEffectManager.Instance.StopEffect(_partcileEffect, _owner.ACOwner._avatarController, -1);
        //CharacterEffectManager.Instance.StopEffect(_modleEffect, _owner.ACOwner._avatarController, -1);
    }


    protected void GotoAttackBySpecialType()
    {
        if (SkillData.CurrentLevelData.specialAttackType == 2)
        {
            _shouldGotoNextHit = true;
            _jumpToSkillEnd = true;
        }
        else
        {
            _shouldGotoNextHit = true;
            _jumpToSkillEnd = false;
        }

        AttackEnd();
    }

    public override void AttackEnd()
    {
        if (_currentState == AttackBase.ATTACK_STATE.STEP_1)
        {
            StopEffect();

            _currentState = AttackBase.ATTACK_STATE.STEP_2;
            _attackCanSwitch = true;
            _shouldGotoNextHit = true;

            //if (SkillData.CurrentLevelData.specialAttackType == 2)
            //{
            //    _owner.ConditionValue = AttackConditions.CONDITION_VALUE.DASH_END2;
            //}
            //else
            //{
            //    _owner.ConditionValue = AttackConditions.CONDITION_VALUE.DASH_END1;
            //}

            GotoAttackBySpecialType();

            base.AttackEnd();
        }

    }

    public override bool DirectionKeyEvent(Vector3 direction, bool isPress)
    {
        if (isPress)
        {
            _owner.ACOwner.ACMoveToDirection(ref direction, _angleSpeed);
            return true;
        }
        return true;
    }

    public override bool IsStopAtPoint()
    {
        return true;
    }

    public override void AttackQuit()
    {
        base.AttackQuit();
        _currentState = AttackBase.ATTACK_STATE.STEP_2;
        if (!GameManager.Instance.GamePaused)
        {
            Time.timeScale = GameSettings.Instance.TimeScale;
        }
        _addDashTime = 0;
        _owner.ACOwner.ACRevertToDefalutMoveParams();
    }

}
