using UnityEngine;
using System.Collections;

//only receive damage message, should in damage layer
public class PhysicalMessageReciever : MonoBehaviour {

	public FCObject _parent = null;
	protected FCCommand _fastCommand;
	
	// Use this for initialization
	void Awake()
	{
		_fastCommand = new FCCommand();
		_fastCommand.Set(FCCommand.CMD.STOP,null,FCCommand.STATE.RIGHTNOW,true);
	}
	
	void OnTriggerEnter(Collider other) 
	{
		if(_parent.enabled)
		{

			if(other.gameObject.layer == FCConst.LAYER_WALL
				&& _parent.ObjectID.getOnlyObjectType == FC_OBJECT_TYPE.OBJ_BULLET)
			{
				_fastCommand._cmd = FCCommand.CMD.ATTACK_HIT_WALL;
				CommandManager.Instance.SendFast(ref _fastCommand,_parent);
			}
			else if(other.gameObject.layer == FCConst.LAYER_GROUND
				&& _parent.ObjectID.getOnlyObjectType == FC_OBJECT_TYPE.OBJ_BULLET)
			{
				_fastCommand._cmd = FCCommand.CMD.ATTACK_HIT_GROUND;
				CommandManager.Instance.SendFast(ref _fastCommand,_parent);
			}
		}

	}
}
