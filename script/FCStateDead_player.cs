using UnityEngine;
using System.Collections;

[AddComponentMenu("FC/Logic/FCObject/PlayAndEnemy/Agent/AI/State/Player/Dead")]
public class FCStateDead_player : FCStateAgent {

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
		float timeLast = 3f;
		_stateOwner.SetActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_SUPER_SAIYAJIN);
		_stateOwner.SetActionSwitchFlag(FC_ACTION_SWITCH_FLAG.CANT_MOVE);
		_stateOwner.SetActionSwitchFlag(FC_ACTION_SWITCH_FLAG.CANT_ROTATE);
		PlayAnimation();
		StateIn();
		while(_inState)
		{
			_stateOwner.DeadTaskChange(FCCommand.CMD.STATE_UPDATE);
			if(timeLast >0)
			{
				timeLast -= Time.deltaTime;
				if(timeLast <= 0)
				{
					_stateOwner.DeadTaskChange(FCCommand.CMD.STATE_DONE);
				}
			}
			else
			{
				_stateOwner.DeadTaskChange(FCCommand.CMD.STATE_FINISH);
			}
			StateProcess();
			//Debug.Log("idle state");
			yield return null;
		}
		StateOut();
		_stateOwner.DeadTaskChange(FCCommand.CMD.STATE_QUIT);
		_stateOwner.ChangeState();
	}
}
