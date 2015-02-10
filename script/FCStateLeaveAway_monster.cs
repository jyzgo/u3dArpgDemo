using UnityEngine;
using System.Collections;

public class FCStateLeaveAway_monster : FCStateAgent {

	public override void Init(AIAgent owner)
	{
		base.Init(owner);
	}
	
	public override void Run()
	{
		StartCoroutine(STATE());
	}
	
	void Awake()
	{
		_currentStateID = AIAgent.STATE.LEAVE_AWAY;
	}
	
	IEnumerator STATE()
	{
		_inState = true;
		_stateOwner.AwayTaskChange(FCCommand.CMD.STATE_ENTER);
		StateIn();
		while(_inState)
		{
			_stateOwner.AwayTaskChange(FCCommand.CMD.STATE_UPDATE);
			StateProcess();
			yield return null;
		}
		StateOut();
		_stateOwner.AwayTaskChange(FCCommand.CMD.STATE_QUIT);
		_stateOwner.ChangeState();
	}
}
