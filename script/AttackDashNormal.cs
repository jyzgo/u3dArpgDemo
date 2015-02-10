using UnityEngine;
using System.Collections;

[AddComponentMenu("FC/Logic/FCObject/PlayAndEnemy/Agent/AC/Attack/DashNorma")]
public class AttackDashNormal : AttackBase 
{

	public FC_CHARACTER_EFFECT _attackEffect =  FC_CHARACTER_EFFECT.INVALID;
	
	public override void Init(FCObject owner)
	{
		base.Init(owner);
	}

	public override void AttackEnter()
	{
		base.AttackEnter();
		_currentState = AttackBase.ATTACK_STATE.STEP_1;
        CharacterEffectManager.Instance.PlayEffect(_attackEffect, _owner.ACOwner._avatarController, -1);
	}
	
	public override void AttackUpdate()
	{
		base.AttackUpdate();
		
		if(_currentState == AttackBase.ATTACK_STATE.STEP_1)
		{
			if(_owner.ACOwner.AniGetAnimationNormalizedTime() > 0.95f)
			{
				_currentState = AttackBase.ATTACK_STATE.STEP_2;
				AttackEnd();
			}
		}
		
	}
	
	public override void IsHitTarget(ActionController ac,int sharpness)
	{
		base.IsHitTarget(ac,sharpness);
	}
	
	public override void AniBulletIsFire()
	{
		base.AniBulletIsFire();
	}

    public override void AttackEnd()
    {
        _attackCanSwitch = true;
        _shouldGotoNextHit = true;
        if (SkillData.CurrentLevelData.specialAttackType == 2)
        {
            _owner.ConditionValue = AttackConditions.CONDITION_VALUE.DASH_END2;
        }
        else
        {
            _owner.ConditionValue = AttackConditions.CONDITION_VALUE.DASH_END1;
        }
        base.AttackEnd();
    }
	
	public override void AttackQuit()
	{
		base.AttackQuit();
	}
	
	public override bool DirectionKeyEvent(Vector3 direction, bool isPress)
	{
		return true;	
	}
	
	public override bool IsStopAtPoint()
	{
		return true;
	}
	
	protected override bool AKEvent(FC_KEY_BIND ekb, bool isPress)
	{
		return true;
	}
	
}
