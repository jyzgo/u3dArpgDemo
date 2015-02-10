using UnityEngine;
using System.Collections;

public class PassiveReflexParry : PassiveSkillBase {
	
	public float _chanceToBlock = 0.01f;
	
	public AIAgent.STATE[] _statesCantAutoDefense;
	public string[] _skillCantAutoDefense;
	public float _coolDownTime = 3f;
	protected float _coolDownTimeCount = 0;
	public string _parrySkill = "Parry";
	public int _parrySkillCombo = 2;
	
	protected override void OnInit()
	{
		if(_owner.ACOwner.IsPlayer)
		{
            //_chanceToBlock = _owner._percents;
			_coolDownTime = _owner.coolDownTimeMax;
		}
		_owner.ACOwner.AIUse._isHitBySomeone += IsHit;
		StartCoroutine(STATE());
		
	}
	
	public bool IsHit(ActionController target)
	{
		bool ret = false;
		if(_owner.beActive && _coolDownTimeCount <=0)
		{
			ret = true;
			AIAgent.STATE ass = _owner.ACOwner.AIUse.AIStateAgent.PreStateID;
			AIAgent.STATE assc = _owner.ACOwner.AIUse.AIStateAgent.CurrentStateID;
			foreach(AIAgent.STATE state in _statesCantAutoDefense)
			{
				if(state == ass && ass == assc)
				{
					ret = false;
					break;
				}
			}
			if(ret)
			{
				if(_owner.ACOwner.AIUse.CurrentSkill != null)
				{
					string sn = _owner.ACOwner.AIUse.CurrentSkill._skillName;
					foreach(string skill in _skillCantAutoDefense)
					{
						if(skill == sn)
						{
							ret = false;
							break;
						}
					}
				}
			}
			if(ret)
			{
				ret = (Random.Range(0,1f) <= _chanceToBlock);
			}
			if(ret)
			{
				_owner.ACOwner.AIUse.GoToAttackForce(_parrySkill, _parrySkillCombo);
				_owner.ACOwner.AIUse.ParryTarget = target;
				_coolDownTimeCount = _coolDownTime;
				_owner.ACOwner.AIUse.SetActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_GOD);
			}
		}
		return ret;
		//if()
	}
	
	protected IEnumerator STATE()
	{
		while(true)
		{
			if(_coolDownTimeCount > -1)
			{
				_coolDownTimeCount -= Time.deltaTime;
			}
			yield return null;
		}
	}
}
