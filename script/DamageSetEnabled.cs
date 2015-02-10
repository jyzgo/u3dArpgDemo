using UnityEngine;
using System.Collections;

[System.Serializable]
public class DamageSetEnabled
{
	public bool _enableNone;
	public float _physicalPercent;
	public float _coldPercent;
	public float _electricPercent;
	public float _firePercent;
	public float _poisonPercent;
	public float _dragonPercent;
	
	private int _damageSet;
	public float _damageTotalPercent;
	
	public int DamageSet
	{
		get
		{
			return _damageSet;
		}
	}
	public void  UpdateDamageSet()
	{
		_damageSet = 0;
		if(_physicalPercent>0)
		{
			Utils.SetFlag((int)FC_DAMAGE_TYPE.PHYSICAL,ref _damageSet);
		}
		if(_coldPercent>0)
		{
			Utils.SetFlag((int)FC_DAMAGE_TYPE.ICE,ref _damageSet);
		}
		if(_firePercent>0)
		{
			Utils.SetFlag((int)FC_DAMAGE_TYPE.FIRE,ref _damageSet);
		}
		if(_poisonPercent>0)
		{
			Utils.SetFlag((int)FC_DAMAGE_TYPE.POISON,ref _damageSet);
		}
		if(_electricPercent>0)
		{
			Utils.SetFlag((int)FC_DAMAGE_TYPE.THUNDER,ref _damageSet);
		}
		if(_enableNone)
		{
			_damageSet = 0;
		}
	}	
}
