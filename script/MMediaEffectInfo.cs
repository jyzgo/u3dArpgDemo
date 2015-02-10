using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class MMediaEffectInfoAll
{
	public bool _holdWhenEnd = false;
	public bool _needForceEnd = true;
	public float _lastTime = -1;
	
	protected ActionController _owner = null;
	
	public void Init(ActionController owner)
	{
		_owner = owner;
	}
	
	public virtual void PlayEffect(){
	}
	
	public virtual void StopEffect(){
	}
}

[System.Serializable]
public class MMediaEffectInfoCemara: MMediaEffectInfoAll
{
	public EnumCameraEffect _cameraEffect = EnumCameraEffect.none;
	public EnumShakeEffect _shakeLevel = EnumShakeEffect.none;
	
	
	public override void PlayEffect()
	{
		CameraController.Instance.StartCameraEffect(_cameraEffect,_shakeLevel,_holdWhenEnd);
	}
	
	public override void StopEffect()
	{
		if(_needForceEnd)
		{
			CameraController.Instance.StopCameraEffect();
		}
	}
}

[System.Serializable]
public class MMediaEffectInfoSight: MMediaEffectInfoAll
{
	public FC_CHARACTER_EFFECT _bindEffect = FC_CHARACTER_EFFECT.INVALID;
	public override void PlayEffect()
	{
		CharacterEffectManager.Instance.PlayEffect(_bindEffect,
					_owner._avatarController,
					-1);
	}
	
	public override void StopEffect()
	{
		if(_needForceEnd && CharacterEffectManager.Instance != null
			&& _owner != null)
		{
			CharacterEffectManager.Instance.StopEffect(_bindEffect,
					_owner._avatarController,
					-1);
		}
	}
}

[System.Serializable]
public class MMediaEffectInfoSfx: MMediaEffectInfoAll
{
	public string _sfxName = "";
	public string[] _sfxNames = null;
	
	protected AudioSource _audioSource = null;
	public override void PlayEffect()
	{
		string sn = _sfxName;
		if(_sfxNames != null && _sfxNames.Length >0)
		{
			sn = _sfxNames[Random.Range(0,_sfxNames.Length -1)];
		}
		if(_audioSource != null)
		{
			_audioSource.Stop();
		}
		_audioSource = null;
		if(sn.Contains("loop"))
		{
			_audioSource = SoundManager.Instance.PlaySoundEffect(sn, true);
		}
		else
		{
			_audioSource = SoundManager.Instance.PlaySoundEffect(sn);
		}
	}
	
	public override void StopEffect()
	{
		if(_needForceEnd)
		{
			if(_audioSource != null)
			{
				_audioSource.Stop();
			}
		}
	}
}

[System.Serializable]
public class MMediaEffectInfoListAll
{
	public FC_EFFECT_EVENT_POS _effectPos;
	public float _startDelayTime = -1;
	
	
	public MMediaEffectInfoCemara[] _effectInfosCamera;
	public MMediaEffectInfoSfx[] _effectInfosSfx;
	public MMediaEffectInfoSight[] _effectInfosSight;
}

[System.Serializable]
public class MMediaEffectInfoMapAll
{
	public MMediaEffectInfoListAll[] _mmEffectInfoList = null;
	protected float _currentCountTime = -1;
	public void PlayEffect(FC_EFFECT_EVENT_POS eeep, ActionController ac, float currentTime)
	{
		foreach(MMediaEffectInfoListAll meil in _mmEffectInfoList)
		{
			if(meil._effectPos == eeep)
			{
				
				if((currentTime >= meil._startDelayTime && _currentCountTime <= meil._startDelayTime)
					|| meil._startDelayTime <=Mathf.Epsilon || meil._effectPos == FC_EFFECT_EVENT_POS.AT_ANY_TIME)
				{
					foreach(MMediaEffectInfoCemara mei in meil._effectInfosCamera)
					{
						if(ac != null && ac.IsPlayer && !ac.IsPlayerSelf)
						{
							return;
						}
						mei.Init(ac);
						mei.PlayEffect();
					}
					foreach(MMediaEffectInfoSfx mei in meil._effectInfosSfx)
					{
						mei.Init(ac);
						mei.PlayEffect();
					}
					foreach(MMediaEffectInfoSight mei in meil._effectInfosSight)
					{
						mei.Init(ac);
						mei.PlayEffect();
					}
				}
			}
		}
		_currentCountTime = currentTime;
	}
	public void StopEffect()
	{
		foreach(MMediaEffectInfoListAll meil in _mmEffectInfoList)
		{
			foreach(MMediaEffectInfoCemara mei in meil._effectInfosCamera)
			{
				mei.StopEffect();
			}
			foreach(MMediaEffectInfoSfx mei in meil._effectInfosSfx)
			{
				mei.StopEffect();
			}
			foreach(MMediaEffectInfoSight mei in meil._effectInfosSight)
			{
				mei.StopEffect();
			}
		}
	}
}
