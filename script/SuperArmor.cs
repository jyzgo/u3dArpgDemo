using UnityEngine;
using System.Collections;

public class SuperArmor {
	
	protected float[] _armorValues;
	protected float[] _armorValuesDefault; 
	
	protected float[] _absorbValues;
	protected float[] _absorbDefaultValue;
	
	//life Time Should > 0.1f
	protected float[] _lifeTime;
	protected float[] _lifeTimeDefault;
	
	protected AIAgent _owner;
	//SuperArmor
	//can ignore normal hit
	//public const int SUPER_ARMOR_LVL0 = 0;
	// can absorb damage and all type hit
	//public const int SUPER_ARMOR_LVL1 = 1;
	
	// if absorbValue >= 1, means will not hurt owner
	public void Init(AIAgent owner)
	{
		
		_armorValues = new float[FCConst.SUPER_ARMOR_LVL_MAX]; 
		_armorValuesDefault = new float[FCConst.SUPER_ARMOR_LVL_MAX]; 
		_absorbValues = new float[FCConst.SUPER_ARMOR_LVL_MAX];
		_absorbDefaultValue = new float[FCConst.SUPER_ARMOR_LVL_MAX];
		_lifeTime = new float[FCConst.SUPER_ARMOR_LVL_MAX];
		_lifeTimeDefault = new float[FCConst.SUPER_ARMOR_LVL_MAX];
		_owner = owner;
		for(int i =0; i < FCConst.SUPER_ARMOR_LVL_MAX; i++)
		{
			_armorValues[i] = 0;
			_armorValuesDefault[i] = 0;
			_absorbValues[i] = 0;
			_absorbDefaultValue[i] = 0;
			_lifeTime[i] = 0;
			_lifeTimeDefault[i] = 0;
		}
	}
	
	public void CreateArmor(int totalValue, float absorbValue,float life, int armorLevel)
	{
		_armorValues[armorLevel] = _armorValuesDefault[armorLevel] = totalValue;
		_absorbValues[armorLevel] = _absorbDefaultValue[armorLevel] = absorbValue;
		_lifeTime[armorLevel] = _lifeTimeDefault[armorLevel] = life;
	}
	
	public void UpdateSuperArmor()
	{
		if(_lifeTime[FCConst.SUPER_ARMOR_LVL2] > 0 
			&& _armorValues[FCConst.SUPER_ARMOR_LVL2] > 0
			&& _lifeTime[FCConst.SUPER_ARMOR_LVL2] > 0)
		{
			_lifeTime[FCConst.SUPER_ARMOR_LVL2] -= Time.deltaTime;
			if(_lifeTime[FCConst.SUPER_ARMOR_LVL2] <= 0)
			{
				BreakArmor(FCConst.SUPER_ARMOR_LVL2);
			}
		}
	}
	
	public float DamageAbsorb(int armorLevel)
	{
		return _absorbValues[armorLevel];
	}
	public int ActiveArmor()
	{
		if(_armorValues[FCConst.SUPER_ARMOR_LVL2] >0)
		{
			return FCConst.SUPER_ARMOR_LVL2;
		}
		else if(_armorValues[FCConst.SUPER_ARMOR_LVL1] >0)
		{
			return FCConst.SUPER_ARMOR_LVL1;
		}
		else if(_armorValues[FCConst.SUPER_ARMOR_LVL0] >0)
		{
			return FCConst.SUPER_ARMOR_LVL0;
		}
		return -1;
	}
	public void BreakArmor(int armorLevel)
	{
		_armorValues[armorLevel] = -1;
	}
	public void Revive(int armorLevel)
	{
		_armorValues[armorLevel] = _armorValuesDefault[armorLevel];
		_absorbValues[armorLevel] = _absorbDefaultValue[armorLevel];
	}
	
	/// <summary>
	/// Gets the armor remain.
	/// </summary>
	/// <returns>
	/// The armor remain. 1 measn 100%
	/// </returns>
	public float GetArmorRemain(int armorLvl)
	{
		if(_armorValues[armorLvl] > 0 && _armorValuesDefault[armorLvl] >0 )
		{
			return _armorValues[armorLvl]/_armorValuesDefault[armorLvl];
		}
		else
		{
			return 0;
		}
	}
	
	public void SetArmor(int armorLvl, float minPercents)
	{
		if(_armorValuesDefault[armorLvl] > 0)
		{
			int armorv = (int)(_armorValuesDefault[armorLvl] * minPercents);
			if(_armorValues[armorLvl] < armorv)
			{
				_armorValues[armorLvl] = armorv;
			}
		}
	}
	public bool ArmorIsBroken(int armorLvl)
	{
		return _armorValues[armorLvl] <= 0;
	}
	//if result >0 ,means super armor is destroy, will do damage to owner
	//
	public int TryToHit(int forceValue, float damage, AttackHitType eht,ref int damageResult)
	{
		int armorBroken = -1;
		bool ret = true;
		if(_owner.CurrentAttack != null && _owner.CurrentAttack._needIgnoreSuperArmor)
		{
			ret = false;
		}
		if(_armorValues[FCConst.SUPER_ARMOR_LVL2] > 0 && ret)
		{
			damageResult = (int)(damage*_absorbValues[FCConst.SUPER_ARMOR_LVL2]);
			_armorValues[FCConst.SUPER_ARMOR_LVL2] -= forceValue;
			_armorValues[FCConst.SUPER_ARMOR_LVL2] -= damageResult;
			//thats real damage
			damageResult = (int)damage - damageResult;
			forceValue = 0;
			if(_armorValues[FCConst.SUPER_ARMOR_LVL2] <= 0)
			{
				// super armor is broken
				// try to use next level super armor to block attack
				armorBroken = FCConst.SUPER_ARMOR_LVL2;
			}
			else
			{
				//return armorBroken;
			}
		}
		// if super armor is broken, try to destroy normal armor
		if(_armorValues[FCConst.SUPER_ARMOR_LVL1] > 0)
		{
			int dr1 = (int)(damageResult*_absorbValues[FCConst.SUPER_ARMOR_LVL1]);
			_armorValues[FCConst.SUPER_ARMOR_LVL1] -= forceValue;
			if(forceValue<=0)
			{
				_armorValues[FCConst.SUPER_ARMOR_LVL1] -= damage;
			}
			else
			{
				_armorValues[FCConst.SUPER_ARMOR_LVL1] -= dr1;
			}
			//thats real damage
			damageResult = (int)damageResult - dr1;
			if(_armorValues[FCConst.SUPER_ARMOR_LVL1] <= 0)
			{
				// super armor is broken
				forceValue = 0;
				armorBroken = FCConst.SUPER_ARMOR_LVL1;
				if(!_owner.HasState(AIAgent.STATE.ARMOR1_BROKEN))
				{
					Revive(FCConst.SUPER_ARMOR_LVL1);
				}
			}
			return armorBroken;
		}
		if(_armorValues[FCConst.SUPER_ARMOR_LVL1]<=0 && _armorValues[FCConst.SUPER_ARMOR_LVL2]<=0)
		{
			_armorValues[FCConst.SUPER_ARMOR_LVL0] -= forceValue;
#if xingtianbo
			if(((eht == AttackHitType.Normal || eht >= AttackHitType.NORMAL_HURT) && _armorValues[FCConst.SUPER_ARMOR_LVL0] <= 0)
				|| (eht > AttackHitType.Normal && eht < AttackHitType.NORMAL_HURT) || (eht > AttackHitType.BLEND_HURT))
			{
				//normal armor is broken
				armorBroken = FCConst.SUPER_ARMOR_LVL0;
				
			}
#endif

            if ((eht == AttackHitType.None  && _armorValues[FCConst.SUPER_ARMOR_LVL0] <= 0)
                || (eht > AttackHitType.None))
            {
                //normal armor is broken
                armorBroken = FCConst.SUPER_ARMOR_LVL0;

            }
			if(_armorValues[FCConst.SUPER_ARMOR_LVL0] <= 0)
			{
				Revive(FCConst.SUPER_ARMOR_LVL0);
			}
		}
		if(damage > 0 && damageResult <= 1)
		{
			damageResult = 1; 
		}
		return armorBroken;
	}
	
	
}
