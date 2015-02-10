using UnityEngine;
using System.Collections;

public class AttackContinuity : AttackBase {
	
	protected bool _energyCostPhase = false;
	float _energyCostCounter = 0.0f;
	
	public float _timeLast = 2f;
	protected float _timeCount = 0;
		
	public override void Init(FCObject owner)
	{
		base.Init(owner);
	}
	
	
	public override void AttackEnter()
	{
		base.AttackEnter();
		_energyCostCounter = 0.0f;
		_energyCostPhase = false;
		_timeCount = _timeLast;
	}
	
	public override void AttackUpdate()
	{
		base.AttackUpdate();
		UpdateBindEffects();
		_timeCount -= Time.deltaTime;
		bool actionStop = false;
		if(_timeCount <= 0)
		{
			actionStop = true;
		}
		
		// check if energy is enough
		if(_energyCostPhase) {
			float energyInFrame = _energyCost * Time.deltaTime;
			_energyCostCounter += energyInFrame;
			int energyFloor = Mathf.FloorToInt(_energyCostCounter);
			_energyCostCounter -= energyFloor;
			actionStop = _owner.ACOwner.IsPlayerSelf && (actionStop || _owner.ACOwner.CostEnergy(-energyFloor, 1));
		}
		
		if(actionStop) {
			_attackCanSwitch = true;
			_shouldGotoNextHit = true;
			AttackEnd();
			
			if (_owner.ACOwner.IsPlayerSelf)
			{
				CommandManager.Instance.SendCommandToOthers(FCCommand.CMD.ACTION_CANCEL,_owner.ACOwner.ObjectID,
					_owner.ACOwner.ThisTransform.localPosition,
					null,
					FC_PARAM_TYPE.NONE,
					null,
					FC_PARAM_TYPE.NONE,
					null,
					FC_PARAM_TYPE.NONE);
			}
		}
	}
	
	protected override bool AKEvent(FC_KEY_BIND ekb, bool isPress)
	{
		return true;
	}
	
	public override bool InitSkillData(SkillData skillData, AIAgent owner)
	{
		bool ret = base.InitSkillData(skillData, owner);
		return ret;
	}
	
	public override void AniBulletIsFire()
	{
		base.AniBulletIsFire();
	}
	
	public override void AttackEnd()
	{
		base.AttackEnd();
		_energyCostPhase = false;
	}
	
	public override void AttackQuit()
	{
		base.AttackQuit();
		_energyCostPhase = false;
	}
}
