using UnityEngine;
using System.Collections;

[AddComponentMenu("FC/Logic/FCObject/PlayAndEnemy/Agent/AI/State/MeleeNormal/Run")]
public class FCStateRun_meleeNormal : FCStateAgent {
	
	public enum STATE
	{
		SEEK,
		PATROL,
		NORMAL
	}
	
	public override void Run()
	{
	
		int wantedSubStateID = -1;
		if(_stateOwner.HasPathWay && _stateOwner.RunOnPath)
		{
			wantedSubStateID = (int)STATE.PATROL;
		}
		else if( !_stateOwner.ACOwner.IsPlayerSelf &&
			_stateOwner.TargetAC != null && _stateOwner.IsOnSeek)
		{
			wantedSubStateID = (int)STATE.SEEK;
		}
		else
		{
			wantedSubStateID = (int)STATE.NORMAL;
		}
		StartSubState(wantedSubStateID);
		
		
		//play red trail effect
//		CharacterEffectManager.Instance.PlayEffect(FC_CHARACTER_EFFECT.TRAIL_RED, 
//			_stateOwner.ACOwner._avatarController, 
//			-1);	
	}
	
	void StartSubState(int idx)
	{
		_currentSubStateID = idx;
		PlayAnimation();
		if(_currentSubStateID == (int)STATE.PATROL && !_inState)
		{
			StartCoroutine(PATROL());
		}
		if(_currentSubStateID == (int)STATE.NORMAL && !_inState)
		{
			
			StartCoroutine(NORMAL());
		}
		if(_currentSubStateID == (int)STATE.SEEK && !_inState)
		{
			StartCoroutine(SEEK());
		}
	}
	public override void StopRun()
	{
		_inState = false;
		if(_currentSubStateID == (int)STATE.SEEK)
		{
			_stateOwner.IsOnSeek = false;
		}
		
		//stop red trail effect
//		CharacterEffectManager.Instance.StopEffect(FC_CHARACTER_EFFECT.TRAIL_RED, 
//			_stateOwner.ACOwner._avatarController, 
//			0);
	}
	
	public override bool CanGoto()
	{
		return(!_stateOwner.HasActionSwitchFlag(FC_ACTION_SWITCH_FLAG.CANT_MOVE));
	}
	
	void Awake()
	{
		_currentStateID = AIAgent.STATE.RUN;
	}
		
	IEnumerator SEEK()
	{
		_inState = true;
		float timeLast = 0.2f;
		_stateOwner.RunTaskChange(FCCommand.CMD.STATE_ENTER);
		StateIn();
		while(_inState)
		{
			if(_stateOwner.IsInAttackDistance)
			{
				_stateOwner.RunTaskChange(FCCommand.CMD.STATE_FINISH);
			}
			if(CanGoto())
			{
				if(timeLast >0)
				{
					timeLast -= Time.deltaTime;
					if(timeLast <=0.01f)
					{
						_stateOwner.RunTaskChange(FCCommand.CMD.STATE_DONE);
						timeLast+=0.2f;
						if(timeLast<=0)
						{
							timeLast = 0.2f;
						}
					}
				}
			}
			else
			{
				_stateOwner.RunTaskChange(FCCommand.CMD.STATE_DONE);
			}
			StateProcess();
			//Debug.Log("run state");
			yield return null;
		}
		StateOut();
		_stateOwner.RunTaskChange(FCCommand.CMD.STATE_QUIT);
		_stateOwner.ChangeState();
	}
	
	IEnumerator NORMAL()
	{
		_inState = true;
		float timeLast = 1f;
		_stateOwner.RunTaskChange(FCCommand.CMD.STATE_ENTER);
		StateIn();
		while(_inState)
		{
			_stateOwner.RunTaskChange(FCCommand.CMD.STATE_UPDATE);
			if(timeLast >0)
			{
				timeLast -= Time.deltaTime;
				if(timeLast<=0)
				{
					_stateOwner.InHostControl = true;
					timeLast = 1;
				}
			}
			StateProcess();
			yield return null;
		}
		StateOut();
		_stateOwner.RunTaskChange(FCCommand.CMD.STATE_QUIT);
		_stateOwner.ChangeState();
	}
	
	IEnumerator PATROL()
	{
		_inState = true;
		_stateOwner.RunTaskChange(FCCommand.CMD.STATE_ENTER);
		StateIn();
		while(_inState)
		{
			StateProcess();
			yield return null;
		}
		StateOut();
		_stateOwner.RunTaskChange(FCCommand.CMD.STATE_QUIT);
		_stateOwner.ChangeState();
	}
}
