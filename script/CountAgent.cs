using UnityEngine;
using System.Collections;

public class CountAgent : MonoBehaviour {
	
	ActionController _owner;
	protected float[] _skillTime;
	protected float[] _skillMinTime;
	protected int _energyCurrent = 0;
	protected int _energyGainTotal = 0;
	protected int _energyGainByPotion = 0;
	protected int _energyGainByOther = 0;
	protected int _maxAttackPoint = 0;
	protected int _currentMaxAttackPoint = 0;
	public int _energyCost;
	public int[] _energyGain;
	public void Init(FCObject owner)
	{
		_owner = owner as ActionController;
		_energyCurrent = _owner.Energy;
		if(LevelManager.Singleton.TotalCounter == null)
		{
			LevelManager.Singleton.TotalCounter = new float[(int)LevelManager.VarNeedCount.MAX];
		}
		for(int i = 0; i < _skillTime.Length; i++)
		{
			//LevelManager.Singleton.TotalCounter[i+(int)LevelManager.VarNeedCount.SKILL_1_ENG_COST] = 2000;
		}
		LevelManager.Singleton.TotalCounter[(int)LevelManager.VarNeedCount.ENERGY_MAX] = _energyCurrent;
        _maxAttackPoint = _owner.Data.TotalAttack / 3;
		LevelManager.Singleton.TotalCounter[(int)LevelManager.VarNeedCount.ENERGY_CURRENT] = _energyCurrent;
		
		//_maxAttackPoint = _owner.Data.BaseDamage 
	}
	
	void Awake()
	{
		_owner = null;
		_skillTime = new float[10];
		_skillMinTime = new float[10];
		_energyGain = new int[10];
		for(int i =0; i < _skillMinTime.Length; i++)
		{
			_skillMinTime[i] = 999;
		}
	}
	
	public void OnSkillEnd(int skillID)
	{
		_skillTime[skillID] = 0;
	}
	public void OnSkillEnter(int skillID)
	{
		if(skillID > 0 
				&& _owner.ACGetCurrentAttack() != null 
				&& _owner.ACGetCurrentAttack()._energyCost != 0)
		{
			LevelManager.Singleton.TotalCounter[skillID+(int)LevelManager.VarNeedCount.SKILL_1_ENG_COST] = _energyCost;
		}
		if(_owner.AIUse.CurrentSkill.CoolDownTimeMax > _skillTime[skillID] + 0.1f 
			&& LevelManager.Singleton.TotalCounter[skillID+(int)LevelManager.VarNeedCount.SKILL_1_UES_TIMES] > 0)
		{
			Debug.LogError("This may be a JB client");
			if(_skillTime[skillID] <  _skillMinTime[skillID])
			{
				_skillMinTime[skillID] = _skillTime[skillID];
				LevelManager.Singleton.TotalCounter[skillID+(int)LevelManager.VarNeedCount.SKILL_1_CD_MIN_TIME] = (int)(_skillMinTime[skillID] * 100);
			}
		}
		LevelManager.Singleton.TotalCounter[skillID+(int)LevelManager.VarNeedCount.SKILL_1_UES_TIMES]++;
	}
	
	public void EnergyCount(int eng,int skillID)
	{
		if(eng > 0 && skillID >=0)
		{
			_energyGain[skillID] += eng;
			_energyGainTotal += eng;
			LevelManager.Singleton.TotalCounter[(int)LevelManager.VarNeedCount.ENERGY_GAIN_TOTAL] += eng;
		}
		else if(eng > 0 && skillID == -2)
		{
			_energyGainByPotion += eng;
			LevelManager.Singleton.TotalCounter[(int)LevelManager.VarNeedCount.ENERGY_GAIN_BY_POTION] += eng;
		}
		else if(eng > 0 && skillID == -1)
		{
			_energyGainByOther += eng;
			LevelManager.Singleton.TotalCounter[(int)LevelManager.VarNeedCount.ENERGY_GAIN_BY_OTHER] += eng;
		}
		else if(eng < 0)
		{
			_energyCost += eng;
			LevelManager.Singleton.TotalCounter[(int)LevelManager.VarNeedCount.ENERGY_COST] += eng;
			
		}
		
		_energyCurrent = _owner.Energy;
		LevelManager.Singleton.TotalCounter[(int)LevelManager.VarNeedCount.ENERGY_CURRENT] = _energyCurrent;
		//Debug.Log(_energyGainTotal);
		//Debug.Log(_energyGainByPotion);
		//Debug.Log(_energyGainByOther);
		//Debug.Log(_energyCost);
		//Debug.Log(_energyCurrent);
	}
	
	protected void Update()
	{
		for(int i = 0; i < _skillTime.Length; i++)
		{
			_skillTime[i]+=Time.deltaTime;
		}
		if(_owner != null)
		{
			if(_maxAttackPoint  < _owner.Data.TotalAttack/3)
			{
				_maxAttackPoint = _owner.Data.TotalAttack/3;
				LevelManager.Singleton.TotalCounter[(int)LevelManager.VarNeedCount.ATTACK_MAX_POINT] = _maxAttackPoint;
			}
		}
	}
	
	public void OnTryToHit(int damage)
	{
		if(damage > LevelManager.Singleton.TotalCounter[(int)LevelManager.VarNeedCount.ATTACK_MAX_POINT1])
		{
			LevelManager.Singleton.TotalCounter[(int)LevelManager.VarNeedCount.ATTACK_MAX_POINT1] = damage;
		}
	}
	public void OnHit(int flag)
	{
		Debug.Log((LevelManager.VarNeedCount)flag);
		LevelManager.Singleton.TotalCounter[flag]++;
	}
	
	public void OnQuit()
	{
		//if()
	}
}
