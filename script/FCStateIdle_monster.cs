using UnityEngine;
using System.Collections;

public class FCStateIdle_monster : FCStateAgent {

	public override void Run()
	{
		StartCoroutine(STATE());
	}
	
	void Awake()
	{
		_currentStateID = AIAgent.STATE.IDLE;
	}
	
	IEnumerator STATE()
	{
		_inState = true;
		_stateOwner.IdleTaskChange(FCCommand.CMD.STATE_ENTER);
		float timeLast = _stateOwner.TimeForIdleThisTime;
		if(_inState)
		{
			PlayAnimation();
		}
		if(_stateOwner.TargetAC)
		{
			_stateOwner.FaceToTarget(_stateOwner.TargetAC);
		}
		StateIn();
		while(_inState)
		{
			_stateOwner.IdleTaskChange(FCCommand.CMD.STATE_UPDATE);
			if(timeLast >0)
			{
				timeLast -= Time.deltaTime;
			}
			else
			{
				if(_stateOwner.ACOwner.TargetAC != null && _stateOwner.ACOwner.TargetAC.IsAlived)
				{
					_stateOwner.IdleTaskChange(FCCommand.CMD.STATE_DONE);
				}
			}
			StateProcess();
			yield return null;
		}
		StateOut();
		_stateOwner.IdleTaskChange(FCCommand.CMD.STATE_QUIT);
		_stateOwner.ChangeState();
	}
}
