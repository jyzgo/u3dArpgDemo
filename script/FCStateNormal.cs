using UnityEngine;
using System.Collections;

[AddComponentMenu("FC/Logic/FCObject/PlayAndEnemy/Agent/AI/State/FCStateAgent")]
public class FCStateAgent : MonoBehaviour
{
	protected bool _inState = false;
	protected AIAgent.STATE _currentStateID;
	
	protected int _currentSubStateID;
	
	protected float _lifeTime = 0;
	
	public MMediaEffectInfoMapAll _mmEffectMap = null;
	
	public float LifeTime
	{
		get
		{
			return _lifeTime;
		}
		set
		{
			_lifeTime = value;
		}
	}
	public int CurrentSubStateID
	{
		get
		{
			return _currentSubStateID;
		}
		set
		{
			_currentSubStateID = value;
		}
	}
	
	public AIAgent.STATE CurrentStateID
	{
		get
		{
			return _currentStateID;
		}
	}
	protected AIAgent _stateOwner;
	public AIAgent StateOwner
	{
		get {return _stateOwner;}
	}
	
	public virtual void Init(AIAgent owner)
	{
		_stateOwner = owner;
		_inState = false;
	}
	
	public virtual bool CanGoto()
	{
		return true;
	}
	public virtual void Run()
	{
		
	}
	
	public virtual void PlayEffect(FC_EFFECT_EVENT_POS eeep, ActionController ac)
	{
		
	}
	
	public virtual void StopRun()
	{
		_inState = false;
	}
	
	public virtual void SetSubAgent(FCAgent ewa)
	{
		
	}
	
	public void OnFireBullet()
	{
		if(_mmEffectMap != null)
		{
			_mmEffectMap.PlayEffect(FC_EFFECT_EVENT_POS.AT_FIREBULLET, _stateOwner.ACOwner, _lifeTime);
		}
	}
	
	public virtual FCAgent GetSubAgent()
	{
		return null;
	}
	
	public virtual void StateIn()
	{
        if (_mmEffectMap != null)
        {
            if (_currentStateID == AIAgent.STATE.HURT)
            {
                if (_stateOwner.HurtAgent.CurrentHitType == AttackHitType.KnockDown
                || _stateOwner.HurtAgent.CurrentHitType == AttackHitType.KnockBack
                || _stateOwner.HurtAgent.CurrentHitType == AttackHitType.HitFly)
                {
                    _mmEffectMap.PlayEffect(FC_EFFECT_EVENT_POS.AT_KNOCK_BACK, _stateOwner.ACOwner, _lifeTime);
                }
                else
                {
                    _mmEffectMap.PlayEffect(FC_EFFECT_EVENT_POS.AT_BEGIN, _stateOwner.ACOwner, _lifeTime);
                }
            }
            else
            {
                _mmEffectMap.PlayEffect(FC_EFFECT_EVENT_POS.AT_BEGIN, _stateOwner.ACOwner, _lifeTime);
            }
        }
		_lifeTime = 0;
	}
	
	public virtual void StateOut()
	{
		if(_mmEffectMap != null)
		{
			_mmEffectMap.StopEffect();
		}
		_lifeTime = 0;
		if(_stateOwner.ACOwner._onStateQuit != null)
		{
			_stateOwner.ACOwner._onStateQuit(_currentStateID);
		}
	}
	
	protected void OnDestroy()
	{
		if(_mmEffectMap != null)
		{
			_mmEffectMap.StopEffect();
		}
	}
	public virtual void StateProcess()
	{
		if(_mmEffectMap != null)
		{
			_mmEffectMap.PlayEffect(FC_EFFECT_EVENT_POS.AT_SPEC_TIME, _stateOwner.ACOwner, _lifeTime);
		}
		_lifeTime+=Time.deltaTime;
	}
	// every state should overide this function
	public virtual void PlayAnimation()
	{
        switch (_currentStateID)
        {
            case AIAgent.STATE.HURT:
                if (_stateOwner.HurtAgent.CurrentHitType == AttackHitType.Dizzy)
                {
                    _stateOwner.AnimationSwitch._aniIdx = FC_All_ANI_ENUM.dizzy;
                    //play dizzy effect
                    CharacterEffectManager.Instance.PlayEffect(FC_CHARACTER_EFFECT.DIZZY,
                        _stateOwner.ACOwner._avatarController,
                        _stateOwner.HurtAgent.EffectTime);
                }
                else
                {
                    CharacterEffectManager.Instance.StopEffect(FC_CHARACTER_EFFECT.DIZZY,
                        _stateOwner.ACOwner._avatarController,
                        -1);
                    if (_stateOwner.HurtAgent.CurrentHitType == AttackHitType.BlackHole)
                    {
                        _stateOwner.AnimationSwitch._aniIdx = FC_All_ANI_ENUM.hurt;
                    }
                    else if (_stateOwner.HurtAgent.CurrentHitType == AttackHitType.ParrySuccess)
                    {
                        _stateOwner.AnimationSwitch._aniIdx = FC_All_ANI_ENUM.hurt_parryBack;
                    }
                    else if (_stateOwner.HurtAgent.CurrentHitType == AttackHitType.HurtFlash)
                    {
                        _stateOwner.AnimationSwitch._aniIdx = FC_All_ANI_ENUM.hurt_groups;
                    }
                    else if (_stateOwner.HurtAgent.CurrentHitType == AttackHitType.KnockBack)
                    {
                        _stateOwner.AnimationSwitch._aniIdx = FC_All_ANI_ENUM.knock_back;
                    }
                    else if (_stateOwner.HurtAgent.CurrentHitType == AttackHitType.HitFly)
                    {
                        _stateOwner.AnimationSwitch._aniIdx = FC_All_ANI_ENUM.hurt_flyup_start;
                    }
                    else if (_stateOwner.HurtAgent.CurrentHitType == AttackHitType.ParryFail)
                    {
                        _stateOwner.AnimationSwitch._aniIdx = FC_All_ANI_ENUM.block1_break_iceShield_break;
                    }
                    else if (_stateOwner.HurtAgent.CurrentHitType == AttackHitType.KnockDown)
                    {
                        _stateOwner.AnimationSwitch._aniIdx = FC_All_ANI_ENUM.knockDown;
                    }
                    else
                    {
                        if (_stateOwner.HurtAgent._haveRandomNormalHurt)
                        {
                            if (_stateOwner.AnimationSwitch._aniIdx == FC_All_ANI_ENUM.hurt)
                            {
                                _stateOwner.AnimationSwitch._aniIdx = FC_All_ANI_ENUM.hurt2;
                            }
                            else if (_stateOwner.AnimationSwitch._aniIdx == FC_All_ANI_ENUM.hurt2)
                            {
                                _stateOwner.AnimationSwitch._aniIdx = FC_All_ANI_ENUM.hurt;
                            }
                            else
                            {
                                int ret = Random.Range(0, 1);
                                if (ret == 0)
                                {
                                    _stateOwner.AnimationSwitch._aniIdx = FC_All_ANI_ENUM.hurt;
                                }
                                else
                                {
                                    _stateOwner.AnimationSwitch._aniIdx = FC_All_ANI_ENUM.hurt2;
                                }
                            }
                        }
                        else
                        {
                            _stateOwner.AnimationSwitch._aniIdx = FC_All_ANI_ENUM.hurt;
                        }
                    }
                }

                break;
            case AIAgent.STATE.DEAD:
                if (_stateOwner.NeedPlayDeadAnimation)
                {
                    if (_stateOwner.DeadWithFly)
                    {
                        _stateOwner.AnimationSwitch._aniIdx = FC_All_ANI_ENUM.knockDown;
                    }
                    else
                    {
                        _stateOwner.AnimationSwitch._aniIdx = FC_All_ANI_ENUM.die;
                    }
                }
                else
                {
                    _stateOwner.AnimationSwitch._aniIdx = FC_All_ANI_ENUM.none;
                }
                break;
            case AIAgent.STATE.REVIVE:
                _stateOwner.AnimationSwitch._aniIdx = FC_All_ANI_ENUM.revive;
                break;
            case AIAgent.STATE.STAND:
                _stateOwner.AnimationSwitch._aniIdx = FC_All_ANI_ENUM.stand;
                break;
            case AIAgent.STATE.RUN:
                _stateOwner.AnimationSwitch._aniIdx = FC_All_ANI_ENUM.run;
                break;
            case AIAgent.STATE.IDLE:
                if (_stateOwner._isInTown)
                {
                    _stateOwner.AnimationSwitch._aniIdx = FC_All_ANI_ENUM.idle;
                }
                else if (_stateOwner._needHideWeaponWhenRun
                    && _stateOwner.AIStateAgent._preState.CurrentStateID == AIAgent.STATE.RUN)
                {
                    _stateOwner.AnimationSwitch._aniIdx = FC_All_ANI_ENUM.idle2;
                }
                else
                {
                    if (_stateOwner.ACOwner.HangWeaponBack)
                    {
                        _stateOwner.AnimationSwitch._aniIdx = FC_All_ANI_ENUM.idle2;
                    }
                    else
                    {
                        _stateOwner.AnimationSwitch._aniIdx = FC_All_ANI_ENUM.idle;
                    }
                }
                break;
            case AIAgent.STATE.LEVEL_UP:
                _stateOwner.AnimationSwitch._aniIdx = FC_All_ANI_ENUM.power_up;
                break;
            case AIAgent.STATE.DEFY:
                _stateOwner.AnimationSwitch._aniIdx = FC_All_ANI_ENUM.defy;
                break;
            case AIAgent.STATE.ARMOR2_BROKEN:
                _stateOwner.AnimationSwitch._aniIdx = FC_All_ANI_ENUM.armor_break;
                break;
            case AIAgent.STATE.BORN:
                _stateOwner.AnimationSwitch._aniIdx = FC_All_ANI_ENUM.born_begin;
                break;
            case AIAgent.STATE.ARMOR1_BROKEN:
                _stateOwner.AnimationSwitch._aniIdx = FC_All_ANI_ENUM.idle_weak;
                break;
            case AIAgent.STATE.SUMMON:
                _stateOwner.AnimationSwitch._aniIdx = FC_All_ANI_ENUM.necromancer_summon_biont;
                break;
            case AIAgent.STATE.DUMMY:
                if (_stateOwner.ACOwner.IsPlayerSelf)
                {
                    _stateOwner.AnimationSwitch._aniIdx = FC_All_ANI_ENUM.idle2;
                }
                else
                {
                    _stateOwner.AnimationSwitch._aniIdx = FC_All_ANI_ENUM.idle;
                }
                break;
        }

		//Debug.Log ("animation id = " + _stateOwner.AnimationSwitch._aniIdx);
		_stateOwner.PlayAnimation();
	}
}
