using UnityEngine;
using System.Collections;


[System.Serializable]
public class WorldBundlesData
{
	public string   _worldName;
	public string[] _bundles;
	public string   _downloadTipsIDS;
	public bool     _downloadInLaunch = true;
}

public class BundlesConfig : ScriptableObject
{
	public WorldBundlesData[] _worlds;
	
	public WorldBundlesData GetBundlesByWorld(string world_name)
	{
		foreach (WorldBundlesData wbd in _worlds)
		{
			if (world_name == wbd._worldName)
			{
				return wbd;
			}
		}
		
		Assertion.Check(false);
		
		return null;
	}
}
