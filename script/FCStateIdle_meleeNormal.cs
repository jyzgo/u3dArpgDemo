using UnityEngine;
using System.Collections;

public class FCStateIdle_meleeNormal : FCStateAgent {

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

		float timeLast = 2f;
		PlayAnimation();
		StateIn();
		while(_inState)
		{
			_stateOwner.IdleTaskChange(FCCommand.CMD.STATE_UPDATE);
			if(timeLast >0)
			{
				timeLast -= Time.deltaTime;
				
				if(!_stateOwner.ACOwner.IsPlayer)
				{
					_stateOwner.FaceToTarget(_stateOwner.TargetAC);
				}
			}
			else
			{
				_stateOwner.IdleTaskChange(FCCommand.CMD.STATE_DONE);
			}
			StateProcess();
			yield return null;
		}
		StateOut();
		_stateOwner.IdleTaskChange(FCCommand.CMD.STATE_QUIT);
		_stateOwner.ChangeState();
	}
}
