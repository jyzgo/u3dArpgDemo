using UnityEngine;
using System.Collections;

[AddComponentMenu("FC/Logic/FCObject/PlayAndEnemy/Agent/AC/NetworkAgent")]
public class NetworkAgent : MonoBehaviour, FCAgent {
	
	private ActionController _owner;	
	protected float[] _timeCounters = new float[(int)FCConst.DATA_SYNC_TYPE.NUM];	
	
	private AIAgent.STATE _clientAICurrStateID = AIAgent.STATE.NONE;
	public AIAgent.STATE ClientAICurrStateID
	{
		get { return _clientAICurrStateID; }
	}
	
	const int SMOOTH = 10;
	
	protected Vector3 _lastPos;	
	protected Vector3 _lerpToPos;
	protected Vector3 _lastRot;	
	protected AIAgent.STATE _lastAIState;

//    bool _isLerpEnabled = false;
	bool _isIgnoreSyncEnabled = false;
	public bool IsIgnoreSyncEnabled
	{
		get {return _isIgnoreSyncEnabled;}
		set { _isIgnoreSyncEnabled = value;}
	}
	
	public static string GetTypeName()
	{
		return "NetworkAgent";
	}	
	
	public void Init(FCObject owner)
	{
		_owner = owner as ActionController;
	}	
	
	// Use this for initialization
	void Start () {
		
	}
	
	public void SetPosLerpTo(Vector3 pos)
	{
		_lerpToPos = pos;
//		_isLerpEnabled = true;
	}
	
	// Update is called once per frame
	void Update () {
		
		if(PhotonNetwork.room == null)
			return;
		
		if (!_owner.IsPlayerSelf) return;
		
		//set my index
		CommandManager.Instance._syncCommandScript._myIndex = MatchPlayerManager.Instance.GetPlayerNetworkIndex();
		
		if(GameManager.Instance.IsMultiplayMode){
			UpdateSyncCommandForMultiplayer();
		}
		
		if(GameManager.Instance.IsPVPMode){
			UpdateSyncCommandForPVP();
		}
	}
	
	void UpdateSyncCommandForMultiplayer()
	{
		if(!GameManager.Instance.IsMultiplayMode) return ;
		
		//send update pos command to other 
		for(int i = 0 ; i < _timeCounters.Length ; i++){
			if(_timeCounters[i]>=0)
			{
				_timeCounters[i] -= Time.deltaTime;
				if(_timeCounters[i] <=0 )
				{
					FCConst.DATA_SYNC_TYPE type = (FCConst.DATA_SYNC_TYPE)i;
					switch(type)
					{
						case FCConst.DATA_SYNC_TYPE.POSITION:	
						{
							//change pos in sync object
							//if (_lastPos != _owner.ThisTransform.localPosition)
							float POSITION_SYNC_THRESHOLD = FCConst.DATA_SYNC_THRESHOLD[(int)FCConst.DATA_SYNC_TYPE.POSITION];						
							if (Vector3.Distance(_lastPos, _owner.ThisTransform.localPosition) > POSITION_SYNC_THRESHOLD )
							{
								_lastPos = _owner.ThisTransform.localPosition;
							
								//my player can send pos to others
								//others, only host can send pos to others, such as boss.
								//set my pos
								CommandManager.Instance._syncCommandScript._myPosition
									= _lastPos;
							}
						}break;
						case FCConst.DATA_SYNC_TYPE.ROTATION:	
						{
							float ROTATION_SYNC_THRESHOLD = FCConst.DATA_SYNC_THRESHOLD[(int)FCConst.DATA_SYNC_TYPE.ROTATION];						
							if (Vector3.Distance(_lastRot, _owner.ThisTransform.eulerAngles) > ROTATION_SYNC_THRESHOLD)
							{
								_lastRot = _owner.ThisTransform.eulerAngles;
								//set my rotation
								CommandManager.Instance._syncCommandScript._myRotation
									= _lastRot;
							}
						}break;
						default:
							break;
					}
				}
			}	
		}
	}
	
	void UpdateSyncCommandForPVP()
	{
		if(!GameManager.Instance.IsPVPMode) return ;
		
		_lastPos = _owner.ThisTransform.localPosition;					
		//set my pos
		CommandManager.Instance._syncCommandScript._myPosition
			= _lastPos;

		_lastRot = _owner.ThisTransform.eulerAngles;
		//set my rotation
		CommandManager.Instance._syncCommandScript._myRotation
			= _lastRot;

		_lastAIState = _owner.AIUse.AIStateAgent.CurrentStateID;
		//set my AI state
		CommandManager.Instance._syncCommandScript._myAIState = _lastAIState;
		
	}
	
	/*void LateUpdate()
	{
		if(PhotonNetwork.room == null) return;
		
		if(!_isLerpEnabled || !_owner.IsClientPlayer) return;
		
		_owner.ThisTransform.localPosition  = Vector3.Lerp( _owner._currPosition , _lerpToPos , SMOOTH * Time.deltaTime);
		if( Vector3.Distance(_owner.ThisTransform.localPosition , _lerpToPos) < 0.1f ){
			_isLerpEnabled = false;
		}
	}*/
	
	public virtual bool HandleCommand(ref FCCommand ewd)
	{
		
		if(PhotonNetwork.room == null)
			return false;	
		
		
		switch(ewd._cmd)
		{
			case FCCommand.CMD.CLIENT_MOVE_TO_POINT:	
			{
				if (ewd._param1Type != FC_PARAM_TYPE.VECTOR3)
					Debug.LogError("get a CLIENT_MOVE_TO_POINT but param1 is not a vector3");
				else
				{
					_owner._dataSync._position = (Vector3)ewd._param1;
					return true;
				}
			}
			break;
			
			case FCCommand.CMD.CLIENT_HURT:	
			{
				if (ewd._param1Type != FC_PARAM_TYPE.VECTOR3 || ewd._param2Type != FC_PARAM_TYPE.VECTOR3)
					Debug.LogError("get a CLIENT_HURT but param1 , 2 is not a vector3");
				else{
//					Vector3 param1 = (Vector3)ewd._param1;
//				    FC_HIT_TYPE eht = (FC_HIT_TYPE)param1.x;
//					bool isCritical = (param1.y == 0.0 ? false : true);
					//int realDamage = (int)param1.z;
				
//					Vector3 hitDirection = (Vector3)ewd._param2;
					
					_owner.SelfMoveAgent.SetPosition(ewd._commandPosition);
					//_owner.ACHandleHurt(eht , isCritical ,realDamage , hitDirection  , true , true , false );
					//Debug.Log("Get FCCommand.CMD.CLIENT_HURT !!!!!!!!!!!!! hitDir = " + hitDirection);
					//CommandManager.Instance.SendFastToSelf(ref ewd);
					return true;
				}
			}
			break;
			
			case FCCommand.CMD.CLIENT_HURT_HP:	
			{
				if (ewd._param1Type != FC_PARAM_TYPE.INT || ewd._param2Type != FC_PARAM_TYPE.INT)
					Debug.LogError("get a CLIENT_HURT_HP but param1 is not a int");
				else
				{
					_owner.ACReduceHP((int)ewd._param1 , (int)ewd._param2 ,false,false, false, false);
//					_owner._dataSync._hitPoint = (int)ewd._param1;
					return true;
				}
			}
			break;		
			
			
			case FCCommand.CMD.CLIENT_THREAT:	
			{
				if ((ewd._param1Type != FC_PARAM_TYPE.INT) || (ewd._param2Type != FC_PARAM_TYPE.INT))
					Debug.LogError("get a CLIENT_THREAT but param is not a int");
				else
				{
					//get player objid
					OBJECT_ID player_ID = ObjectManager.Instance.GetObjectByNetworkID((int)ewd._param2);
					if (player_ID == null)
						Debug.LogError("receive increase threat but bad player");
				
					//get player AC
					ActionController target = player_ID.fcObj as ActionController;
				
					//increase threat
					_owner.ACIncreaseThreat((int)ewd._param1, false, target);
					return true;
				}
			}
			break;				
			
			
			case FCCommand.CMD.CLIENT_LEVELUP:	
			{
				if (ewd._param1Type != FC_PARAM_TYPE.INT)
					Debug.LogError("get a CLIENT_LEVELUP but param is not a int");
				else
				{
					//level up
					_owner.OnLevelUp_FromNet((int)ewd._param1);
					return true;
				}
			}
			break;		
			
			case FCCommand.CMD.CLIENT_CURRSTATE:	
			{
				if (ewd._param1Type != FC_PARAM_TYPE.INT){
					Debug.LogError("get a CLIENT_CURRSTATE_ID but param is not a int");
				//}else if (ewd._param2Type != FC_PARAM_TYPE.FLOAT){
				//	Debug.LogError("get a CLIENT_CURRSTATE_ID but param2 is not float type");
				}else{
					_clientAICurrStateID = (AIAgent.STATE)ewd._param1;
				
					//float y = (float)ewd._param2;
					//Quaternion rotation = Quaternion.Euler(new Vector3(0, y, 0));
					//Vector3 v3 = rotation * Vector3.forward;
					//_owner.ACRotateTo(v3,-1,true,true);
			
					return true;
				}
			}
			break;
			
			case FCCommand.CMD.CLIENT_POTION_HP:	
			{
				if (ewd._param1Type != FC_PARAM_TYPE.FLOAT || ewd._param2Type != FC_PARAM_TYPE.FLOAT)
					Debug.LogError("get a CLIENT_POTION_HP but param is not a float");
				else
				{
					//increase HP
					_owner.ACEatPotion((float)ewd._param1, (float)ewd._param2, FCConst.POTION_TIME,FCPotionType.Health);
					return true;
				}
			}
			break;
			
			case FCCommand.CMD.CLIENT_POTION_ENERGY:	
			{
				if (ewd._param1Type != FC_PARAM_TYPE.FLOAT || ewd._param2Type != FC_PARAM_TYPE.FLOAT)
					Debug.LogError("get a CLIENT_POTION_ENERGY but param is not a float");
				else
				{
					//increase EP
					_owner.ACEatPotion((float)ewd._param1, (float)ewd._param2, FCConst.POTION_TIME,FCPotionType.Mana);
					return true;
				}
			}
			break;
			
			case FCCommand.CMD.CLIENT_REVIVE:
			{
				if (ewd._param1Type != FC_PARAM_TYPE.NONE)
					Debug.LogError("get a CLIENT_REVIVE but param is not a float");
				else
				{
					//revive now
					//_owner.GoToRevive();
					return true;
				}
			}
			break;
		}
		
		return false;
	}
	
}
