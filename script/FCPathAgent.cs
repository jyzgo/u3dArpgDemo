using UnityEngine;
using System.Collections;

[AddComponentMenu("FC/Logic/FCObject/PlayAndEnemy/Agent/AI/FCPathAgent")]
public class FCPathAgent : MonoBehaviour , FCAgent {

	private AIAgent _owner;
	private int _currentStep = -1;
	public bool _isLoop;
	
	protected FCPath _path;
	//public string _pathName;
	
	protected Vector3[] _realPathnodes;
	
	
	public Vector3 CurrentPathPoint
	{
		get
		{
			return  _realPathnodes[_currentStep];
		}
	}
	
	public int CurrentStep
	{
		get
		{
			return _currentStep;
		}
		set
		{
			_currentStep = Mathf.Clamp((int)value,0,_realPathnodes.Length -1);
		}
	}
	
	public bool HasPath
	{
		get
		{
			return (_path != null);
		}
	}
	void Awake()
	{
		_isLoop = true;
	}
	
	public void Init(FCObject owner)
	{
		_owner = owner as AIAgent;
		if(_owner.PathName != null && _owner.PathName != "")
		{
			GameObject gb = LevelManager.Singleton.PathManager.transform.FindChild(_owner.PathName).gameObject;
			_path = gb.GetComponent<FCPath>();
		}
		if(_path)
		{
			_path.GetPath(out _realPathnodes);
		}
	}
	public bool GotoNextPathPoint()
	{
		if(_path == null)
		{
			return false;
		}
		_currentStep++;
		 if(_currentStep> _realPathnodes.Length-1)
		{
			_currentStep = _realPathnodes.Length-1;
			if(_isLoop)
			{
				_currentStep = 0;
			}
			else
			{
				return false;
			}
		}
		return true;
	}
}
