using UnityEngine;
using System.Collections;

public class FCStateArmor1Break_normal : FCStateAgent 
{

	public override void Run()
	{
		StartCoroutine(STATE());
	}
	
	void Awake()
	{
		_currentStateID = AIAgent.STATE.ARMOR1_BROKEN;
	}
	
	IEnumerator STATE()
	{
		_inState = true;
		float timeCount = _stateOwner._timeForSuperArmor1Break;
		_stateOwner.Armor1BrokenTaskChange(FCCommand.CMD.STATE_ENTER);
		PlayAnimation();
		while(_inState)
		{
			if(timeCount >0)
			{
				timeCount -= Time.deltaTime;
				_stateOwner.Armor1BrokenTaskChange(FCCommand.CMD.STATE_UPDATE);
				if(timeCount <= 0)
				{
					_stateOwner.Armor1BrokenTaskChange(FCCommand.CMD.STATE_DONE);
				}
			}
			yield return null;
		}
		_stateOwner.Armor1BrokenTaskChange(FCCommand.CMD.STATE_QUIT);
		_stateOwner.ChangeState();
	}
}
