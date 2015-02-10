using UnityEngine;
using System.Collections;

public class FCStateLevelUp_meleeNormal : FCStateAgent {

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
		_currentStateID = AIAgent.STATE.LEVEL_UP;
	}
	
	IEnumerator STATE()
	{
		_inState = true;
		
		_stateOwner.LevelUpTaskChange(FCCommand.CMD.STATE_ENTER);
		PlayAnimation();
		StateIn();
		while(_inState)
		{
			_stateOwner.LevelUpTaskChange(FCCommand.CMD.STATE_UPDATE);
			StateProcess();
			yield return null;
		}
		StateOut();
		_stateOwner.LevelUpTaskChange(FCCommand.CMD.STATE_QUIT);
		_stateOwner.ChangeState();
	}
	
	
}
