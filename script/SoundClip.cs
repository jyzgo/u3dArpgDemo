using UnityEngine;
using System;
using System.Collections;

[Serializable]
public class SoundClip
{
	public string _name;
	protected AudioClip _clip = null;
	public bool HasLoaded
	{
		get
		{
			return _clip != null;
		}
	}
	public string _clipPath;
	public AudioClip Clip {
		get {
			if(_clip == null)
			{
				_clip = Resources.Load(_clipPath,typeof(AudioClip)) as AudioClip;
				if(_clip == null)
				{
					Debug.LogError("Can't load AudioClip name:[" + _name+"] ,  path:["+ _clipPath+"]");
				}
			}
			return _clip;
		}
	}
	public int _rate = 100;
	public int _priority = 128;
	public bool _onlyOne = true;
	
	public void ClearClip()
	{
		_clip = null;
	}
}
	

