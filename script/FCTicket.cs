using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FCTicket{
	
	protected float _timeCounter = 0;
	protected float _timeCounterMin = 0;
	protected int _useTimes = 0;
	protected bool _isActived = false;
	protected ActionController _ticketOwner = null;
	
	protected FCCommand _fastCommand;
	protected List<FCTicket> _arrayOwner;
	
	protected float _timeDelay;
	protected float _timeStamp;
	public List<FCTicket> ArrayOwner
	{
		get
		{
			return _arrayOwner;
		}
		set
		{
			_arrayOwner = value;
		}
	}
	
	public ActionController TicketOwner
	{
		get
		{
			return _ticketOwner;
		}
	}
	public void Init()
	{
		_fastCommand = new FCCommand();
		_arrayOwner = null;
	}
	public void ActiveKey(float timeLast, float timeLastMin,int useTimes, List<FCTicket> etListOwner, ActionController ac,float timeDelay)
	{
		if(!_isActived)
		{
			_ticketOwner = ac;
			_timeCounter = timeLast;
			_timeCounterMin = timeLastMin;
			_useTimes = useTimes;
			_fastCommand._cmd = FCCommand.CMD.ACTION_GAIN_TICKET;
			_fastCommand._param1 = this;
			_fastCommand._param1Type = FC_PARAM_TYPE.OBJECT;
			CommandManager.Instance.SendFast(ref _fastCommand, ac);
			_arrayOwner = etListOwner;
			_timeDelay = timeDelay;
			//manager add this tickck to updatelist
		}
		_isActived = true;
	}
	
	public bool CanUse()
	{
		return _useTimes >=0 &&  _timeDelay <= 0;
	}
	public bool UseTicket()
	{
		bool ret = false;
		if(_useTimes >= 0 && _timeDelay <= 0)
		{
			_useTimes --;
			if(_useTimes <=0 && _timeCounterMin <= 0)
			{
				FCTicketManager.Instance.InvalidTicket(this);
				_useTimes = -1;
			}
			ret = true;
		}
		
		return ret;
	}
	
	//only called by ticketmanager
	public void DeActiveKey()
	{
		if(_isActived)
		{
			_fastCommand._cmd = FCCommand.CMD.ACTION_DISMISS_TICKET;
			CommandManager.Instance.SendFast(ref _fastCommand, _ticketOwner);
			//remove from updateList
		}
		_isActived = false;
	}
	
	public void STATE()
	{
		if(_isActived)
		{
			if(_timeDelay >= 0)
			{
				_timeDelay -= Time.deltaTime;
				if(_timeDelay <=0 )
				{
					_fastCommand._cmd = FCCommand.CMD.ACTION_SHOULD_GOTO_ATTACK;
					CommandManager.Instance.SendFast(ref _fastCommand, _ticketOwner);
				}
			}
			else
			{
				_timeCounter -= Time.deltaTime;
				_timeCounterMin -= Time.deltaTime;
			}
			
			if(_timeCounter <= 0 || !_ticketOwner.IsAlived || (_timeCounterMin <= 0 && _useTimes < 0))
			{
				FCTicketManager.Instance.InvalidTicket(this);
			}
		}
	}
}
