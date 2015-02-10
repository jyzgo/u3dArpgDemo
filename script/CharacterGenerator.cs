using UnityEditor;
using UnityEngine;
using System.Collections;

public class CharacterGenerator : ScriptableWizard
{
    public enum CharacterMode
    {
        ForBattle = 0,
        ForTown,
        ForPreview
    }
    public string _characterName;
    public CharacterMode _mode;
    public bool _checkStandardModel = true;
    public bool _checkHighModel;
    public bool _checkLowModel;
    // running every 100 frames.
    void Update()
    {
        // check if the selected item is in character's folder.
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        isValid = path.StartsWith(CharacterImporter._characterRoot);
        if (isValid)
        {
            foreach (string s in CharacterImporter._ignorePath)
            {
                if (path.StartsWith(s))
                {
                    isValid = false;
                    break;
                }
            }
        }
        helpString = (isValid ? "" : "please select folders in \'Assets/Characters/\'");
    }

    void OnWizardCreate()
    {
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        string[] groups = path.Split('/');
        // check ilegal path depth.
        if (groups.Length < 4 || groups.Length > 5)
        {
            Debug.LogError(path + ": it's not an invalid path!");
            return;
        }
        // find fbx file.
        string fbxpath = groups[0] + '/' + groups[1] + '/' + groups[2] + '/' + groups[3] + '/' + groups[3] + ".fbx";
        string prefabpath = groups[0] + '/' + groups[1] + '/' + groups[2] + '/' + groups[3] + '/' + groups[3] + '_' + _characterName + ".prefab";
        string builderpath = groups[0] + '/' + groups[1] + '/' + groups[2] + '/' + groups[3] + '/' + groups[3] + "_atlas.fbx";
        // create character based on fbx
        if (_checkStandardModel)
        {
            GameObject go = CreateObjectByPath(fbxpath, prefabpath, builderpath, 1);
            if (go == null)
            {
                Debug.LogError("failed to create " + prefabpath);
                return;
            }
            Selection.activeObject = go;
        }
        // check low model.
        if (_checkLowModel)
        {
            fbxpath = groups[0] + '/' + groups[1] + '/' + groups[2] + '/' + groups[3] + '/' + groups[3] + "_low.fbx";
            prefabpath = groups[0] + '/' + groups[1] + '/' + groups[2] + '/' + groups[3] + '/' + groups[3] + '_' + _characterName + ".prefab";
            GameObject go = CreateObjectByPath(fbxpath, prefabpath, builderpath, 0);
            if (go == null)
            {
                Debug.LogError("failed to create " + prefabpath);
                return;
            }
        }
        // check high model.
        if (_checkHighModel)
        {
            fbxpath = groups[0] + '/' + groups[1] + '/' + groups[2] + '/' + groups[3] + '/' + groups[3] + "_high.fbx";
            prefabpath = groups[0] + '/' + groups[1] + '/' + groups[2] + '/' + groups[3] + '/' + groups[3] + '_' + _characterName + ".prefab";
            GameObject go = CreateObjectByPath(fbxpath, prefabpath, builderpath, 2);
            if (go == null)
            {
                Debug.LogError("failed to create " + prefabpath);
                return;
            }
        }
    }


    // lod: 0 for battle, 1 for town, 2 for preview
    GameObject CreateObjectByPath(string fbxpath, string prefabpath, string builderpath, int lod)
    {
        GameObject go = AssetDatabase.LoadAssetAtPath(fbxpath, typeof(GameObject)) as GameObject;
        if (go == null)
        {
            Debug.LogWarning(fbxpath + ": can not find this file!");
            return null;
        }

        // assemble character.
        go = GameObject.Instantiate(go) as GameObject;
        AvatarController ac = go.AddComponent<AvatarController>();
        Animator animator = go.GetComponent<Animator>();

        // fill appropriate parameters.
        if (animator != null)
        {
            //animator.animatePhysics = false;
            animator.updateMode = AnimatorUpdateMode.Normal;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            RuntimeAnimatorController rac = AssetDatabase.LoadAssetAtPath(prefabpath.Replace(".prefab", ".controller"), typeof(RuntimeAnimatorController)) as RuntimeAnimatorController;
            if (rac != null)
            {
                animator.runtimeAnimatorController = rac;
            }
        }

        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
        ac._characterRenderers = renderers;

        // create material builder.
        GameObject builderobj = AssetDatabase.LoadAssetAtPath(builderpath, typeof(GameObject)) as GameObject;
        MaterialBuilder builder = null;
        if (builderobj != null)
        {
            // found builder model, get builder from this model.
            builder = builderobj.GetComponent<ModifiableMaterialBuilder>();
        }
        if (builderobj == null)
        {
            // can not find builder from model, create one and attach it to prefab
            builderobj = Utils.NewGameObjectWithParent("SimpleMaterialBuilder", go.transform).gameObject;
            builder = builderobj.AddComponent<SimpleMaterialBuilder>();
        }
        ac._materialBuilder = builder;
        // create material.
        string matPath = prefabpath.Replace(".prefab", ".mat");
        Material mat = null;
        if (!System.IO.File.Exists(matPath))
        {
            Shader[] shaders = new Shader[]{ // shader is depend on lod
				Shader.Find("InJoy/character/common"), Shader.Find("InJoy/character/common"), Shader.Find("InJoy/character/preview")};
            mat = new Material(shaders[(int)_mode]);
            AssetDatabase.CreateAsset(mat, prefabpath.Replace(".prefab", ".mat"));
        }
        mat = AssetDatabase.LoadAssetAtPath(prefabpath.Replace(".prefab", ".mat"), typeof(Material)) as Material;
        ac._baseMaterial = mat;
        // init material setting of mesh renderers.
        foreach (Renderer r in ac._characterRenderers)
        {
            r.sharedMaterial = mat;
        }
        // for battle, town, and preview. additional process will be applyed seperately.
        OptionalProcess[] processes = new OptionalProcess[3] { new BattleProcess(), new TownProcess(), new PreviewProcess() };
        GameObject oldGo = AssetDatabase.LoadAssetAtPath(prefabpath, typeof(GameObject)) as GameObject;
        processes[(int)_mode].Process(go, oldGo);
        // save prefab
        GameObject ret = PrefabUtility.CreatePrefab(prefabpath, go);
        // end-up
        DestroyImmediate(go);
        return ret;
    }

    [MenuItem("Tools/Character/Create Character Wizard")]
    static void CreateCharacter()
    {
        ScriptableWizard.DisplayWizard("Create Character", typeof(CharacterGenerator), "Create");
    }

    public class OptionalProcess
    {
        public virtual void Process(GameObject go, GameObject oldGo) { }
    }
    public class BattleProcess : OptionalProcess
    {
        public override void Process(GameObject go, GameObject oldGo)
        {
            AvatarController ac = go.GetComponent<AvatarController>();
            // NavMesh settings.
            NavMeshAgent nma = go.AddComponent<NavMeshAgent>();
            Bounds b = ac._characterRenderers[0].bounds;
            foreach (Renderer r in ac._characterRenderers)
            {
                b.Encapsulate(r.bounds.max);
                b.Encapsulate(r.bounds.min);
            }
            nma.height = b.size.y;
            nma.radius = (b.extents.x + b.extents.z) * 0.5f;
            nma.baseOffset = 0.0f;
            nma.avoidancePriority = 0;
            NavMeshAgent oldNma = null;
            if (oldGo != null)
            {
                oldNma = oldGo.GetComponent<NavMeshAgent>();
            }
            if (oldNma != null)
            {
                nma.radius = oldNma.radius;
                nma.height = oldNma.height;
            }
            // add hp indicator.
            string UIPath = "Assets/Characters/Common/HP/UI(Player).prefab";
            if (ac._materialBuilder is SimpleMaterialBuilder)
            {
                UIPath = "Assets/Characters/Common/HP/UI(Monster).prefab";
            }
            GameObject hp = AssetDatabase.LoadAssetAtPath(UIPath, typeof(GameObject)) as GameObject;
            hp = GameObject.Instantiate(hp) as GameObject;
            hp.name = "UI";
            hp.transform.parent = go.transform;
            // WARN: set position and rotation only, let scale go.
            hp.transform.position = go.transform.position + Vector3.up * nma.height; // attation: root possibly has rotation, calculate UI in world space.
            hp.transform.localRotation = Quaternion.identity;
            ac._uiHPController = hp.GetComponent<UIHPController>();

            // add point light.
            GameObject light = AssetDatabase.LoadAssetAtPath("Assets/Characters/Common/halo.prefab", typeof(GameObject)) as GameObject;
            light = GameObject.Instantiate(light) as GameObject;
            light.name = "SpotLight";
            light.transform.parent = go.transform;
            light.transform.localPosition = Vector3.up * -1.5f;
            light.transform.localRotation = Quaternion.identity;

            ac._quality = CharacterGraphicsQuality.CharacterGraphicsQuality_Battle;

            // add box collider.
            Rigidbody rb = go.AddComponent<Rigidbody>();
            rb.interpolation = RigidbodyInterpolation.None;
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
            CapsuleCollider col = go.AddComponent<CapsuleCollider>();
            col.isTrigger = true;
            Vector3 center = new Vector3(0.0f, nma.height * 0.5f, 0.0f);
            float height = nma.height;
            float radius = nma.radius;
            CapsuleCollider oldColider = null;
            if (oldGo != null)
            {
                oldColider = oldGo.GetComponent<CapsuleCollider>();
            }
            if (oldColider != null)
            {
                center = oldColider.center;
                height = oldColider.height;
                radius = oldColider.radius;
            }
            col.center = center;
            col.height = height;
            col.radius = radius;

            // add foot point.
            GameObject foot_point = new GameObject("node_foot_point");
            foot_point.transform.parent = go.transform;
            foot_point.transform.localPosition = Vector3.zero;
            foot_point.transform.localRotation = Quaternion.identity;
            foot_point.transform.localScale = Vector3.one;

            // add head point.
            GameObject head_point = new GameObject("node_head_point");
            head_point.transform.parent = go.transform;
            head_point.transform.localPosition = Vector3.up * 2.0f;
            head_point.transform.localRotation = Quaternion.identity;
            head_point.transform.localScale = Vector3.one;

            // add Time Scale Listener
            go.AddComponent<TimeScaleListener>();

            // keep character controller.
            CharacterController cc = null;
            if (oldGo != null)
            {
                oldGo.GetComponent<CharacterController>();
            }
            if (cc != null)
            {
                CharacterController newCC = go.AddComponent<CharacterController>();
                newCC.slopeLimit = cc.slopeLimit;
                newCC.stepOffset = cc.stepOffset;
                newCC.radius = cc.radius;
                newCC.height = cc.height;
                newCC.center = cc.center;
            }
        }
    }
    public class TownProcess : OptionalProcess
    {
        public override void Process(GameObject go, GameObject oldGo)
        {
            AvatarController ac = go.GetComponent<AvatarController>();
            ac._allowPetAssembled = true;
            ac._quality = CharacterGraphicsQuality.CharacterGraphicsQuality_Town;
            // NavMesh settings.
            NavMeshAgent nma = go.AddComponent<NavMeshAgent>();
            Bounds b = ac._characterRenderers[0].bounds;
            foreach (Renderer r in ac._characterRenderers)
            {
                b.Encapsulate(r.bounds.max);
                b.Encapsulate(r.bounds.min);
            }
            nma.height = b.size.y;
            nma.radius = (b.extents.x + b.extents.z) * 0.5f;
            nma.baseOffset = 0.0f;
            nma.avoidancePriority = 0;
            NavMeshAgent oldNma = null;
            if (oldGo != null)
            {
                oldNma = oldGo.GetComponent<NavMeshAgent>();
            }
            if (oldNma != null)
            {
                nma.radius = oldNma.radius;
                nma.height = oldNma.height;
            }
            // add box collider.
            Rigidbody rb = go.AddComponent<Rigidbody>();
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;

            BoxCollider collider = go.AddComponent<BoxCollider>();
            collider.center = Vector3.up;
            collider.size = new Vector3(1.5f, 2.0f, 1.5f);
            collider.isTrigger = true;

            Transform transCollider = Utils.NewGameObjectWithParent("TapEvent", go.transform);
            transCollider.gameObject.layer = LayerMask.NameToLayer("2DUI");
            transCollider.LookAt(transCollider.position + Vector3.forward, Vector3.up);

            collider = transCollider.gameObject.AddComponent<BoxCollider>();
            collider.center = Vector3.up;
            collider.size = new Vector3(1.5f, 2.0f, 1.5f);
            collider.isTrigger = true;
            transCollider.gameObject.AddComponent<OnTapEvent>();

            // add point light.
            GameObject light = AssetDatabase.LoadAssetAtPath("Assets/Characters/Common/halo.prefab", typeof(GameObject)) as GameObject;
            light = GameObject.Instantiate(light) as GameObject;
            light.name = "SpotLight";
            light.transform.parent = go.transform;
            light.transform.localPosition = Vector3.up * -1.5f;
            light.transform.localRotation = Quaternion.identity;

            // add hp indicator.
            GameObject hp = AssetDatabase.LoadAssetAtPath("Assets/Characters/Common/HP/UI(TownPlayer).prefab", typeof(GameObject)) as GameObject;
            hp = GameObject.Instantiate(hp) as GameObject;
            hp.name = "UI";
            hp.transform.parent = go.transform;
            // WARN: set position and rotation only, let scale go.
            hp.transform.position = go.transform.position + Vector3.up * nma.height; // attation: root possibly has rotation, calculate UI in world space.
            hp.transform.localRotation = Quaternion.identity;
            ac._uiHPController = hp.GetComponent<UIHPController>();

            // keep character controller.
            if (oldGo != null)
            {
                CharacterController cc = oldGo.GetComponent<CharacterController>();
                if (cc != null)
                {
                    CharacterController newCC = go.AddComponent<CharacterController>();
                    newCC.slopeLimit = cc.slopeLimit;
                    newCC.stepOffset = cc.stepOffset;
                    newCC.radius = cc.radius;
                    newCC.height = cc.height;
                    newCC.center = cc.center;
                }
            }
        }
    }

    public class PreviewProcess : OptionalProcess
    {
        public override void Process(GameObject go, GameObject oldGo)
        {
            AvatarController ac = go.GetComponent<AvatarController>();
            ac._quality = CharacterGraphicsQuality.CharacterGraphicsQuality_Preview;

            // add equipment root.
            GameObject equipRoot = new GameObject("EquipmentRoot");
            equipRoot.transform.parent = go.transform;
            equipRoot.transform.localPosition = Vector3.zero;
            equipRoot.transform.localRotation = Quaternion.identity;
            equipRoot.transform.localScale = Vector3.one;

            // add mesh unloader
            System.Collections.Generic.List<Mesh> meshes = new System.Collections.Generic.List<Mesh>();
            MeshUnloader mu = go.AddComponent<MeshUnloader>();
            SkinnedMeshRenderer[] srenderers = go.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer smr in srenderers)
            {
                meshes.Add(smr.sharedMesh);
            }
            MeshFilter[] filters = go.GetComponentsInChildren<MeshFilter>();
            foreach (MeshFilter mf in filters)
            {
                meshes.Add(mf.sharedMesh);
            }
            mu._meshes = meshes.ToArray();
        }
    }

}
