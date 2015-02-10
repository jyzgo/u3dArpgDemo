using UnityEngine;
using System.Collections;

[AddComponentMenu("FC/Logic/FCObject/PlayAndEnemy/Agent/AI/State/Town/Run")]
public class FCStateRun_town : FCStateAgent {
	
	public override void Run()
	{
		StartCoroutine(NORMAL());
	}
		
	void Awake()
	{
		_currentStateID = AIAgent.STATE.RUN;
	}
		
	
	IEnumerator NORMAL()
	{
		_inState = true;
		_stateOwner.HandleInnerCmd(FCCommand.CMD.STATE_ENTER,null);
		PlayAnimation();
		StateIn();
		while(_inState)
		{
			StateProcess();
			yield return null;
		}
		StateOut();
		_stateOwner.ChangeState();
	}
	
}
