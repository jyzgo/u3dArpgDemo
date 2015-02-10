using UnityEngine;
using System.Collections;

[AddComponentMenu("FC/Logic/FCObject/PlayAndEnemy/Agent/AC/DiscoverAgent")]
public class DiscoverAgent : MonoBehaviour , FCAgent{
	
	#region data
	private ActionController _owner;
	public int _sightLevel;
	public float _sightRaduisFront;
	public float _sightAngle;
	public float _sightRaduisBack;
	
	protected int _sightLevelCurrent;
	protected float _sightRaduisFrontCurrent;
	protected float _sightAngleCurrent;
	protected float _sightRaduisBackCurrent;
	
	private bool _isInDiscover = false;
	#endregion
	
	public static string GetTypeName()
	{
		return "DiscoverAgent";
	}
	
	public void Init(FCObject owner)
	{
		_owner = owner as ActionController;
	}
	
	public void BeginToDiscover(bool useMaxPower)
	{
		_isInDiscover = true;
		_sightLevelCurrent = _sightLevel;
		if(useMaxPower)
		{
			_sightRaduisFrontCurrent = 0;
			_sightAngleCurrent = 0;
			_sightRaduisBackCurrent = 65535;
		}
		else
		{
			_sightRaduisFrontCurrent = _sightRaduisFront;
			_sightAngleCurrent = _sightAngle;
			_sightRaduisBackCurrent = _sightRaduisBack;
		}
		StartCoroutine(DISCOVER());
	}
	
	IEnumerator DISCOVER()
	{
		float timeLast = 0.1f;
		while(_isInDiscover)
		{
			if(timeLast>0)
			{
				timeLast -= Time.deltaTime;
				if(timeLast<=0)
				{
					timeLast+=0.1f;
					ActionController ac = ActionControllerManager.Instance.GetEnemyTargetBySight(_owner.ThisTransform,_sightRaduisFrontCurrent,_sightRaduisBackCurrent,_owner.Faction,_sightAngleCurrent,true);
					if(ac !=null)
					{
						CommandManager.Instance.Send(FCCommand.CMD.TARGET_FINDED,ac,FC_PARAM_TYPE.INT,_owner.ObjectID, FCCommand.STATE.RIGHTNOW,true);
						_isInDiscover = false;
					}
				}
			}
			yield return null;
		}
	}
	
	void Update()
	{
		
	}
	
	
	
}
