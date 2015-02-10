using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SyncCommand : MonoBehaviour {
	
	public short _myIndex = 0; //player index 0~3 + NETWORK_ID_HERO_START
	public Vector3 _myPosition = Vector3.zero;
	public Vector3 _myLastPosition = Vector3.zero;
	
	public Vector3 _myRotation = Vector3.zero; //eularAngles 
	public Vector3 _myLastRotation = Vector3.zero;//eularAngles
	
	public AIAgent.STATE _myAIState = AIAgent.STATE.NONE;
	public AIAgent.STATE _myLastAIState = AIAgent.STATE.NONE;
	
	FC_COMMAND_NETSTREAM ecns = new FC_COMMAND_NETSTREAM();
	
	private int _myCmdIdx = 0;
	public int CmdIdx 
	{
		get {return _myCmdIdx;}
	}
	
	public bool IsWriteEnabled {set ; get;}
	public bool IsReadEnabled  {set ; get;}
	
	void Start()
	{
		IsWriteEnabled = true;
		IsReadEnabled = true;
	}
	
    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if(!ObjectManager.Instance || ObjectManager.Instance.ObjectCount == 0) return;	
			
		short myIndex = 0; //player index, 0~3 + NETWORK_ID_HERO_START
		Vector3 pos = Vector3.zero; //player position
		Vector3 rot = Vector3.zero; //player rotation , eularAngles
		int cmdIdx = 0;
		short ai_state = -1; //player AI state
	
        if (stream.isWriting)
		{
			if(!IsWriteEnabled) return ;
			
			if(_myAIState == AIAgent.STATE.MAX) return;
			
			ActionController ac = ObjectManager.Instance.GetObjectByNetworkID(_myIndex).fcObj as ActionController;
			//output
			myIndex = (short)_myIndex;
			stream.Serialize(ref myIndex);
        			
            pos = ac.ThisTransform.localPosition;
			_myRotation = ac.ThisTransform.eulerAngles;
			_myAIState = ac.AIUse.AIStateAgent.CurrentStateID;
            stream.Serialize(ref pos);
			if(GameManager.Instance.IsPVPMode){	
				rot = _myRotation;
				stream.Serialize(ref rot);

				_myCmdIdx = CommandManager.Instance.GetNextCommandStreamID(_myIndex);
				cmdIdx = _myCmdIdx;
				stream.Serialize(ref cmdIdx);
				
				ai_state = (short)_myAIState;
				stream.Serialize(ref ai_state);
			}
        } 
		else
		{
			if(!IsReadEnabled) return;
			
			//read
            stream.Serialize(ref myIndex);
			_myIndex = myIndex;
		
            stream.Serialize(ref pos);
            _myPosition = pos;
			if(GameManager.Instance.IsPVPMode){
				stream.Serialize(ref rot);
            	_myRotation = rot;
				
				//sync command stream index
				stream.Serialize(ref cmdIdx);
				_myCmdIdx = cmdIdx;
				
				//ai state
				stream.Serialize(ref ai_state);
				_myAIState = (AIAgent.STATE)ai_state;
				
				ecns._commandIndex = _myCmdIdx;
				ecns._currentPosition = _myPosition;
				ecns._currentRotation = _myRotation;
				ecns._state = _myAIState;
				ecns.timeStamp = info.timestamp;
				CommandManager.Instance.WriteCommandToCache(_myIndex, _myPosition, true,_myCmdIdx ,  ecns );
			}
        }
	}
	
	
	//code for original serialize command
/*	
	
    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
		
		if (stream.isWriting) {
			//output package	
			
			List<FCCommand> commandList = CommandManager.Instance._outputCommandList;

			//optimize, remove redumdant message such as pos sync
			CommandManager.Instance.OptimizeOutputCommandList();
			
			int commandCount = commandList.Count;
					
			stream.Serialize(ref test);
			
			//save command count
			stream.Serialize(ref commandCount);			
	
			
			if (commandCount > 0)
			{
				//save each command
				for (int i=0; i<commandCount; i++)
				{
					FCCommand command = commandList[i];
						
					//save cmd
					int cmd = (int)command._cmd;
					stream.Serialize(ref cmd);
					
					//save network id
					int networkID = command._objID.NetworkId;
					stream.Serialize(ref networkID);
					
					//save params type, params 
					SerializeWriteParamByType(stream, command._param1, command._param1Type);
					SerializeWriteParamByType(stream, command._param2, command._param2Type);
					SerializeWriteParamByType(stream, command._param3, command._param3Type);					
				}
				
				//clear output message list
				//commandList.Clear();				
			}
			
        } 
		else {
			//input package
			stream.Serialize(ref test);
			
			int commandCount = 0;
            stream.Serialize(ref commandCount);
			
			
			//define input params 
			int cmd = 0;
			int networkID = 0;
			FC_PARAM_TYPE param1Type = FC_PARAM_TYPE.NONE;
			FC_PARAM_TYPE param2Type = FC_PARAM_TYPE.NONE;
			FC_PARAM_TYPE param3Type = FC_PARAM_TYPE.NONE;
			object param1 = null;
			object param2 = null;
			object param3 = null;
			
			
			for (int i=0; i<commandCount; i++)
			{
				Debug.Log("receive command count : " + commandCount);
		
				//read cmd
				stream.Serialize(ref cmd);
			
				//read network id
				stream.Serialize(ref networkID);
				
				//read param
				SerializeReadParamByType(stream, out param1, out param1Type);
				SerializeReadParamByType(stream, out param2, out param2Type);
				SerializeReadParamByType(stream, out param3, out param3Type);				
				
				//get object id by network id
				OBJECT_ID object_ID = ObjectManager.Instance.GetObjectByNetworkID(networkID);				
				if (object_ID == null)
				{
					Debug.LogError("get a command but no object id with network id : " + networkID);
				}
				else
				{
					FCCommand ewd = new FCCommand();
					ewd.Set((FCCommand.CMD)cmd,
						object_ID,
						param1, param1Type,
						param2, param2Type,
						param3, param3Type,
						FCCommand.STATE.RIGHTNOW, false);
					
					CommandManager.Instance._inputCommandList.Add(ewd);	
				}
				
			}
			
		}
    }

	
	//Serialize a param type and the param into the stream by its type
	private void SerializeWriteParamByType(BitStream stream, object param, FC_PARAM_TYPE paramType)
	{
		//I save all param types even if the 1st param type is none. I can discard the followings
		
		int paramTypeAsInt = (int)paramType;
		stream.Serialize(ref paramTypeAsInt);
	
		
		switch (paramType)
		{
		case FC_PARAM_TYPE.NONE:
			{
								
			}
			break;
		case FC_PARAM_TYPE.BOOL:
			{
				bool paramAsBool = (bool)param;
				stream.Serialize(ref paramAsBool);			
			}
			break;			
		case FC_PARAM_TYPE.FLOAT:
			{
				float paramAsFloat = (float)param;
				stream.Serialize(ref paramAsFloat);			
			}
			break;			
		case FC_PARAM_TYPE.INT:
			{
				int paramAsInt = (int)param;
				stream.Serialize(ref paramAsInt);			
			}
			break;			
		case FC_PARAM_TYPE.VECTOR3:
			{
				Vector3 paramAsVector = (Vector3)param;
				stream.Serialize(ref paramAsVector);			
			}
			break;					
		default:
			{
				Debug.LogError("invalid command param type :" + paramType);
			}
			break;
		}
	}
	
	//Serialize a param type and the param from the stream by its type
	private void SerializeReadParamByType(BitStream stream, out object param, out FC_PARAM_TYPE paramType)
	{
		param = null;
		
		//read param type
		int paramTypeAsInt = 0;
		stream.Serialize(ref paramTypeAsInt);
		paramType = (FC_PARAM_TYPE)paramTypeAsInt;
	
		
		switch (paramType)
		{
		case FC_PARAM_TYPE.NONE:
			{
								
			}
			break;
		case FC_PARAM_TYPE.BOOL:
			{
				bool paramAsBool = false;
				stream.Serialize(ref paramAsBool);	
				param = paramAsBool;
			}
			break;			
		case FC_PARAM_TYPE.FLOAT:
			{
				float paramAsFloat = 0.0f;
				stream.Serialize(ref paramAsFloat);	
				param = paramAsFloat;					
			}
			break;			
		case FC_PARAM_TYPE.INT:
			{
				int paramAsInt = 0;
				stream.Serialize(ref paramAsInt);	
				param = paramAsInt;						
			}
			break;			
		case FC_PARAM_TYPE.VECTOR3:
			{
				Vector3 paramAsVector = Vector3.zero;
				stream.Serialize(ref paramAsVector);	
				param = paramAsVector;				
			}
			break;					
		default:
			{
				Debug.LogError("invalid command param type :" + paramType);
			}
			break;
		}
		
	}	
	*/
	
}
