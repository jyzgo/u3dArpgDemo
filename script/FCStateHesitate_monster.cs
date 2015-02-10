using UnityEngine;
using System.Collections;

public class FCStateHesitate_monster : FCStateAgent {

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
		_currentStateID = AIAgent.STATE.HESITATE;
	}
	
	IEnumerator STATE()
	{
		_inState = true;
		
		_stateOwner.HesitateTaskChange(FCCommand.CMD.STATE_ENTER);
		while(_inState)
		{
			_stateOwner.HesitateTaskChange(FCCommand.CMD.STATE_UPDATE);
			yield return null;
		}
		_stateOwner.HesitateTaskChange(FCCommand.CMD.STATE_QUIT);
		_stateOwner.ChangeState();
	}
}
