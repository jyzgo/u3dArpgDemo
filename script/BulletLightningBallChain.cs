using UnityEngine;
using System.Collections;

public class BulletLightningBallChain : MonoBehaviour {
	
	public MeshRenderer _chain;
	public float _totalDuration;
	public float _visibleDuration;
	
	[HideInInspector]
	public BulletLightningBall _pool;
	[HideInInspector]
	public EnumEquipSlot _targetPart;
	[HideInInspector]
	public ActionController _target;
	float _timer;
	Transform _myTransform;
	Transform _targetTransform;
	protected FCCommand _fastCommand;
	public string _sfx;
	public FCBullet _owner =null;
		
	public ActionController Target {
		get {return _target;}
	}
	
	void Awake() {
		_myTransform = GetComponent<Transform>();
		_chain.enabled = false;
		_fastCommand = new FCCommand();
	}
	
	public void End()
	{
		gameObject.SetActive(false);
		_pool.AddFreeChain(this);
		_target = null;
	}
	
	void Update() {
		_timer -= Time.deltaTime;
		// get a invisible gap to perform 'winking'
		if(_timer < _totalDuration - _visibleDuration) {
			_chain.enabled = false;
		}
		// get over a loop. restart next 'winking'
		if(_timer < 0.0f) {
			// check if enemy is alive.
			if(!_target.IsAlived) {
				End();
				_pool.AssignChainToNewTarget(this);
				return;
			}
			_chain.enabled = true;
			_timer = _totalDuration;
			CommandManager.Instance.SendFast(ref _fastCommand, _target);
			SoundManager.Instance.PlaySoundEffect(_sfx);
			// add hurt effect on enemy.
			_owner.IsHitSomeone(_pool.Owner, _pool, _pool._damageType, _target);
		}
		// position & rotation settings.
		_myTransform.LookAt(_targetTransform);
		Vector3 s = _myTransform.localScale;
		s.z = (_pool.ThisTransform.position - _targetTransform.position).magnitude;
		_myTransform.localScale = s;
	}
	
	public void Active(ActionController target, FCBullet owner) {
		_target = target;
		
		_myTransform.localPosition = _pool._core.position;
		_targetTransform = target.ACGetTransformByName(_targetPart);
		gameObject.SetActive(true);
		
		_timer = -1.0f; // delay 1st hurt to update.
		// init hurt parameter.
		_fastCommand._param1 = _pool;
		_fastCommand._cmd = FCCommand.CMD.HURT;
		_fastCommand._param2 = _target;
		_fastCommand._param3 = 0;
		_owner = owner;
	}
}
