using UnityEngine;
using System.Collections;

[System.Serializable]
public class AcPrefab{
	public string _Id;
	public string _acCharacterId;
	
}

public class AcPrefabList : ScriptableObject {
	public AcPrefab[] _dataList;
	
	public AcPrefab Find(string acId)
	{
		foreach(AcPrefab ac in _dataList)
		{
			if(ac._Id == acId)
			{
				return ac;
			}
		}
		
		Debug.LogError("AcPrefabList->Fine:["+acId+"]");
		return null;
	}
	
}
