using UnityEngine;
using System.Collections;
using System;

public class FC_COMMAND_NETSTREAM
{
	public AIAgent.STATE _state;
	public Vector3 _currentPosition;
	public Vector3 _currentRotation;
	public int _commandIndex;
	public double timeStamp;
	
	public FC_COMMAND_NETSTREAM()
	{
	}
	public FC_COMMAND_NETSTREAM(FC_COMMAND_NETSTREAM src)
	{
		_state = src._state;
		_currentPosition = src._currentPosition;
		_currentRotation = src._currentRotation;
		_commandIndex = src._commandIndex;
		timeStamp = src.timeStamp;
	}
}
public class FCCommand
{
	public enum CMD
	{
		MOVE = 0,
		MOVE_TO_POINT,

			
		STOP = 100,
		STOP_IS_ARRIVE_POINT,
		
		SET = 200,
		SET_TO_DEFAULT_SPEED,
		SET_HITPOINT_CHANGEBY,
		
		STATE = 300,
		STATE_DONE,
		STATE_FINISH,
		STATE_ENTER,
		STATE_QUIT,
		STATE_GOTO,
		STATE_UPDATE,
		
		TARGET = 400,
		TARGET_BEGIN_TO_FIND,
		TARGET_FINDED,
		TARGET_IN_ATTACK_DISTANCE,
		TARGET_CHANGE,
		
		HURT = 500,

		
		INIT = 600,
		INIT_ATTACK_INFO,
		
		ATTACK = 700,
		ATTACK_IS_IN_ATTACK_MODE,
		ATTACK_WITH_SPEC_CONTS,
		ATTACK_HIT_TARGET,
		ATTACK_HIT_WALL,
		ATTACK_OUT_OF_RANGE,
		ATTACK_HIT_GROUND,
		
		
		DIRECTION = 800,
		DIRECTION_FOLLOW_FORWARD,
		DIRECTION_UNLOCK,
		DIRECTION_FACE_TARGET,
		
		DIE = 900,
		DIE_NORMAL,
		
		NET = 1000,
		NET_HAS_EVENT,
		
		INPUT = 1100,
		INPUT_KEY_PRESS,
		INPUT_KEY_RELEASE,
		
		//animation
		ANIMATION = 1200,
		ANIMATION_OVER,
		
		ROTATE = 1300,
		
		ACTION = 1400,
		ACTION_CANCEL,
		ACTION_DISMISS_TICKET,
		ACTION_GAIN_TICKET,
		ACTION_SHOULD_GOTO_ATTACK,
		ACTION_IS_NEAR_NPC,
		ACTION_IS_AWAY_NPC,
		ACTION_NEW_WAY,
		ACTION_TO_IDLE,
		ACTION_TO_ATTACK_POS_SYNC,
		ACTION_TO_HURT_POS_SYNC,
		ACTION_EOT,
		ACTION_TO_STAND,
		
		POTION = 1500,
		POTION_HP,
		POTION_ENERGY,
		
		POSITION = 1600,
		POSITION_LERP_TO,
		
		REVIVE = 1700,
		
		//from client 
		CLIENT = 10000,
		CLIENT_MOVE = 10000, 
		CLIENT_MOVE_TO_POINT, //only for sync pos		
		
		CLIENT_HURT = 10500,
		CLIENT_HURT_HP,
		
		CLIENT_THREAT = 11000,
		
		CLIENT_LEVELUP = 12000,
		
		CLIENT_CURRSTATE = 13000,
		
		CLIENT_POTION = 14000,
		CLIENT_POTION_HP,
		CLIENT_POTION_ENERGY,
		
		CLIENT_REVIVE = 14100,
		
		INVALID,
		
	};
	
	public enum CMD_HURT
	{
		NONE = CMD.HURT,
	}
	public enum STATE
	{
		DEACTIVE,
		RIGHTNOW,
		WAITING,
		SLEEP
	}
	public CMD _cmd;
	public object _param1;
	public object _param2;
	public object _param3;
	public FC_PARAM_TYPE _param1Type;
	public FC_PARAM_TYPE _param2Type;
	public FC_PARAM_TYPE _param3Type;
	public OBJECT_ID _objID;
	public STATE _state;
	public bool _isHost;
	
	public Vector3 _commandPosition;
	// 0 means command must be excute
	public int _cmdIndex = 0;
	public bool _isRun = false;
	public bool _needRunPerFrame = false;
	public bool _canDrop = false;
	public double _timestamp;
	
	public FCCommand()
	{
		_cmdIndex = 0;
		_cmd = CMD.INVALID;
		_isRun = false;
		_needRunPerFrame = false;
		_canDrop = false;
	}
	
	public FCCommand(int cmdIndex)
	{
		_cmdIndex = cmdIndex;
		_cmd = CMD.INVALID;
		_isRun = false;
		_needRunPerFrame = false;
		_canDrop = false;
	}
	
	public FCCommand(FC_COMMAND_NETSTREAM ecns)
	{
		_cmdIndex = ecns._commandIndex;
		if(ecns._state == AIAgent.STATE.RUN)
		{
			_cmd = CMD.ACTION_NEW_WAY;
		}
		else if(ecns._state == AIAgent.STATE.ATTACK)
		{
			_cmd = CMD.ACTION_TO_ATTACK_POS_SYNC;
		}
		else if(ecns._state == AIAgent.STATE.HURT)
		{
			_cmd = CMD.ACTION_TO_HURT_POS_SYNC;
		}
		else if(ecns._state == AIAgent.STATE.STAND)
		{
			_cmd = CMD.ACTION_TO_STAND;
		}
		else
		{
			_cmd = CMD.ACTION_TO_IDLE;
		}
		
		_param1 = ecns._currentPosition;
		_param2 = ecns._currentRotation;
		_param3 = ecns._state;
		_isRun = false;
		_needRunPerFrame = false;
		_canDrop = false;
	}
	
	public FCCommand(FCCommand ewc)
	{
		_cmdIndex = ewc._cmdIndex;
		_cmd = ewc._cmd;
		_param1 = ewc._param1;
		_param2 = ewc._param2;
		_param3 = ewc._param3;
		_param1Type = ewc._param1Type;
		_param2Type = ewc._param2Type;
		_param3Type = ewc._param3Type;
		_objID = ewc._objID;
		_state = ewc._state;
		_isHost = ewc._isHost;
	}
	
	public void Set(CMD cmd,OBJECT_ID objID,
		object param1,FC_PARAM_TYPE p1Type,
		object param2,FC_PARAM_TYPE p2Type,
		object param3,FC_PARAM_TYPE p3Type,
		STATE state,bool isHost)
	{
		_cmd = cmd;
		_param1 = param1;
		_param2 = param2;
		_param3 = param3;
		_param1Type = p1Type;
		_param2Type = p2Type;
		_param3Type = p3Type;
		_objID = objID;
		_state = state;
		_isHost = isHost;
	}
	
	public void Set(CMD cmd,OBJECT_ID objID,STATE state,bool isHost)
	{
		_cmd = cmd;
		_param1 = null;
		_param2 = null;
		_param3 = null;
		_param1Type = FC_PARAM_TYPE.NONE;
		_param2Type = FC_PARAM_TYPE.NONE;
		_param3Type = FC_PARAM_TYPE.NONE;
		_objID = objID;
		_state = state;
		_isHost = isHost;
	}
	
	public void Set(CMD cmd,OBJECT_ID objID,object param1,FC_PARAM_TYPE p1Type,STATE state,bool isHost)
	{
		_cmd = cmd;
		_param1 = param1;
		_param2 = null;
		_param3 = null;
		_param1Type = p1Type;
		_param2Type = FC_PARAM_TYPE.NONE;
		_param3Type = FC_PARAM_TYPE.NONE;
		_objID = objID;
		_state = state;
		_isHost = isHost;
	}
	
	public void Set(CMD cmd,OBJECT_ID objID,object param1,FC_PARAM_TYPE p1Type,object param2,FC_PARAM_TYPE p2Type,STATE state,bool isHost)
	{
		_cmd = cmd;
		_param1 = param1;
		_param2 = param2;
		_param3 = null;
		
		_param1Type = p1Type;
		_param2Type = p2Type;
		_param3Type = FC_PARAM_TYPE.NONE;
		_objID = objID;
		_state = state;
		_isHost = isHost;
	}
}
