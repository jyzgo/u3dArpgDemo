using UnityEngine;
using System.Collections;

[AddComponentMenu("FC/Logic/FCObject/PlayAndEnemy/Agent/AI/State/MeleeNormal/Hurt")]
public class FCStateHurt_meleeNormal : FCStateAgent {
	
	FCHurtAgent _hurtAgent;
	
	public override void Init(AIAgent owner)
	{
		base.Init(owner);
		_hurtAgent = _stateOwner.HurtAgent;
	}
	
	public override void Run()
	{
		StartCoroutine(STATE());
	}
	
	void Awake()
	{
		_currentStateID = AIAgent.STATE.HURT;
	}
	
	IEnumerator STATE()
	{
		_inState = true;
		_lifeTime = 0;
		
		_stateOwner.HurtTaskChange(FCCommand.CMD.STATE_ENTER);
		PlayAnimation();
		StateIn();
		while(_inState)
		{
			_hurtAgent.HurtUpdate();
			StateProcess();
			//Debug.Log("hurt state");
			yield return null;
		}
		StateOut();
		_stateOwner.HurtTaskChange(FCCommand.CMD.STATE_QUIT);
		_stateOwner.ChangeState();
	}
}
