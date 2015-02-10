using UnityEngine;
using System.Collections;

public class AttackLevelUp : AttackBase {

	protected override void AniOver()
	{
		if(_currentState != AttackBase.ATTACK_STATE.ALL_DONE)
		{
			if(_currentAnimationCount >= _animationCount)
			{
				_nextAttackIdx = FCConst.UNVIABLE_ATTACK_INDEX;
				_currentState = AttackBase.ATTACK_STATE.ALL_DONE;
				_owner.ACOwner.ACStop();
				_owner.ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.CANT_MOVE);
				_owner.ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.CANT_ROTATE);
				//revive will give player some seconds in god mode
				//_owner.ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_GOD);
				_owner.ACOwner.ACRestoreToDefaultSpeed();
				_owner.LevelUpTaskChange(FCCommand.CMD.STATE_DONE);
			}
		}
	}
	
	protected override bool AKEvent(FC_KEY_BIND ekb, bool isPress)
	{
		return true;
	}
	
	public override void AttackEnter()
	{
		base.AttackEnter();
		_owner.SetActionSwitchFlag(FC_ACTION_SWITCH_FLAG.CANT_ROTATE);
		_owner.SetActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_GOD);
		_currentState = AttackBase.ATTACK_STATE.STEP_1;
		//_owner.ACOwner.ACFire(_firePortIdx);
	}
	
	public override void AniBulletIsFire()
	{
		base.AniBulletIsFire();
	}
}
