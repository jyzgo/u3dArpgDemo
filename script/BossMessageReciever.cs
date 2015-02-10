using UnityEngine;
using System.Collections;

public class BossMessageReciever : MonoBehaviour {
	
	public ActionController _parent = null;
	public bool _beActive = false;
	protected FCCommand _fastCommand;
	
	void Awake()
	{
		_fastCommand = new FCCommand();
		_fastCommand.Set(FCCommand.CMD.STOP,null,FCCommand.STATE.RIGHTNOW,true);
		
	}
	
	void OnTriggerStay(Collider collisionInfo)
	{
		//return;
		if(_parent == null)
		{
			_parent = GetComponent<ActionController>();
		}
		if(_parent.enabled && _beActive)
		{
			ActionController ac = ActionControllerManager.Instance.GetACByCollider(collisionInfo);
			if(ac != null && ac.IsAlived)
			{
				ac.ACEffectByGravity(-10, transform, Vector3.up, 0.01f, true, false);
			}
		}
	}
}
