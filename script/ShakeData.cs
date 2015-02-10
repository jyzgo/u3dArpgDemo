using UnityEngine;
using System.Collections;

[System.Serializable]
public class SHAKE
{
	public string _name;
	public string _animation;
	public float _scale;
	public float _time;
}
public class ShakeData : ScriptableObject {

	public SHAKE[] _shakeData;
}
