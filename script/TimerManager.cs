using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TimerManager: MonoBehaviour {
	
	private LinkedList<TimerEvent> _timeListReal = new LinkedList<TimerEvent>();
	private LinkedList<TimerEvent> _timeList = new LinkedList<TimerEvent>();
	
	private float _totalSeconds;
	private float _totalSecondsReal;
	private float _lastTime = 0f;
	
	private static TimerManager _instance;
	public static TimerManager Instance
	{
		get {return _instance;}
	}
	
	// Use this for initialization
	void Start () 
	{
		_lastTime = Time.realtimeSinceStartup;
	}
	
	// Update is called once per frame
	void Update ()
	{
		float dt = Time.realtimeSinceStartup - _lastTime;
		_lastTime = Time.realtimeSinceStartup;
		_totalSecondsReal += dt;
		_totalSeconds += Time.deltaTime;
		checkEvent(_timeList, _totalSeconds);
		checkEvent(_timeListReal, _totalSecondsReal);
		cleanDeleted(_timeList, _totalSeconds);
		cleanDeleted(_timeListReal, _totalSecondsReal);
	}
	
	void Awake()
	{
		_totalSeconds = 0;
		_totalSecondsReal = 0;
		_instance = this;
	}
	
	void OnDestroy()
	{
		_instance = null;
	}
	
	public void register(System.DateTime dt, TimerCallBack cb)
	{
		register(dt, cb, 0);
	}
	
	public void register(System.DateTime dt, TimerCallBack cb, int id)
	{
		float target = (float)dt.Subtract(System.DateTime.UtcNow).TotalSeconds;
		register(target, cb, id, false);
	}
	
	public void register(float next, TimerCallBack cb, bool isLoop)
	{
		register(next, cb, 0, isLoop);
	}
	
	public void register(float next, TimerCallBack cb, int id, bool isLoop)
	{
		TimerEvent ne = new TimerEvent(next, cb, isLoop, id);
		register(ne, _timeList, _totalSeconds);
	}
	
	public void register(float next, TimerCallBack cb, int id, bool isLoop, bool isReal)
	{
		TimerEvent ne = new TimerEvent(next, cb, isLoop, id);
		if(isReal)
		{
			register(ne, _timeListReal, _totalSecondsReal);
		}
		else
		{
			register(ne, _timeList, _totalSeconds);
		}
	}
	
	public void register(float next, TimerCallBack cb, bool isLoop, bool isReal)
	{
		register(next, cb, 0, isLoop, isReal);
	}
	
	public void registerReal(System.DateTime dt, TimerCallBack cb)
	{
		registerReal(dt, cb, 0);
	}
	
	public void registerReal(System.DateTime dt, TimerCallBack cb, int id)
	{
		float target = (float)dt.Subtract(System.DateTime.UtcNow).TotalSeconds;
		registerReal(target, cb, id, false);
	}
	
	public void registerReal(float next, TimerCallBack cb, bool isLoop)
	{
		registerReal(next, cb, 0, isLoop);
	}
	
	public void registerReal(float next, TimerCallBack cb, int id, bool isLoop)
	{
		TimerEvent ne = new TimerEvent(next, cb, isLoop, id);
		register(ne, _timeListReal, _totalSecondsReal);
	}
	
	private void register(TimerEvent ne, LinkedList<TimerEvent> list, float totalTime)
	{
		ne._targetTime = ne._nextTime + totalTime - ne._offsetTime;
		LinkedListNode<TimerEvent> node = list.First;
		while(node != null)
		{
			TimerEvent e = node.Value;
			if(e._targetTime >= ne._targetTime)
			{
				break;
			}
			node = node.Next;
		}
		if(node == null)
		{
			list.AddLast(ne);
		}
		else
		{
			list.AddBefore(node, ne);
		}
	}
	
	private void checkEvent(LinkedList<TimerEvent> list, float totalTime)
	{
		LinkedListNode<TimerEvent> node = list.First;
		List<TimerEvent> tempList = new List<TimerEvent>();
		while(node != null)
		{
			TimerEvent e = node.Value;
			if(e._isDelete)
			{
				node = node.Next;
			}
			else
			{
				if(e._targetTime > totalTime)
				{
					break;
				}
				e._offsetTime = totalTime - e._targetTime;
				e._callBack.onTimeUp(e._id, e._offsetTime);
				if(e._isLoop)
				{
					int multi = (int)(e._offsetTime / e._nextTime);
					e._offsetTime -= e._nextTime * multi;
					tempList.Add(e.clone());
				}
				e.delete();
				node = node.Next;
			}
		}
		foreach(TimerEvent te in tempList)
		{
			register(te, list, totalTime);
		}
	}
	
	private void cleanDeleted(LinkedList<TimerEvent> list, float totalTime)
	{
		LinkedListNode<TimerEvent> node = list.First;
		LinkedListNode<TimerEvent> temp;
		while(node != null)
		{
			TimerEvent e = node.Value;
			temp = node;
			node = node.Next;
			if(e._isDelete)
			{
				list.Remove(temp);
			}
			else if(e._targetTime > totalTime)
			{
				break;
			}
		}
	}
	
	//return the remained time
	private float removeEvent(TimerCallBack callBack, int id, LinkedList<TimerEvent> list, float totalTime)
	{
		LinkedListNode<TimerEvent> node = list.First;
		while(node != null)
		{
			TimerEvent e = node.Value;
			if(e._callBack == callBack && e._id == id)
			{
				e._callBack = null;
				e.delete();
				return e._targetTime - totalTime;
			}
			node = node.Next;
		}
		return 0;
	}
	
	public float removeEvent(TimerCallBack callBack)
	{
		return removeEvent(callBack, 0, _timeList, _totalSeconds);
	}
	
	public float removeEventReal(TimerCallBack callBack)
	{
		return removeEvent(callBack, 0, _timeListReal, _totalSecondsReal);
	}
	
	public float removeEvent(TimerCallBack callBack, bool isReal)
	{
		if(isReal)
		{
			return removeEvent(callBack, 0, _timeListReal, _totalSecondsReal);
		}
		else
		{
			return removeEvent(callBack, 0, _timeList, _totalSeconds);
		}
	}
	
	public float removeEvent(TimerCallBack callBack, int id)
	{
		return removeEvent(callBack, id, _timeList, _totalSeconds);
	}
	
	public float removeEventReal(TimerCallBack callBack, int id)
	{
		return removeEvent(callBack, id, _timeListReal, _totalSecondsReal);
	}
	
	public float removeEvent(TimerCallBack callBack, int id, bool isReal)
	{
		if(isReal)
		{
			return removeEvent(callBack, id, _timeListReal, _totalSecondsReal);
		}
		else
		{
			return removeEvent(callBack, id, _timeList, _totalSeconds);
		}
	}
	
	public void removeAllOfOne(TimerCallBack callBack)
	{
		removeAllOfOne(callBack, _timeList);
	}
	
	public void removeAllOfOneReal(TimerCallBack callBack)
	{
		removeAllOfOne(callBack, _timeListReal);
	}
	
	public void removeAllOfOne(TimerCallBack callBack, bool isReal)
	{
		if(isReal)
		{
			removeAllOfOne(callBack, _timeListReal);
		}
		else
		{
			removeAllOfOne(callBack, _timeList);
		}
	}
	
	private void removeAllOfOne(TimerCallBack callBack, LinkedList<TimerEvent> list)
	{
		LinkedListNode<TimerEvent> node = list.First;
		while(node != null)
		{
			TimerEvent e = node.Value;
			if(e._callBack == callBack)
			{
				e._callBack = null;
				e.delete();
			}
			node = node.Next;
		}
	}
}
