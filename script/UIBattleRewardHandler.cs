using UnityEngine;
using System.Collections;

public class UIBattleRewardHandler : MonoBehaviour 
{
	public MeshRenderer _background;
	public MeshRenderer _icon;

	// Use this for initialization
	void Start () {
	
	}
	
	public void SetIcon(string icon)
	{
		Texture tex = InJoy.AssetBundles.AssetBundles.Load(icon) as Texture;
		
		_icon.material.mainTexture = tex;
	}
}
