using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FCTicketManager : MonoBehaviour {
	
	public int _ticketForMelee = 4;
	public int _ticketForRanger = 3;
	public int _ticketForElite = 2;
	
	//means enemy can use the ticket for 15s
	public float _ticketLife = 15;
	public float _ticketLifeMin = 5;
	//means enemy can use the ticket can use one ticket for 3 attack
	public int _ticketUseTimes = 3;
	
	protected float _timeDelayCounter =0;
	//for active ticket users
	protected List<FCTicket> _ticketArrayMelee;
	protected List<FCTicket> _ticketArrayRanger;
	protected List<FCTicket> _ticketArrayElite;
	
	protected List<FCTicket> _ticketArrayInvalid;
	protected List<FCTicket> _ticketArrayNoUsed;
	
	//when Invalid Ticket ,we should put the ticket owner to list,and should not give them ticket for secs
	public float _timeForRecordPlayers = 5;
	protected List<ActionController> _ticketOwners;
	
	protected static FCTicketManager _instance = null;
	
	protected bool _inState = false;
	
	public static FCTicketManager Instance
	{
		get
		{
			return _instance;
		}
	}
	
	void Awake()
	{
		_instance = this;
		_ticketArrayMelee = new List<FCTicket>();
		_ticketArrayRanger = new List<FCTicket>();
		_ticketArrayElite = new List<FCTicket>();
		_ticketArrayInvalid = new List<FCTicket>();
		_ticketArrayNoUsed = new List<FCTicket>();
		_ticketOwners = new List<ActionController>();
		_inState = false;
		_timeDelayCounter = 0;
		StartTicketManager();
	}
	
	void OnDestroy()
	{
		if(_instance == this)
		{
			_instance = null;
		}
	}	
	
	public bool ApplyTicket(ActionController ac)
	{
		bool ret = false;
		if(ac.DangerLevel < FCConst.ELITE_DANGER_LEVEL)
		{
			if(ac.IsRanger)
			{
				ret = ActiveTicket(_ticketArrayRanger, _ticketForRanger, ac);
			}
			else
			{
				ret = ActiveTicket(_ticketArrayMelee, _ticketForMelee, ac);
			}
		}
		else if(ac.DangerLevel < FCConst.BOSS_DANGER_LEVEL)
		{
			ret = ActiveTicket(_ticketArrayElite, _ticketForElite, ac);
		}
		return ret;
	}
	
	protected bool ActiveTicket(List<FCTicket> etList, int countMax, ActionController ac)
	{
		bool ret = false;
		if(etList.Count < countMax && (!_ticketOwners.Contains(ac) || countMax >= ActionControllerManager.Instance.GetAllEnemyAliveCount()) && ac.MonsterTicket == null)
		{
			FCTicket et = null;
			if(_ticketArrayNoUsed.Count>0)
			{
				et = _ticketArrayNoUsed[0];
				etList.Add(_ticketArrayNoUsed[0]);
				_ticketArrayNoUsed.RemoveAt(0);
			}
			else
			{
				et = new FCTicket();
				et.Init();
				etList.Add(et);
			}
			float timeMin = _ticketLifeMin;
			float timeCD = 0.5f;
			if(countMax >= ActionControllerManager.Instance.GetAllEnemyAliveCount())
			{
				timeMin = 0.5f;
				timeCD = 0.1f;
			}
			if(ac.DangerLevel >= FCConst.ELITE_DANGER_LEVEL)
			{
				et.ActiveKey(_ticketLife, timeMin, _ticketUseTimes, etList, ac, 0);
			}
			else
			{
				et.ActiveKey(_ticketLife, timeMin, _ticketUseTimes, etList, ac, _timeDelayCounter);
				_timeDelayCounter += timeCD;
			}
			if(_timeDelayCounter <= 0.1f)
			{
				ret = true;
			}
			
		}
		return ret;
	}
	
	public void InvalidTicket(FCTicket et)
	{
		_ticketArrayInvalid.Add(et);
		if(!_ticketOwners.Contains(et.TicketOwner))
		{
			_ticketOwners.Add(et.TicketOwner);
		}
		et.DeActiveKey();
	}
	
	public void StartTicketManager()
	{
		if(_inState == false)
		{
			_inState = true;
			StartCoroutine(STATE());
		}
	}
	public void StopTicketManager()
	{
		_inState = false;
	}
	
		
	private IEnumerator STATE()
	{
		//every one sed ,we will clear array to record player who used the ticket ever
		float timeCounter = _timeForRecordPlayers;
		
		while(_inState)
		{
			if(_timeDelayCounter >= 0)
			{
				_timeDelayCounter -= Time.deltaTime;
			}
			while(_ticketArrayInvalid.Count >0)
			{
				_ticketArrayNoUsed.Add(_ticketArrayInvalid[0]);
				_ticketArrayInvalid[0].ArrayOwner.Remove(_ticketArrayInvalid[0]);
				_ticketArrayInvalid[0].ArrayOwner = null;
				_ticketArrayInvalid.RemoveAt(0);
			}
			if(_ticketArrayMelee.Count >0)
			{
				foreach(FCTicket et in _ticketArrayMelee)
				{
					et.STATE();
				}
			}
			if(_ticketArrayRanger.Count >0)
			{
				foreach(FCTicket et in _ticketArrayRanger)
				{
					et.STATE();
				}
			}
			if(_ticketArrayElite.Count >0)
			{
				foreach(FCTicket et in _ticketArrayElite)
				{
					et.STATE();
				}
			}
			if(timeCounter >0)
			{
				timeCounter -= Time.deltaTime;
				if(timeCounter <= 0)
				{
					timeCounter = _timeForRecordPlayers;
					_ticketOwners.Clear();	
				}
			}
			yield return null;
		}
	}
}
