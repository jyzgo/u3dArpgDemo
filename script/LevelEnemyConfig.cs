using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class EnemyInstanceInfo
{
	public string enemyLabel;
	public string path;
	public BornPoint.SpawnPointType spawnAt;
	public List<LootObjData> lootTable;
	public float delayTime;		//a pause to take before spawning

	public EnemyInstanceInfo Clone()
	{
		EnemyInstanceInfo clone = new EnemyInstanceInfo();
		clone.enemyLabel = enemyLabel;
		clone.path = path;
		clone.spawnAt = spawnAt;
		clone.lootTable = lootTable;
		clone.delayTime = delayTime;
		return clone;
	}
}