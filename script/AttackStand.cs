using UnityEngine;
using System.Collections;

public class AttackStand : AttackBase {
	
	public float _godTimeForMosnter = 0.3f;
	protected override void AniOver()
	{
		if(_currentState == AttackBase.ATTACK_STATE.STEP_1)
		{
			//Debug.Log("AniIsOver");
			_nextAttackIdx = FCConst.UNVIABLE_ATTACK_INDEX;
			_currentState = AttackBase.ATTACK_STATE.ALL_DONE;
			_owner.ACOwner.ACStop();
			_owner.ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.CANT_MOVE);
			_owner.ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.CANT_ROTATE);
			_owner.ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_GOD);

			_owner.ACOwner.ACRestoreToDefaultSpeed();
			_owner.StandTaskChange(FCCommand.CMD.STATE_DONE);
			
		}
	}
	
	public override void AttackEnter()
	{
		if(!_owner.ACOwner.IsPlayer)
		{
			_godTime = _godTimeForMosnter;
			_hasRigidBody2 = true;
		}
		base.AttackEnter();
		_owner.SetActionSwitchFlag(FC_ACTION_SWITCH_FLAG.CANT_ROTATE);
		if(_owner.ACOwner.IsPlayer)
		{
			_owner.SetActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_GOD);
		}
		_currentState = AttackBase.ATTACK_STATE.STEP_1;
		//_owner.ACOwner.ACFire(_firePortIdx);
	}
	
	public override void AniBulletIsFire()
	{
		base.AniBulletIsFire();
	}
	
	protected override bool AKEvent(FC_KEY_BIND ekb, bool isPress)
	{
		return true;
	}
}
