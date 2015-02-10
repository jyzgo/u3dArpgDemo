using UnityEngine;
using System.Collections;

public class ContinuityMessageReceiver : MessageReciever {

	// characters stay in the collider's range.
	System.Collections.Generic.List<ActionController> _charInRange;
	// Use this for initialization
	
	protected override void FirstInit()
	{
		base.FirstInit();
		_charInRange = new System.Collections.Generic.List<ActionController>();
	}
	
	protected override void OnHitSomeOne(Collider other)
	{
		
	}
	
	public override void DeActiveLogic ()
	{
		base.DeActiveLogic ();
		if(_charInRange != null)
		{
			_charInRange.Clear();
		}
	}

	void OnTriggerStay(Collider other) 
	{
		if(_parent != null && _parent.enabled)
		{
			ActionController ac = ActionControllerManager.Instance.GetACByCollider(other);
			
			if(ac != null && ac == _parent) return;
			if(_parent.ObjectID.ObjectType == FC_OBJECT_TYPE.OBJ_BULLET)
			{
				if(ac == ((FCBullet)_parent).Owner ) return;
			}
			
			if(ac != null && !_charInRange.Contains(ac))
			{
				_charInRange.Add(ac);
				_fastCommand._param1 = ac;
				_fastCommand._cmd = FCCommand.CMD.ATTACK_HIT_TARGET;
				CommandManager.Instance.SendFast(ref _fastCommand,_parent);
			}
		}
	}
	
	void OnTriggerExit(Collider other) 
	{
		if(_parent != null && _parent.enabled) {
			ActionController ac = ActionControllerManager.Instance.GetACByCollider(other);
			if(ac != null) {
				_charInRange.Remove(ac);
				_fastCommand._param1 = ac;
				_fastCommand._cmd = FCCommand.CMD.ATTACK_OUT_OF_RANGE;
				CommandManager.Instance.SendFast(ref _fastCommand, _parent);
			}
		}
	}
}
