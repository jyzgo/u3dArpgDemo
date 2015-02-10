using UnityEngine;
using System.Collections;

[AddComponentMenu("FC/Logic/FCObject/PlayAndEnemy/AI/AIForTown")]
public class AIForTown : AIAgent {
	
	protected Transform _faceTarget = null;
	protected Vector3 _targetDirection = Vector3.zero;
	
	public Transform FaceTarget
	{	
		get
		{
			return _faceTarget;
		}
	}
	// Use this for initialization
	public override void ActiveAI()
	{
		base.ActiveAI();
		SetNextState(STATE.BORN);
		_owner.IsPlayerSelf = false;
		if(_owner.ThisObject.name.Contains("warrior"))
		{
			_owner.SwitchWeaponTo(EnumEquipSlot.weapon_hang, _defaultWeaponType);
		}
	}
	
	public override bool HandleInnerCmd(FCCommand.CMD cmd,object param0)
	{
		return HandleInnerCmd(cmd,param0,null,null,null);
	}
	
	public override bool HandleInnerCmd(FCCommand.CMD cmd,object param0,object param1,object param2,object param3)
	{
		bool ret = false;
		if(_sAHandleCmd != null)
		{
			ret = _sAHandleCmd(cmd,param0,param1,param2,param3);
		}
		if(!ret)
		{
			switch(cmd)
			{
			//state
			case FCCommand.CMD.STATE_GOTO:
			{
				AIAgent.STATE ast = (AIAgent.STATE)param0;
				ret = SetNextState(ast);
				break;
			}
			case FCCommand.CMD.STATE_ENTER:
				switch(_state.CurrentStateID)
				{
				case STATE.RUN:
				{
					GotoNextPathPoint();
					ret = true;
					break;
				}
				case STATE.IDLE:
				{
					if(_faceTarget == null)
					{
						_owner.ACRotateTo(_targetDirection,-1, true, true);
					}
					break;
				}
				}
				break;

			case FCCommand.CMD.STATE_DONE:
			{
				switch(_state.CurrentStateID)
				{	
				case STATE.BORN:
				{
					_owner.SetShipForSpeedAndForward(MoveAgent.ROTATE_FLAG.FOLLOW_SPEED);
					_owner.ACSetMoveMode(MoveAgent.MOVE_MODE.BY_MOVE_MANUAL);
					if(HasPathWay && !_controlByPlayer)
					{
						if(_pathway.HasPath)
						{
							_runOnPath = true;
							SetNextState(AIAgent.STATE.RUN);
						}
						else
						{
							_runOnPath = false;
							SetNextState(AIAgent.STATE.IDLE);
						}
					}
					else
					{
						_runOnPath = false;
						HandleInnerCmd(FCCommand.CMD.DIRECTION_FOLLOW_FORWARD,null);
						SetNextState(AIAgent.STATE.IDLE);
					}
					ret = true;
					break;
				}
				case STATE.RUN:
				{
					ret = SetNextState(AIAgent.STATE.IDLE);
					break;
				}
				case STATE.IDLE:
				{
					ret = SetNextState(AIAgent.STATE.RUN);
					break;
				}
				}	
				
				break;
			}
	
			case FCCommand.CMD.MOVE_TO_POINT:
			{
				Vector3 p = (Vector3)param1;
				_owner.ACMove(ref p);
				ret = true;
				break;
			}
			

			//stop
			case FCCommand.CMD.STOP:
				_owner.ACStop();
				ret = true;
				break;
			case FCCommand.CMD.STOP_IS_ARRIVE_POINT:
			{
				_targetDirection = _owner.ThisTransform.forward;
				SetNextState(AIAgent.STATE.IDLE);
				break;
			}
				
			//Set
			case FCCommand.CMD.SET_TO_DEFAULT_SPEED:
			{
				ACOwner.CurrentSpeed = _owner.Data.TotalMoveSpeed;
				ret = true;
				break;
			}

				
			//direction
			case FCCommand.CMD.DIRECTION_FOLLOW_FORWARD:
			{
				_owner.SetShipForSpeedAndForward(MoveAgent.ROTATE_FLAG.FOLLOW_SPEED);
				ret = true;
				break;
			}
			case FCCommand.CMD.DIRECTION_UNLOCK:
			{
				_owner.SetShipForSpeedAndForward(MoveAgent.ROTATE_FLAG.UNLOCK);
				ret = true;
				break;
			}
				
			//action
			case FCCommand.CMD.ACTION_IS_AWAY_NPC:
			{
				Transform tf = (Transform) param1;
				if(tf == _faceTarget)
				{
					_faceTarget = null;
				}
				ret = true;
				break;
			}
				
			case FCCommand.CMD.ACTION_IS_NEAR_NPC:
			{
				Transform tf = (Transform) param1;
				_faceTarget = tf;
				ret = true;
				break;
			}
			}
		}
		return ret;
		
	}

	public override bool HandleCommand(ref FCCommand ewd)
	{
		int rootcmd = ((int)ewd._cmd/100)*100;
		int subcmd = ((int)ewd._cmd);
		bool canrun = true;
		bool ret = false;
		
		if(canrun)
		{
			if(rootcmd <0 
				|| (rootcmd == (int)FCCommand.CMD.MOVE && HasActionSwitchFlag(FC_ACTION_SWITCH_FLAG.CANT_MOVE))
			)
			{
				canrun = false;
			}
		}

		if(canrun)
		{
			switch((FCCommand.CMD)rootcmd)
			{
			case FCCommand.CMD.MOVE:
				if(_state != null && _state.CurrentStateID == AIAgent.STATE.IDLE)
				{
					ret = SetNextState(AIAgent.STATE.RUN);
				}
				break;
			}
			if(subcmd>=0)
			{
				ret = HandleInnerCmd((FCCommand.CMD)subcmd,null,ewd._param1,ewd._param2,ewd._param3);
			}
		}
		return ret;
	}
}
