using UnityEngine;
using System.Collections;

public class PassiveIceBoundry : PassiveSkillBase 
{
	public float _chanceToFireIce = 0.01f;
	
	public AIAgent.STATE[] _statesCantAutoIce;
	public string[] _skillCantAutoIce;
	public float _coolDownTime = 3f;
	public string _iceSkillName = "";
	
	
	protected float _coolDownTimeCount = 0;
	protected override void OnInit()
	{
		if(_owner.ACOwner.IsPlayer)
		{
            //_chanceToFireIce = _owner._percents;
		}
		_coolDownTime = _owner.coolDownTimeMax;
		_owner.ACOwner.AIUse._isHitBySomeone += IsHit;
		if(_coolDownTime <=0)
		{
			_coolDownTime = 3f;
		}
		StartCoroutine(STATE());
		
	}

	public bool IsHit(ActionController target)
	{
		bool ret = false;
		AIAgent.STATE ass = _owner.ACOwner.AIUse.AIStateAgent.CurrentStateID;
		if(_owner.beActive && _coolDownTimeCount <=0)
		{
			ret = true;
			
			foreach(AIAgent.STATE state in _statesCantAutoIce)
			{
				if(state == ass)
				{
					ret = false;
					break;
				}
			}
			
			if(_owner.ACOwner.AIUse.CurrentSkill != null)
			{
				string sn = _owner.ACOwner.AIUse.CurrentSkill._skillName;
				foreach(string skill in _skillCantAutoIce)
				{
					if(skill == sn)
					{
						ret = false;
						break;
					}
				}
			}
			
			if(ret && Random.Range(0,1f) <= _chanceToFireIce)
			{
				_coolDownTimeCount = _coolDownTime;
				ret = true;
				Vector3 dir = target.ThisTransform.localPosition - _owner.ACOwner.ThisTransform.localPosition;
				dir.y = 0;
				dir.Normalize();
				if(dir != Vector3.zero)
				{
					_owner.ACOwner.ACRotateTo(dir,-1,true,true);
				}
				_owner.ACOwner.AIUse.GoToAttackForce(_iceSkillName, 0);
			}
			else
			{
				ret = false;
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
