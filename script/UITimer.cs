using UnityEngine;
using System.Collections;

public class UITimer : MonoBehaviour {
	
	public interface UITimeUp
	{
		void OnTimeUp();
	}
	
	public enum TimerFormat
	{
		hour_min_sec,
		hour_min,
		day_hour_min,
	}
	
	public UITimeUp _messageTarget;
	
	public bool _isRealTime = true;
	public bool _isCountDown = true;
	
	public UILabel _timerText;
	public TimerFormat _format = TimerFormat.hour_min_sec;
	public GameObject[] _items;
	
	private float _totalSecond;
	private float _startTime;
	private float _lastTime;
	private float _passedTime;
	private float _updateTime;
	
	void Awake()
	{
	}
	
	void OnDestory()
	{
		_messageTarget = null;
	}
	
	public void setVisible(bool isVisible)
	{
		if(_items != null)
		{
			foreach(GameObject go in _items)
			{
				go.SetActive(isVisible);
			}
		}
		if(_timerText != null)
		{
			_timerText.gameObject.SetActive(isVisible);
		}
	}
	

	public void startTimer(float seconds)
	{
		_totalSecond = seconds;
		if(_isRealTime)
		{
			_startTime = Time.realtimeSinceStartup;
			_lastTime = _startTime;
		}
		_updateTime = 0;
		_passedTime = 0;
		updateText();
	}
	
	public void stopTimer()
	{
		_passedTime = _totalSecond + 1;
	}
	
	private float getSecondNow()
	{
		if(_isRealTime)
		{
			return Time.realtimeSinceStartup;
		}
		return 0;
	}

	// Use this for initialization
	void Start () {
	
	}
	
	private void updateText()
	{
		if(_timerText == null)
		{
			return;
		}
		float time = _isCountDown? _totalSecond - _passedTime: _passedTime;
		int day = (int)(time / 3600 / 24);
		time -= day * 3600 * 24;
		int hour = (int)(time / 3600);
		time -= hour * 3600;
		int min = (int)(time / 60);
		time -= min * 60;
		int second = Mathf.RoundToInt(time);
		switch(_format)
		{
		case TimerFormat.hour_min:
			_timerText.text = hour + "h:" + min + "m";
			break;
		case TimerFormat.hour_min_sec:
			_timerText.text = hour + "h:" + min + "m:" + second + "s";
			break;
		case TimerFormat.day_hour_min:
			_timerText.text = day + "d:" + hour + "h:" + min + "m";
			break;
		default:
			break;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if(_passedTime < _totalSecond)
		{
			float delta = Time.deltaTime;
			if(_isRealTime)
			{
				delta = Time.realtimeSinceStartup - _lastTime;
				_lastTime = Time.realtimeSinceStartup;
			}
			_passedTime += delta;
			_updateTime += delta;
			if(_updateTime >= 1)
			{
				_updateTime = 0;
				updateText();
			}
			if(_passedTime >= _totalSecond && _messageTarget != null)
			{
				_messageTarget.OnTimeUp();
			}
			
		}
	}
}
