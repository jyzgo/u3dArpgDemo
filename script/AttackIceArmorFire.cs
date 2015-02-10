using UnityEngine;
using System.Collections;


public class AttackIceArmorFire : AttackBase 
{

	public FC_GLOBAL_EFFECT _attackEffect = FC_GLOBAL_EFFECT.ICE_ARMOR_SUCCESS;
	
	private FC_GLOBAL_EFFECT _iceGround = FC_GLOBAL_EFFECT.ICE_GROUND;
	public FC_GLOBAL_EFFECT _iceGround0 = FC_GLOBAL_EFFECT.ICE_GROUND;
	public FC_GLOBAL_EFFECT _iceGround1 = FC_GLOBAL_EFFECT.ICE_GROUND1;
	public FC_GLOBAL_EFFECT _iceGround2 = FC_GLOBAL_EFFECT.ICE_GROUND2;
	
	public FC_GLOBAL_EFFECT _flashHide = FC_GLOBAL_EFFECT.FLASH_HIDE;
	public FC_GLOBAL_EFFECT _flashSHow = FC_GLOBAL_EFFECT.FLASH_SHOW;
	public FC_CHARACTER_EFFECT _shieldEffect =  FC_CHARACTER_EFFECT.ICE_ARMOR;
	
	
	public string _flashHideSound = "";
	public string _flashShowSound = "";
	
	public float _showIceTime = 0.05f;
	public float _hideEffectTime = 0.08f;
	public float _hideTime = 0.12f;
	public float _showEffectTime = 0.30f;
	public float _showTime = 0.44f;
	
	private int _step = 0;
	
	protected bool _controlBySelf = false;
	protected Vector3 _sourcePos = Vector3.zero;
	
	public override void Init(FCObject owner)
	{
		base.Init(owner);
	}
	
	protected override void AniOver()
	{
		if(_currentState >= AttackBase.ATTACK_STATE.STEP_2)
		{
			AttackEnd();
		}
	}
	
	
	public override void AttackEnter()
	{
		base.AttackEnter();
		
		_iceGround = _iceGround0;
		if( SkillData.CurrentLevelData.effect == 1)
		{
			_iceGround = _iceGround1;
		}else if( SkillData.CurrentLevelData.effect == 2)
		{
			_iceGround = _iceGround2;
		}
		
		_controlBySelf = false;
		_step = 0;
		Vector3 dir = Vector3.zero;
		
		if(_owner.KeyAgent.keyIsPress(FC_KEY_BIND.DIRECTION))
		{
			dir = _owner.KeyAgent._directionWanted;
			_controlBySelf = true;
			_owner.ACOwner.SetAniMoveSpeedScale(-_aniMoveSpeedScale);
		}
		else if(_owner.ACOwner.IsClientPlayer){
			dir = _owner.ThisTransform.forward;
			_controlBySelf = true;
			_owner.ACOwner.SetAniMoveSpeedScale(-_aniMoveSpeedScale);
		}
		/*else if(_owner.ParryTarget != null)
		{
			dir = _owner.ParryTarget.ThisTransform.position - _owner.ACOwner.ThisTransform.position;
			dir.y = 0;
			dir.Normalize();
		}*/
		else if(CheatManager.flashMode == 2)
		{
			_controlBySelf = true;
			dir = _owner.ACOwner.ThisTransform.forward;
			_owner.ACOwner.SetAniMoveSpeedScale(-_aniMoveSpeedScale);
		}
		else
		{
			dir = _owner.ThisTransform.forward;
		}
		_sourcePos = _owner.ACOwner.ThisTransform.localPosition;
		if(dir != Vector3.zero)
		{
			_owner.ACOwner.ACRotateToDirection(ref dir, true);
			if(_currentBindKey != FC_KEY_BIND.NONE)
			{
				if(!_owner.KeyAgent.keyIsPress(FC_KEY_BIND.DIRECTION))
				{
					_owner.KeyAgent._directionWanted = dir;
				}
			}
		}
		_owner.ParryTarget = null;
		
		if(_owner.ACOwner.IsClientPlayer)
		{
			_owner._updateAttackPos = true;
			_owner._attackMoveSpeed = 2;
		}

		_owner.SetActionSwitchFlag(FC_ACTION_SWITCH_FLAG.CANT_ROTATE);

		_owner.ACOwner.CanAcceptTimeScale = true;
		_owner.ACOwner.ACEnableCollisionWithOtherACS(false);
		
		
		_currentState = AttackBase.ATTACK_STATE.STEP_1;
		
	
		//BeginEffect();
	}
	
	
	
	private void BeginEffect()
	{
		
		Transform trans = Utils.FindTransformByNodeName(_owner.ACOwner.ThisTransform, "node_right_weapon");

		
		Vector3 pos = trans.position;			
		GlobalEffectManager.Instance.PlayEffect(_attackEffect, pos);
		
		CharacterEffectManager.Instance.PlayEffect(_shieldEffect ,_owner.ACOwner._avatarController, -1);
		
		
		pos = _owner.ACOwner.ThisTransform.position;
		
		
		
		
		GlobalEffectManager.Instance.PlayEffect(_iceGround, pos);
	
		_owner.ACOwner.ACFire(FirePortIdx);
	}
	
	public override bool InitSkillData(SkillData skillData, AIAgent owner)
	{
		bool ret = base.InitSkillData(skillData, owner);
		if(ret)
		{
			RangerAgent.FirePort fp = _owner.ACOwner.ACGetFirePort(_fireInfos[0].FirePortIdx);
			fp._fireCount = 1;
		}
		return ret;
	}
	
	private void HideEffect()
	{
		Vector3 pos = _owner.ACOwner.ThisTransform.position;			
		GlobalEffectManager.Instance.PlayEffect(_flashHide, pos);
		if(_flashHideSound != null && _flashHideSound != "")
		{
			SoundManager.Instance.PlaySoundEffect(_flashHideSound);
		}
	}
	
	private void Hide()
	{
		_owner.ACOwner._avatarController.ChangeMeshRenderers(false);
	}
	
	
	private void ShowEffect()
	{
		Vector3 pos = _owner.ACOwner.ThisTransform.position;			
		GlobalEffectManager.Instance.PlayEffect(_flashSHow, pos);
		
		if(_flashShowSound != null && _flashShowSound != "")
		{
			SoundManager.Instance.PlaySoundEffect(_flashShowSound);
		}
	}
	
	private void Show()
	{
		_owner.ACOwner._avatarController.ChangeMeshRenderers(true);
		CharacterEffectManager.Instance.PlayEffect(_shieldEffect ,_owner.ACOwner._avatarController, -1);
		//if(CheatManager._flashMode == 0)
		{
			//if( _owner.KeyAgent.keyIsPress( _currentBindKey ))
			{
				_controlBySelf = false;
				_currentPortIdx = 1;
				Vector3 v3 = _sourcePos - _owner.ACOwner.ThisTransform.localPosition;
				v3.y = 0;
				float lengthSqrt = v3.sqrMagnitude;
				
				v3.Normalize();
				if(v3 == Vector3.zero)
				{
					v3 = -_owner.ACOwner.ThisTransform.forward;
				}
				if(ActionControllerManager.Instance.EnemyIsInRanger
					(v3, _owner.ACOwner.ThisTransform.localPosition, lengthSqrt*2, _owner.ACOwner.Faction)
					|| _hitTargetCount > 0)
				{
					_owner.ACOwner.ACRotateToDirection(ref v3, true);
					if(!_owner.KeyAgent.keyIsPress(FC_KEY_BIND.DIRECTION))
					{
						_owner.KeyAgent._directionWanted = v3;	
					}
				}
				else
				{
					_currentPortIdx = -10;
					_controlBySelf = true;
					_attackCanSwitch = true;
					AttackEnd();
				}
			}
			//else
			//{
			//	_currentPortIdx = -10;
			//	_controlBySelf = true;
			//}
	
		}

		//if(_controlBySelf)
		//{
		//	_attackCanSwitch = true;
		//	AttackEnd();
		//}
	}
	

	public override void AttackUpdate()
	{
		base.AttackUpdate();
		if(_currentState == AttackBase.ATTACK_STATE.STEP_1)
		{
			
			if(_step == 0 && _owner.ACOwner.AniGetAnimationNormalizedTime() > _showIceTime)
			{
				BeginEffect();
				_step = 1;
			}
			
			
			if(_step == 1 && _owner.ACOwner.AniGetAnimationNormalizedTime() > _hideEffectTime)
			{
				HideEffect();
				_step = 2;
			}
			
			if(_step ==2 &&  _owner.ACOwner.AniGetAnimationNormalizedTime() > _hideTime)
			{
				Hide();
				_step =3;
			}
			
			if(_step ==3 && _owner.ACOwner.AniGetAnimationNormalizedTime() >_showEffectTime)
			{
				ShowEffect();
				_step = 4;
			}
			
			
			if(_step == 4 && _owner.ACOwner.AniGetAnimationNormalizedTime() > _showTime)
			{
				Show();
				_step = 5;
				_currentState = AttackBase.ATTACK_STATE.STEP_2;
			}			
		}
		

	}
	
	public override void IsHitTarget(ActionController ac,int sharpness)
	{
		base.IsHitTarget(ac, sharpness);
	}
	
	public override void AniBulletIsFire()
	{
		//use fire bullet event as attackcanswitch
		if(_controlBySelf && _currentPortIdx < -5)
		{
			AttackEnd();
		}
		else
		{
			base.AniBulletIsFire();
			//means if flash is control by player, we need not fire ice bullet again
			if(_controlBySelf && _currentPortIdx >0)
			{
				_currentPortIdx = -10;
			}
		}
	}
	
	
	public override void AttackEnd()
	{
		if(_controlBySelf)
		{
			_shouldGotoNextHit = false;
		}
		base.AttackEnd();		
	}
	

	public override void AttackQuit()
	{
		base.AttackQuit();
		
		_owner.ACOwner._avatarController.ChangeMeshRenderers(true);
		
		//_owner.ACOwner.ACDisableCollisionsWithOther(false);
		if(_owner.ACOwner.CanAcceptTimeScale)
		{
			_owner.ACOwner.CanAcceptTimeScale = false;
		}
		if(!GameManager.Instance.GamePaused)
		{
            Time.timeScale = GameSettings.Instance.TimeScale;
		}
		_owner.ACOwner.ACAniSpeedRecoverToNormal();
		
		if(_owner.ACOwner.IsClientPlayer)
		{
			if(_owner._updateAttackPos)
			{
				_owner.ACOwner.ACStop();
			}
			_owner._updateAttackPos = false;
		}
		
	}
	
	
	public override bool DirectionKeyEvent(Vector3 direction, bool isPress)
	{
		return true;	
	}
	
	protected override bool AKEvent(FC_KEY_BIND ekb, bool isPress)
	{
		return true;
	}
	
	public override bool IsStopAtPoint()
	{
		return true;
	}
	
}
