using UnityEngine;
using System.Collections;

public class AttackHexagrare : AttackBase
{
    public FC_GLOBAL_EFFECT hexagrareEffect = FC_GLOBAL_EFFECT.HEXAGRATE_START;
    private bool _effectNeedRemove = false;

    public override void Init(FCObject owner)
    {
        base.Init(owner);

        _owner.ACOwner._onACDead += OnACOwnerDead;
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
        BeginEffect();
        _shouldGotoNextHit = false;
        _effectNeedRemove = false;
    }

    protected void GotoFinalAttack()
    {
        _shouldGotoNextHit = true;
        _jumpToSkillEnd = true;
        AttackEnd();
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

    public override void AniBulletIsFire()
    {
        base.AniBulletIsFire();
    }

    public override void AttackUpdate()
    {
        base.AttackUpdate();

        if (_attackCanSwitch)
        {
            if (_isFirstAttack)
            {
                if (_currentPortIdx < 0)
                {
                    GotoFinalAttack();
                }
                else
                {
                    GotoAttackBySpecialType();
                }
            }
        }
    }

    private void BeginEffect()
    {
        Vector3 pos = _owner.ACOwner.ThisTransform.position;
        GlobalEffectManager.Instance.PlayEffect(hexagrareEffect, pos);
    }


    private void OnACOwnerDead(ActionController ac)
    {
        GlobalEffectManager.Instance.StopEffect(hexagrareEffect);
    }

}
