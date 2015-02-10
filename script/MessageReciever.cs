using UnityEngine;
using System.Collections;

[AddComponentMenu("FC/Utils/ForAttacker/MessageReciever")]
public class MessageReciever : MonoBehaviour {

	public FCObject _parent = null;
	public int _hitID;
	protected bool _isActive = false;
	protected Collider _selfCollider;
	protected FCCommand _fastCommand;
	private GameObject _thisObject;
	
	public bool IsActive
	{
		get
		{
			return _isActive;
		}
	}
	public enum COLLIDER_TYPE
	{
		Box,
		Sphere,
		Capsule
	}
	
	protected COLLIDER_TYPE _colliderType = COLLIDER_TYPE.Box;
		// Use this for initialization
	
	protected virtual void OnHitSomeOne(Collider other)
	{
		if(_parent.enabled)
		{
			ActionController ac = ActionControllerManager.Instance.GetACByCollider(other);
		
			if(ac != null && ac == _parent) return;
			
			//if(ac.IsClientPlayer) return;
			
			if(_parent.ObjectID.ObjectType == FC_OBJECT_TYPE.OBJ_BULLET)
			{
				if(ac == ((FCBullet)_parent).Owner ) return;
			}
			
			bool ret = false;
			if(ac != null)
			{
				_fastCommand._param1 = _parent;
				_fastCommand._cmd = FCCommand.CMD.HURT;
				_fastCommand._param2 = ac;
				_fastCommand._param3 = _hitID;
				Debug.LogWarning("attack from " + _parent.name + "target = " + ac.name);
									
				ret = CommandManager.Instance.SendFast(ref _fastCommand,ac);	
			}
			if(_parent != null && ret)
			{
				_fastCommand._param1 = ac;
				_fastCommand._cmd = FCCommand.CMD.ATTACK_HIT_TARGET;
				CommandManager.Instance.SendFast(ref _fastCommand,_parent);
			}
		}
	}
	
	protected virtual void FirstInit()
	{
		_fastCommand = new FCCommand();
		_fastCommand.Set(FCCommand.CMD.STOP,null,FCCommand.STATE.RIGHTNOW,true);
		_thisObject = gameObject;
		_selfCollider = _thisObject.collider;
		if(_selfCollider.GetType().ToString().Contains("Sphere"))
		{
			_colliderType = COLLIDER_TYPE.Sphere;
		}
		else if(_selfCollider.GetType().ToString().Contains("Capsule"))
		{
			_colliderType = COLLIDER_TYPE.Capsule;
		}
		if(_parent!= null && _parent.ObjectID.ObjectType == FC_OBJECT_TYPE.OBJ_BULLET)
		{
			((FCBullet)_parent)._damageReceiver = this;
			DeActiveLogic();
		}
		
	}
	void Awake()
	{
		FirstInit();
	}
	
	public virtual void ActiveLogic()
	{
		if(_parent.ObjectID.ObjectType == FC_OBJECT_TYPE.OBJ_BULLET)
		{
			if(((FCBullet)_parent)._canHitAll)
			{
				_thisObject.layer = FCConst.LAYER_ENEMY_WEAPON;
			}
			else
			{
				switch(((FCBullet)_parent).Faction)
				{
				case FC_AC_FACTIOH_TYPE.NEUTRAL_1:
					_thisObject.layer = FCConst.LAYER_NEUTRAL_WEAPON_DAMAGE_1;
					break;
	
				case FC_AC_FACTIOH_TYPE.NEUTRAL_2:
					_thisObject.layer = FCConst.LAYER_NEUTRAL_WEAPON_DAMAGE_2;			
					break;
				}
			}
		}
		_selfCollider.enabled = true;
		_isActive = true;
	}
	
	public virtual void DeActiveLogic()
	{
		_selfCollider.enabled = false;
		_isActive = false;
	}
	
	
	void OnTriggerEnter(Collider other) 
	{
		OnHitSomeOne(other);
	}
	
	public void SetRadius(float radius)
	{
		if(_colliderType == COLLIDER_TYPE.Sphere)
		{
			((SphereCollider)_selfCollider).radius = radius;
		}
		else if(_colliderType == COLLIDER_TYPE.Capsule)
		{
			((CapsuleCollider)_selfCollider).radius = radius;
		}
	}
	
	public float GetRadius()
	{
		if(_colliderType == COLLIDER_TYPE.Sphere)
		{
			return ((SphereCollider)_selfCollider).radius;
		}
		else if(_colliderType == COLLIDER_TYPE.Capsule)
		{
			return ((CapsuleCollider)_selfCollider).radius;
		}
		return 0;
	}
}
