using UnityEngine;
using System.Collections;


public class AttackDashFight : AttackBase
{
    public override void Init(FCObject owner)
    {
        base.Init(owner);
    }

    protected override void AniOver()
    {
        if (_hitTargetCount > 0 && _endByHitTarget)
        {
            _endByAnimationOverThisTime = false;
        }
        if (_currentAnimationCount >= _animationCount)
        {
            AttackEnd();
        }
        else if (_currentAnimationCount >= _animationCount - 1)
        {
            //only effect to mosnter
            if (_owner.CurrentSkill != null && _owner.CurrentSkill.WithDefy && _isFinalAttack)
            {
                _attackCanSwitch = false;
                //if some attack shound end by animation over
                _endByAnimationOverThisTime = false;
            }
        }
    }

    public override void AttackEnter()
    {
        base.AttackEnter();
        _frameCount = 0;
        _currentState = AttackBase.ATTACK_STATE.STEP_1;
        if (_owner.CurrentSkill != null)
        {
            _owner.CurrentSkill.FinalAttackIsHitTarget = false;
        }
    }


    public override void AttackUpdate()
    {
        base.AttackUpdate();
        if (_currentState == AttackBase.ATTACK_STATE.STEP_1)
        {
            _attackLastTime -= Time.deltaTime;
            if (_shouldGotoNextHit && _attackCanSwitch)
            {
                if (_owner.CurrentSkill != null
                    && _currentBindKey == _owner.KeyAgent.ActiveKey
                    && _attackConditions != null
                    && _attackConditions.Length == 1
                    && _attackConditions[0]._attCon == AttackConditions.ATTACK_JUMP_CONDITIONS.TO_END
                    && _owner.ACOwner.IsPlayerSelf)
                {

                }
                else
                {
                    if (!_endByAnimationOverThisTime)
                    {
                        AttackEnd();
                    }
                }
            }
            else
            {
                if ((_attackLastTime <= 0 && !_shouldGotoNextHit && _cacheKeyPress) || _attackCanSwitch)
                {
                    if ((_currentBindKey == FC_KEY_BIND.NONE && !_owner.ACOwner.IsPlayer)
                        || (_currentBindKey != FC_KEY_BIND.NONE && _currentBindKey == _owner.KeyAgent.ActiveKey)
                        || (_currentBindKey == FC_KEY_BIND.NONE && _owner.ACOwner.IsPlayerSelf && (int)_owner.KeyAgent.ActiveKey > (int)FC_KEY_BIND.DIRECTION))
                    {
                        //Debug.Log("_shouldGotoNextHit");
                        _shouldGotoNextHit = true;
                        if (_currentBindKey == FC_KEY_BIND.NONE
                            && _owner.ACOwner.IsPlayerSelf
                            && (int)_owner.KeyAgent.ActiveKey > (int)FC_KEY_BIND.DIRECTION)
                        {
                            _owner.HandleKeyPress((int)_owner.KeyAgent.ActiveKey, Vector3.zero);
                        }
                        //_attackCanSwitch = true;
                    }
                }
            }
            if (_attackCanSwitch
                && (_currentBindKey != FC_KEY_BIND.NONE || _owner.ACOwner.IsPlayerSelf)
                && _owner.KeyAgent.ActiveKey == FC_KEY_BIND.DIRECTION)
            {
                AttackEnd();
            }
        }
    }

    public override void IsHitTarget(ActionController ac, int sharpness)
    {
        base.IsHitTarget(ac, sharpness);
        if (_owner.CurrentSkill != null && _isFinalAttack)
        {
            _owner.CurrentSkill.FinalAttackIsHitTarget = true;
        }
        if (_owner.CurrentSkill != null && _owner.CurrentSkill.WithDefy && _isFinalAttack)
        {
            _endByAnimationOverThisTime = false;
        }
    }

    protected override bool AKEvent(FC_KEY_BIND ekb, bool isPress)
    {
        return true;
    }

    public override void AniBulletIsFire()
    {
        base.AniBulletIsFire();
    }

    public override void AttackEnd()
    {
        if (SkillData.CurrentLevelData.specialAttackType == 2)
        {
            _jumpToSkillEnd = true;
        }
        else
        {
            _jumpToSkillEnd = false;
        }

        base.AttackEnd();
    }

    public override void AttackQuit()
    {
        base.AttackQuit();
    }

}
