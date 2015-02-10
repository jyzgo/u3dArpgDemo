using UnityEngine;
using System.Collections;

public class PassiveEnergySiphon : PassiveSkillBase {
	
	public float _energySiphonValue = 0.1f;
	public int _siphonMaxPerHit = 10;
	public float _coolDownTime = 0.1f;
	
	public bool _enableTimeSystem = false;
	protected float _energyCount = 0;
	protected float _cdTimeCount = 0;
	public float _recoverySpeed = 10; //10 point/s
	protected float _siphonTotalCount = 0;
	protected override void OnInit()
	{
		if(_owner.ACOwner.IsPlayer)
		{
            //_energySiphonValue = _owner._percents;
            //_siphonMaxPerHit = (int)_owner._trueValue;
		}
		_recoverySpeed = _owner.coolDownTimeMax;
		if(_coolDownTime == 0)
		{
			_coolDownTime = 1;
		}
		_owner.ACOwner._onHitTarget += OnHitTarget;
		StartCoroutine(STATE());
	}
	
	public void OnHitTarget(ActionController ac, bool isFromSkill)
	{
		if(_owner.beActive 
			&& _siphonTotalCount >=0)
		{
			_energyCount += _energySiphonValue;
			if(_energyCount >=1)
			{
				
				_owner.ACOwner.CostEnergy((int)_energyCount, -1);
				_energyCount -= (int)_energyCount;
			}
			_siphonTotalCount -= _energySiphonValue;
		}
	}
	
	private IEnumerator STATE()
	{
		while(true)
		{
			if(!_enableTimeSystem)
			{
				_siphonTotalCount = _siphonMaxPerHit;
			}
			else
			{
				_siphonTotalCount += Time.deltaTime*_recoverySpeed;
				if(_siphonTotalCount > _siphonMaxPerHit)
				{
					_siphonTotalCount = _siphonMaxPerHit;
				}
			}
			yield return null;
		}
	}
}
