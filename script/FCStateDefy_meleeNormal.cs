using UnityEngine;
using System.Collections;

public class FCStateDefy_meleeNormal : FCStateAgent {

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
		_currentStateID = AIAgent.STATE.DEFY;
	}
	
	IEnumerator STATE()
	{
		_inState = true;
		
		_stateOwner.DefyTaskChange(FCCommand.CMD.STATE_ENTER);
		PlayAnimation();
		StateIn();
		while(_inState)
		{
			_stateOwner.DefyTaskChange(FCCommand.CMD.STATE_UPDATE);
			StateProcess();
			yield return null;
		}
		StateOut();
		_stateOwner.DefyTaskChange(FCCommand.CMD.STATE_QUIT);
		_stateOwner.ChangeState();
	}
}
