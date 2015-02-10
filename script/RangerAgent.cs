using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("FC/Logic/FCObject/PlayAndEnemy/Agent/AC/RangerAgent")]

[System.Serializable]
public class RangerAgent : FCObject ,FCAgent {
	#region data
	private ActionController _owner;
	private BulletList _bulletListSource;
	public GameObject _bulletAgents;
	protected FCCommand _fastCommand;
	public FirePort[] _firePorts = null;
	
	[System.Serializable]
	public class FireRangeInfo
	{
		public enum RANGER_SHAPE
		{
			Sphere,
			Box
		}
		public RANGER_SHAPE _shape = RANGER_SHAPE.Sphere;
		public float _param1 = 1f;
		public float _param2 = 1f;
		public float _effectTime = 0.25f;
		public bool _needAnchor = false;
		
	}
	[System.Serializable]
	public class FirePort
	{
		public string _portName = "";
		public int _fireCount = -1;
		public string[] _bulletNames; 
		public EnumEquipSlot[] _firePointNames = null;
		protected Transform[] _firePoints = null;
		
		public bool _shootByFirePointDirection = false;
		protected int _fireAngleTotal = 0;
		protected int _fireAngleTotalY = 0;
		
		public int _fireAngleLeft = 0; 
		public int _fireAngleRight = 0;
		
		public int _fireAngleUp = 0;
		public int _fireAngleDown = 0;
		
		public FC_FIRE_WAY _fireWay = FC_FIRE_WAY.RANDOM;
		public int _angleStep;
		public FireRangeInfo _rangeInfo = null;
		
		// uniform parameters.
		public float _attribute1;
		public float _attribute2;
		
		public float _delayTime = 0;
		
		protected bool _isFromSkill = false;
		
		public bool IsFromSkill
		{
			get
			{
				return _isFromSkill;
			}
			set
			{
				_isFromSkill = value;
			}
		}
		
		protected int _angleOffest;
		protected int _angleOffestY;
		
		protected float _dotDamageTime = 0;
		
		//means the data of this fireport is modified by skill data
		protected bool _isOverride = false;
		
		//if true, means this port is firing bullet by Coroutine
		protected bool _beginToFire = false;
		
		public bool BeginToFire
		{
			get
			{
				return _beginToFire;
			}
			set
			{
				_beginToFire = value;
			}
		}
		
		//now only use for flash jump count
		protected int _attackCount;
		
		public int AttackCount
		{
			get
			{
				return _attackCount;
			}
			set
			{
				_attackCount = value;
			}
		}
		
		public bool IsOverride
		{
			get
			{
				return _isOverride;
			}
			set
			{
				_isOverride = value;
			}
		}
		
		public float DotDamageTime
		{
			get
			{
				return _dotDamageTime;
			}
			set
			{
				_dotDamageTime = value;
			}
		}
		
		protected float _damageScale = 1f;
		
		protected float _damageScaleSource = 1f;
		
		public float DamageScale
		{
			get
			{
				return _damageScale;
			}
			set
			{
				_damageScale = value;
			}
		}
		
		public float DamageScaleSource
		{
			get
			{
				return _damageScaleSource;
			}
			set
			{
				_damageScaleSource = value;
			}
		}
	
		public int AngleOffset
		{
			get
			{
				return _angleOffest;
			}
		}
		
		public int AngleOffsetY
		{
			get
			{
				return _angleOffestY;
			}
		}
		
		public int _fireCountMaxPerTimes = 1;
		
		public float _fireCDTime = 0.1f;
		public Transform[] FirePoints
		{
			get
			{
				return _firePoints;
			}
		}
		public void RefreshPorts(ActionController ac)
		{
			if(_firePointNames != null && _firePointNames.Length != 0)
			{
				_firePoints = new Transform[_firePointNames.Length];
				for(int i =0;i< _firePointNames.Length;i++)
				{
					_firePoints[i] = ac.ACGetTransformByName(_firePointNames[i]);
				}
				_fireAngleTotal = _fireAngleRight - _fireAngleLeft;
				_fireAngleTotalY = _fireAngleUp - _fireAngleDown;
				if(_angleStep>0)
				{
					_angleOffest = _fireAngleTotal/_angleStep;
				}
				else
				{
					_angleOffest = 0;
				}
				_damageScale = _damageScaleSource;
			}
		}
		
		public void RefreshPorts(FCBullet eb)
		{
			if(_firePointNames != null && _firePointNames.Length != 0)
			{
				_firePoints = new Transform[_firePointNames.Length];
				for(int i =0;i< _firePointNames.Length;i++)
				{
					_firePoints[i] = eb.ACGetTransformByName(_firePointNames[i]);
				}
				_fireAngleTotal = _fireAngleRight - _fireAngleLeft;
				if(_angleStep>0)
				{
					_angleOffest = _fireAngleTotal/_angleStep;
				}
				else
				{
					_angleOffest = 0;
				}
			}
		}
	}
	
	public static string GetTypeName()
	{
		return "RangerAgent";
	}
	
	public void Init(FCObject owner)
	{
		_owner = owner as ActionController;
		_fastCommand = new FCCommand();
		_fastCommand.Set(FCCommand.CMD.STOP,_owner.ObjectID,FCCommand.STATE.RIGHTNOW,true);

	}
	
	protected override void Awake()
	{
		base.Awake();
		_bulletListSource = _bulletAgents.GetComponentInChildren<BulletList>();
	}
	
	public static void KillBulletWithFirePort(int portIdx,RangerAgent ra)
	{
		BulletList bulletListSource = ra._bulletListSource;
		FirePort[] firePorts = ra._firePorts;
		if(firePorts != null && portIdx < firePorts.Length)
		{
			if(firePorts[portIdx]._fireCount != 0)
			{
				for(int i =0;i < firePorts[portIdx]._bulletNames.Length;i++)
				{
					bulletListSource.KillABulletByNameAndFireport(firePorts[portIdx]._bulletNames[i], firePorts[portIdx]._portName);
				}
			}
		}
	}
	static void FireBullet(string bulletName,int angle,int angleY,Transform tf,FirePort fp, float lifeTime,ActionController owner)
	{
		FCBullet eb = owner.GetBulletFromPool(bulletName);
		ActionController ac = null;
		if(owner.IsPlayer)
		{
			if(eb._seekLevel == FCBullet.SEEK_LEVEL.NORMAL)
			{
				ac = ActionControllerManager.Instance.GetEnemyTargetBySight
					(owner.ThisTransform,eb._maxTargetingDistance ,0,owner.Faction,FCConst.SEEK_ANGLE_NORMAL,true);
			}
			else if (eb._seekLevel == FCBullet.SEEK_LEVEL.PRO)
			{
				ac = ActionControllerManager.Instance.GetEnemyTargetBySight
					(owner.ThisTransform,eb._maxTargetingDistance ,0,owner.Faction,FCConst.SEEK_ANGLE_NORMAL,FCConst.SEEK_ANGLE_PRO,true);
			}
		}
		else
		{
			if(eb._seekLevel == FCBullet.SEEK_LEVEL.NORMAL)
			{
				//ac = ActionControllerManager.Instance.GetEnemyTargetBySight
				//	(owner.ThisTransform,eb._maxTargetingDistance ,0,owner.Faction,FC_CONST.SEEK_ANGLE_NORMAL,true);
			}
			else if (eb._seekLevel == FCBullet.SEEK_LEVEL.PRO)
			{
				if(owner.TargetAC != null)
				{
					ac = owner.TargetAC;
				}
			}
		}
		if(eb != null)
		{
			/*_fastCommand._cmd = FCCommand.CMD.DIRECTION_FACE_TARGET;
			_fastCommand._param1 = ac;
			CommandManager.Instance.SendFast(ref _fastCommand,_owner);*/
			eb.Init(owner);
			if(eb.IsRangerBullet)
			{
				eb.FireRanger(null,tf,fp);
				//(eb as BulletRanger).Fire(5f,3f,FC_HIT_TYPE.DIZZY,tf,false);
			}
			else
			{
				eb.Fire(ac,tf,angle,angleY,lifeTime, fp);
				if(eb._controlByAttack 
					&& ac != null 
					&& owner.ACGetCurrentAttack() != null 
					&& owner.ACGetCurrentAttack()._needAttackCorrect)
				{
					Vector3 v3 = ac.ThisTransform.localPosition - owner.ThisTransform.localPosition;
					v3.y =0;
					v3.Normalize();
					if(v3 != Vector3.zero)
					{
						owner.ACRotateTo(v3, -1, true, true);
					}
				}
			}
			if(eb._controlByAttack)
			{
				owner.ACAddBulletToAttack(eb);
			}
		}

	}
	
	public static void StopFire(FirePort firePort)
	{
		firePort.BeginToFire = false;
	}
	public static void FireBullet(FirePort[] firePorts, int portIdx, float lifeTime,ActionController owner)
	{
		if(firePorts != null && portIdx < firePorts.Length)
		{
			//if(firePorts[portIdx]._shakeScreen)
			//{
				//may we should remove shake in this class ,instead ,we use mmeffect 
				//CameraController.Instance.StartCameraEffect(EnumCameraEffect.effect_0);
			//}
		
			if(firePorts[portIdx]._fireCount != 0 && (firePorts[portIdx]._fireCount <= firePorts[portIdx]._fireCountMaxPerTimes && firePorts[portIdx]._delayTime<= 0))
			{
				if(firePorts[portIdx]._fireWay == FC_FIRE_WAY.FROM_ONE_SIDE_TO_OTHER)
				{
					if(firePorts[portIdx]._fireCount == 1)
					{
						string bulletName = firePorts[portIdx]._bulletNames[0];
						FireBullet(bulletName, 0,firePorts[portIdx]._fireAngleUp ,firePorts[portIdx].FirePoints[0], firePorts[portIdx], lifeTime, owner);
					}
					else
					{
						int startAngle = firePorts[portIdx]._fireAngleLeft;
						int startAngleY = firePorts[portIdx]._fireAngleUp;
						for(int i =0;i< firePorts[portIdx]._fireCount;i++)
						{
							string bulletName = firePorts[portIdx]._bulletNames[i];
							
							FireBullet(bulletName,startAngle,startAngleY,firePorts[portIdx].FirePoints[i],firePorts[portIdx], lifeTime, owner);
							startAngle += firePorts[portIdx].AngleOffset;
							startAngleY += firePorts[portIdx].AngleOffsetY;
						}
					}
				}
				else
				{
					for(int i =0;i< firePorts[portIdx]._fireCount;i++)
					{
						string bulletName = firePorts[portIdx]._bulletNames[i];
						
						int offsetAngle = Random.Range(firePorts[portIdx]._fireAngleLeft,firePorts[portIdx]._fireAngleRight);
						int offsetAngleY = Random.Range(firePorts[portIdx]._fireAngleUp,firePorts[portIdx]._fireAngleDown);
						FireBullet(bulletName,offsetAngle,offsetAngleY,firePorts[portIdx].FirePoints[i],firePorts[portIdx], lifeTime, owner);
					}
				}
				
			}
			else
			{
				owner.StartCoroutine(FireMore(firePorts, portIdx, lifeTime, owner));
			}
		}
	}
	
	static public IEnumerator FireMore(FirePort[] firePorts, int portIdx,float lifeTime, ActionController owner)
	{
		int maxCount = firePorts[portIdx]._fireCount;
		//float timeCounter = firePorts[portIdx]._fireCDTime;
		float timeCounter = firePorts[portIdx]._delayTime;
		bool inState = true;
		int startAngle = firePorts[portIdx]._fireAngleLeft;
		int startAngleY = firePorts[portIdx]._fireAngleUp;
		int bulletIdx = 0;
		int bulletPortIdx = 0;
		firePorts[portIdx].BeginToFire = true;
#if WQ_CODE_WIP
		int offsetAngle = _firePorts[portIdx]._fireAngleTotal/(2-1);
#endif
		while(inState && firePorts[portIdx].BeginToFire)
		{
			if(owner.IsAlived)
			{
				if(timeCounter >0)
				{
					timeCounter -= Time.deltaTime;
				}
				else
				{
					int fireCount = firePorts[portIdx]._fireCountMaxPerTimes;
					if(firePorts[portIdx]._fireCountMaxPerTimes >= maxCount)
					{
						fireCount = maxCount;
						inState = false;
					}
					else
					{
						maxCount -= fireCount;
					}
					if(firePorts[portIdx]._fireWay == FC_FIRE_WAY.RANDOM)
					{
						startAngle = firePorts[portIdx]._fireAngleLeft;
						startAngleY = firePorts[portIdx]._fireAngleUp;
						for(int i =0;i<fireCount;i++)
						{
							int angle = 0;
							if(firePorts[portIdx].AngleOffset == 0)
							{
								angle = Random.Range(firePorts[portIdx]._fireAngleLeft,firePorts[portIdx]._fireAngleRight);
							}
							else
							{
								angle = Random.Range(startAngle,startAngle+firePorts[portIdx].AngleOffset);
							}
							
							int angleY = 0;
							if(firePorts[portIdx].AngleOffsetY == 0)
							{
								angleY = Random.Range(firePorts[portIdx]._fireAngleUp,firePorts[portIdx]._fireAngleDown);
							}
							else
							{
								angleY = Random.Range(startAngleY,startAngleY+firePorts[portIdx].AngleOffsetY);
							}
							bulletIdx = (bulletIdx)%firePorts[portIdx]._bulletNames.Length;
							bulletPortIdx = (bulletPortIdx)%firePorts[portIdx].FirePoints.Length;
							string bulletName = firePorts[portIdx]._bulletNames[bulletIdx];
							FireBullet(bulletName,angle,angleY,firePorts[portIdx].FirePoints[bulletPortIdx],firePorts[portIdx], lifeTime, owner);
							startAngle += firePorts[portIdx].AngleOffset;
							if(startAngle > firePorts[portIdx]._fireAngleRight)
							{
								startAngle = firePorts[portIdx]._fireAngleLeft;
							}
							startAngleY += firePorts[portIdx].AngleOffsetY;
							if(startAngleY > firePorts[portIdx]._fireAngleDown)
							{
								startAngleY = firePorts[portIdx]._fireAngleUp;
							}
							bulletIdx++;
							bulletPortIdx++;
						}
					}
					else
					{
						
						for(int i =0;i<fireCount;i++)
						{
							int angle = startAngle;
							int angleY = startAngleY;
							bulletIdx = (bulletIdx)%firePorts[portIdx]._bulletNames.Length;
							bulletPortIdx = (bulletPortIdx)%firePorts[portIdx].FirePoints.Length;
							string bulletName = firePorts[portIdx]._bulletNames[bulletIdx];
							FireBullet(bulletName,angle,angleY,firePorts[portIdx].FirePoints[bulletPortIdx],firePorts[portIdx], lifeTime, owner);
							startAngle += firePorts[portIdx].AngleOffset;
							startAngleY += firePorts[portIdx].AngleOffsetY;
							if(startAngle > firePorts[portIdx]._fireAngleRight)
							{
								startAngle = firePorts[portIdx]._fireAngleLeft;
							}
							if(startAngleY > firePorts[portIdx]._fireAngleDown)
							{
								startAngleY = firePorts[portIdx]._fireAngleUp;
							}
							bulletIdx++;
							bulletPortIdx++;
						}
					}
					
					timeCounter = firePorts[portIdx]._fireCDTime;
				}

			}
			else
			{
				inState = false;
			}
			yield return null;
		}
		firePorts[portIdx].BeginToFire = false;
	}
	public void RefreshAllPorts()
	{
		foreach(FirePort fp in _firePorts)
		{
			if(fp != null)
			{
				fp.RefreshPorts(_owner);
			}
		}
	}
	public void ReturnToPool(FCBullet eb)
	{
		_bulletListSource.ReturnToPool(eb);
	}
	
	public FCBullet GetFromPool(string bulletName)
	{
		return _bulletListSource.GetFromPool(bulletName);
	}
	
	protected override void OnDestroy()
	{
		_bulletListSource = null;
	}
	#endregion
}

		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		