using UnityEngine;
using System.Collections;

public class FCStateAttack_meleeNormal : FCStateAgent {
	
	private AttackBase _attackBase;
	
	public AttackBase CurrentAttackBase
	{
		set
		{
			_attackBase = value;
		}
	}
	public override void Run()
	{
		StartCoroutine(STATE());
	}
	
	public override void SetSubAgent(FCAgent ewa)
	{
		if(ewa != null)
		{
			_attackBase = ewa as AttackBase;
		}
		else
		{
			_attackBase = null;
		}
	}
	
	public override FCAgent GetSubAgent()
	{
		return _attackBase;
	}
	
	void Awake()
	{
		_currentStateID = AIAgent.STATE.ATTACK;
	}
	
	IEnumerator STATE()
	{
		_inState = true;
		_stateOwner.AttackTaskChange(FCCommand.CMD.STATE_ENTER);

        if (_stateOwner.TargetAC)
        {
            _stateOwner.FaceToTarget(_stateOwner.TargetAC);
        }

		PlayAnimation();

		_attackBase.AttackLastTime = _stateOwner.ACOwner.AniGetAnimationLength()*1f/3f;
		while(_inState)
		{
			if(!GameManager.Instance.GamePaused)
			{
				if(!_stateOwner.IsInAttackEndDelay)
				{
					_attackBase.AttackUpdate();
				}
			}
			yield return null;
		}
		_stateOwner.AttackTaskChange(FCCommand.CMD.STATE_QUIT);
		_stateOwner.ChangeState();
	}
	
	public override void PlayAnimation()
	{
		_stateOwner.AnimationSwitch._aniIdx = _attackBase._attackAni;
		_stateOwner.PlayAnimation();
	}
}
