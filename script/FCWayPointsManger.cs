using UnityEngine;
using System.Collections;

public class FCWayPointsManger : MonoBehaviour {
	
	public FCWayPoints[] _wayPoints;
	protected static FCWayPointsManger _wayPointsManager;
	
	public static FCWayPointsManger Instance
	{
		get
		{
			return _wayPointsManager;
		}
	}
	
	void Awake()
	{
		_wayPointsManager = this;
	}
	
	//wpidx current target way point
	//get next way point by forward
	public int GetWayPointByForward(string wayName, ActionController self, int wpIdx, int preWpIdx, out Vector3 wpPos)
	{
		bool ret = false;
		wpPos = Vector3.zero;
		FCWayPoints wp = null;
		Transform farTF = null;
		int currentIdx = wpIdx;

		int nextIdx = wpIdx -1;
		int nextIdx1 = wpIdx +1;

		foreach(FCWayPoints ewp in  _wayPoints)
		{
			if(ewp._name == wayName )
			{
				wp = ewp;
				if(preWpIdx <0)
				{
					preWpIdx = wpIdx -1;
					if(preWpIdx < 0)
					{
						preWpIdx = wp._wayPoints.Length -1;
					}
					if(preWpIdx > wp._wayPoints.Length -1)
					{
						preWpIdx = 0;
					}
				}
				if(currentIdx <0)
				{
					currentIdx = Random.Range(0,ewp._wayPoints.Length-1);
				}
				
				if(nextIdx < 0)
				{
					nextIdx = wp._wayPoints.Length -1;
				}
				if(nextIdx1 > wp._wayPoints.Length -1)
				{
					nextIdx1 = 0;
				}
				Vector3 v3 = wp._wayPoints[nextIdx].position - wp._wayPoints[currentIdx].position;
				Vector3 dir = self.ThisTransform.forward;
				if(wpIdx != preWpIdx && preWpIdx >=0 && preWpIdx < wp._wayPoints.Length)
				{
					dir = wp._wayPoints[currentIdx].position - wp._wayPoints[preWpIdx].position;
				}
				dir.y = 0;
				v3.y = 0;
				float angle = Vector3.Angle(dir, v3);
				v3 = wp._wayPoints[nextIdx1].position - wp._wayPoints[currentIdx].position;
				v3.y = 0;
				float angle1 = Vector3.Angle(dir, v3);
				if(angle < angle1)
				{
					currentIdx = nextIdx;
				}
				else
				{
					currentIdx = nextIdx1;
				}
				farTF = wp._wayPoints[currentIdx];
				break;
			}
			
		}
		if(farTF != null)
		{
			wpPos = farTF.position;
			ret = true;
		}
		if(ret)
		{
			return currentIdx;
		}
		else
		{
			return -1;
		}
	}
	
	//if ret = false ,means something wrong with  waypoint system
	public bool GetFarthestWayPoint(string wayName, ActionController target, out Vector3 wpPos)
	{
		bool ret = false;
		wpPos = Vector3.zero;
		FCWayPoints wp = null;
		Transform farTF = null;
		foreach(FCWayPoints ewp in  _wayPoints)
		{
			if(ewp._name == wayName )
			{
				wp = ewp;
				float disSqrt = -1;
			
				foreach(Transform tf in wp._wayPoints)
				{
					Vector3 v3 = (tf.position-target.ThisTransform.localPosition);
					v3.y =0;
					float disSqrtPlayerToWp = v3.sqrMagnitude;
					if(disSqrtPlayerToWp > disSqrt)
					{
						farTF = tf;
						disSqrt = disSqrtPlayerToWp;
					}
				}
				break;
			}
			
		}
		if(farTF != null)
		{
			wpPos = farTF.position;
			ret = true;
		}
		return ret;
	}
	public bool GetWayPoint(string wayName,int wpIdx, out Vector3 wpPos)
	{
		bool ret = false;
		wpPos = Vector3.zero;
		FCWayPoints wp = null;
		foreach(FCWayPoints ewp in  _wayPoints)
		{
			if(ewp._name == wayName )
			{
				wp = ewp;
				break;
			}
		}
		if(wpIdx >= wp._wayPoints.Length || wpIdx <0)
		{
			wpIdx = Random.Range(0,wp._wayPoints.Length-1);
		}
		ret = true;
		wpPos = wp._wayPoints[wpIdx].position;
		return ret;
	}
	
	public int GetWayPointRandom(string wayName)
	{
		int ret = 0;
		foreach(FCWayPoints ewp in  _wayPoints)
		{
			if(ewp._name == wayName )
			{
				ret = Random.Range(0,ewp._wayPoints.Length-1);
				break;
			}
		}
		return ret;
	}
	
	/// <summary>
	/// Gets the way point.
	/// </summary>
	/// <returns>
	/// The way point nearest by wolf
	/// </returns>
	/// <param name='self'>
	/// the transform of monster
	/// </param>
	/// <param name='target'>
	/// the transform of player
	/// </param>
	/// <param name='wayName'>
	/// Way name for monster use
	/// </param>
	/// <param name='safeDistance'>
	/// Safe distance(safe distance + distance from Centroid to head)
	/// </param>
	public int GetWayPoint(ActionController self, ActionController target, string wayName,float safeDistance)
	{
		int ret = -1;
		float safeDistanceSqrt = safeDistance*safeDistance;
		Vector3 fromMonsterToPlayer = target.ThisTransform.localPosition - self.ThisTransform.localPosition;
		fromMonsterToPlayer.y = 0;
		float radius = target.BodyRadius;
		float angle = Mathf.Atan2(fromMonsterToPlayer.magnitude, radius) * Mathf.Rad2Deg;
		float anglePlayerAndMonster = Vector3.Angle(self.ThisTransform.forward, target.ThisTransform.localPosition- self.ThisTransform.localPosition);
		if(anglePlayerAndMonster < angle && safeDistanceSqrt < fromMonsterToPlayer.sqrMagnitude)
		{
			//means monster is so far from player, need not find a point to away
			//can run to player directly
			ret = -1;
		}
		else
		{
			//we need a way point to run away from player
			//if angle between monster to player and forward of monster < angle, means monster is face to player
			
			FCWayPoints wp = null;
			foreach(FCWayPoints ewp in  _wayPoints)
			{
				if(ewp._name == wayName )
				{
					wp = ewp;
					break;
				}
			}
			if(wp != null)
			{
				// we need a random way point and go there
				/*int startidx = Random.Range(0, wp._wayPoints.Length-1);
				for(int i = startidx; i<startidx+wp._wayPoints.Length;i++)
				{
					int idx = startidx%wp._wayPoints.Length;
					float angleWayPointToPlayer = 
						Vector3.Angle(wp._wayPoints[i].localPosition - self.ThisTransform.localPosition, target.ThisTransform.localPosition - self.ThisTransform.localPosition);
					if(angleWayPointToPlayer > angle)
					{
						ret = idx;
						break;
					}
				}
				if(ret == -1)
				{
					ret = startidx;
				}
				*/
				//new code to get nearest point to self
				float distanceSqrt = 999999; 
				for(int i = 0 ; i<wp._wayPoints.Length; i++)
				{
					Vector3 v3 = (wp._wayPoints[i].position - self.ThisTransform.localPosition);
					v3.y =0;
					float disSqrt = v3.sqrMagnitude;
					if(disSqrt < distanceSqrt)
					{
						distanceSqrt = disSqrt;
						ret = i;
					}
				}
			}
		}
		return ret;
	}
}
