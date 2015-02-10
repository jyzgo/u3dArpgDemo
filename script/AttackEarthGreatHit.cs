using UnityEngine;
using System.Collections;

public class AttackEarthGreatHit : AttackBase {
	
	public float _baseSkillPowerSkill = 0.4f;
	public float _skillPowerSkillPerHit = 0.1f;
	public Vector3[] _weaponScaleForHit;
	protected int _activeEffectIndex;
    //private BattleCharEffect _battleCharEffect = null;
	
	public BindEffectInfo _effectShowSword;
		
	public override void AttackEnter ()
	{
		base.AttackEnter ();
		_addDamageScale = _baseSkillPowerSkill 
			+ _owner.CurrentSkill.PublicIntValue * _skillPowerSkillPerHit;
		_activeEffectIndex = _owner.CurrentSkill.PublicIntValue;
		if(_activeEffectIndex >= _bindEffects.Length/2)
		{
			_activeEffectIndex = _bindEffects.Length/2-1;
			if(_activeEffectIndex <=0)
			{
				_activeEffectIndex = 0;
			}
		}
		_weaponScaleX = (int)_weaponScaleForHit[_activeEffectIndex].x;
		_weaponScaleY = (int)_weaponScaleForHit[_activeEffectIndex].y;
		_weaponScaleZ = (int)_weaponScaleForHit[_activeEffectIndex].z;
		/*_battleCharEffect = _owner.CurrentSkill.PublicObjectValue as BattleCharEffect;
		if(!_isFinalAttack)
		{
			_battleCharEffect.ShowEffect(0);
			_battleCharEffect.ShowStartEffect(false);
			_battleCharEffect.ShowSpecialEndEffect(1);
		}*/
		_effectShowSword.IsPlaying = false;
		_effectShowSword.HasPlayed = false;
	}
	
	protected override void AniOver ()
	{
		base.AniOver ();
		_shouldGotoNextHit = false;
		AttackEnd();
	}
	protected override bool AKEvent (FC_KEY_BIND ekb, bool isPress)
	{
		if(ekb > FC_KEY_BIND.DIRECTION && isPress)
		{
			ekb = _currentBindKey;
		}
		else
		{
			base.AKEvent(ekb,isPress);
		}
		return true;
	}
	
	public override void AniBulletIsFire ()
	{
		base.AniBulletIsFire ();
	}
	
	public override void AttackUpdate ()
	{
		base.AttackUpdate ();

	}
	
	public override void AttackQuit ()
	{
		base.AttackQuit ();
		if (_effectShowSword.IsPlaying)
		{
			//is playing, stop it?
			CharacterEffectManager.Instance.StopEffect(_effectShowSword._bindEffect,
				_owner.ACOwner._avatarController,
				-1);
			_effectShowSword.IsPlaying = false;
		}
	}
	
	protected void UpdateBindEffect(BindEffectInfo effInfo, float currentAnimPercent)
	{
		if(effInfo != null)
		{
			if (effInfo.IsPlaying)
			{
				//is playing, stop it?
				if (currentAnimPercent > effInfo._endPercent)
				{
					CharacterEffectManager.Instance.StopEffect(effInfo._bindEffect,
						_owner.ACOwner._avatarController,
						-1);
					effInfo.IsPlaying = false;
				}
			}
			else if (!effInfo.HasPlayed)
			{
				//not playing, if I have not played it before, play it?
				if ((currentAnimPercent >= effInfo._startPercent) 
					&& (currentAnimPercent < effInfo._endPercent))
				{
					CharacterEffectManager.Instance.PlayEffect(effInfo._bindEffect,
						_owner.ACOwner._avatarController,
						-1);
					effInfo.IsPlaying = true;
					effInfo.HasPlayed = true;
				}
			}
		}
	}
	
	protected override void UpdateBindEffects()
	{
		//play and stop effect depends on the time
		int index = 0;
		float currentAnimPercent = _owner.ACOwner.AniGetAnimationNormalizedTime();
		if (_attackActiveCount > 0)
		{
			currentAnimPercent += _attackActiveCount -1 ;
		}
		int offset = _bindEffects.Length/2;
		foreach(BindEffectInfo effInfo in _bindEffects)
		{
			if (index == _activeEffectIndex || index == (_activeEffectIndex+offset))
			{
				UpdateBindEffect(effInfo, currentAnimPercent);
			}
			index ++;
		}
		UpdateBindEffect(_effectShowSword, currentAnimPercent);
	}
	
	public override bool InitSkillData(SkillData skillData, AIAgent owner)
	{
		bool ret = true;
		if(skillData != null)
		{
			_owner = owner as AIAgent;
			if(_skillData != skillData && skillData != null)
			{
				_skillData = skillData;
			}
			_damageScale = _damageScaleSource *skillData.CurrentLevelData.damageScale;
            InitEnergyCost(_owner.ACOwner.Data.TotalReduceEnergy);	
		}
		else
		{
			ret = false;
			_damageScale = _damageScaleSource;
		}
		return ret;
	}
}
