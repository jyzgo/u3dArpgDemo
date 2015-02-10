using UnityEngine;
using System.Collections;

public class BlockadeController : MonoBehaviour {
	
	bool _active = false;	
	string _prefabName = null;
	string _fadeoutSfxName = null;
	string _fadeinSfxName = null;	
	FC_GLOBAL_EFFECT _fadeoutEffect;
	
	public void Init() 
	{
		_active = false;
		
		//get block and get parameters
		Blockade blockAde = gameObject.GetComponent<Blockade>();
		Assertion.Check(blockAde != null);
		
		_prefabName = blockAde._gateForGame;
		_fadeoutSfxName = blockAde._fadeinSfxName;
		_fadeinSfxName = blockAde._fadeinSfxName;
		_fadeoutEffect = blockAde._fadeoutEffect;
	}
	
	public void CreateGates()
	{
		Object prefab = InJoy.AssetBundles.AssetBundles.Load(_prefabName, typeof(GameObject));
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
				t.localScale = new Vector3(collider.size.x,t.localScale.y,collider.size.z);//collider.size;
				t.localPosition += collider.center;
				collider.enabled = true;
			}
		}		
	}
	
	public void Active()
	{
		// Whether it's already created?
		if (_active == true)
		{
			return;
		}
		
		_active = true;
		
		gameObject.SetActive(true);
		
		// Remove the gameobject for editor.
		foreach (Transform t in gameObject.transform)
		{
			GameObject.Destroy(t.gameObject);
		}
		
		CreateGates();
		
		// Play a sound effect when a gate is borned.
		if (_fadeinSfxName != "")
		{
			SoundManager.Instance.PlaySoundEffect(_fadeinSfxName);
		}	
		ObjectManager.Instance.GetMyActionController().ACSetWalkLayer(FCConst.WALKMASK_NORMAL);
	}
	
	
	public void DeActive()	
	{
		// Whether it's already created?
		if (_active == false)
		{
			return;
		}
		
		_active = false;
		
		//  Disable the boxcollider.
		BoxCollider collider = gameObject.GetComponent<BoxCollider>();
		if (null != collider)
		{
			collider.enabled = false;
		}
		
		// Play a sound effect when fading out.
		if (_fadeoutSfxName != "")
		{
			SoundManager.Instance.PlaySoundEffect(_fadeoutSfxName);
		}
		
		// Play a paticale effect when fading out.
		if (gameObject.transform.childCount > 0)
		{
			Transform t = gameObject.transform.GetChild(0);
			GlobalEffectManager.Instance.PlayEffect(_fadeoutEffect, t.position, t.rotation, t.localScale);
		}
		
		foreach (Transform t in gameObject.transform)
		{
			GameObject.Destroy(t.gameObject);
		}		
		ObjectManager.Instance.GetMyActionController().ACSetWalkLayer(FCConst.WALKMASK_ALL);
	}
}
