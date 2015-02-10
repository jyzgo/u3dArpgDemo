using UnityEngine;
using System.Collections;

public class FCStateIdle_town : FCStateAgent {

	public override void Run()
	{
		StartCoroutine(STATE());
	}
	
	void Awake()
	{
		_currentStateID = AIAgent.STATE.IDLE;
	}
	
	IEnumerator STATE()
	{
		_inState = true;
		_stateOwner.HandleInnerCmd(FCCommand.CMD.STATE_ENTER,null);
		AIForTown ait = _stateOwner as AIForTown;
		
		float timeLast = Random.Range(ait._timeForIdleMin,ait._timeForIdleMax);
		PlayAnimation();
		while(_inState)
		{
			if(timeLast>0)
			{
				timeLast -= Time.deltaTime;
				if(ait.FaceTarget != null)
				{
					ait.FaceToTarget(ait.FaceTarget, true);
				}
			}
			else
			{
				_stateOwner.HandleInnerCmd(FCCommand.CMD.STATE_DONE,null);
			}
			//Debug.Log("idle state");
			yield return null;
		}
		_stateOwner.ChangeState();
	}
}
