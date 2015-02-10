using UnityEngine;
using System.Collections;

public class FCStateAvoid_rangerNormal: FCStateAgent {

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
		_currentStateID = AIAgent.STATE.AVOID_AND_SHOOT;
	}
	
	IEnumerator STATE()
	{
		_inState = true;
		
		_stateOwner.AvoidAndShootTaskChange(FCCommand.CMD.STATE_ENTER);
		while(_inState)
		{
			_stateOwner.AvoidAndShootTaskChange(FCCommand.CMD.STATE_UPDATE);
			yield return null;
		}
		_stateOwner.AvoidAndShootTaskChange(FCCommand.CMD.STATE_QUIT);
		_stateOwner.ChangeState();
	}
}
