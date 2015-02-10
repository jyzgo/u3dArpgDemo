using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("FC/Logic/FCObject/WeaponAndArmor/FCWeapon")]


public class FCWeapon : FCEquipmentsBase,AttackUnit {
	
	public FC_EQUIPMENTS_TYPE _weaponType;
	public WEAPON_HIT_TYPE _weaponHitType = WEAPON_HIT_TYPE.SLASH;
	protected string _hitSound = "";
	
	public int _sharpness = 100;
	
	public List<Transform> _realBodys;
	
	public enum WEAPON_HIT_TYPE
	{
		NONE,
		SLASH,
		BLUNT,
		CLAW,
		ENERGY,
		ALL
	}
	ActionController _owner;
	
	[System.Serializable]
	public class BladeInfo
	{
		// one weapon may have more than one blade slide
		public BladeSlide[] _bladeSlide = null;
		
		protected BladeSlide[] _currentBladeSlide = null;
		public BladeSlide[] CurrentBladeSlide
		{
			get
			{
				return _currentBladeSlide;
			}
			set
			{
				_currentBladeSlide = value;
			}
		}
		
		public Transform _originalTransform = null;
		public Transform _currentTransform = null;
		public Transform _bladeTransform = null;
		public GameObject []_colliders = null;
		public Vector3 []_bladerCenter = null;
		public Vector3 []_bladerSize = null;
		public BoxCollider [] _bladeReal = null;
		public MessageReciever[] _messageReceivers = null;
        public GameObject[] cubes = null;
		
		public FC_EQUIPMENTS_TYPE _bladeType = FC_EQUIPMENTS_TYPE.MAX;
	}
	
	private BladeInfo[] _bladeInfos = null;
	private int _bladeInfosIdx = 0;
	
	private AttackInfo _attackInfo = new AttackInfo();
	protected FC_DAMAGE_TYPE _damageType = FC_DAMAGE_TYPE.PHYSICAL;
	
	public ActionController Owner
	{
		get
		{
			return _owner;
		}
	}
	protected override void Awake()
	{
		base.Awake();
		ObjectID.ObjectType = FC_OBJECT_TYPE.OBJ_WEAPON;
		_attackInfo._hitType = AttackHitType.KnockDown;
		_attackInfo._effectTime = 4f;
		_bladeInfos = new BladeInfo[_equipmentSlots.Length];
		switch(_weaponHitType)
		{
		case WEAPON_HIT_TYPE.BLUNT:
			_hitSound = FCConst.WEAPON_HIT_SOUND_BLUNT;
			break;
		case WEAPON_HIT_TYPE.SLASH:
			_hitSound = FCConst.WEAPON_HIT_SOUND_SLASH;
			break;
		case WEAPON_HIT_TYPE.CLAW:
			_hitSound = FCConst.WEAPON_HIT_SOUND_CLAW;
			break;
		case WEAPON_HIT_TYPE.NONE:
			_hitSound = "";
			break;
		}
	}


	
	public FCWeapon.WEAPON_HIT_TYPE GetAttackerType()
	{
		return _weaponHitType;
	}
	
	public override void SetOwner(FCObject owner)
	{
		_owner = owner as ActionController;
		if (_owner)
		{
			_owner.RegisterToWeaponList(this);
		}
	}
	
	public void SwitchWeaponTo(EnumEquipSlot slot, FC_EQUIPMENTS_TYPE weaponTypeFlag)
	{
		//bool ret = false;
		Transform transSlot = null;
		
		if(slot != EnumEquipSlot.MAX)
		{
			transSlot = _owner._avatarController.GetSlotNode(slot);
		}
		SwitchWeaponTo(transSlot, weaponTypeFlag);
	
	}
	
	//transSlot = null ,means weapon model to default node
	public void SwitchWeaponTo(Transform transSlot, FC_EQUIPMENTS_TYPE weaponTypeFlag)
	{
		bool ret = false;

		if(weaponTypeFlag == FC_EQUIPMENTS_TYPE.MAX
			|| weaponTypeFlag == _weaponType)
		{
			ret = true;
		}
		foreach(BladeInfo bi in _bladeInfos)
		{
			if(ret || weaponTypeFlag == bi._bladeType)	
			{
				if(transSlot == null)
				{
					Utils.SetParent(bi._bladeTransform, bi._originalTransform);
					bi._currentTransform = null;
				}
				else
				{
					Utils.SetParent(bi._bladeTransform, transSlot);
					bi._currentTransform = transSlot;
				}
			}
		}
	
	}
	public override void OnAssembled (EnumEquipSlot slot, GameObject go, FC_EQUIPMENTS_TYPE equipmentType)
	{
		//_bladeInfosIdx = 0;
		if(_bladeInfos != null && _bladeInfosIdx >= _bladeInfos.Length)
		{
			_bladeInfosIdx = 0;
		}
		Transform []transforms = go.GetComponentsInChildren<Transform>();
		List<GameObject> gl = new List<GameObject>();
		List<MessageReciever> ml = new List<MessageReciever>();
		List<BladeSlide> bl = new List<BladeSlide>();
		
		_bladeInfos[_bladeInfosIdx] = new BladeInfo();
		
		_bladeInfos[_bladeInfosIdx]._bladeType = equipmentType;
		_bladeInfos[_bladeInfosIdx]._bladeTransform = go.transform;
		_bladeInfos[_bladeInfosIdx]._originalTransform = go.transform.parent;
		_bladeInfos[_bladeInfosIdx]._currentTransform = go.transform.parent;

		foreach(Transform t in transforms)
		{
			GameObject subgo = t.gameObject;
			Collider c = subgo.GetComponent<Collider>();
			if(c != null) {
				gl.Add(subgo);
			}

			MessageReciever mr = subgo.GetComponent<MessageReciever>();
			if(mr != null)
			{
				ml.Add(mr);
			}
			BladeSlide blade = subgo.GetComponent<BladeSlide>();
			if(blade != null)
			{
				bl.Add(blade);
			}
		}

		_bladeInfos[_bladeInfosIdx]._colliders = gl.ToArray();
		_bladeInfos[_bladeInfosIdx]._messageReceivers = ml.ToArray();
		_bladeInfos[_bladeInfosIdx]._bladerCenter = new Vector3[_bladeInfos[_bladeInfosIdx]._colliders.Length];
		_bladeInfos[_bladeInfosIdx]._bladerSize = new Vector3[_bladeInfos[_bladeInfosIdx]._colliders.Length];
		_bladeInfos[_bladeInfosIdx]._bladeReal = new BoxCollider[_bladeInfos[_bladeInfosIdx]._colliders.Length];
		_bladeInfos [_bladeInfosIdx].cubes = new GameObject[_bladeInfos [_bladeInfosIdx]._colliders.Length];

		if(_bladeInfos[_bladeInfosIdx]._colliders.Length != 0)
		{
			for(int i = 0; i< _bladeInfos[_bladeInfosIdx]._colliders.Length; i++)
			{
				_bladeInfos[_bladeInfosIdx]._bladeReal[i]  = _bladeInfos[_bladeInfosIdx]._colliders[i].collider as BoxCollider;
				_bladeInfos[_bladeInfosIdx]._bladerCenter[i] = _bladeInfos[_bladeInfosIdx]._bladeReal[i].center;
				_bladeInfos[_bladeInfosIdx]._bladerSize[i] = _bladeInfos[_bladeInfosIdx]._bladeReal[i].size;
                _bladeInfos[_bladeInfosIdx].cubes[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                _bladeInfos[_bladeInfosIdx].cubes[i].transform.parent = go.transform;
                _bladeInfos[_bladeInfosIdx].cubes[i].transform.gameObject.SetActive(false);
			}
		}

		_bladeInfos[_bladeInfosIdx]._bladeSlide = bl.ToArray();
		_bladeInfosIdx++;
	}
	
	//open or close blade of weapon and resize the collider bounds
	//if bladeLayer == FC_CONST.LAYER_DEFAULT, means deactive the blade of weapon
	private static void ReleaseBlade(BladeInfo bi, int bladeLayer, bool openIt, int xScale, int yScale, int zScale, bool needHitID)
	{
		int i =0;
		foreach(GameObject go in bi._colliders) 
		{
			go.layer = bladeLayer;

            GameObject collider = bi._colliders[i];
            BoxCollider bladeCollider = bi._bladeReal[i];
			Vector3 center = bi._bladerCenter[i];
			Vector3 sizeNew = bi._bladerSize[i];

			if(openIt)
			{	
				if(xScale != 100)
				{
					sizeNew.x = sizeNew.x*(xScale /100f);
				}
				if(yScale != 100)
				{
					sizeNew.y = sizeNew.y*(yScale /100f);
				}
				//we only need weapon to be longer at one direction
				if(zScale != 100)
				{
					sizeNew.z = sizeNew.z*(zScale /100f);
					center.z = center.z + (sizeNew.z-bi._bladerSize[i].z)/2;
				}

			}

            bladeCollider.center = center;
            bladeCollider.size = sizeNew;

            if (CheatManager.weaponCollider)
            {
                Transform t = bi.cubes[i].transform;
                t.localPosition = collider.transform.localPosition + center;
                t.localRotation = collider.transform.localRotation;
                t.localScale = sizeNew;
                t.gameObject.SetActive(openIt);
            }

			i++;
		}
		foreach(MessageReciever mr in bi._messageReceivers)
		{
			if(openIt)
			{
				if(needHitID)
				{
					mr._hitID = GameManager.GainHitID();
				}
				else
				{
					mr._hitID = 0;
				}
			}
			else
			{
				mr._hitID = -1;
			}
		}
		foreach(BladeSlide blade in bi._bladeSlide)
		{
			blade.Emit = openIt;
		}
	}
	public static void ReleaseBlade(FCWeapon ewn,bool openIt,FC_EQUIPMENTS_TYPE weaponTypeFlag,AttackBase ab)
	{
		ActionController ac = ewn._owner;

		int bladeLayer = FCConst.LAYER_DEFAULT;
		if(openIt)
		{
			if(ab._canHitAll)
			{
				bladeLayer = FCConst.LAYER_ENEMY_WEAPON;
			}
			else
			{
				bladeLayer = (int)ac.Faction+1;
			}

            ewn._attackInfo._attackPoints = ac.TotalAttackPoints;
			ewn._attackInfo._isFromSkill = ewn._owner.IsFromSkill;
			ewn._attackInfo._damageScale = ab.DamageScale;
			ewn._attackInfo._pushStrength = ab._pushStrength;
			ewn._attackInfo._pushTime = ab._pushTime;
			ewn._attackInfo._pushAngle = ab._pushAngle;
			ewn._attackInfo._pushByPoint = ab._pushByPoint;
			ewn._attackInfo._damageType = ab.GetDamageType();
		}
		else
		{
			ewn._attackInfo._pushStrength = 0;
			ewn._attackInfo._pushTime = 0;
			ewn._attackInfo._damageType = FC_DAMAGE_TYPE.NONE;
		}
		int ewt = (int)ewn._weaponType;
		int wt = (int)weaponTypeFlag;
		if(weaponTypeFlag == FC_EQUIPMENTS_TYPE.MAX 
			|| ewn._weaponType == weaponTypeFlag
			|| ewt >= 1500 && ewt/10 == wt/10)
		{
			foreach(BladeInfo bi in ewn._bladeInfos)
			{
				
				if(ab != null)
				{
					if(weaponTypeFlag != FC_EQUIPMENTS_TYPE.WEAPON_LIGHT
						&& weaponTypeFlag != FC_EQUIPMENTS_TYPE.WEAPON_THIGH
						&& weaponTypeFlag != FC_EQUIPMENTS_TYPE.MAX)
					{
						if(weaponTypeFlag != bi._bladeType)
						{
							continue;
						}
					}
					ReleaseBlade(bi, bladeLayer, openIt, ab._weaponScaleX, ab._weaponScaleY, ab._weaponScaleZ, !ab._needNotHitID);
				}
				else
				{
					ReleaseBlade(bi, bladeLayer, openIt, 1, 1, 1, true);
				}
			}
		}
		/*else
		{
			foreach(BladeInfo bi in ewn._bladeInfos)
			{
				if(ab != null)
				{
					ReleaseBlade(bi, bladeLayer, openIt, ab._weaponScaleX, ab._weaponScaleY, ab._weaponScaleZ);
				}
				else
				{
					ReleaseBlade(bi, bladeLayer, openIt, 1, 1, 1);
				}
			}
		}*/	
	}
	
	public virtual ActionController GetOwner()
	{
		return _owner;
	}
	
	public virtual AttackHitType GetHitType()
	{
		return _owner.CurrentWeaponHitType;
	}
	
	public virtual void GetHurtDirection(Transform targetTransform,ref Vector3 direction)
	{
		if(_owner.HaveAttackCorrectMelee || _owner.HaveAttackPushBack)
		{
			Vector3 v1 = targetTransform.localPosition - _owner.ThisTransform.localPosition;
			v1.y =0;
			v1.Normalize();
			if(v1 != Vector3.zero)
			{
				float angle = Vector3.Angle(_owner.ThisTransform.forward,v1);
				if(angle < _owner.AttackCorrectAngleForMelee || _owner.HaveAttackPushBack)
				{
					direction =  _owner.ThisTransform.forward;
				}
				else
				{
					direction = -(_owner.ThisTransform.localPosition-targetTransform.localPosition);
				}
			}
		}
		else
		{
			direction = -(_owner.ThisTransform.localPosition-targetTransform.localPosition);
		}
		direction.y =0;
		direction.Normalize();
		if(direction == Vector3.zero)
		{
			direction = -targetTransform.forward;
		}
	}
	
	public bool IsFrom2P()
	{
		return (_owner.IsClientPlayer);
	}
	
	public AttackInfo GetAttackInfo()
	{
		_attackInfo._hitType = _owner.CurrentWeaponHitType;
		return _attackInfo;
	}
	
	public override bool HandleCommand(ref FCCommand ewd)
	{
		switch(ewd._cmd)
		{
			case FCCommand.CMD.ATTACK_HIT_TARGET:
			{
				ActionController target = ewd._param1 as ActionController;
				HandleHitTarget.HandleHit(_owner, this, _damageType, target);
			
				if(GetAttackInfo()._hitType == AttackHitType.ForceBack)
				{
					AIAgent aiAgent = _owner.gameObject.GetComponentInChildren<AIAgent>();
					if(aiAgent != null)
					{
						aiAgent.AddHitBackList(target);
					}
				}
				if(_hitSound != "")
				{
					SoundManager.Instance.PlaySoundEffect(_hitSound);
				}
			}
			break;
			case FCCommand.CMD.ATTACK_OUT_OF_RANGE:
			{
				ActionController target = ewd._param1 as ActionController;

                if (GetAttackInfo()._hitType == AttackHitType.ForceBack)
				{
					AIAgent aiAgent = _owner.gameObject.GetComponentInChildren<AIAgent>();
					if(aiAgent != null)
					{
						if(!GameManager.Instance.IsPVPMode)
						{
							aiAgent.RemoveHitBackList(target);
						}
					}
				}	
			}
			break;
		}
		return true;
	}
	
	public int GetFinalDamage(DefenseInfo di,out bool isCriticalHit, ActionController target)
	{
		int ret = 0;
		ret = DamageCounter.Result(_attackInfo, di, out isCriticalHit, _owner, target);
		if(isCriticalHit && _owner.IsPlayerSelf)
		{
			_owner.IsCriticalHit = isCriticalHit;
		}
		return ret;
	}

    public Eot[] GetEots(DefenseInfo di)
    {
        if (_owner.ACGetCurrentAttack() != null)
        {
            foreach (Eot eot in _owner.ACGetCurrentAttack()._eots)
            {
                switch (eot.eotType)
                {
                    case Eot.EOT_TYPE.EOT_PHYSICAL:
                        eot.OwnerDamage = _attackInfo._attackPoints[0] - di._defensePoints[0];
                        break;
                    case Eot.EOT_TYPE.EOT_FIRE:
                        eot.OwnerDamage = _attackInfo._attackPoints[1] - di._defensePoints[1];
                        break;
                    case Eot.EOT_TYPE.EOT_ICE:
                        eot.OwnerDamage = _attackInfo._attackPoints[1] - di._defensePoints[1];
                        break;
                    case Eot.EOT_TYPE.EOT_THUNDER:
                        eot.OwnerDamage = _attackInfo._attackPoints[1] - di._defensePoints[1];
                        break;
                    case Eot.EOT_TYPE.EOT_POISON:
                        eot.OwnerDamage = _attackInfo._attackPoints[1] - di._defensePoints[1];
                        break;
                    default:
                        eot.OwnerDamage = _attackInfo._attackPoints[0] - di._defensePoints[0];
                        break;
                }
            }

            return _owner.ACGetCurrentAttack()._eots;
        }
        return null;
    }
	
	public int GetSharpness()
	{
		return _sharpness;
	}
	
	public bool CanHit(ActionController ac)
	{
		return true;
	}
	
	public FC_OBJECT_TYPE GetObjType()
	{
		return ObjectID.getOnlyObjectType;
	}
	
	public Transform GetAttackerTransform()
	{
		return _owner.ThisTransform;
	}

}
