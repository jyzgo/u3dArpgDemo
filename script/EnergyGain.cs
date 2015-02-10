using UnityEngine;
using System.Collections;

public class EnergyGain{
	
	static protected float _damageLine1 = 0.3f;
	static protected float _damageLine2 = 0.7f;
	static protected float _energyByDamage0 = 300;
	static protected float _energyByDamage1 = 700;
	static protected float _energyByDamage2 = 1000;
	
	static protected int _comboLine1 = 25;
	static protected int _comboLine2 = 65;
	static protected int _energyByCombo0 = 10;
	static protected int _energyByCombo1 = 20;
	static protected int _energyByCombo2 = 30;
	
	static protected float _damageLine1PvP = 0.3f;
	static protected float _damageLine2PvP = 0.7f;
	static protected float _energyByDamage0PvP = 1000;
	static protected float _energyByDamage1PvP = 1000;
	static protected float _energyByDamage2PvP = 1000;
	
	static protected int _comboLine1PvP = 25;
	static protected int _comboLine2PvP = 65;
	static protected int _energyByCombo0PvP = 20;
	static protected int _energyByCombo1PvP = 40;
	static protected int _energyByCombo2PvP = 60;
	
	static public int GetEnergyByHurt(float damagePercents)
	{
		//pvp mode
		if(GameManager.Instance.IsPVPMode)
		{
			float ret = 0;
			if(damagePercents < _damageLine1PvP)
			{
				ret = _energyByDamage0PvP*damagePercents;
			}
			else if(damagePercents < _damageLine2PvP)
			{
				ret = _energyByDamage1PvP*damagePercents;
			}
			else
			{
				ret = _energyByDamage2PvP*damagePercents;
			}
			if(ret < 1 && ret > Random.Range(0,1f))
			{
				ret = 1;
			}
			//if ret = x.9 , the result should be x+1
			ret+=0.1f;
			return (int)ret;
		}
		else
		{
			float ret = 0;
			if(damagePercents < _damageLine1)
			{
				ret = _energyByDamage0*damagePercents;
			}
			else if(damagePercents < _damageLine2)
			{
				ret = _energyByDamage1*damagePercents;
			}
			else
			{
				ret = _energyByDamage2*damagePercents;
			}
			if(ret < 1 && ret > Random.Range(0,1f))
			{
				ret = 1;
			}
			//if ret = x.9 , the result should be x+1
			ret+=0.1f;
			return (int)ret;
		}

	}
	
	static public int GetEnergyByAttack(int comboCount)
	{
		//pvp mode
		if(GameManager.Instance.IsPVPMode)
		{
			int ret = 0;
            if (comboCount < _comboLine1PvP)
			{
				ret = _energyByCombo0PvP;
			}
            else if (comboCount < _comboLine2PvP)
			{
				ret = _energyByCombo1PvP;
			}
			else
			{
				ret = _energyByCombo2PvP;
			}
			return ret;
		}
		else
		{
			int ret = 0;
			if(comboCount<_comboLine1)
			{
				ret = _energyByCombo0;
			}
			else if(comboCount<_comboLine2)
			{
				ret = _energyByCombo1;
			}
			else
			{
				ret = _energyByCombo2;
			}

			return ret;
		}
	}
}
