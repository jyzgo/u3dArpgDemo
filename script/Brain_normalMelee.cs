using UnityEngine;
using System.Collections;

[AddComponentMenu("FC/Logic/FCObject/PlayAndEnemy/Agent/AI/BrainAgent/Brain_normalMelee")]
public class Brain_normalMelee : BrainAgent 
{
	protected AIAgent _owner;
	public override void Init(FCObject owner)
	{
		_owner = owner as AIAgent;
	}
	
	public override AIAgent.STATE GetNextStateByRun(bool findedTarget,bool inFinding)
	{
		AIAgent.STATE state = AIAgent.STATE.MAX;
		if(_owner.ACOwner.IsAlived)
		{
			if(findedTarget && _owner.TargetAC != null && _owner.TargetAC.IsAlived)
			{
				state = AIAgent.STATE.ATTACK;
			}
			else if(!inFinding)
			{
				state = AIAgent.STATE.IDLE;
			}
			else if(inFinding)
			{
				if(_owner.CanSeekTarget())
				{
					_owner.HandleInnerCmd(FCCommand.CMD.MOVE_TO_POINT,null,_owner.TargetAC.ThisTransform.localPosition,null,null);
				}
				else
				{
					_owner.HandleInnerCmd(FCCommand.CMD.STOP_IS_ARRIVE_POINT,null);
					state = AIAgent.STATE.IDLE;
				}
			}
			else
			{
				state = AIAgent.STATE.IDLE;
			}
		}
		else
		{
			state = AIAgent.STATE.DEAD;
		}
		return state;
	}
	
	public override AIAgent.STATE GetNextStateByBorn()
	{
		AIAgent.STATE state = AIAgent.STATE.MAX;
		state = AIAgent.STATE.IDLE;
		return state;
	}
}
