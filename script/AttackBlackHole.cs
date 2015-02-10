using UnityEngine;
using System.Collections;

public class AttackBlackHole : AttackNormal {
	
	public float _timeCount = 0.2f;
	protected float _timeForFireFlyUp = 0;
	public override void AttackEnter ()
	{
		base.AttackEnter ();
		_timeForFireFlyUp = _timeCount;
		_owner.ACOwner.ACFire(FirePortIdx);
		_owner.ACOwner.ACSetAvoidPriority(FCConst.HILL_WEIGHT);
		
	}
	
	public override void AttackUpdate ()
	{
		base.AttackUpdate ();
		if(_currentState == AttackBase.ATTACK_STATE.STEP_1)
		{
			if(_timeForFireFlyUp > 0)
			{
				_timeForFireFlyUp -= Time.deltaTime;
				if(_timeForFireFlyUp <= 0)
				{
					_owner.ACOwner.ACFire(FirePortIdx);
				}
			}
		}
	}
	
	public override void AttackQuit ()
	{
		base.AttackQuit ();
		_owner.ACOwner.ACSetAvoidPriority(_owner._bodyMass);
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
			InitEnergyCost(_owner.ACOwner.Data.TotalReduceEnergy);
			_damageScale = _damageScaleSource *skillData.CurrentLevelData.damageScale;
			if(_fireInfos != null && _fireInfos.Length != 0)
			{
				foreach(FireportInfo fpi in _fireInfos)
				{
					if(/*fpi.FirePortIdx < 0 && */fpi._firePortName != "")
					{ 
						fpi.FirePortIdx = _owner.ACOwner.ACGetFirePortIndex(fpi._firePortName);
						RangerAgent.FirePort fp = _owner.ACOwner.ACGetFirePort(fpi.FirePortIdx);
						//if(!fp.IsOverride)
						{
							OverrideFirePort(fp, skillData);
							fp.IsOverride = true;
						}
					}
				}
			}
		}
		else
		{
			ret = false;
			_damageScale = _damageScaleSource;
		}
		return ret;
	}
	
	protected override void OverrideFirePort(RangerAgent.FirePort firePort, SkillData skillData) {
		firePort.DamageScale = firePort.DamageScaleSource * skillData.CurrentLevelData.damageScale;
	}
	
}
