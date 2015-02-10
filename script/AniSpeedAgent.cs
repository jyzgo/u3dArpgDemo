using UnityEngine;
using System.Collections;

public class AniSpeedAgent : FCObject ,FCAgent {
	
	public ActionController _owner;
	
	private float _speedDown = 0.0f;
	private float _speedUp= 0.0f;
	private float _speedMin = 1.0f;
	private float _speedMinTime = 0;
	private float _speedMinLastTime = 0.1f;

	private bool _speedDownFlag = false;
	private float _curSpeed = 1.0f;
	private float _curSpeedMax = -1.0f;
	private float _speedOrg = -1.0f;
	public float _speedUpScale = 1.5f;
	
	public bool InState
	{
		get
		{
			return _inState;
		}
	}
	private bool _inState;
	
	public void Init(FCObject owner)
	{
		_owner = owner as ActionController;
	}
	
	public static string GetTypeName()
	{
		return "AniSpeedAgent";
	}
	
	public void SlowDownAnimation(float speed_down, float speed_up, float speed_Min, float speed_min_time, float speedMinLastTime, float dumpReduce)
	{
		if(_speedDownFlag && _inState && dumpReduce < 0.01f)
		{
			return;
		}
		else
		{
			_speedOrg = _owner.AniGetAnimationOrgSpeed();
			_curSpeedMax = _speedOrg * _speedUpScale;
			if(speed_up > 999)
			{
				_curSpeedMax = _curSpeedMax*1.2f;
			}
			if(!_speedDownFlag && _inState)
			{
				float upPercents = _curSpeed/_curSpeedMax;
				if(upPercents >0.9f)
				{}
				else if(upPercents >(0.6f*dumpReduce))
				{
					speed_min_time = (_curSpeed/_curSpeedMax)*speed_min_time;
					speedMinLastTime = (_curSpeed/_curSpeedMax)*speed_min_time;
				}
				else
				{
					return;
				}
			}
			_speedDownFlag =  true;
			_speedDown = speed_down;
			_speedUp = speed_up;
			_speedMin = speed_Min;
			
			_speedMinTime = speed_min_time;
			
			_curSpeed = _speedOrg;
			_speedMinLastTime = speedMinLastTime;
			if(!_inState)
			{
				StartCoroutine(STATE());
			}
		}

	}
	
	public void SlowDownAnimation(float speed_down, float speed_Min, float speedMinLastTime)
	{
		if(_inState)
		{
			return;
		}
		_speedOrg = _owner.AniGetAnimationOrgSpeed();
		_curSpeed = _speedOrg;
		_curSpeedMax = _speedOrg * _speedUpScale;
		_speedDownFlag =  true;
		_speedDown = speed_down;
		_speedMin = speed_Min;
		_speedMinLastTime = speedMinLastTime;
		StartCoroutine(STATE_1());
	}
	
	public void StopEffect(bool beForce)
	{
		if(beForce && _inState)
		{
			StopAllCoroutines();
			_owner.AniSetAnimationSpeed(_speedOrg*_owner.BufferSpeedPercent);
			_inState = false;
		}
	
	}
	
	IEnumerator STATE_1()
	{
		_inState = true;
		while(_inState)
		{
			if(_speedDownFlag)
			{
				_curSpeed  -= _speedDown * Time.deltaTime;
	
				if(_curSpeed < _speedMin)
				{
					_curSpeed = _speedMin;					
					_speedDownFlag = false;
				}
			}
			else
			{
				_speedMinLastTime -= Time.deltaTime;
				if(_speedMinLastTime <= 0)
				{
					_inState = false;
				}
			}
			_owner.AniSetAnimationSpeed(_curSpeed);
			yield return null;
		}
		_owner.AniSetAnimationSpeed(_speedOrg*_owner.BufferSpeedPercent);
	}
	
	IEnumerator STATE()
	{
		_inState = true;
		while(_inState)
		{
			if(_speedDownFlag)
			{
				_curSpeed  -= _speedDown * Time.deltaTime;
	
				if(_curSpeed < _speedMin)
				{
					_curSpeed = _speedMin;					
					_speedMinTime -= Time.deltaTime;
					if(_speedMinTime < 0)
					{	
						_speedDownFlag = false;
					}
				}
			}
			else
			{
				_speedMinLastTime -= Time.deltaTime;
				if(_speedMinLastTime <= 0)
				{
					_curSpeed += _speedUp * Time.deltaTime;
				}
				
				if(_curSpeed >_curSpeedMax)
				{
					_curSpeed = _speedOrg;
					_inState = false;
					
				}
			}
			_owner.AniSetAnimationSpeed(_curSpeed);
			yield return null;
		}
		_owner.AniSetAnimationSpeed(_speedOrg*_owner.BufferSpeedPercent);
	}
	
}
