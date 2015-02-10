using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("FC/Logic/FCObject/PlayAndEnemy/Agent/AC/ThreatAgent")]
public class ThreatAgent : MonoBehaviour,FCAgent {
	
	ActionController _owner;
	
	public class ThreatTable
	{
		public int[] _threatList = null;
		public ActionController[] _targetList = null;
		public int _count =0;
		public int _length = FCConst.MAX_PLAYERS;
		public int _currentTargetIdx;
	}
	protected ThreatTable _threatTable = null;
	
	protected bool _instate = false;
	protected FCCommand _fastCommand;
	protected int _nextTargetIdx = -1;
	
	public static string GetTypeName()
	{
		return "ThreatAgent";
	}
	
	void Awake()
	{
		_fastCommand = new FCCommand();
		_fastCommand.Set(FCCommand.CMD.STOP,null,FCCommand.STATE.RIGHTNOW,true);
		_threatTable = new ThreatTable();
		_threatTable._length = 4;
		_threatTable._threatList = new int[_threatTable._length];
		_threatTable._targetList = new ActionController[_threatTable._length];
		_threatTable._count = 0;
		
	}
	
	public void Init(FCObject owner)
	{
		_owner = owner as ActionController;
		for(int i =0;i<_threatTable._length;i++)
		{
			_threatTable._threatList[i] = -1;
			_threatTable._targetList[i] = null;
			_threatTable._count = 0;
		}
	}
	
	public void StartToRun()
	{
		_instate = true;
		StartCoroutine(STATE());
	}
	
	public void ClearThreat(ActionController ac)
	{
		if(_threatTable._count == 0 || ac == null)
		{
			return;
		}
		if(_threatTable._targetList[_threatTable._currentTargetIdx] == ac)
		{
			_threatTable._threatList[_threatTable._currentTargetIdx] = -1;
			if(_threatTable._count > 1)
			{
				int idx = -1;
				int tv = -1;
				for(int i =0;i<_threatTable._length;i++)
				{
					if(_threatTable._threatList[i] > tv )
					{
						idx = i;
						tv = _threatTable._threatList[i];
					}
				}
				if(idx>=0)
				{
					_nextTargetIdx = idx;
					_threatTable._currentTargetIdx = _nextTargetIdx;
				}
			}
		}
		else
		{
			int idx = -1;
			for(int i =0;i<_threatTable._length;i++)
			{
				if(_threatTable._targetList[i] == ac)
				{
					idx = i;
					break;
				}
			}
			if(idx>=0)
			{
				_threatTable._threatList[idx]=-1;
			}
		}
	}
	
	public void AddTargetToList(ActionController ac)
	{
		if(ac == null)
		{
			return;
		}
		int idx = -1;
		for(int i =0;i<_threatTable._length;i++)
		{
			if(_threatTable._targetList[i] == ac)
			{
				idx = i;
				break;
			}
		}
		if(idx<0 && _threatTable._count < _threatTable._length)
		{
			idx = _threatTable._count;
			_threatTable._count++;
			_threatTable._threatList[idx] = 0;
			_threatTable._targetList[idx] = ac;
		}
	}
	public void Increase(int threatValue,ActionController ac)
	{
		if(ac == null || threatValue == 0 || !ac.IsAlived)
		{
			return;
		}
		if(_threatTable._count == 0)
		{
			_threatTable._count = 1;
			_threatTable._targetList[0] = ac;
			_nextTargetIdx = 0;
			_threatTable._currentTargetIdx =0;
		}
		else if(_threatTable._targetList[_threatTable._currentTargetIdx] == ac)
		{
			_threatTable._threatList[_threatTable._currentTargetIdx]+=threatValue;
		}
		else
		{
			int idx = -1;
			for(int i =0;i<_threatTable._length;i++)
			{
				if(_threatTable._targetList[i] == ac)
				{
					idx = i;
					break;
				}
			}
			if(idx>=0)
			{
				_threatTable._threatList[idx]+=threatValue;

			}
			//ToDo: if we need add new players when playing game, we need more code here
			else if(idx<0 && _threatTable._count < _threatTable._length)
			{
				idx = _threatTable._count;
				_threatTable._count++;
				_threatTable._threatList[idx] = threatValue;
				_threatTable._targetList[idx] = ac;
			}
			if(idx >=0 && _threatTable._threatList[idx] > _threatTable._threatList[_threatTable._currentTargetIdx]*1.1f)
			{
				_nextTargetIdx = idx;
				_threatTable._currentTargetIdx = _nextTargetIdx;
			}
		}
	}
	
	void ChangeThreatTarget(int idx)
	{
		if(_nextTargetIdx>=0 && _threatTable._targetList[idx].IsAlived)
		{
			_fastCommand._cmd = FCCommand.CMD.TARGET_CHANGE;
			_fastCommand._param1 = _threatTable._targetList[idx];
			_fastCommand._param1Type = FC_PARAM_TYPE.INT;
			CommandManager.Instance.SendFast(ref _fastCommand,_owner);
			_nextTargetIdx = -1;
		}

	}
	
	public void SomeTargetDead()
	{
		int i;
		for(i =0;i <_threatTable._count;i++)
		{
			if(!_threatTable._targetList[i].IsAlived)
			{
				_threatTable._threatList[i] = -1;
				if(i == _nextTargetIdx)
				{
					_nextTargetIdx = -1;
				}	
			}
		}
		if(_threatTable._targetList[_threatTable._currentTargetIdx] != null && !_threatTable._targetList[_threatTable._currentTargetIdx].IsAlived && _nextTargetIdx<0)
		{
			int threat = -1;
			for(i =0;i <_threatTable._count; i++)
			{
				if(_threatTable._targetList[i].IsAlived && _threatTable._threatList[i] > threat)
				{
					_nextTargetIdx = i;
					threat = _threatTable._threatList[i];
				}
			}
		}

	}
	
	IEnumerator STATE()
	{
		while(_instate)
		{
			ChangeThreatTarget(_nextTargetIdx);
			yield return null;
		}
	}
	
}
