using UnityEngine;
using System.Collections;

public class TimerEvent
{
	public float _targetTime;	
	public bool _isLoop;
	public TimerCallBack _callBack;
	public float _nextTime;
	public int _id;
	public float _offsetTime;
	public bool _isDelete;
	
	public TimerEvent(float next, TimerCallBack callBack, bool isLoop, int id)
	{
		_nextTime = next;
		_callBack = callBack;
		_isLoop = isLoop;
		_id = id;
		_offsetTime = 0;
		_isDelete = false;
	}
	
	public void delete()
	{
		_isDelete = true;
		_isLoop = false;
	}
	
	public TimerEvent clone()
	{
		TimerEvent e = new TimerEvent(_nextTime, _callBack, _isLoop, _id);
		e._offsetTime = _offsetTime;
		return e;
	}
}
