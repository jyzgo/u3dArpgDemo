using UnityEngine;
using System.Collections;

[AddComponentMenu("FC/Logic/FCObject/PlayAndEnemy/Agent/AI/State/MeleeNormal/Dead")]
public class FCStateDead_meleeNormal : FCStateAgent {

	public override void Run()
	{
		StartCoroutine(STATE());
	}
	
	void Awake()
	{
		_currentStateID = AIAgent.STATE.DEAD;
	}
	
	IEnumerator STATE()
	{
		_inState = true;
		_stateOwner.DeadTaskChange(FCCommand.CMD.STATE_ENTER);
		_stateOwner.SetActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_SUPER_SAIYAJIN);
		if(!_stateOwner._deadWithExplode)
		{
			PlayAnimation();
		}
		StateIn();
		float timeLast = 3f;
		if(_stateOwner._deadWithExplode)
		{
			_inState = false;
		}
		while(_inState)
		{
			if(timeLast >0)
			{
				timeLast -= Time.deltaTime;
			}
			else
			{
				_stateOwner.DeadTaskChange(FCCommand.CMD.STATE_DONE);
				_inState = false;
			}
			StateProcess();
			yield return null;
		}
		StateOut();
		_stateOwner.DeadTaskChange(FCCommand.CMD.STATE_QUIT);
	}
}
