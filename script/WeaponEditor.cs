using UnityEditor;
using UnityEngine;
using System.Collections;

public class WeaponEditor : ScriptableWizard {
	
	public bool _addBladeSlide;
	public GameObject []_upgradeEffects;
	public GameObject []_weaponModels;
	
	[MenuItem("Tools/Weapon/Create Weapon %#w")]
	static void CreateWeapon() {
		ScriptableWizard.DisplayWizard("Create Weapons", typeof(WeaponEditor), "Create");
	}
	
	void OnWizardCreate() {
		
		foreach(GameObject go in _weaponModels) {
			string path = AssetDatabase.GetAssetPath(go);
			GameObject inst = GameObject.Instantiate(go, Vector3.zero, Quaternion.identity) as GameObject;
			string matPath = path.ToLower().Replace(".fbx", ".mat");
			matPath = matPath.Replace("_left", "");
			matPath = matPath.Replace("_right", "");
			Material mat = AssetDatabase.LoadAssetAtPath(matPath, typeof(Material)) as Material;
			// collect mesh renderers. Most of weapons has only 1 renderer.
			Renderer []renderers = inst.GetComponentsInChildren<Renderer>();
			// get max bounding box.
			Bounds bnd = renderers[0].bounds;
			for(int i = 1;i < renderers.Length;++i) {
				if(renderers[i].gameObject.tag == "") {
					bnd.Encapsulate(renderers[i].bounds);
				}
			}
			// add blade slide.
			if(_addBladeSlide && path.Contains("_mid.") && renderers.Length > 0) {
				BladeSlide bs = inst.AddComponent<BladeSlide>();
				GameObject a = new GameObject("A");
				a.transform.parent = inst.transform;
				a.transform.localPosition = Vector3.forward * bnd.max.z;
				GameObject b = new GameObject("B");
				b.transform.parent = inst.transform;
				b.transform.localPosition = Vector3.forward * (bnd.min.z + bnd.size.z * 0.25f);
				bs.PointA = a.transform;
				bs.PointB = b.transform;
				bs.BladeMaterial = AssetDatabase.LoadAssetAtPath("Assets/Weapon/Common/Blade/swing_default.mat", typeof(Material)) as Material;
			}
			
			if(path.Contains("_mid.")) {
				// add physics collider & callback script.
				GameObject weaponCollider = new GameObject("Collider");
				bool isHero = false;
				if(path.StartsWith("Assets/Weapon/Warrior/")
					|| path.StartsWith("Assets/Weapon/Monk/")
					|| path.StartsWith("Assets/Weapon/Mage/")) {
					isHero = true;
				}
				if(isHero) {
					weaponCollider.AddComponent<PlayerMessageReceiver>();
				}
				else {
					weaponCollider.AddComponent<MessageReciever>();
				}
				BoxCollider bc = weaponCollider.AddComponent<BoxCollider>();
				bc.center = bnd.center;
				bc.size = bnd.size;
				bc.isTrigger = true;
				GameObject oldGo = AssetDatabase.LoadAssetAtPath(path.Replace(".fbx", ".prefab"), typeof(GameObject)) as GameObject;
				if(oldGo != null) {
					oldGo = GameObject.Instantiate(oldGo) as GameObject;
					BoxCollider oldBc = oldGo.GetComponentInChildren<BoxCollider>();
					if(oldBc != null) {
						bc.center = oldBc.center;
						bc.size = oldBc.size;
					}
					GameObject.DestroyImmediate(oldGo);
				}
				weaponCollider.transform.parent = inst.transform;
				weaponCollider.transform.localPosition = Vector3.zero;
				weaponCollider.transform.localRotation = Quaternion.identity;
				weaponCollider.transform.localScale = Vector3.one;
				
				if(isHero) {
					// hook upgrade effect.
					foreach(GameObject upgrade in _upgradeEffects) {
						GameObject u = GameObject.Instantiate(upgrade) as GameObject;
						bool addEffect = false; // decide whether to add this effect.
						Vector3 localPosition = Vector3.forward * (bnd.center.z + bnd.size.z * 0.5f);
						Quaternion localRot = Quaternion.identity;
						Vector3 localScale = Vector3.one;
						UpgradeEffect ue = u.GetComponent<UpgradeEffect>();
						// ignore this object if no UpgradeEffect component attached on.
						if(ue != null) {
							addEffect = true;
							// for Shadow Effect, renderer of weapon is needed
							if(ue is UpgradeShadowEffect) {
								addEffect = (renderers.Length > 0);
								if(renderers.Length > 0) {
									UpgradeShadowEffect use = ue as UpgradeShadowEffect;
									use._reference = renderers[0];
								}
							}
						}
						// if effect is allowed to add, hang it under weapon.
						if(addEffect) {
							u.transform.parent = inst.transform;
							u.transform.localPosition = localPosition;
							u.transform.localRotation = localRot;
							u.transform.localScale = localScale;
						}
						// otherwise destroy this temprary object.
						else {
							GameObject.DestroyImmediate(u);
						}
					}
				}
			}
			// set material and layer for renderable objects.
			foreach(Renderer r in renderers) {
				if(r.gameObject.tag == "Untagged") {
					r.sharedMaterial = mat;
					r.gameObject.layer = LayerMask.NameToLayer("CHARACTER");
				}
			}
			PrefabUtility.CreatePrefab(path.ToLower().Replace(".fbx", ".prefab"), inst);
			DestroyImmediate(inst);
		}
	}
}
