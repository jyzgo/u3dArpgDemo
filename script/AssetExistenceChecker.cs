using UnityEditor;
using UnityEngine;
using System.Collections;

public class AssetExistenceChecker : AssetPostprocessor {
	
	static string CharacterConfigPath = "Assets/GlobalManagers/Data/Characters/CharacterTable.asset";
	static string WeaponPrefabPrefix = "Assets/Entity/Weapon/";
	
	static void OnPostprocessAllAssets(string[] importAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromPath) {
		
		foreach(string asset in importAssets) {
			if(asset == CharacterConfigPath) {
				CheckCharacterConfig(asset);
			}
			else if(asset.StartsWith(WeaponPrefabPrefix) && asset.EndsWith(".prefab")) {
				CheckWeaponResource(asset);
			}
		}
	}
	
	static void CheckCharacterConfig(string asset) {
		CharacterTable ct = AssetDatabase.LoadAssetAtPath(asset, typeof(CharacterTable)) as CharacterTable;
		if(ct != null) {
			foreach(CharacterInformation ci in ct.characterTableList) {
				if(ci.aiAgentPath != "" && !System.IO.File.Exists(ci.aiAgentPath)) {
					Debug.LogError("Asset " + ci.aiAgentPath + " is needed by " + asset);
				}
				if(ci.modelPath != "" && !System.IO.File.Exists(ci.modelPath)) {
					Debug.LogError("Asset " + ci.modelPath + " is needed by " + asset);
				}
			}
		}
		else {
			Debug.LogError(asset + ": wrong ScriptableObject type.");
		}
	}

    static void CheckWeaponResource(string asset)
    {
        GameObject go = AssetDatabase.LoadAssetAtPath(asset, typeof(GameObject)) as GameObject;
        if (go != null)
        {
            FCWeapon[] weapons = go.GetComponents<FCWeapon>();
            foreach (FCWeapon w in weapons)
            {
                foreach (FCEquipmentsBase.EquipmentSlot slot in w._equipmentSlots)
                {
                    if (!System.IO.File.Exists(slot._modelPath))
                    {
                        Debug.LogError("Asset " + slot._modelPath + " is needed by " + asset);
                    }
                }
            }
        }
    }
}
