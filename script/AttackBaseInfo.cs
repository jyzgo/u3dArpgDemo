using UnityEngine;
using System.Collections;

[System.Serializable]
public class AttackBaseInfo 
{
	protected bool _enabled;
	public string _name;
	public float _attackDistanceMin = 0;
	public float _attackDistanceMax = 0;
	public int _energyCost = 0;
	public bool _isFirstAttack;
	
	protected int _attackHashCode = -1;
	
	public bool Enabled
	{
		set
		{
			_enabled = value;
		}
		get
		{
			return _enabled;
		}
	}
	public int AttackHashCode
	{
		set
		{
			_attackHashCode = value;
		}
		get
		{
			return _attackHashCode;
		}
	}
}
