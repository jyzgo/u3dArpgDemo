using UnityEngine;
using System.Collections;


public class LockGate : AirGate 
{
	// Use this for initialization
	protected override void Awake () 
	{
		// Remove the gameobject for editor.
		foreach (Transform t in gameObject.transform)
		{
			GameObject.Destroy(t.gameObject);
		}
		
		CreateGates();
	}
	
	protected override void CreateGates()
	{		
		Object prefab = InJoy.AssetBundles.AssetBundles.Load(_gateForGame, typeof(GameObject));
		GameObject go = GameObject.Instantiate(prefab) as GameObject;
		
		if (go != null)
		{
			Transform t = go.transform;
			t.parent = gameObject.transform;
			t.localPosition = Vector3.zero;
			t.localRotation = Quaternion.identity;
			
			BoxCollider collider = gameObject.GetComponent<BoxCollider>();
			
			if (null != collider)
			{
				t.localScale = collider.size;
				collider.enabled = false;
			}
			
			//go.layer = gameObject.layer;
		}
	}
	
	public override void Deactive()
	{
		foreach (Transform t in gameObject.transform)
		{
			GameObject.Destroy(t.gameObject);
		}
	}
}
