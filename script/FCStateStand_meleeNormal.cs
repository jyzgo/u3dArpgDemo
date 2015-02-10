using UnityEngine;
using System.Collections;

[AddComponentMenu("FC/Logic/FCObject/PlayAndEnemy/Agent/AI/State/MeleeNormal/Stand")]
public class FCStateStand_meleeNormal : FCStateAgent {

	public override void Run()
	{
		StartCoroutine(STATE());
	}
	
	void Awake()
	{
		_currentStateID = AIAgent.STATE.STAND;
	}
	
	IEnumerator STATE()
	{
		_inState = true;
		PlayAnimation();
		//Debug.Log(_stateOwner.ACOwner.AniGetAnimationNormalizedTime() );
		_stateOwner.StandTaskChange(FCCommand.CMD.STATE_ENTER);
		StateIn();
		while(_inState)
		{
			_stateOwner.StandTaskChange(FCCommand.CMD.STATE_UPDATE);
			StateProcess();
			yield return null;
		}
		StateOut();
		_stateOwner.StandTaskChange(FCCommand.CMD.STATE_QUIT);
		_stateOwner.ChangeState();
	}
}
