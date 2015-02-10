using UnityEngine;
using System.Collections;

public class EffectInstance : MonoBehaviour {
	public int _partLevel = 0;
	public bool _considerForce = false; //consider force, true: will consider force stop or not, else stop whenever force or not
	public float _lifeTime = 9999; //if the effect is a global effect, it will be disable and back to pool when life time is zero
	public float _deadTime = -1; //if > 0, when stop effect, it will stay these time.
	
	private float _lifeTick = 0; //current tick from start effect
	
	private Transform _myTrnasform = null;
	public Transform myTransform {
		get { return _myTrnasform;}
		set { _myTrnasform = value;}
	}
	
	private GameObject _myObject = null;
	public GameObject myObject {
		get { return _myObject;}
		set { _myObject = value;}
	}
	public float LifeTick
	{
		get 
		{
			return _lifeTick;
		}		
		set
		{
			_lifeTick = value;
		}
	}
	
	
	private float _deadTick = -1;
	public float DeadTick{
		get{return _deadTick;}
		set{_deadTick = value;}
	}
	
	
	private Transform _pointLightTransform = null;
	private float _originalSize = 1;
	
	public virtual void Awake()
	{
		_myTrnasform = gameObject.transform;
		_myObject = gameObject;
		InitPointLight();
	}
	
	private void InitPointLight()
	{
		_pointLightTransform = null;
		
		if(_deadTime > 0)
		{
			PointLightRenderer pointLight = GetComponentInChildren<PointLightRenderer>();
			if(pointLight != null)
			{
				_pointLightTransform = pointLight.gameObject.transform;
				_originalSize  = _pointLightTransform.localScale.x;
			}
		}
	}
	
	public void UpdatePointLight()
	{
		if(_deadTime > 0)
		{
			if(_pointLightTransform != null)
			{
				if(_lifeTick < 0)
				{
					float scale = _deadTick /_deadTime;
					if(scale <=0)
					{
						scale = 0.1f;	
					}
					float size = _originalSize * scale;
					Vector3 newScale = new Vector3(size, size, size);
					_pointLightTransform.localScale = newScale;
				}
			}
		}
	}
	
	private void ResetPointLight()
	{
		if(_pointLightTransform != null)
		{
			float size = _originalSize;
			Vector3 newScale = new Vector3(size, size, size);
			_pointLightTransform.localScale = newScale;
		}
	}
	
	
	
	private bool _effectStarted = false;
	public bool effectStarted
	{
		get 
		{
			return _effectStarted;
		}		
		set
		{
			_effectStarted = value;
		}
	}

	private FC_CHARACTER_EFFECT _character_effect_id = FC_CHARACTER_EFFECT.INVALID;
	private FC_GLOBAL_EFFECT _global_effect_id = FC_GLOBAL_EFFECT.INVALID;
	
	public FC_CHARACTER_EFFECT character_effect_id
	{
		set
		{
			_character_effect_id = value;
		}
	}
	
	public FC_GLOBAL_EFFECT global_effect_id
	{
		set
		{
			_global_effect_id = value;
		}
	}	
	
	virtual public void BeginEffect() { ResetPointLight();}
	virtual public void FinishEffect(bool force) {}
}
