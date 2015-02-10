using UnityEngine;
using System.Collections;

public class HeroIconRenderer : MonoBehaviour {
	
	private static HeroIconRenderer _inst;
	public static HeroIconRenderer Singleton {
		get {return _inst; }
	}
	
	void Awake() {
		if(_inst != null)
		{
			Debug.LogError("HeroIconRenderer: detected singleton instance has existed. Destroy this one " + gameObject.name);
			Destroy(this);
			return;
		}
		
		_inst = this;
	}
	
	void OnDestroy() {
		if(_inst == this)
		{
			_inst = null;
		}
		foreach(System.Collections.Generic.List<IconInfo> l in _heroIcons) {
			foreach(IconInfo ii in l) {
				ii.texture.Release();
			}
		}
	}
	
	class IconInfo {
		public string key;
		public RenderTexture texture;
	};
	
	System.Collections.Generic.List<IconInfo> []_heroIcons = new System.Collections.Generic.List<IconInfo>[3] {
																new System.Collections.Generic.List<IconInfo>(),
																new System.Collections.Generic.List<IconInfo>(),
																new System.Collections.Generic.List<IconInfo>()};

    public void AddIconExternally(PlayerInfo info, Texture texture)
    {
		Debug.Log("AddIconExternal.");
		Texture found = SearchTextureByInfo(info);
		if(found == null) {
			string key = GetKeyFromInfo(info);
			if(string.IsNullOrEmpty(key)) {
				Debug.LogWarning("Can not generate key, exit.");
				return;
			}
			RenderTexture newTex = new RenderTexture(texture.width, texture.height, 0, RenderTextureFormat.ARGB32);
			Graphics.Blit(texture, newTex);
			IconInfo newInfo = new IconInfo();
			newInfo.key = key;
			newInfo.texture = newTex;
			_heroIcons[info.RoleID].Add(newInfo);
			Debug.Log("create new icon.");
		}
		Debug.Log("icon existed, exit.");
	}
	
	public Texture GetIconByPlayerInfo(PlayerInfo info) {
		Debug.Log("GetIconByPlayerInfo");
		Texture ret = SearchTextureByInfo(info);
		if(ret == null) {
			string key = GetKeyFromInfo(info);
			if(string.IsNullOrEmpty(key)) {
				Debug.LogWarning("Can not generate key from equipment, exit.");
				return null;
			}
			Debug.Log("begin to create new texture");
			GameObject tempChar = CharacterAssembler.Singleton.AssembleCharacterWithoutAI(GameSettings.Instance.roleSettings[info.RoleID].townLabel);
			AvatarController ac = tempChar.GetComponent<AvatarController>();
			ac.Init("", "", info.RoleID);
			Assertion.Check(ac != null);
			System.Collections.Generic.List<GameObject> EquipInstances = new System.Collections.Generic.List<GameObject>();
			PlayerInfo.GetOtherEquipmentInstanceWithIds(EquipInstances, info.equipIds);

			foreach(GameObject g in EquipInstances) {
				FCEquipmentsBase []es = g.GetComponentsInChildren<FCEquipmentsBase>();
				foreach(FCEquipmentsBase eeb in es) {
					EquipmentAssembler.Singleton.Assemble(eeb, ac);
				}
				GameObject.Destroy(g);
			}
			
			ac.TakeIcon();
			RenderTexture icon = ac._icon;
			Assertion.Check(icon != null);
			RenderTexture newTex = new RenderTexture(icon.width, icon.height, 0, RenderTextureFormat.ARGB32);
			Graphics.Blit(icon, newTex);
			IconInfo newIcon = new IconInfo();
			newIcon.key = key;
			newIcon.texture = newTex;
			_heroIcons[info.RoleID].Add(newIcon);
			
			GameObject.Destroy(tempChar);
			Debug.Log("create new icon");
			return newTex;
		}
		Debug.Log("icon existed, return old one.");
		return ret;
		
	}

    string GetKeyFromInfo(PlayerInfo info)
    {
		Debug.Log("begin search equipments...");
		foreach(EquipmentIdx idx in info.equipIds) {
			Debug.Log("reading " + idx._id);
			string []strs = idx._id.Split(new char[]{'_'}, System.StringSplitOptions.RemoveEmptyEntries);
			if(strs.Length > 2 && strs[1] == "helm") {
				string newid = strs[0] + "_" + strs[1] + "_" + strs[2];
				Debug.Log("new key is " + newid);
				return newid;
			}
		}
		Debug.Log("search key failed.");
		return null;
	}

    Texture SearchTextureByInfo(PlayerInfo info)
    {
		Debug.Log("start searching texture...");
		string key = GetKeyFromInfo(info);
		if(string.IsNullOrEmpty(key)) {
			return null;
		}
		foreach (IconInfo ii in _heroIcons[info.RoleID])
		{
			if(ii.key == key) {
				Debug.Log("found " + key + " in array");
				return ii.texture;
			}
		}
		Debug.Log("Search texture Failed");
		return null;
	}
	
	[ContextMenu("List All Icons")]
	void ListAllIcons() {
		foreach(System.Collections.Generic.List<IconInfo> l in _heroIcons) {
			foreach(IconInfo ii in l) {
				Debug.Log(ii.key + " " + ii.texture.width + "X" + ii.texture.height);
			}
		}
	}
}
