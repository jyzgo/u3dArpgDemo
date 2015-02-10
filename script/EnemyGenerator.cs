using UnityEditor;
using UnityEngine;
using System.Collections;

public class EnemyGenerator : ScriptableWizard {

	[System.Serializable]
	public class EquipmentInfo {

		public string _name;
		[HideInInspector]
		public Transform _parent;

		public GameObject _equipmentFbx;
		public Material _suit;
	}

	class InternalEquipmentInfo {
		public Material _suit;
		public GameObject _equipmentInstance;
		public GameObject _equipmentSource;

		public void BuildEquipment(EquipmentInfo source) {
			if(_equipmentInstance != null) {
				GameObject.DestroyImmediate(_equipmentInstance);
			}
			_equipmentSource = source._equipmentFbx;
			if(_equipmentSource != null) {
				_equipmentInstance = GameObject.Instantiate(_equipmentSource) as GameObject;
				_equipmentInstance.transform.parent = source._parent;
				_equipmentInstance.transform.localPosition = Vector3.zero;
				_equipmentInstance.transform.localRotation = Quaternion.identity;
				_equipmentInstance.transform.localScale = Vector3.one;
				BuildMaterial(source);
			}
		}

		public void BuildMaterial(EquipmentInfo source) {
			_suit = source._suit;
			if(_equipmentInstance != null) {
				Renderer renderer = _equipmentInstance.GetComponent<Renderer>();
				if(renderer != null) {
					renderer.sharedMaterial = _suit;
				}
			}
		}
	}

	public enum UITypes
	{
		Normal = 0,
		Elite,
		None,
	};

	public UITypes _uiType;
	public string _name;
	public GameObject _enemyFbx = null;
	public Material _material = null;
	public bool _needShadow;
	public bool _bossComponent;
	public bool _needDamageCollider = false;

	Material _internalMat = null;
	GameObject _internalFbx = null;

	public EquipmentInfo []_equipments = new EquipmentInfo[0];
	InternalEquipmentInfo []_internalEquipments = new InternalEquipmentInfo[0];

	GameObject _previewModel = null;

	void OnWizardUpdate () {
		// check if enemy fbx is updated.
		if(_enemyFbx != _internalFbx) {
			UpdateFBX();
		}
		// check body material.
		if(_material != _internalMat) {
			UpdateMaterial();
		}
		// update changed equipment one by one.
		for(int i = 0;i < _equipments.Length;++i) {
			if(_internalEquipments[i]._equipmentSource != _equipments[i]._equipmentFbx) {
				_internalEquipments[i].BuildEquipment(_equipments[i]);
			}
			if(_internalEquipments[i]._suit != _equipments[i]._suit) {
				_internalEquipments[i].BuildMaterial(_equipments[i]);
			}
		}

		isValid = (_previewModel != null);
	}

	void UpdateFBX() {
		if(_previewModel != null) {
			DestroyImmediate(_previewModel);
		}
		_previewModel = GameObject.Instantiate(_enemyFbx) as GameObject;
		_internalFbx = _enemyFbx;
		// clear equipments.
		_equipments = null;
		_internalEquipments = null;
		_internalMat = null;
		// search hang on point.
		Transform []transforms = _previewModel.GetComponentsInChildren<Transform>();
		System.Collections.Generic.List<Transform> _nodes = new System.Collections.Generic.List<Transform>();
		foreach(Transform t in transforms) {
			if(t.gameObject.name.StartsWith("node_")) {
				_nodes.Add(t);
			}
		}
		_equipments = new EquipmentInfo[_nodes.Count];
		_internalEquipments = new InternalEquipmentInfo[_nodes.Count];
		for(int i = 0;i < _equipments.Length;++i) {
			_equipments[i] = new EquipmentInfo();
			_equipments[i]._name = _nodes[i].gameObject.name;
			_equipments[i]._parent = _nodes[i];
			_equipments[i]._suit = null;
			_equipments[i]._equipmentFbx = null;

			_internalEquipments[i] = new InternalEquipmentInfo();
			_internalEquipments[i].BuildEquipment(_equipments[i]);
		}
	}

	void UpdateMaterial() {
		if(_previewModel != null) {
			Renderer r = _previewModel.GetComponentInChildren<SkinnedMeshRenderer>();
			if(r != null) {
				r.sharedMaterial = _material;
				_internalMat = _material;
			}
		}
	}

	[MenuItem("Tools/Character/Create Enemy Wizard")]
	static void CreateEnemy() {
		ScriptableWizard.DisplayWizard("Create Enemy", typeof(EnemyGenerator), "Create");
	}

	void OnDestroy() {
		if(_previewModel != null) {
			DestroyImmediate(_previewModel);
		}
	}

	void OnWizardCreate() {
		string path = AssetDatabase.GetAssetPath(_enemyFbx);
		path = path.Replace(".fbx", "_" + _name + ".prefab");
		GameObject oldGo = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;

		AvatarController ac = _previewModel.AddComponent<AvatarController>();
		Animator animator = _previewModel.GetComponent<Animator>();
		// fill appropriate parameters.
		if(animator != null) {
			//animator.animatePhysics = true;
            animator.updateMode = AnimatorUpdateMode.AnimatePhysics;
		}

		Renderer []renderers = _previewModel.GetComponentsInChildren<Renderer>();
		System.Collections.Generic.List<Renderer> avildRenderer = new System.Collections.Generic.List<Renderer>();
		foreach(Renderer r in renderers) {
			if(r.gameObject.tag != "weaponEffect") {
				avildRenderer.Add(r);
			}
		}
		ac._characterRenderers = avildRenderer.ToArray();

		// can not find builder from model, create one and attach it to prefab
        GameObject builderobj = Utils.NewGameObjectWithParent("SimpleMaterialBuilder", _previewModel.transform).gameObject;
		MaterialBuilder builder = builderobj.AddComponent<SimpleMaterialBuilder>();
		ac._materialBuilder = builder;

		ac._baseMaterial = _material;

		// NavMesh settings.
		NavMeshAgent nma = _previewModel.AddComponent<NavMeshAgent>();
		Bounds b = ac._characterRenderers[0].bounds;
		foreach(Renderer r in ac._characterRenderers) {
			b.Encapsulate(r.bounds.max);
			b.Encapsulate(r.bounds.min);
		}
		nma.height = b.size.y;
		nma.radius = (b.extents.x + b.extents.z) * 0.5f;
		nma.baseOffset = 0.0f;
		nma.avoidancePriority = 50;
		NavMeshAgent oldNma = null;
		if(oldGo != null) {
			oldNma = oldGo.GetComponent<NavMeshAgent>();
		}
		if(oldNma != null) {
			nma.radius = oldNma.radius;
			nma.height = oldNma.height;
		}
		// add shadow.
		ac._dynamicShadow = !_needShadow;
		if(_needShadow) {
			GameObject shadow = AssetDatabase.LoadAssetAtPath("Assets/Characters/Common/shadow.prefab", typeof(GameObject)) as GameObject;
			shadow = GameObject.Instantiate(shadow) as GameObject;
			shadow.transform.parent = _previewModel.transform;
			shadow.transform.localPosition = Vector3.zero;
			shadow.transform.localRotation = Quaternion.identity;
			shadow.transform.localScale = Vector3.one * nma.radius * 2;
			if(oldGo != null) {
				Transform oldShadow = oldGo.transform.FindChild("shadow");
				if(oldShadow != null) {
					shadow.transform.localScale = oldShadow.localScale;
				}
			}
		}
		// add boss message receiver
		if(_bossComponent) {
			_previewModel.AddComponent<BossMessageReciever>();
		}
		// add hp indicator.
		string UIPath = "Assets/Characters/Common/HP/UI(Monster).prefab";
		if(_uiType == UITypes.Elite)
		{
			UIPath = "Assets/Characters/Common/HP/UI(EliteMonster).prefab";
		}
		else if(_uiType == UITypes.None)
		{
			UIPath = "";
		}

		if(_uiType != UITypes.None)
		{
			GameObject hp = AssetDatabase.LoadAssetAtPath(UIPath, typeof(GameObject)) as GameObject;
			hp = GameObject.Instantiate(hp) as GameObject;
			hp.name = "UI";
			hp.transform.parent = _previewModel.transform;
			// WARN: set position and rotation only, let scale go.
			hp.transform.position = _previewModel.transform.position + Vector3.up * nma.height; // attation: root possibly has rotation, calculate UI in world space.
			hp.transform.localRotation = Quaternion.identity;
			ac._uiHPController = hp.GetComponent<UIHPController>();
		}

		ac._quality = CharacterGraphicsQuality.CharacterGraphicsQuality_Battle;

		// add box collider.
		Rigidbody rb = _previewModel.AddComponent<Rigidbody>();
		rb.interpolation = RigidbodyInterpolation.None;
		rb.useGravity = false;
		rb.isKinematic = true;
		rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
		bool colliderAdded = false;
		BoxCollider bc = null;
		if(oldGo != null) {
			bc = oldGo.GetComponent<BoxCollider>();
		}
		if(bc != null) {
			BoxCollider newBC = _previewModel.AddComponent<BoxCollider>();
			newBC.isTrigger = false;
			newBC.center = bc.center;
			newBC.size = bc.size;
			colliderAdded = true;
		}
		if(!colliderAdded) {
			CapsuleCollider capC = null;
			if(oldGo != null) {
				oldGo.GetComponent<CapsuleCollider>();
			}
			if(capC != null) {
				CapsuleCollider newCapC = _previewModel.AddComponent<CapsuleCollider>();
				newCapC.isTrigger = false;
				newCapC.height = capC.height;
				newCapC.radius = capC.radius;
				newCapC.center = capC.center;
				colliderAdded = true;
			}
		}
		if(!colliderAdded) {
			bc = _previewModel.AddComponent<BoxCollider>();
			bc.isTrigger = false;
			Vector3 center = new Vector3(0.0f, nma.height * 0.5f, 0.0f);
			Vector3 size = new Vector3(nma.radius * 2.0f, nma.height, nma.radius * 2.0f);
			bc.center = center;
			bc.size = size;
		}
		// add foot point.
		GameObject foot_point = new GameObject("node_foot_point");
		foot_point.transform.parent = _previewModel.transform;
		foot_point.transform.localPosition = Vector3.zero;
		foot_point.transform.localRotation = Quaternion.identity;
		foot_point.transform.localScale = Vector3.one;

		// add head point.
		GameObject head_point = new GameObject("node_head_point");
		head_point.transform.parent = _previewModel.transform;
		head_point.transform.localPosition = Vector3.up * 2.0f;
		head_point.transform.localRotation = Quaternion.identity;
		head_point.transform.localScale = Vector3.one;

		//add damage collider child object
		if(_needDamageCollider) {
			GameObject damage = AssetDatabase.LoadAssetAtPath("Assets/Characters/Common/DamageCollider.prefab", typeof(GameObject)) as GameObject;
			damage = GameObject.Instantiate(damage) as GameObject;
			damage.transform.parent = _previewModel.transform;
			damage.transform.localPosition = Vector3.zero;
			damage.transform.localRotation = Quaternion.identity;
			damage.transform.localScale = Vector3.one;
			damage.name = "DamageCollider";
		}



		PrefabUtility.CreatePrefab(path, _previewModel);
	}
}
