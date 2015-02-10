using UnityEngine;
using System.Collections;

public class AttackShadowStrikeEnd : AttackNormal {
	
	public AttackShadowStrike _attackPre = null;
	public float _slowSpeed = 0.1f;
	
	protected bool _effectIsPlayed = false;
	public override void AttackEnter()
	{
		base.AttackEnter();
		_owner.SetActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_SUPER_SAIYAJIN);
		_owner.ACOwner.ACFire(FirePortIdx);
		_effectIsPlayed = false;
	}
	
	public override void AttackUpdate ()
	{
		base.AttackUpdate ();
		
	}
	
	public override void AniBulletIsFire ()
	{
		if(_currentPortIdx == 1)
		{
			_attackPre.BattleBaseEffect.ShowEffect(0);
			_attackPre.BattleBaseEffect.ShowStartEffect(false);
			_attackPre.BattleBaseEffect.ShowSpecialEndEffect(1);
			_owner.ACOwner.ACSlowDownTime(0.05f, _slowSpeed);
			_effectIsPlayed = true;
		}
		base.AniBulletIsFire ();
		
		
	}
	
	protected override bool AKEvent (FC_KEY_BIND ekb, bool isPress)
	{
		return base.AKEvent (ekb, isPress);
	}
	public override void AttackQuit ()
	{
		base.AttackQuit ();
		_owner.ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_SUPER_SAIYAJIN);
		if(!_effectIsPlayed)
		{
			_attackPre.BattleBaseEffect.ShowEffect(0);
			_attackPre.BattleBaseEffect.ShowStartEffect(false);
			_attackPre.BattleBaseEffect.ShowSpecialEndEffect(0);
		}
	}
	protected override void OverrideFirePort(RangerAgent.FirePort firePort, SkillData skillData) {
		//firePort._rangeInfo._param1 = skillData.CurrentLevelData._attackRange;
		firePort._rangeInfo._effectTime = skillData.CurrentLevelData.attackEffectTime;
		firePort.DamageScale = firePort.DamageScaleSource * skillData.CurrentLevelData.damageScale;
		//need not override fire count
		//firePort._fireCount = _skillData.CurrentLevelData._bulletEquotient;
		firePort.DotDamageTime = _skillData.CurrentLevelData.dotDamageTime;
		firePort._attribute1 = skillData.CurrentLevelData.attribute1;
		firePort._attribute2 = skillData.CurrentLevelData.attribute2;
		firePort.AttackCount = skillData.CurrentLevelData.attackNumber;
	}
}
