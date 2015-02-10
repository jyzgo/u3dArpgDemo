using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("FC/Logic/FCObject/Bullet/BulletFlash")]
public class BulletFlash : FCBullet {
	
	public float _effectDistance;
	public float _effectAngle;
	public int _maxJumpCount;
	public float _reducePercent;
	public float _effectLastTime;
	public float _dispareTime;
	
	
	protected ThunderBulletDisplayer _thunderBulletDisplayer;
	
	public class FlashPointInfo
	{
		public List<Transform> _targetTransformList;
		public List<int> _bornIdx;
		
		public int Count
		{
			get
			{
				return _targetTransformList.Count;
			}
		}
		
		public FlashPointInfo()
		{
			_targetTransformList = new List<Transform>();
			_bornIdx = new List<int>();
		}
		
		public void Clear()
		{
			_targetTransformList.Clear();
			_bornIdx.Clear();
		}
		
		public void Add(Transform tf,int idx)
		{
			_targetTransformList.Add(tf);
			_bornIdx.Add(idx);
		}
		
		public void RemoveAt(int idx)
		{
			_targetTransformList.RemoveAt(idx);
			_bornIdx.Add(idx);
		}
	}
	
	private FlashPointInfo _flashPointInfo;
	private List<ActionController> _targetACList;
	private List<ActionController> _allAcCanHitList;
	
	protected float _currentAP;
	protected int _currentJumpCount;
	
	protected override void Awake()
	{
		base.Awake();
		_flashPointInfo = new FlashPointInfo();
		_targetACList = new List<ActionController>();
		_allAcCanHitList = new List<ActionController>();
		_thunderBulletDisplayer  = _bulletDisplayers[0] as ThunderBulletDisplayer;
		_thunderBulletDisplayer.SetChainLength(_maxJumpCount+1);
	}
	
	public override void Init(FCObject owner)
	{
		base.Init(owner);
		_isRangerBullet = false;
	}
	void StartFlash()
	{}
	
	void StopFlash()
	{
		
	}
	
	protected override IEnumerator STATE()
	{
		_inState = true;
		if(LifeTime <0)
		{
			LifeTime = _hitInfo[_step]._lifeTime;
		}
		_attackInfo._hitType = _hitInfo[_step]._hitType;
		_flashPointInfo.Clear();
		_targetACList.Clear();
		_allAcCanHitList.Clear();
		float effectLastTime = _effectLastTime;
		_flashPointInfo.Add(_firePoint,_thunderBulletDisplayer.AddPoint(_firePoint.position));
		bool updateLogic = true;
		
		if(_target == null)
		{
			Vector3 pos= _owner.ThisTransform.forward*FCConst.FLASH_NO_TARGET_LENGTH+_firePoint.position;
			_thunderBulletDisplayer.AddPoint(pos);
			_thunderBulletDisplayer.PlayBlindSpark(pos);
			_currentJumpCount = _maxJumpCount;
			updateLogic = false;
			_effectLastTime = 0.5f;
		}
		else
		{
			bool ret = CommandManager.Instance.Send(FCCommand.CMD.HURT,this,FC_PARAM_TYPE.OBJECT,FC_HURT_SOURCE.BULLET,FC_PARAM_TYPE.INT,_target.ObjectID,FCCommand.STATE.RIGHTNOW,true);
			if(ret)
			{
				CommandManager.Instance.Send(FCCommand.CMD.ATTACK_HIT_TARGET,
					_target,FC_PARAM_TYPE.OBJECT,
					ObjectID,FCCommand.STATE.RIGHTNOW,true);
			}
			_flashPointInfo.Add(_target.ACGetTransformByName(_targetSolt),_thunderBulletDisplayer.AddPoint(_target.ACGetTransformByName(_targetSolt).position));
			
			//_thunderBulletDisplayer.Refresh();
			_targetACList.Add(_target);
			_currentAP = 1;
			_currentJumpCount = 0;
			Vector3 r1 = _target.ThisTransform.localPosition - ThisTransform.localPosition;
			r1.y = 0;
			r1.Normalize();
			if(r1 != Vector3.zero)
			{
				ThisTransform.forward = r1;
			}
			ThisTransform.localPosition = _target.ThisTransform.localPosition;
		}
		int jumpcount = 2;
		float timeCounter1 = 0;
		float timeCounter2 = 0;
		float maxLostTime = _hitInfo[_step]._lifeTime*3;
		
		while(_inState)
		{
			if(LifeTime>0 && updateLogic)
			{
				LifeTime -= Time.deltaTime;
				if(LifeTime<=0)
				{
					LifeTime = _hitInfo[_step]._lifeTime;
					if(GetNextTarget(ref _target))
					{
						_currentAP = 1-_reducePercent*_currentJumpCount;
						
						_flashPointInfo.Add(_target.ACGetTransformByName(_targetSolt),_thunderBulletDisplayer.AddPoint(_target.ACGetTransformByName(_targetSolt).position));

						_targetACList.Add(_target);
						jumpcount++;
						//CommandManager.Instance.Send(FCCommand.CMD.HURT,this,FC_PARAM_TYPE.OBJECT,FC_HURT_SOURCE.BULLET,FC_PARAM_TYPE.INT, _target.ObjectID,FCCommand.STATE.RIGHTNOW,true);
						
						bool ret = CommandManager.Instance.Send(FCCommand.CMD.HURT,this,FC_PARAM_TYPE.OBJECT,FC_HURT_SOURCE.BULLET,FC_PARAM_TYPE.INT,_target.ObjectID,FCCommand.STATE.RIGHTNOW,true);
						if(ret)
						{
							CommandManager.Instance.Send(FCCommand.CMD.ATTACK_HIT_TARGET,
								_target,FC_PARAM_TYPE.OBJECT,
							ObjectID,FCCommand.STATE.RIGHTNOW,true);
						}
						
						Vector3 r1 = _target.ThisTransform.localPosition - ThisTransform.localPosition;
						r1.y = 0;
						r1.Normalize();
						if(r1 != Vector3.zero)
						{
							ThisTransform.forward = r1;
						}
						ThisTransform.localPosition = _target.ThisTransform.localPosition;
						
					}
					else
					{
						updateLogic = false;
					}
				}
				if(timeCounter2 <maxLostTime)
				{
					timeCounter2+=Time.deltaTime;
					if(timeCounter2>=maxLostTime)
					{
						timeCounter2-=maxLostTime;
						if(_targetACList.Count>0)
						{
							_targetACList.RemoveAt(0);
						}
					}
				}
			}
			if(!updateLogic)
			{
				if(effectLastTime>0)
				{
					effectLastTime -= Time.deltaTime;
				}
				else
				{
					if(_dispareTime<0)
					{
						_inState = false;
					}
					else if(Mathf.Approximately(_dispareTime, 0.0f))
					{
						if(jumpcount<2)
						{
							_inState = false;
						}
						else
						{
							jumpcount--;
							_thunderBulletDisplayer.RemoveFirst();
						}
					}
					else
					{
						if(timeCounter1 <_dispareTime)
						{
							timeCounter1+=Time.deltaTime;
							if(timeCounter1>=_dispareTime)
							{
								timeCounter1-=_dispareTime;
								if(jumpcount<2)
								{
									_inState = false;
								}
								else
								{
									jumpcount--;
									//_thunderBulletDisplayer.Refresh();
									_thunderBulletDisplayer.RemoveFirst();
								}
							}
						}
					}
				}

			}
			for(int i =0;i<_flashPointInfo.Count;i++)
			{
				if(_flashPointInfo._targetTransformList[i] != null)
				{
					_thunderBulletDisplayer.UpdatePos(_flashPointInfo._bornIdx[i],_flashPointInfo._targetTransformList[i].position);
				}
			}
			yield return null;
		}
		_flashPointInfo.Clear();
		_thunderBulletDisplayer.Clear();
		Dead();
	}
	
	public override void Dead()
	{
		StopAllCoroutines();
		base.Dead();
	}
	
	protected override void ActiceLogicSelf(RangerAgent.FirePort rfp)
	{
		if(rfp != null && rfp.IsOverride)
		{
			_maxJumpCount = rfp.AttackCount;
		}
	}
	
	void RenderFlash()
	{

	}
	bool GetNextTarget(ref ActionController target)
	{
		ActionController nextTarget = null;
		_allAcCanHitList.Clear();
		if(_currentJumpCount<_maxJumpCount)
		{
			_currentJumpCount++;			
			ActionControllerManager.Instance.GetEnemyTargetsBySight(ThisTransform,_effectDistance,0,_faction,_effectAngle,ref _allAcCanHitList);
			float effectDistanceSqrt = _effectDistance*_effectDistance+1;
			if(_allAcCanHitList.Count>0)
			{
				bool r1 = true;
				foreach(ActionController ac in _allAcCanHitList)
				{
					if(ac != target)
					{
						float len = (ac.ThisTransform.localPosition-ThisTransform.localPosition).sqrMagnitude;
						bool r2 =  _targetACList.Contains(ac);
						if((r1 && ((r2 && len<effectDistanceSqrt) || !r2)) || (!r1 && !r2 && len<effectDistanceSqrt))
						{
							nextTarget = ac;
							r1 = r2;
							effectDistanceSqrt = len;
						}
					}
				}
			}
		}
		if(nextTarget != null)
		{
			target = nextTarget;
		}
		return (nextTarget != null);
	}
}
