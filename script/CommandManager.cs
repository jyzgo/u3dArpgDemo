using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("FC/Logic/Manager/CommandManager")]

public class CommandManager : Photon.MonoBehaviour {
	
	[System.Serializable]
	public class CommandFilters
	{
		public FC_AI_TYPE _aiType;
		public FC_NET_SYNC_TYPE[] _typeList;
	}
	public CommandFilters[] _cmdFiltersList;

	private Dictionary<int , int> _commandStreamIdDic = new Dictionary<int , int>();
	
	//lists for command list for sync
	private bool _useSyncMode = true;
	public List<FCCommand> _outputCommandList = new List<FCCommand>();
	public List<FCCommand> _inputCommandList = new List<FCCommand>();
	public GameObject _syncCommandObjectPrefeb = null; //
	public SyncCommand _syncCommandScript = null; //my allocated sync obj's script
	
	private List<SyncCommand> _syncCommandScripArray = new List<SyncCommand>();
	private Dictionary<int, List<FC_COMMAND_NETSTREAM>> _commandCmdStreamCache = new  Dictionary<int, List<FC_COMMAND_NETSTREAM>>();//for player
	private Dictionary<int, List<FCCommand>> _commandRpcCache = new  Dictionary<int, List<FCCommand>>();//for player
		//SyncCommand[] _syncCommandScripArray = new SyncCommand[FC_CONST.MAX_PLAYERS];
	
	private int[] _syncTypeFlag;
	
	private Dictionary<int,List<FCCommand>> _commandArray = new Dictionary<int, List<FCCommand>>();
	List<FCCommand> _deActiveCommandList = new List<FCCommand>();
	
	private static CommandManager _instance;
	private FCCommand _fastCommand;
	
	public static CommandManager Instance
	{
		get
		{
			return _instance;
		}
	}
	
	void OnDestroy() {
		if(_instance == this) {
			_instance = null;
		}
	}
	
	void Awake()
	{
		
		_instance = this;
		_fastCommand = new FCCommand();
		_syncTypeFlag = new int[(int)FC_AI_TYPE.MAX];
		for(int i =0;i<_syncTypeFlag.Length;i++)
		{
			_syncTypeFlag[i] = 0;
			Utils.SetFlag((int)FC_NET_SYNC_TYPE.HIT_PONIT, ref _syncTypeFlag[i]);
		}
		foreach(CommandFilters cfs in _cmdFiltersList)
		{
			foreach(FC_NET_SYNC_TYPE nst in cfs._typeList)
			{
				Utils.SetFlag((int)nst, ref _syncTypeFlag[(int)cfs._aiType]);
			}
		}
		
		_commandStreamIdDic.Clear();
		
		if (_useSyncMode && (_syncCommandScript == null))
			if ((_syncCommandObjectPrefeb != null) &&  (PhotonNetwork.room != null))
			{
				//network instantiate sync obj
				GameObject syncCommandObject = PhotonNetwork.Instantiate(_syncCommandObjectPrefeb.name,
					transform.position, transform.rotation, 0) as GameObject;
			
				_syncCommandScript = syncCommandObject.GetComponent<SyncCommand>();
				if (_syncCommandScript == null)
					Debug.LogError("error, sync command object do not have a sync script");		
			}
	}
	
	void FixedUpdate()
	{
		if(PhotonNetwork.room != null){
			//single player will not send comment to others
			UpdateInputCommanList();
			UpdateOutputCommanList();
			UpdateSyncCommands();
		}
	}
	
	public bool CanSendCommand(int objectType,FC_NET_SYNC_TYPE enst)
	{
		int ot = objectType & 0xff;
		if(Utils.HasFlag((int)enst,_syncTypeFlag[ot]))
		{
			return true;
		}
		return false;
	}

	public int GetNextCommandStreamID(int networkIndex)
	{
		if(!_commandStreamIdDic.ContainsKey(networkIndex))
		{
			_commandStreamIdDic[networkIndex] = 0;
		}else{
			_commandStreamIdDic[networkIndex]++;
		}

		return _commandStreamIdDic[networkIndex];
	}
	
	//I need this way to reduce copy params
	public bool Send(FCCommand.CMD cmd,object param1,FC_PARAM_TYPE p1Type,object param2,FC_PARAM_TYPE p2Type,object param3,FC_PARAM_TYPE p3Type,OBJECT_ID objectID,FCCommand.STATE state,bool isHost)
	{
		bool ret = false;
		if(state != FCCommand.STATE.RIGHTNOW)
		{
			FCCommand ewd = null;
			if(_deActiveCommandList.Count != 0)
			{
				ewd = _deActiveCommandList[0];
				_deActiveCommandList.Remove(ewd);
			}
			else
			{
				ewd= new FCCommand();
			}
			ewd.Set(cmd,objectID,param1,p1Type,param2,p2Type,param3,p3Type,state,isHost);
			AddCmdToArray(ewd);
		}
		else
		{
			_fastCommand.Set(cmd,objectID,param1,p1Type,param2,p2Type,param3,p3Type,state,isHost);
			if(objectID.HandleCommand(ref _fastCommand))
			{
				_fastCommand._objID = null;
				ret = true;
			}
			else
			{
				//AddCmdToArray(ewd);
				// may need to add it to command list
			}
		}
		return ret;
	}
	
	public bool Send(FCCommand.CMD cmd,object param1,FC_PARAM_TYPE p1Type,object param2,FC_PARAM_TYPE p2Type,OBJECT_ID objectID,FCCommand.STATE state,bool isHost)
	{
		bool ret = false;	
		if(state != FCCommand.STATE.RIGHTNOW)
		{
			FCCommand ewd = null;
			if(_deActiveCommandList.Count != 0)
			{
				ewd = _deActiveCommandList[0];
				_deActiveCommandList.Remove(ewd);
			}
			else
			{
				ewd= new FCCommand();
			}
			ewd.Set(cmd,objectID,param1,p1Type,param2,p2Type,state,isHost);
			AddCmdToArray(ewd);
		}
		else
		{
			_fastCommand.Set(cmd,objectID,param1,p1Type,param2,p2Type,state,isHost);
			if(objectID.HandleCommand(ref _fastCommand))
			{
				_fastCommand._objID = null;
				ret = true;
			}
			else
			{
				//AddCmdToArray(ewd);
				// may need to add it to command list
			}
		}
		return ret;
	}
	
	public bool Send(FCCommand.CMD cmd,object param1,FC_PARAM_TYPE p1Type, OBJECT_ID objectID,FCCommand.STATE state,bool isHost)
	{
		bool ret = false;
		if(state != FCCommand.STATE.RIGHTNOW)
		{
			FCCommand ewd = null;
			if(_deActiveCommandList.Count != 0)
			{
				ewd = _deActiveCommandList[0];
				_deActiveCommandList.Remove(ewd);
			}
			else
			{
				ewd= new FCCommand();
			}
			ewd.Set(cmd,objectID,param1,p1Type,state,isHost);
			AddCmdToArray(ewd);
		}
		else
		{
			_fastCommand.Set(cmd,objectID,param1,p1Type,state,isHost);
			if(objectID.HandleCommand(ref _fastCommand))
			{
				_fastCommand._objID = null;
				ret = true;
			}
			else
			{
				//AddCmdToArray(ewd);
				// may need to add it to command list
			}
		}
		return ret;
	}
	
	public bool Send(FCCommand.CMD cmd,OBJECT_ID objectID,FCCommand.STATE state,bool isHost)
	{
		bool ret = false;
		if(state != FCCommand.STATE.RIGHTNOW)
		{
			FCCommand ewd = null;
			if(_deActiveCommandList.Count != 0)
			{
				ewd = _deActiveCommandList[0];
				_deActiveCommandList.Remove(ewd);
			}
			else
			{
				ewd= new FCCommand();
			}
			ewd.Set(cmd,objectID,state,isHost);
			AddCmdToArray(ewd);
		}
		else
		{
			_fastCommand.Set(cmd,objectID,state,isHost);
			if(objectID.HandleCommand(ref _fastCommand))
			{
				_fastCommand._objID = null;
				ret = true;
			}
			else
			{
				//AddCmdToArray(ewd);
				// may need to add it to command list
			}
		}
		return ret;
	}
	
	
	
	public bool SendFastToSelf(ref FCCommand ewd)
	{
		return ewd._objID.HandleCommand(ref ewd);
	}
	
	public bool SendFast(ref FCCommand ewd,FCObject eb)
	{
		if(ewd._cmd == FCCommand.CMD.INVALID) return false;
		
		if(ewd._cmd == FCCommand.CMD.ATTACK_HIT_TARGET)
		{
			ActionController ac = ewd._param1 as ActionController;
			if(ac.IsPlayerSelf)
			{
				//ac.
			}
		}
		return eb.ObjectID.HandleCommand(ref ewd);
	}
	
	public void AddCmdToArray(FCCommand ewd)
	{
		List<FCCommand> ewdl = null;
		if(_commandArray.ContainsKey((int)ewd._objID))
		{
			ewdl = _commandArray[(int)ewd._objID];
		}
		else
		{
			ewdl = new List<FCCommand>();
			_commandArray[(int)ewd._objID] = ewdl;
		}
		ewdl.Add(ewd);
		Send(FCCommand.CMD.NET_HAS_EVENT,ewdl[0],FC_PARAM_TYPE.OTHERS,ewdl[0]._objID,FCCommand.STATE.RIGHTNOW,true);
	}
	
	public void SetNextNetCommand(int objID)
	{
		List<FCCommand> ewdl = _commandArray[objID];
		if(ewdl == null)
		{
			Send(FCCommand.CMD.NET_HAS_EVENT,null,FC_PARAM_TYPE.OTHERS,ObjectManager.Instance.GetObjectByID(objID) ,FCCommand.STATE.RIGHTNOW,true);
		}
		else if(ewdl.Count <2)
		{
			ewdl.Clear();
			Send(FCCommand.CMD.NET_HAS_EVENT,null,FC_PARAM_TYPE.OTHERS,ObjectManager.Instance.GetObjectByID(objID) ,FCCommand.STATE.RIGHTNOW,true);
		}
		else
		{
			ewdl.RemoveAt(0);
			Send(FCCommand.CMD.NET_HAS_EVENT,ewdl[0],FC_PARAM_TYPE.OTHERS,ewdl[0]._objID ,FCCommand.STATE.RIGHTNOW,true);
		}
	}
	
	
	#region network methods

	//receive msg, 0 param
	[RPC]
    void ReceiveCommand0(int cmd, int networkID, int commandIndex, Vector3 commandPos, PhotonMessageInfo msgInfo) {
		
		Debug.Log("[RPC] [receive] command:" + (FCCommand.CMD)cmd + " obj:" + networkID + " from player:" + msgInfo.sender  
						+ " delayMS : " + ((int)((PhotonNetwork.time - msgInfo.timestamp) * 1000)).ToString());
		
		//put the command into input command list
		OBJECT_ID object_ID = ObjectManager.Instance.GetObjectByNetworkID(networkID);

		if (object_ID == null)
			Debug.Log("command target object is not exist: " + networkID);
		else
		{
			FCCommand ewd = new FCCommand();
			ewd.Set((FCCommand.CMD)cmd,
				object_ID,
				null, FC_PARAM_TYPE.NONE,
				null, FC_PARAM_TYPE.NONE,
				null, FC_PARAM_TYPE.NONE,
				FCCommand.STATE.RIGHTNOW,
				false);
			WriteCommandToCache(object_ID.NetworkId, commandPos, false, commandIndex, ewd);
		}
    }	
	
	//receive msg, 1 vector
	[RPC]
    void ReceiveCommand1v(int cmd, Vector3 vectorParam, int networkID, int commandIndex, Vector3 commandPos, PhotonMessageInfo msgInfo) {
		
		Debug.Log("[RPC] [receive] command:" + (FCCommand.CMD)cmd + " obj:" + networkID + " from player:" + msgInfo.sender + " Vector:" + vectorParam  
						+ " delayMS : " + ((int)((PhotonNetwork.time - msgInfo.timestamp) * 1000)).ToString());
		
		//put the command into input command list
		OBJECT_ID object_ID = ObjectManager.Instance.GetObjectByNetworkID(networkID);

		if (object_ID == null)
			Debug.Log("command target object is not exist: " + networkID);
		else
		{
			FCCommand ewd = new FCCommand();
			ewd.Set((FCCommand.CMD)cmd,
				object_ID,
				vectorParam, FC_PARAM_TYPE.VECTOR3,
				null, FC_PARAM_TYPE.NONE,
				null, FC_PARAM_TYPE.NONE,
				FCCommand.STATE.RIGHTNOW,
				false);
			
			WriteCommandToCache(object_ID.NetworkId, commandPos, false, commandIndex, ewd);
		}
    }			
	
	
	//receive msg, 1 float
	[RPC]
    void ReceiveCommand1f(int cmd, float floatParam, int networkID, int commandIndex, Vector3 commandPos, PhotonMessageInfo msgInfo) {
		
		Debug.Log("[RPC] [receive] command:" + (FCCommand.CMD)cmd + " obj:" + networkID + " from player:" + msgInfo.sender + " Float:" + floatParam  
						+ " delayMS : " + ((int)((PhotonNetwork.time - msgInfo.timestamp) * 1000)).ToString());
			
		//put the command into input command list
		OBJECT_ID object_ID = ObjectManager.Instance.GetObjectByNetworkID(networkID);

		if (object_ID == null)
			Debug.Log("command target object is not exist: " + networkID);
		else
		{
			FCCommand ewd = new FCCommand();
			ewd.Set((FCCommand.CMD)cmd,
				object_ID,
				floatParam, FC_PARAM_TYPE.FLOAT,
				null, FC_PARAM_TYPE.NONE,
				null, FC_PARAM_TYPE.NONE,
				FCCommand.STATE.RIGHTNOW,
				false);
			
			WriteCommandToCache(object_ID.NetworkId, commandPos, false, commandIndex, ewd);
		}
    }			
	
	
	//receive msg, 1 int
	[RPC]
    void ReceiveCommand1i(int cmd, int intParam, int networkID, int commandIndex, Vector3 commandPos, PhotonMessageInfo msgInfo) {
		
		Debug.Log("[RPC] [receive] command:" + (FCCommand.CMD)cmd + " obj:" + networkID + " from player:" + msgInfo.sender + " Int:" + intParam  
						+ " delayMS : " + ((int)((PhotonNetwork.time - msgInfo.timestamp) * 1000)).ToString());
			
		//put the command into input command list
		OBJECT_ID object_ID = ObjectManager.Instance.GetObjectByNetworkID(networkID);

		if (object_ID == null)
			Debug.Log("command target object is not exist: " + networkID);
		else
		{
			FCCommand ewd = new FCCommand();
			ewd.Set((FCCommand.CMD)cmd,
				object_ID,
				intParam, FC_PARAM_TYPE.INT,
				null, FC_PARAM_TYPE.NONE,
				null, FC_PARAM_TYPE.NONE,
				FCCommand.STATE.RIGHTNOW,
				false);
			
			WriteCommandToCache(object_ID.NetworkId, commandPos, false, commandIndex, ewd);
		}
    }		
	
	//receive msg, 1 int, 2 int
	[RPC]
    void ReceiveCommand1i2i(int cmd, int intParam1, int intParam2, int networkID, int commandIndex, Vector3 commandPos, PhotonMessageInfo msgInfo) {
		
		Debug.Log("[RPC] [receive] command:" + (FCCommand.CMD)cmd + " obj:" + networkID + " from player:" + msgInfo.sender 
						+ " delayMS : " + ((int)((PhotonNetwork.time - msgInfo.timestamp) * 1000)).ToString());
		
			
		//put the command into input command list
		OBJECT_ID object_ID = ObjectManager.Instance.GetObjectByNetworkID(networkID);

		if (object_ID == null)
			Debug.Log("command target object is not exist: " + networkID);
		else
		{
			FCCommand ewd = new FCCommand();
			ewd.Set((FCCommand.CMD)cmd,
				object_ID,
				intParam1, FC_PARAM_TYPE.INT,
				intParam2, FC_PARAM_TYPE.INT,
				null, FC_PARAM_TYPE.NONE,
				FCCommand.STATE.RIGHTNOW,
				false);
			
			WriteCommandToCache(object_ID.NetworkId, commandPos, false, commandIndex, ewd);
		}
    }		
	
	//receive msg, 1 int, 2 int, 3 int
	[RPC]
    void ReceiveCommand1i2i3i(int cmd, int intParam1, int intParam2, int intParam3, int networkID, int commandIndex, Vector3 commandPos, PhotonMessageInfo msgInfo) {
		
		Debug.Log("[RPC] [receive] command:" + (FCCommand.CMD)cmd + " obj:" + networkID + " from player:" + msgInfo.sender  
						+ " delayMS : " + ((int)((PhotonNetwork.time - msgInfo.timestamp) * 1000)).ToString());
			
		//put the command into input command list
		OBJECT_ID object_ID = ObjectManager.Instance.GetObjectByNetworkID(networkID);

		if (object_ID == null)
			Debug.Log("command target object is not exist: " + networkID);
		else
		{
			FCCommand ewd = new FCCommand();
			ewd.Set((FCCommand.CMD)cmd,
				object_ID,
				intParam1, FC_PARAM_TYPE.INT,
				intParam2, FC_PARAM_TYPE.INT,
				intParam3, FC_PARAM_TYPE.INT,
				FCCommand.STATE.RIGHTNOW,
				false);
			
			WriteCommandToCache(object_ID.NetworkId, commandPos, false, commandIndex, ewd);
		}
    }		
	
	//receive msg, 1 int, 2 int, 3 float
	[RPC]
    void ReceiveCommand1i2i3f(int cmd, int intParam1, int intParam2, float floatParam3, int networkID, int commandIndex, Vector3 commandPos, PhotonMessageInfo msgInfo) {
		
		Debug.Log("[RPC] [receive] command:" + (FCCommand.CMD)cmd + " obj:" + networkID + " from player:" + msgInfo.sender  
						+ " delayMS : " + ((int)((PhotonNetwork.time - msgInfo.timestamp) * 1000)).ToString());
			
		//put the command into input command list
		OBJECT_ID object_ID = ObjectManager.Instance.GetObjectByNetworkID(networkID);

		if (object_ID == null)
			Debug.Log("command target object is not exist: " + networkID);
		else
		{
			FCCommand ewd = new FCCommand();
			ewd.Set((FCCommand.CMD)cmd,
				object_ID,
				intParam1, FC_PARAM_TYPE.INT,
				intParam2, FC_PARAM_TYPE.INT,
				floatParam3, FC_PARAM_TYPE.FLOAT,
				FCCommand.STATE.RIGHTNOW,
				false);
			
			WriteCommandToCache(object_ID.NetworkId, commandPos, false, commandIndex, ewd);
		}
    }			
	
	
	//receive msg, 1 float, 2 float
	[RPC]
    void ReceiveCommand1f2f(int cmd, float floatParam1, float floatParam2, int networkID, int commandIndex, Vector3 commandPos, PhotonMessageInfo msgInfo) {
		
		Debug.Log("[RPC] [receive] command:" + (FCCommand.CMD)cmd + " obj:" + networkID + " from player:" + msgInfo.sender  
						+ " delayMS : " + ((int)((PhotonNetwork.time - msgInfo.timestamp) * 1000)).ToString());
			
		//put the command into input command list
		OBJECT_ID object_ID = ObjectManager.Instance.GetObjectByNetworkID(networkID);

		if (object_ID == null)
			Debug.Log("command target object is not exist: " + networkID);
		else
		{
			FCCommand ewd = new FCCommand();
			ewd.Set((FCCommand.CMD)cmd,
				object_ID,
				floatParam1, FC_PARAM_TYPE.FLOAT,
				floatParam2, FC_PARAM_TYPE.FLOAT,
				null, FC_PARAM_TYPE.NONE,
				FCCommand.STATE.RIGHTNOW,
				false);
			
			WriteCommandToCache(object_ID.NetworkId, commandPos, false, commandIndex, ewd);
		}
    }	
	
	//receive msg, 1 Vector3, 2 Vector3
	[RPC]
    void ReceiveCommand2v(int cmd, Vector3 vParam1, Vector3 vParam2 , int networkID, int commandIndex, Vector3 commandPos, PhotonMessageInfo msgInfo) {
		
		Debug.Log("[RPC] [receive] command:" + (FCCommand.CMD)cmd + " obj:" + networkID + " from player:" + msgInfo.sender  
						+ " delayMS : " + ((int)((PhotonNetwork.time - msgInfo.timestamp) * 1000)).ToString());
		
		//put the command into input command list
		OBJECT_ID object_ID = ObjectManager.Instance.GetObjectByNetworkID(networkID);

		if (object_ID == null)
			Debug.Log("command target object is not exist: " + networkID);
		else
		{
			FCCommand ewd = new FCCommand();
			ewd.Set((FCCommand.CMD)cmd,
				object_ID,
				vParam1, FC_PARAM_TYPE.VECTOR3,
				vParam2, FC_PARAM_TYPE.VECTOR3,
				null, FC_PARAM_TYPE.NONE,
				FCCommand.STATE.RIGHTNOW,
				false);
			
			WriteCommandToCache(object_ID.NetworkId, commandPos, false, commandIndex, ewd);
		}
    }	
	
	//receive msg, 2 Quaternion
	[RPC]
    void ReceiveCommand2q(int cmd, Quaternion qParam1, Quaternion qParam2 , int networkID, int commandIndex, Vector3 commandPos, PhotonMessageInfo msgInfo) {
		
		//put the command into input command list
		OBJECT_ID object_ID = ObjectManager.Instance.GetObjectByNetworkID(networkID);

		if (object_ID == null)
			Debug.Log("command target object is not exist: " + networkID);
		else
		{
			FCCommand ewd = new FCCommand();
			ewd.Set((FCCommand.CMD)cmd,
				object_ID,
				qParam1, FC_PARAM_TYPE.QUATERNION,
				qParam2, FC_PARAM_TYPE.QUATERNION,
				null, FC_PARAM_TYPE.NONE,
				FCCommand.STATE.RIGHTNOW,
				false);
			
			WriteCommandToCache(object_ID.NetworkId, commandPos, false, commandIndex, ewd);
		}
    }	
	
	
	//send msg to all clients except me

	public void SendCommandToOthers(FCCommand.CMD cmd, 
			OBJECT_ID objID,
			Vector3 commandPosition,
			object param1,
			FC_PARAM_TYPE p1Type,
			object param2,
			FC_PARAM_TYPE p2Type,
			object param3,
			FC_PARAM_TYPE p3Type
		)
	{
	
		//single player will not send comment to others
		if(PhotonNetwork.room == null)
			return;
		
		Debug.Log("[send] command:" + cmd + " , obj_network_id:" + objID.NetworkId);
		
		//build command instance and add into output list
		FCCommand ewd = new FCCommand(GetNextCommandStreamID(objID.NetworkId));
		ewd.Set((FCCommand.CMD)cmd, objID, param1, p1Type, param2, p2Type, param3, p3Type,
			FCCommand.STATE.RIGHTNOW, false);
		ewd._commandPosition = commandPosition;
		if(CheatManager.netDelay >0)
		{
			StartCoroutine(AddCommand(ewd));
		}
		else
		{
			_outputCommandList.Add(ewd);
		}
		//
		//_outputCommandList.Add(ewd);	
	}	
	
	private IEnumerator AddCommand(FCCommand ewd)
	{
		yield return new WaitForSeconds(CheatManager.netDelay);
		_outputCommandList.Add(ewd);
	}
	//optimize the output command list, remove redundant commands
	public void OptimizeOutputCommandList()
	{
	}
	

	
	//update and distribute output command list
	private void UpdateOutputCommanList()
	{

		//check output command list and distribute
		int outputCommandListCount = _outputCommandList.Count;

		if (outputCommandListCount > 0)
		{
			for (int i=0; i<outputCommandListCount; i++)
			{
				//handle commands
				FCCommand command = _outputCommandList[i];			

				// 0 param
				if (command._param1Type == FC_PARAM_TYPE.NONE)
				{
					photonView.RPC("ReceiveCommand0",
						PhotonTargets.OthersBuffered,
						(int)command._cmd,
						command._objID.NetworkId,
						GetNextCommandStreamID(command._objID.NetworkId),
						command._commandPosition);
					continue;					
				}
				
				
				// 1 param, vector
				if ((command._param1Type == FC_PARAM_TYPE.VECTOR3) && 
					(command._param2Type == FC_PARAM_TYPE.NONE))
				{
					photonView.RPC("ReceiveCommand1v",
						PhotonTargets.Others,
						(int)command._cmd,
						(Vector3)command._param1,
						command._objID.NetworkId,
						GetNextCommandStreamID(command._objID.NetworkId),
						command._commandPosition);
					continue;					
				}			
				
				// 1 param, float
				if ((command._param1Type == FC_PARAM_TYPE.FLOAT) &&
					(command._param2Type == FC_PARAM_TYPE.NONE))				
				{
					photonView.RPC("ReceiveCommand1f",
						PhotonTargets.Others,
						(int)command._cmd,
						(float)command._param1,
						command._objID.NetworkId,
						GetNextCommandStreamID(command._objID.NetworkId),
						command._commandPosition);
					continue;					
				}		
				
				// 1 param, int
				if ((command._param1Type == FC_PARAM_TYPE.INT) &&
					(command._param2Type == FC_PARAM_TYPE.NONE))				
				{
					photonView.RPC("ReceiveCommand1i",
						PhotonTargets.Others,
						(int)command._cmd,
						(int)command._param1,
						command._objID.NetworkId,
						GetNextCommandStreamID(command._objID.NetworkId),
						command._commandPosition);
					continue;					
				}				
				
				// 2 param, int, int
				if ((command._param1Type == FC_PARAM_TYPE.INT) &&
					(command._param2Type == FC_PARAM_TYPE.INT) &&
					(command._param3Type == FC_PARAM_TYPE.NONE))				
				{
					photonView.RPC("ReceiveCommand1i2i",
						PhotonTargets.Others,
						(int)command._cmd,
						(int)command._param1,
						(int)command._param2,
						command._objID.NetworkId,
						GetNextCommandStreamID(command._objID.NetworkId),
						command._commandPosition);
					continue;					
				}					
		
				// 3 param, int, int, int
				if ((command._param1Type == FC_PARAM_TYPE.INT) &&
					(command._param2Type == FC_PARAM_TYPE.INT) &&
					(command._param3Type == FC_PARAM_TYPE.INT))				
				{
					photonView.RPC("ReceiveCommand1i2i3i",
						PhotonTargets.Others,
						(int)command._cmd,
						(int)command._param1,
						(int)command._param2,
						(int)command._param3,
						command._objID.NetworkId,
						GetNextCommandStreamID(command._objID.NetworkId),
						command._commandPosition);
					continue;					
				}	
				
				// 3 param, int, int, float
				if ((command._param1Type == FC_PARAM_TYPE.INT) &&
					(command._param2Type == FC_PARAM_TYPE.INT) &&
					(command._param3Type == FC_PARAM_TYPE.FLOAT))				
				{
					photonView.RPC("ReceiveCommand1i2i3f",
						PhotonTargets.Others,
						(int)command._cmd,
						(int)command._param1,
						(int)command._param2,
						(float)command._param3,
						command._objID.NetworkId,
						GetNextCommandStreamID(command._objID.NetworkId),
						command._commandPosition);
					continue;					
				}	
				
				// 2 param, float, float
				if ((command._param1Type == FC_PARAM_TYPE.FLOAT) &&
					(command._param2Type == FC_PARAM_TYPE.FLOAT))				
				{
					photonView.RPC("ReceiveCommand1f2f",
						PhotonTargets.Others,
						(int)command._cmd,
						(float)command._param1,
						(float)command._param2,
						command._objID.NetworkId,
						GetNextCommandStreamID(command._objID.NetworkId),
						command._commandPosition);
					continue;					
				}			
				
				// 2 param, vector3 , vector3
				if ((command._param1Type == FC_PARAM_TYPE.VECTOR3) && 
					(command._param2Type == FC_PARAM_TYPE.VECTOR3))
				{
					photonView.RPC("ReceiveCommand2v",
						PhotonTargets.Others,
						(int)command._cmd,
						(Vector3)command._param1,
						(Vector3)command._param2,
						command._objID.NetworkId,
						GetNextCommandStreamID(command._objID.NetworkId),
						command._commandPosition);
					continue;					
				}	
				
				// 2 param, Quaternion , vector3
				if ((command._param1Type == FC_PARAM_TYPE.QUATERNION) && 
					(command._param2Type == FC_PARAM_TYPE.QUATERNION))
				{
					photonView.RPC("ReceiveCommand2q",
						PhotonTargets.Others,
						(int)command._cmd,
						(Quaternion)command._param1,
						(Quaternion)command._param2,
						command._objID.NetworkId,
						GetNextCommandStreamID(command._objID.NetworkId),
						command._commandPosition);
					continue;					
				}	
			}
			
			
			//clear output command list
			_outputCommandList.Clear();
		}
	
	}	
	//update and distribute input command list
	private void UpdateInputCommanList()
	{
		//check input command list and distribute
		int inputCommandListCount = _inputCommandList.Count;

		if (inputCommandListCount > 0)
		{

			Debug.Log("[receive] command count: " + inputCommandListCount);		

			for (int i=0; i<inputCommandListCount; i++)
			{
				//handle commands
				FCCommand command = _inputCommandList[i];
				Debug.Log("[receive] command : " + command._cmd);	
				
				
				switch(command._cmd)
				{
				case FCCommand.CMD.CLIENT_MOVE_TO_POINT:
				case FCCommand.CMD.CLIENT_HURT:
				case FCCommand.CMD.CLIENT_HURT_HP:
				case FCCommand.CMD.CLIENT_THREAT:
				case FCCommand.CMD.CLIENT_LEVELUP:
				case FCCommand.CMD.CLIENT_CURRSTATE:
				case FCCommand.CMD.CLIENT_POTION_HP:
				case FCCommand.CMD.CLIENT_POTION_ENERGY:
				case FCCommand.CMD.CLIENT_REVIVE:
					{
						//handle this right now, network agent will do it
						_inputCommandList[i]._objID.HandleCommand(ref command);
					}
				break;
									
				case FCCommand.CMD.DIE_NORMAL:				
				case FCCommand.CMD.ATTACK_WITH_SPEC_CONTS:	
				case FCCommand.CMD.ACTION_CANCEL:	
					{
						//convert to self fast inner command
						command._isHost = true;
						CommandManager.Instance.SendFastToSelf(ref command);					
					}
				break;					
					

				default:
					{
						Debug.LogError("receive command but cannot handle :" + command._cmd);
					}
				break;	
				}
			}
			
			//clear command list
			_inputCommandList.Clear();			
		}
	
	}
	
	public void CollectSyncCommands()
	{
		//collect all sync objects in command manager
		
		int currentCount = _syncCommandScripArray.Count;
		
		if (currentCount != MatchPlayerManager.Instance.GetPlayerCount())
		{
			// I did not collected all the commands
			//clear old array and try to collect new ones
			_syncCommandScripArray.Clear();
			
			//collect
			SyncCommand []syncCommands = GameObject.FindObjectsOfType(typeof(SyncCommand)) as SyncCommand[];
			
			foreach(SyncCommand sc in syncCommands) {
				_syncCommandScripArray.Add(sc);
			}	
					
			if (_syncCommandScripArray.Count > FCConst.MAX_PLAYERS)
				Debug.LogError("too many command objects!!!");
		
		}
		
	}
	
	public FCCommand ReadCommandFromCache2(int objID)
	{
		FC_COMMAND_NETSTREAM ecns = null;
		FCCommand ewc = null;
		FCCommand ret = null;
		if(_commandCmdStreamCache.ContainsKey(objID))
		{
			List<FC_COMMAND_NETSTREAM> list = _commandCmdStreamCache[objID];
			if(list.Count > 0){
				ecns = list[0];
			}else{
				ecns = null;
			}
		}else{
			ecns = null;
		}
			
		if(_commandRpcCache.ContainsKey(objID))
		{
			List<FCCommand> list = _commandRpcCache[objID];
			if(list.Count > 0){
				ewc = _commandRpcCache[objID][0];
				if(ecns != null && ecns._commandIndex < ewc._cmdIndex)
				{
					ret = new FCCommand(ecns);
					_commandCmdStreamCache[objID].RemoveAt(0);
				}
				else
				{
					ret = ewc;
				//		Debug.Log(string.Format("[rpc command] ReadCommandFromCache2 -- remove  _commandIndex:{0} , _currentPosition:{1} , CMD = {2}" , 
				//		ewc._cmdIndex , ewc._commandPosition , ewc._cmd ));
					_commandRpcCache[objID].RemoveAt(0);
				}
			}else{
				ewc = null;
			}

		}else{
			ewc = null;
		}
		if(ecns != null && ret == null)
		{
			ret = new FCCommand(ecns);
			_commandCmdStreamCache[objID].RemoveAt(0);
		}
		
		return ret;
	}
	
	public FCCommand ReadCommandFromCache2(int objID, ref FCCommand rewc)
	{
		if(rewc == null)
		{
			rewc = ReadCommandFromCache2(objID);
			return rewc;
		}
		FC_COMMAND_NETSTREAM ecns = null;
		FCCommand ewc = null;
		FCCommand ret = rewc;
		if(_commandCmdStreamCache.ContainsKey(objID))
		{
			List<FC_COMMAND_NETSTREAM> list = _commandCmdStreamCache[objID];
			if(list.Count > 0){
				ecns = list[0];
				if(ecns._commandIndex < ret._cmdIndex)
				{
					ret = new FCCommand(ecns);
				}
			}
		}
			
		if(_commandRpcCache.ContainsKey(objID))
		{
			List<FCCommand> list = _commandRpcCache[objID];
			if(list.Count > 0){
				ewc = _commandRpcCache[objID][0];
				if(ewc._cmdIndex < ret._cmdIndex)
				{
					ret = ewc;
				}
			}
		}
		if(ret != rewc)
		{
			rewc = ret;
			if(ecns != null && rewc._cmdIndex == ecns._commandIndex)
			{
				_commandCmdStreamCache[objID].RemoveAt(0);
			}
			else
			{
				//Debug.Log(string.Format("[rpc command] ReadCommandFromCache2 2params -- remove  _commandIndex:{0} , _currentPosition:{1} , CMD = {2}" , 
				//		ewc._cmdIndex , ewc._commandPosition , ewc._cmd ));
				
				_commandRpcCache[objID].RemoveAt(0);
			}
		}
		else
		{
			ret = null;
		}
		
		return ret;
	}
	
	public void ReadCommandFromCache(int objID , out FCCommand ewc , out FC_COMMAND_NETSTREAM ecns)
	{
		if(_commandCmdStreamCache.ContainsKey(objID))
		{
			List<FC_COMMAND_NETSTREAM> list = _commandCmdStreamCache[objID];
			if(list.Count > 0){
				ecns = list[0];
				_commandCmdStreamCache[objID].RemoveAt(0);
			}else{
				ecns = null;
			}
		}else{
			ecns = null;
		}
				
		if(_commandRpcCache.ContainsKey(objID))
		{
			List<FCCommand> list = _commandRpcCache[objID];
			if(list.Count > 0){
				ewc = _commandRpcCache[objID][0];
				_commandRpcCache[objID].RemoveAt(0);
			}else{
				ewc = null;
			}

		}else{
			ewc = null;
		}
	}
	
	public void WriteCommandToCache(int netIndex, Vector3 commandPos, bool streamCommand, params object[] args)
	{
		OBJECT_ID objectID = ObjectManager.Instance.GetObjectByNetworkID(netIndex);
		
		if(objectID == null) {
			Debug.LogError(string.Format("Object ID is not exist !  netIndex = {0} , isStreamCommand = {1} " , netIndex , streamCommand));
			return;
		}
		
		int objID = (int)objectID;
		
		ActionController ac = ObjectManager.Instance.GetObjectByNetworkID(netIndex).fcObj as ActionController;
		
		//Debug.Log("ac object id = " + ac.ObjectID + ", object ID = " + objID);
		//if(ac != null && ac.IsPlayer && GameManager.Instance.IsPVPMode)
		if(GameManager.Instance.IsPVPMode)
		{
			int cidx = (int)args[0]; //command stream idx
			if(streamCommand)
			{
				if(ac.AIUse.AIStateAgent.CurrentStateID == AIAgent.STATE.DEAD || ac.AIUse.AIStateAgent.CurrentStateID == AIAgent.STATE.REVIVE) return;
				
				if(!_commandCmdStreamCache.ContainsKey(objID))
				{
					_commandCmdStreamCache[objID] = new List<FC_COMMAND_NETSTREAM>();
				}
				
				List<FC_COMMAND_NETSTREAM> cpc = _commandCmdStreamCache[objID];
				if((cpc.Count > 0 && cpc[cpc.Count-1]._commandIndex < cidx)
					|| cpc.Count == 0)
				{
					FC_COMMAND_NETSTREAM ecns  = new FC_COMMAND_NETSTREAM((FC_COMMAND_NETSTREAM)args[1]);
					
					ecns._commandIndex = cidx;
					if(ecns._state == AIAgent.STATE.HURT)
					{
						ac.AIUse.UpdateNetCommand(AIAgent.COMMAND_DONE_FLAG.IN_HURT_STATE);
					}
					//Debug.Log(string.Format("[stream command] _commandIndex:{0} , _currentPosition:{1} , _currentRotation{2} , _state{3}" , 
					//	ecns._commandIndex , ecns._currentPosition , ecns._currentRotation ,ecns._state));
					cpc.Add(ecns);
					//Debug.Log("add stream cmd  to list : _myRotation = " + ecns._currentRotation + ", cmd idx = " + ecns._commandIndex);

				}
			}
			else
			{
				if(!_commandRpcCache.ContainsKey(objID))
				{
					_commandRpcCache[objID] = new List<FCCommand>();
				}
				
				FCCommand ewc = new FCCommand((FCCommand)args[1]);
				ewc._cmdIndex = cidx;
				ewc._commandPosition = commandPos;
				ewc._isRun = false;
				ewc._needRunPerFrame = false;
				ewc._canDrop = false;
				//Debug.Log(string.Format("[rpc command] _commandIndex:{0} , _currentPosition:{1} , CMD = {2}" , 
				//		ewc._cmdIndex , ewc._commandPosition , ewc._cmd ));
				
				_commandRpcCache[objID].Add(ewc);
				if(ewc._cmd == FCCommand.CMD.CLIENT_HURT
					|| ewc._cmd == FCCommand.CMD.ACTION_EOT
					|| ewc._cmd == FCCommand.CMD.ATTACK_WITH_SPEC_CONTS)
				{
					//Debug.Log(Time.realtimeSinceStartup);
					ac.AIUse.UpdateNetCommand();
				}
				//if find rpc command idx < player next command index or current 
				//replace command
				//pre command may should return to array
				// all rpc command should have position param
			}
		}
		else
		{
			if(!streamCommand)
			{
				FCCommand ewc = (FCCommand)args[1];
				ewc._cmdIndex = 0;
				_inputCommandList.Add(ewc);
			}
		}
	}
	//update sync commands
	private void UpdateSyncCommands()
	{
		if(GameManager.Instance.GameState != EnumGameState.InBattle) return;
		
		//collect sync commands
		CollectSyncCommands();
	
		foreach(SyncCommand sc in _syncCommandScripArray) {
			if (sc != null)
			{
				if (sc._myIndex != MatchPlayerManager.Instance.GetPlayerNetworkIndex()) //discard myself?
					if (  
							(!GameManager.Instance.IsMultiplayMode && sc._myPosition != Vector3.zero) ||
							(GameManager.Instance.IsPVPMode && (sc._myPosition != Vector3.zero || sc._myRotation != Vector3.zero))
					 	)
					{
						//pos is effective, use this pos
						OBJECT_ID player_ID = ObjectManager.Instance.GetObjectByNetworkID(sc._myIndex);
						
					
						if (player_ID != null)
						{
							//the bit higher than 8 is the high type
							if (player_ID.getOnlyObjectType != FC_OBJECT_TYPE.OBJ_AC)
								Debug.LogError("I am not a player");
							
							ActionController ac = player_ID.fcObj as ActionController;
							//position
						
							//if (sc._myLastPosition != sc._myPosition)
							if (ac.ThisTransform.localPosition != sc._myPosition)
							{
								ac._dataSync._position = sc._myPosition;
							}
							sc._myLastPosition = sc._myPosition;	
							
							if(GameManager.Instance.IsPVPMode)
							{
								//rotation , eularAngles
								//if (sc._myLastRotation != sc._myRotation)
								if (ac.ThisTransform.eulerAngles != sc._myRotation)
								{
									ac._dataSync._rotation = sc._myRotation;
								}
								sc._myLastRotation = sc._myRotation;
							}
						}
					}
			}
		}
	}
		
	#endregion
}
