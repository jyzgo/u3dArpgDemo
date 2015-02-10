using UnityEngine;
using System.Collections;

[AddComponentMenu("FC/Utils/ForAttacker/PlayerMessageReciever")]
public class PlayerMessageReceiver : MessageReciever {

	void OnTriggerExit(Collider other) 
	{
		if(_parent != null && _parent.enabled) {
			ActionController ac = ActionControllerManager.Instance.GetACByCollider(other);
			if(ac != null) {
				_fastCommand._param1 = ac;
				_fastCommand._cmd = FCCommand.CMD.ATTACK_OUT_OF_RANGE;
				CommandManager.Instance.SendFast(ref _fastCommand, _parent);
			}
		}
	}
	
}
