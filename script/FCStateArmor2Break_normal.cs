using UnityEngine;
using System.Collections;

public class FCStateArmor2Break_normal : FCStateAgent {

	public override void Run()
	{
		StartCoroutine(STATE());
	}
	
	void Awake()
	{
		_currentStateID = AIAgent.STATE.ARMOR2_BROKEN;
	}
	
	IEnumerator STATE()
	{
		_inState = true;
		_stateOwner.Armor2BrokenTaskChange(FCCommand.CMD.STATE_ENTER);
		PlayAnimation();
		while(_inState)
		{
			_stateOwner.Armor2BrokenTaskChange(FCCommand.CMD.STATE_UPDATE);
			yield return null;
		}
		_stateOwner.Armor2BrokenTaskChange(FCCommand.CMD.STATE_QUIT);
		_stateOwner.ChangeState();
	}
}
