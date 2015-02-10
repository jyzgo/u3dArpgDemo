using UnityEngine;
using System.Collections;

public class EquipmentAssembler : MonoBehaviour
{

    private static EquipmentAssembler _inst;

    public static EquipmentAssembler Singleton
    {
        get { return _inst; }
    }

    void Awake()
    {
        if (_inst != null)
        {
            Debug.LogError("CharacterDictionary: detected singleton instance has existed. Destroy this one " + gameObject.name);
            Destroy(this);
            return;
        }

        _inst = this;
    }

    void OnDestroy()
    {
        if (_inst == this)
        {
            _inst = null;
        }
    }

    public void Assemble(FCEquipmentsBase equipment, AvatarController avatar)
    {
		// check if the equipment is resource-meet, if not, load default resource.
		FCEquipmentsBase equipmentRes = CheckEquipmentResource(equipment);
		if(equipmentRes == null) {
			return;
		}
		
        System.Collections.Generic.List<Renderer> renderers = new System.Collections.Generic.List<Renderer>();
        //mount the equipment models
        foreach (FCEquipmentsBase.EquipmentSlot slot in equipmentRes._equipmentSlots)
        {
            if (!string.IsNullOrEmpty(slot._modelPath)) //equipment does not have a mesh
            {
                GameObject prefab = InJoy.AssetBundles.AssetBundles.Load(GetModelPathOnQuality(slot._modelPath, avatar._quality)) as GameObject;

                GameObject equipmentInstance = null;

                if (prefab != null)
                {
					Transform transSlot = avatar.GetSlotNode(slot._equipSlot);
					if(equipment._specNode != "" 
						&& equipment._isLogicEquipment)
					{
						transSlot = Utils.GetNode(equipment._specNode,avatar.gameObject).transform;
					}
                    equipmentInstance = Utils.InstantiateGameObjectWithParent(prefab, transSlot).gameObject;

                    equipmentInstance.name = prefab.name;

                    // create connection.
                    MessageReciever[] receivers = equipmentInstance.GetComponentsInChildren<MessageReciever>();
                    foreach (MessageReciever mr in receivers)
                    {
                        mr._parent = equipment;
                    }

                    equipment.OnAssembled(slot._equipSlot,equipmentInstance, slot._equipmentType);
					
					Renderer []equipmentRenderers = equipmentInstance.GetComponentsInChildren<Renderer>();
					foreach(Renderer r in equipmentRenderers) {
						if(r.gameObject.tag != "weaponEffect") {
							renderers.Add(r);
						}
					}
					
					if(!equipment._isLogicEquipment) 
					{
                    	avatar.AddEquipmentNode(slot._equipSlot, equipmentInstance, false);
					}
					
					UpgradeEffect []effects = equipmentInstance.GetComponentsInChildren<UpgradeEffect>();
					foreach(UpgradeEffect e in effects) {
						e.SetGrade(equipment._evolutionLevel);			
					}
                }

            }

        }
        if (equipmentRes is FCArmor)
        {
            string[] avatarAreas = new string[equipmentRes._avatarAreas.Length];
            for (int i = 0; i < avatarAreas.Length; ++i)
            {
                avatarAreas[i] = FCEquipmentsBase.GetAvatarAreaNameByArea(equipmentRes._avatarAreas[i]);
            }
			string baseTexPath = GetTexturePathOnQuailty(equipmentRes._texturePath, avatar._quality);
            Texture2D baseTex = InJoy.AssetBundles.AssetBundles.Load(baseTexPath, typeof(Texture2D)) as Texture2D;
            Assertion.Check(baseTex != null);
            Texture2D normalTex = null;
            Texture2D speTex = null;
            FCArmor armor = equipmentRes as FCArmor;
            string normTexPath = armor._normalTexturePath;
            if (normTexPath != "" && avatar._quality == CharacterGraphicsQuality.CharacterGraphicsQuality_Preview)
            {
                normalTex = InJoy.AssetBundles.AssetBundles.Load(normTexPath, typeof(Texture2D)) as Texture2D;
                Assertion.Check(normalTex != null);
            }
            avatar._materialBuilder.UpdateMaterialOfEquipment(avatar.materialInst, renderers.ToArray(), avatarAreas, baseTex, normalTex, armor._customColor, avatar._quality);
			// clean up.
			Resources.UnloadAsset(baseTex);
			Resources.UnloadAsset(normalTex);
			Resources.UnloadAsset(speTex);
        }
		// for weapon, only collect materials, do not take part in atlas.
		if(equipment is FCWeapon) {
			if(!equipment._isLogicEquipment) {
				avatar.materialInst.GetWeaponMaterial(renderers.ToArray());
			}
		}
		// perhaps there are extra game objects to display upgrade effect.
		foreach(FCEquipmentsBase.EquipmentSlot es in equipmentRes._upgradeSlots) {
			// create a game object for display upgrade effect only.
			GameObject prefab = InJoy.AssetBundles.AssetBundles.Load(es._modelPath) as GameObject;
			Transform transSlot = avatar.GetSlotNode(es._equipSlot);
			GameObject effectInst = Utils.InstantiateGameObjectWithParent(prefab, transSlot).gameObject;
			// active level effect on this instance.
			UpgradeEffect []effects = effectInst.GetComponentsInChildren<UpgradeEffect>();
			foreach(UpgradeEffect e in effects) {
				e.SetGrade(equipment._evolutionLevel);
			}
			avatar.AddEquipmentGradeEffect(es._equipSlot, effectInst);
		}
    }
	
	// CheckEquipmentResource() will return FCEquipmentBase for resource loading.
	public FCEquipmentsBase CheckEquipmentResource(FCEquipmentsBase eeb)
	{
		bool checkFailed = false;
		foreach (FCEquipmentsBase.EquipmentSlot slot in eeb._equipmentSlots)
        {
            if (!string.IsNullOrEmpty(slot._modelPath)) //equipment does not have a mesh
            {
                GameObject prefab = InJoy.AssetBundles.AssetBundles.Load(slot._modelPath, typeof(GameObject)) as GameObject;
                if(prefab == null) {
					checkFailed = true;
					break;
				}
			}
		}
		if(eeb._texturePath != "") {
			Texture2D baseTex = InJoy.AssetBundles.AssetBundles.Load(eeb._texturePath, typeof(Texture2D)) as Texture2D;
            if(baseTex == null) {
				checkFailed = true;
			}
		}
		// check failed, look for default equipment.
		if(checkFailed) {
			return GetDefaultEquipment(eeb);
		}
		
		// check passed, use origin configuration for resource loading.
		return eeb;
	}
	
	public string []_mageDefaultEquipments;
	public string []_monkDefaultEquipments;
	public string []_warriorDefaultEquipments;
	public string []_equipmentMap;
	
	FCEquipmentsBase GetDefaultEquipment(FCEquipmentsBase eeb)
	{
		string equipName = eeb.gameObject.name;
		// looking for character's class...
		string []defaultEquipment = null;
		if(equipName.Contains("mage_")) {
			defaultEquipment = _mageDefaultEquipments;
		}
		else if(equipName.Contains("monk_")) {
			defaultEquipment = _monkDefaultEquipments;
		}
		else if(equipName.Contains("warrior_")) {
			defaultEquipment = _warriorDefaultEquipments;
		}
		if(defaultEquipment == null) {
			Debug.LogError("Can not find default equipment for " + equipName);
			return eeb;
		}
		// looking for type of equipment...
		string equipmentEntity = null;
		for(int i = 0;i < _equipmentMap.Length;++i) {
			if(equipName.Contains(_equipmentMap[i])) {
				equipmentEntity = defaultEquipment[i];
				break;
			}
		}
		if(equipmentEntity == null) {
			Debug.LogError("Can not get type of equipment " + equipName);
			return eeb;
		}
		// try to load default prefab...
		FCEquipmentsBase res = null;
		GameObject go = InJoy.AssetBundles.AssetBundles.Load(equipmentEntity, typeof(GameObject)) as GameObject;
		if(go != null) {
			res = go.GetComponent<FCEquipmentsBase>();
		}
		if(res == null) {
			Debug.LogError("Fail to load default equipment " + equipmentEntity + " for " + equipName);
			return eeb;
		}
		// succeeded to load default equipment.
		Debug.LogWarning("Can not load equipment " + equipName + ", load default equipment " + equipmentEntity + " instead.");
		return res;
	}
	
	public string []_mageDefaultIcons;
	public string []_monkDefaultIcons;
	public string []_warriorDefaultIcons;
	public string []_defaultIconKeywords;
	
	public string _mageErrorIcon;
	public string _monkErrorIcon;
	public string _warriorErrorIcon;
	
	public string GetDefaultIcons(string iconPath)
	{
		Debug.Log("looking for default icon for \"" + iconPath + "\"");
		string []defaultIcons = null;
		string errorIcon = _warriorErrorIcon;
		if(iconPath.Contains("mage_")) {
			defaultIcons = _mageDefaultIcons;
			errorIcon = _mageErrorIcon;
		}
		else if(iconPath.Contains("monk_")) {
			defaultIcons = _monkDefaultIcons;
			errorIcon = _monkErrorIcon;
		}
		else if(iconPath.Contains("warrior_")) {
			defaultIcons = _warriorDefaultIcons;
			errorIcon = _warriorErrorIcon;
		}
		if(defaultIcons == null) {
			Debug.LogError("Can not find default class of icon " + iconPath);
			return errorIcon;
		}
		string defIconPath = null;
		for(int i = 0;i < _defaultIconKeywords.Length;++i) {
			if(iconPath.Contains(_defaultIconKeywords[i])) {
				defIconPath = defaultIcons[i];
				break;
			}
		}
		if(defIconPath == null) {
			Debug.LogError("Can not find type of icon " + iconPath);
			defIconPath = errorIcon;
		}
		return defIconPath;
	}

    string GetModelPathOnQuality(string path, CharacterGraphicsQuality quality)
    {
        int lowResLevel = 0;
        int deviceLevel = GameSettings.Instance.LODSettings.GetDeviceLevel();
        string[] replaceString = new string[] { "_mid", "_mid", "_high" };
        string[] replaceString_lowDevice = new string[] { "_low", "_low", "_high" };
        string replacedString = "_mid";

        string[] activedStr = replaceString;
        if (deviceLevel <= lowResLevel)
        {
            activedStr = replaceString_lowDevice;
        }

        return path.Replace(replacedString, activedStr[(int)quality]);
    }
	
	string GetTexturePathOnQuailty(string path, CharacterGraphicsQuality quality) {
		string []replaceString = new string[] {"_mid.", "_mid.", "."};
		string []replacedString = new string[] {".", ".", "."};
		return path.Replace(replacedString[(int)quality], replaceString[(int)quality]);
	}

}
