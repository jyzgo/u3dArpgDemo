using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("FC/Logic/Manager/ActionControllerManager")]
public class ActionControllerManager : MonoBehaviour {
	
	public Dictionary<FC_AC_FACTIOH_TYPE,List<ActionController>> _acSet;
	
	private Dictionary<Collider, ActionController> _acColliderMap;
	
	private static ActionControllerManager _instance;	
	//private bool _inState = false;
	//protected ActionController _monsterLeader = null;
	public static ActionControllerManager Instance
	{
		get
		{
			return _instance;
		}
	}
	
	void OnDestroy() {
		if(_instance == this) {
			_instance = null;
		}
	}
	
	// Use this for initialization
	void Awake () {
		_acSet = new Dictionary<FC_AC_FACTIOH_TYPE, List<ActionController>>();
		_acSet.Add(FC_AC_FACTIOH_TYPE.NEUTRAL_1,new List<ActionController>());
		_acSet.Add(FC_AC_FACTIOH_TYPE.NEUTRAL_2,new List<ActionController>());
		_acSet.Add(FC_AC_FACTIOH_TYPE.ENEMY,new List<ActionController>());
		_acSet.Add(FC_AC_FACTIOH_TYPE.FRIEND,new List<ActionController>());
		
		_acColliderMap = new Dictionary<Collider, ActionController>();
		
		_instance = this;
		//_monsterLeader = null;
		//StartCoroutine(UpdateMonsterState());
	}
	
	
	public void Register(ActionController ac)
	{
		if(_acSet[ac.Faction] == null)
		{
			_acSet[ac.Faction] = new List<ActionController>();
		}
		_acSet[ac.Faction].Add(ac);
		/*if(ac.Faction == FC_AC_FACTIOH_TYPE.NEUTRAL_2)
		{
			if(_monsterLeader == null)
			{
				_monsterLeader = ac;
				ac._isMonsterLeader = true;
			}
			else
			{
				ac.MonsterLeader = _monsterLeader;
			}
		}*/
	}
	
	public void UnRegister(ActionController ac)
	{
		if(_acSet[ac.Faction] != null)
		{
			_acSet[ac.Faction].Remove(ac);
			/*if(ac.Faction == FC_AC_FACTIOH_TYPE.NEUTRAL_2)
			{
				if(ac._isMonsterLeader)
				{
					ac._isMonsterLeader = false;
					_monsterLeader = null;
					if(_acSet[ac.Faction].Count>0)
					{
						_acSet[ac.Faction][0]._isMonsterLeader = true;
						_monsterLeader = _acSet[ac.Faction][0];
					}
				}
				
			}*/
		}
		
	}
	public void RegisterACByCollider(ActionController ac, Collider collider)
	{
		_acColliderMap[collider] = ac;
	}
	
	public ActionController GetACByCollider(Collider collider)
	{
		ActionController ac = null;
		_acColliderMap.TryGetValue(collider, out ac);
		return ac;
	}
	
	public void ClearThreat(ActionController target)
	{
		foreach(KeyValuePair<FC_AC_FACTIOH_TYPE, List<ActionController>> acList in _acSet)
		{
			if(acList.Key != target.Faction)
			{
				foreach(ActionController ac in acList.Value)
				{
					if(ac == null || !ac.IsAlived)
					{
						continue;
					}
					ac.ACClearThreat(target);
				}
			}
		}
	}
	
	public int GetAllEnemyAliveCount()
	{
		List<ActionController> monsterList = _acSet[FC_AC_FACTIOH_TYPE.NEUTRAL_2];
		return monsterList.Count;
	}
	
	public void PlayerIsRevive(ActionController player)
	{
		//InitThreat(player);
	}
	
	public void PlayerIsDead(ActionController player)
	{
		foreach(KeyValuePair<FC_AC_FACTIOH_TYPE, List<ActionController>> acList in _acSet)
		{
			if(acList.Key != player.Faction)
			{
				foreach(ActionController ac in acList.Value)
				{
					if(ac == null || !ac.IsAlived)
					{
						continue;
					}
					ac.ACSomePlayerInThreatIsDead();
				}
			}
		}
	}
	
	/// <summary>
	/// Determines whether monster is near players with the specified self distance.
	/// </summary>
	/// <returns>
	/// <c>true</c> if monster is near players  with the distance; otherwise, <c>false</c>.
	/// </returns>
	/// <param name='self'>
	/// monster point
	/// </param>
	/// <param name='distance'>
	/// a redius from the position of self
	/// </param>
	public bool IsNearPlayers(ActionController self, float distance)
	{
		List<ActionController> acList = _acSet[FC_AC_FACTIOH_TYPE.NEUTRAL_1];
		bool ret = false;
		foreach(ActionController ac in acList)
		{
			//Todo ; should add flag that disconnect with network
			if(ac == null || !ac.IsAlived)
			{
				continue;
			}
			float distanceSqrt = distance*distance;
			if((ac.ThisTransform.localPosition-self.ThisTransform.localPosition).sqrMagnitude < distanceSqrt)
			{
				ret = true;
				break;
			}
		}
		return ret;
	}
	
	//init threat, add this player to all enemies' list. should invoked after this players are instiated
	public void InitThreat(ActionController target)
	{
		foreach(KeyValuePair<FC_AC_FACTIOH_TYPE, List<ActionController>> acList in _acSet)
		{
			if(acList.Key != target.Faction)
			{
				foreach(ActionController ac in acList.Value)
				{
					if(ac == null || !ac.IsAlived)
					{
						continue;
					}
					
					//add this player to threat list 
					ac.ACAddTargetToThreatList(target);
				}
			}
		}
	}
	
	/*public IEnumerator UpdateMonsterState()
	{
		List<ActionController> monsterList = _acSet[FC_AC_FACTIOH_TYPE.NEUTRAL_2];
		ActionController monsterLeader = null;

		_inState = true;
		while(_inState)
		{
			yield return null;
		}
	}*/
	
	protected void SetNearestAC(float acDisSqrt, ActionController ac, ref float sightLengthSqrt, ref ActionController nearestAC, float sightLengthSqrtSource)
	{
		if(nearestAC == null)
		{
			sightLengthSqrt = acDisSqrt;
			nearestAC = ac;
		}
		else
		{
			if(ac.DangerLevel >= FCConst.ELITE_DANGER_LEVEL && ac.DangerLevel >= nearestAC.DangerLevel)
			{
				if((ac.DangerLevel%10) > (nearestAC.DangerLevel%10))
				{
					sightLengthSqrt = acDisSqrt;
					nearestAC = ac;
				}
				else
				{
					float scoreAC = 0;
					float socreNC = nearestAC.DangerLevel - ac.DangerLevel;
					scoreAC += sightLengthSqrtSource - acDisSqrt;
					socreNC += sightLengthSqrtSource - sightLengthSqrt;
					if(scoreAC > socreNC)
					{
						sightLengthSqrt = acDisSqrt;
						nearestAC = ac;
					}
				}
			}
			else if(ac.DangerLevel < FCConst.ELITE_DANGER_LEVEL && nearestAC.DangerLevel < FCConst.ELITE_DANGER_LEVEL)
			{
				sightLengthSqrt = acDisSqrt;
				nearestAC = ac;
			}
		}
	}
	
	public ActionController GetEnemyTargetBySight(Transform selfTransform,float sightLength,float sightLength1,FC_AC_FACTIOH_TYPE eaf,float angle,bool needTheNearest)
	{
		float sightLengthSqrt = sightLength*sightLength;
		float sightLengthSqrt1= sightLength1*sightLength1;
		float sightLengthSqrtSource = sightLength*sightLength;
		float sightLengthSqrt1Source = sightLength1*sightLength1;
		ActionController nearestAC = null;
		
		foreach(KeyValuePair<FC_AC_FACTIOH_TYPE, List<ActionController>> acList in _acSet)
		{
			if(acList.Key != eaf)
			{
				foreach(ActionController ac in acList.Value)
				{
					if(ac == null || !ac.IsAlived)
					{
						continue;
					}
					float lenSqrt = (ac.ThisTransform.localPosition-selfTransform.position).sqrMagnitude;
					if(lenSqrt <sightLengthSqrt1 
						|| (ac.DangerLevel >= FCConst.ELITE_DANGER_LEVEL && lenSqrt < sightLengthSqrt1Source))
					{
						if(needTheNearest)
						{
							SetNearestAC(lenSqrt, ac, ref sightLengthSqrt1, ref nearestAC, sightLengthSqrt1Source);
						}
						else
						{
							return 	ac;
						}
					}
					else if(lenSqrt <sightLengthSqrt
						|| (ac.DangerLevel >= FCConst.ELITE_DANGER_LEVEL && lenSqrt < sightLengthSqrtSource))
					{
						if(angle>=360)
						{
							if(needTheNearest)
							{
								SetNearestAC(lenSqrt, ac, ref sightLengthSqrt, ref nearestAC, sightLengthSqrtSource);
							}
							else
							{
								return 	ac;
							}
						}
						else
						{
							float anglex = Vector3.Angle(selfTransform.forward,ac.ThisTransform.localPosition-selfTransform.position);
							if(anglex<angle/2)
							{
								if(needTheNearest)
								{
									SetNearestAC(lenSqrt, ac, ref sightLengthSqrt, ref nearestAC, sightLengthSqrtSource);
								}
								else
								{
									return 	ac;
								}
							}
						}
					}
				}
			}
		}
		return nearestAC;
	}
	
	/// <summary>
	/// Gets the target by forward.
	/// </summary>
	/// <returns>
	/// The target has similar forward with self
	/// </returns>
	/// <param name='selfTransform'>
	/// Self transform.
	/// </param>
	/// <param name='sightLength'>
	/// Sight length. the target must in self sight
	/// </param>
	public ActionController GetTargetByForward(Transform selfTransform,float sightLength,FC_AC_FACTIOH_TYPE eaf)
	{
		float sightLengthSqrt = sightLength*sightLength;
		ActionController nearestAC = null;
		float angleN  = 999;
		foreach(KeyValuePair<FC_AC_FACTIOH_TYPE, List<ActionController>> acList in _acSet)
		{
			if(acList.Key != eaf)
			{
				foreach(ActionController ac in acList.Value)
				{
					if(ac == null || !ac.IsAlived)
					{
						continue;
					}
					float lenSqrt = (ac.ThisTransform.localPosition-selfTransform.position).sqrMagnitude;
					if(lenSqrt <sightLengthSqrt)
					{
						float anglex = Vector3.Angle(selfTransform.forward,ac.ThisTransform.localPosition-selfTransform.position);
						if(anglex<angleN)
						{
							nearestAC = ac;
							angleN = anglex;
						}
					}
				}
			}
		}
		return nearestAC;
	}
	
	public bool EnemyIsInRanger(Vector3 direction, Vector3 orgPos, float sightLengthSqrt, FC_AC_FACTIOH_TYPE eaf)
	{
		
		foreach(KeyValuePair<FC_AC_FACTIOH_TYPE, List<ActionController>> acList in _acSet)
		{
			if(acList.Key != eaf)
			{
				foreach(ActionController ac in acList.Value)
				{
					if(ac == null || !ac.IsAlived)
					{
						continue;
					}
					float lenSqrt = (ac.ThisTransform.localPosition-orgPos).sqrMagnitude;
					if(lenSqrt <sightLengthSqrt)
					{
						Vector3 pos1 = ac.ThisTransform.localPosition-orgPos;
						pos1.y = 0;
						float anglex = Vector3.Angle(direction, pos1);
						if(anglex <= 30)
						{
							return true;
						}
					}
				}
			}
		}
		return false;
	}
	public ActionController GetEnemyTargetBySight(Transform selfTransform,float sightLength,float sightLength1,FC_AC_FACTIOH_TYPE eaf,float angleMin,float angleMax,bool needTheNearest)
	{
		if(Mathf.Approximately(angleMin,angleMax))
		{
			return GetEnemyTargetBySight(selfTransform,sightLength,sightLength1,eaf,angleMin,needTheNearest);
		}
		float sightLengthSqrt = sightLength*sightLength;
		float sightLengthSqrt1= sightLength1*sightLength1;
		float sightLengthSqrtSource = sightLength*sightLength;
		float sightLengthSqrt1Source = sightLength1*sightLength1;
		ActionController nearestAC = null;
		foreach(KeyValuePair<FC_AC_FACTIOH_TYPE, List<ActionController>> acList in _acSet)
		{
			if(acList.Key != eaf)
			{
				foreach(ActionController ac in acList.Value)
				{
					if(ac == null || !ac.IsAlived)
					{
						continue;
					}
					float lenSqrt = (ac.ThisTransform.localPosition-selfTransform.position).sqrMagnitude;
					if(lenSqrt <sightLengthSqrt1 
						|| (ac.DangerLevel >= FCConst.ELITE_DANGER_LEVEL && lenSqrt < sightLengthSqrt1Source))
					{
						if(needTheNearest)
						{
							SetNearestAC(lenSqrt, ac, ref sightLengthSqrt1, ref nearestAC, sightLengthSqrt1Source);
						}
						else
						{
							return 	ac;
						}
					}
					else if(lenSqrt <sightLengthSqrt
						|| (ac.DangerLevel >= FCConst.ELITE_DANGER_LEVEL && lenSqrt < sightLengthSqrtSource))
					{
						float angle = ((1-lenSqrt/sightLengthSqrt)*(angleMax-angleMin)+angleMin)/2;
						float anglex = Vector3.Angle(selfTransform.forward,ac.ThisTransform.localPosition-selfTransform.position);
						if(anglex<=angle)
						{
							if(needTheNearest)
							{
								SetNearestAC(lenSqrt, ac, ref sightLengthSqrt, ref nearestAC, sightLengthSqrtSource);
							}
							else
							{
								return 	ac;
							}
						}
					}
				}
			}
		}
		return nearestAC;
	}
	
	public void GetEnemyTargetsBySight(Transform selfTransform,float sightLength,float sightLength1,FC_AC_FACTIOH_TYPE eaf,float angle,ref List<ActionController> acTargetList)
	{
		float sightLengthSqrt = sightLength*sightLength;
		float sightLengthSqrt1= sightLength1*sightLength1;
		acTargetList.Clear();
		foreach(KeyValuePair<FC_AC_FACTIOH_TYPE, List<ActionController>> acList in _acSet)
		{
			if(acList.Key != eaf)
			{
				foreach(ActionController ac in acList.Value)
				{
					if(ac == null || !ac.IsAlived)
					{
						continue;
					}
					float lenSqrt = (ac.ThisTransform.localPosition-selfTransform.localPosition).sqrMagnitude;
					if(lenSqrt <sightLengthSqrt1)
					{
						acTargetList.Add(ac);
					}
					else if(lenSqrt <sightLengthSqrt)
					{
						if(angle>=360)
						{
							acTargetList.Add(ac);
						}
						else
						{
							float anglex = Vector3.Angle(selfTransform.forward,ac.ThisTransform.localPosition-selfTransform.localPosition);
							if(anglex<angle/2)
							{
								acTargetList.Add(ac);
							}
						}
					}
				}
			}
		}
	}
}
