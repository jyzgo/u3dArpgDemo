using UnityEngine;
using System.Collections;

public class PassiveRunHit : PassiveSkillBase 
{
	public float _coolDownTime = 1f;
	public float _runTime = 2f;
	public string _runHitSkillName = "";
	public float _finalSpeed = 9f;
	public MMediaEffectInfoMapAll _mmEffectMap = null;
	protected float _coolDownCount = 0;
	protected int _orgSkillID = -1;
	protected bool _inFastRun = false;
	protected AniSwitch _aniSwitch = new AniSwitch();
	protected override void OnInit()
	{
		if(_owner.ACOwner.IsPlayer)
		{
           FCPassiveSkillAttribute  skillAttribute =  _owner.skillAttributes.Find(delegate(FCPassiveSkillAttribute skillAtribute) { return skillAtribute.skillType == AIHitParams.Special; });
           _runTime = skillAttribute.attributeValue;
		}
		_coolDownTime = _owner.coolDownTimeMax;
		_owner.ACOwner._onAttackEnter += OnUseSkill;
		_owner.ACOwner._onRunUpdate += OnRunUpdate;
		_owner.ACOwner._onStateQuit += OnStateQuit;
		_coolDownCount = _coolDownTime;
		_aniSwitch._aniIdx = FC_All_ANI_ENUM.runFast;
		_finalSpeed = _owner.ACOwner.Data.TotalMoveSpeed;
		StartCoroutine(STATE());
	}
	
	public void OnUseSkill(int skillID)
	{
		if(_owner.ACOwner.AIUse.AIStateAgent._preState.CurrentStateID == AIAgent.STATE.RUN
			&& _owner.ACOwner.AIUse.RunStateTime >= _runTime + 0.1f
			&& _owner.beActive && skillID >=0
			&& _owner.ACOwner.AIUse.AttackCountAgent.GetSkill(skillID).SkillModule._isNormalSkill
			&& _coolDownCount <= 0)
		{
			_owner.ACOwner.AIUse.AttackCountAgent.SetNextSkill(_runHitSkillName, 0, true);
			_coolDownCount = _coolDownTime;
			_orgSkillID = skillID;
			_owner.ACOwner.AIUse.RunStateTime = 0;
		}
	}
	
	public void OnStateQuit(AIAgent.STATE ass)
	{
        //if(_mmEffectMap != null)
        //{
        //    _mmEffectMap.StopEffect();
        //}
	}
	public void OnRunUpdate(float runTime)
	{
		if(runTime <= _runTime)
		{
			_inFastRun = false;
		}
		else if(!_inFastRun && _owner.ACOwner.AniGetAnimationNormalizedTime() > 0.9f)
		{
			_inFastRun = true;
			_owner.ACOwner.ACPlayAnimation(_aniSwitch);
			_owner.ACOwner.CurrentSpeed = _finalSpeed;
			if(_mmEffectMap != null)
			{
				_mmEffectMap.PlayEffect(FC_EFFECT_EVENT_POS.AT_ANY_TIME, _owner.ACOwner, -1);
			}
		}
		else if(_inFastRun)
		{
			_owner.ACOwner.CurrentSpeed = _finalSpeed;
		}
	}
	
	private IEnumerator STATE()
	{
		while(true)
		{
			if(_coolDownCount >-1)
			{
				_coolDownCount -= Time.deltaTime;
			}
			yield return null;
		}
	}
}
