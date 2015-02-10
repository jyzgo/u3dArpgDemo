using UnityEngine;
using System.Collections;

[AddComponentMenu("FC/Logic/FCObject/PlayAndEnemy/Agent/AI/State/Town/Born")]
public class FCStateBorn_town : FCStateAgent {

	public override void Run()
	{
		StartCoroutine(STATE());
	}
	
	void Awake()
	{
		_currentStateID = AIAgent.STATE.BORN;
	}
	
	IEnumerator STATE()
	{
		float timeLast = 2f;
		_inState = true;
		_stateOwner.SetActionSwitchFlag(FC_ACTION_SWITCH_FLAG.CANT_MOVE);
		_stateOwner.SetActionSwitchFlag(FC_ACTION_SWITCH_FLAG.CANT_ROTATE);
		_stateOwner.SetActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_SUPER_SAIYAJIN);
		while(_inState)
		{
			if(timeLast>0)
			{
				timeLast-=Time.deltaTime;
				if(timeLast<=0)
				{
					_stateOwner.HandleInnerCmd(FCCommand.CMD.STATE_DONE,null);
				}
			}
			yield return null;
		}
		_stateOwner.ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_SUPER_SAIYAJIN);
		_stateOwner.ChangeState();

	} 
}
