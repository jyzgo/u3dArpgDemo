using UnityEngine;
using System.Collections;

public enum PlayerColorTypes
{
	Red = 0,
	Blue,
	Green,
	Yellow,
	Count,
}

[System.Serializable]
public class MultiplayerDataSet : ScriptableObject
{
	public int _maxPlayerNumber = 2;
	public PlayerColorTypes[] _playerColorTypeList = new PlayerColorTypes[(int)PlayerColorTypes.Count];
	public Color _redColor;
	public Color _blueColor;
	public Color _greenColor;
	public Color _yellowColor;
	public string[] _photonServerNames;
}

[System.Serializable]
public class PlayerPvPStatisticsData
{
	public int _kills = 0;
	public int _deaths = 0;
	public int _tripleKillCounter = 0;
	public bool _firstBlood = false;
	public int _healthPoint = 0;
	public int _positionOfKills = 0;
	public bool _isDropped = false;
	private bool _isTripleKill = false;
	
	public int Kills {
		get {
			return _kills;
		}
		set {
			_kills = value;
		}
	}

	public int Deaths {
		get {
			return _deaths;
		}
		set {
			_deaths = value;
		}
	}

	public int TripleKillCounter
	{
		get
		{
			return this._tripleKillCounter;
		}
		set
		{
			_tripleKillCounter = value;
			if(_tripleKillCounter >= 3)
			{
				_isTripleKill = true;
			}
		}
	}

	public bool FirstBlood {
		get {
			return _firstBlood;
		}
		set {
			_firstBlood = value;
		}
	}

	public int HealthPoint {
		get {
			return _healthPoint;
		}
		set {
			_healthPoint = value;
		}
	}

	public bool IsDropped
	{
		get
		{
			return this._isDropped;
		}
		set
		{
			_isDropped = value;
		}
	}

	public int PositionOfKills
	{
		get
		{
			return this._positionOfKills;
		}
		set
		{
			_positionOfKills = value;
		}
	}
	
	public bool IsTripleKill
	{
		get{
			return _isTripleKill;
		}
	}
	
}