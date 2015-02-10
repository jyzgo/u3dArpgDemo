using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("FC/Logic/FCObject/PlayAndEnemy/Agent/AI/BulletList")]
public class BulletList : MonoBehaviour {

	protected Dictionary<string,List<FCBullet>> _allBulletArray;
	protected List<FCBullet> _deadArray;
	
	public Dictionary<string,List<FCBullet>> AllBulletArray
	{
		get
		{
			return _allBulletArray;
		}
	}
	
	void Awake()
	{
		_allBulletArray = new Dictionary<string,List<FCBullet>>();
		_deadArray = new List<FCBullet>();
	}
	
	protected FCBullet AddBulletToPool(string bulletName)
	{
		GameObject go = BulletAssembler.Singleton.AssembleBullet(bulletName);
		if(go != null) {
			List<FCBullet> leb = _allBulletArray[bulletName];
			go.transform.parent = LevelManager.Singleton.BulletRoot;
			FCBullet eb = go.GetComponent<FCBullet>();
			eb._name = bulletName;
			eb.enabled = true;
			leb.Add(eb);
			return eb;
		}
		else
		{
			return null;
		}
	}
	
	public void KillABulletByNameAndFireport(string bulletName, string fireport)
	{
		if(!_allBulletArray.ContainsKey(bulletName))
		{
			return;
		}
		List<FCBullet> leb = _allBulletArray[bulletName];
		if(leb.Count>0)
		{
			foreach(FCBullet ebf in leb)
			{
				if(ebf.enabled && !ebf.IsDead && ebf.IsBelongToFirePort(fireport))
				{
					ebf.Dead();
				}
			}
		}
	}
	
	public FCBullet GetFromPool(string bulletName)
	{
		if(!_allBulletArray.ContainsKey(bulletName))
		{
			_allBulletArray.Add(bulletName, new List<FCBullet>());
		}
		List<FCBullet> leb = _allBulletArray[bulletName];
		FCBullet eb = null;
		if(leb.Count>0)
		{
			foreach(FCBullet ebf in leb)
			{
				if(!ebf.enabled && ebf.DeadTime<0)
				{
					ebf.enabled = true;
					eb = ebf;
					break;
				}
			}
		}
		if(eb == null || leb.Count<=0)
		{
			eb = AddBulletToPool(bulletName);
		}
		return eb;
	}
	
	void Update()
	{
		foreach(FCBullet eb in _deadArray)
		{
			if(eb.DeadTime>0)
			{
				eb.DeadTime-=Time.deltaTime;
			}
			else
			{
				eb.DeadTime = -1;
				_deadArray.Remove(eb);
				break;
			}
		}
	}
	public void ReturnToPool(FCBullet eb)
	{
		eb.enabled = false;
		if(eb.DeadTime >=0)
		{
			_deadArray.Add(eb);
		}
	}
	
	public void OnDestroy()
	{
		foreach(KeyValuePair<string, List<FCBullet>> pair in _allBulletArray)
		{
			List<FCBullet> leb = pair.Value;
			foreach(FCBullet eb in leb)
			{
				DestroyObject(eb.ThisObject);
			}
		}
	}
}
	
	
	

