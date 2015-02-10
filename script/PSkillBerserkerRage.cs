using UnityEngine;
using System.Collections;

public class PSkillBerserkerRage : PassiveSkillBase {
	
	
	public float _attackIncrease = 0.1f;
	public float _hpValue = 0.1f;
	
	protected float _hpNextPercents = 1;
	protected int _damageIncrease = -1;

	protected override void OnInit()
	{
		if(_owner.ACOwner.IsPlayer)
		{
			//_attackIncrease = _owner._percents;
			//_hpValue = _owner._trueValue;
		}
		_hpNextPercents = 1 - _hpValue;
		_damageIncrease = -1;
		_owner.ACOwner._hpChangeMessage += HpIsChanged;
		
	}
	public void HpIsChanged(float hpPercents)
	{
		if(_owner.beActive)
		{
			float ret = 0;
			if(_owner.ACOwner.IsAlived)
			{
				hpPercents = _owner.ACOwner.HitPointPercents;
				if(hpPercents <= _hpNextPercents || hpPercents > _hpNextPercents+_hpValue)
				{
					float tp =  1 - _hpValue;
					while(tp > hpPercents)
					{
						tp -= _hpValue;
						ret += _attackIncrease;
					}
					_hpNextPercents = tp;
					if(_hpNextPercents > 1 - _hpValue)
					{
						_hpNextPercents = 1 - _hpValue;
						ret = -2;
					}
				}

			}
			if(ret > Mathf.Epsilon || ret < -1)
			{
                //if(_damageIncrease >=0)
                //{
                //    _owner.ACOwner.Data.passiveAttack -= _damageIncrease;
                //    _damageIncrease = 0;
                //}
                //if(_owner.ACOwner.Data.passiveAttack <= 0)
                //{
                //    _owner.ACOwner.Data.passiveAttack = 0;
                //}
                //if(ret > Mathf.Epsilon)
                //{
                //    _damageIncrease = (int)(_owner.ACOwner.Data.BaseAttack * ret);
                //    _owner.ACOwner.Data.passiveAttack += _damageIncrease;
                //}
                //_owner.ACOwner.ACRefreshDataWithPassive();
			}
		}
		
	}
}
