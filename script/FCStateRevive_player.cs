using UnityEngine;
using System.Collections;

[AddComponentMenu("FC/Logic/FCObject/PlayAndEnemy/Agent/AI/State/Player/Revive")]
public class FCStateRevive_player : FCStateAgent {

	public override void Run()
	{
		StartCoroutine(STATE());
	}
	
	void Awake()
	{
		_currentStateID = AIAgent.STATE.REVIVE;
	}
	
	IEnumerator STATE()
	{
		_inState = true;
		_stateOwner.ReviveTaskChange(FCCommand.CMD.STATE_ENTER);
		PlayAnimation();
		StateIn();
		while(_inState)
		{
			StateProcess();
			_stateOwner.ReviveTaskChange(FCCommand.CMD.STATE_UPDATE);
			//Debug.Log("idle state");
			yield return null;
		}
		StateOut();
		_stateOwner.ReviveTaskChange(FCCommand.CMD.STATE_QUIT);
		_stateOwner.ChangeState();
	}
}
