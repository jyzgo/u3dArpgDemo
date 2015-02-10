using UnityEditor;
using UnityEngine;
using System.Collections;

public class CustomNaviMeshBuilder
{
    private const string k_helper_root_name = "NaviMesh Helper Root";

    public const string k_level_root_name = "level_root";

    public const string k_env_root_name = "env_root";

    public const string k_blockade_root_name = "blockades";

    private const int k_navLayer_not_walkable_for_monster = 3;  //defined in NavMeshLayer

    private static Material _defaultMat;

    [MenuItem("Tools/Navimesh Generator")]
    static void GenerateNavimesh()
    {
        PreProcessScene();

        BuildNaviMesh();

        PostProcessScene();
    }

    /// <summary>
    /// Add extra mesh renderers for navimesh creation to the current scene
    /// </summary>
    static void PreProcessScene()
    {
        if (_defaultMat == null)
        {
            _defaultMat = new Material(Shader.Find("Diffuse"));
        }

        PreprocessGround();

        PreprocessBlockades();
    }

    private static void PreprocessGround()
    {
        GameObject root = GameObject.Find(k_level_root_name + '/' + k_env_root_name);
        
        Assertion.Check(root != null, "Environment root not found!");

        GameObject helperRoot = GameObject.Find(k_helper_root_name);

        if (helperRoot == null)
        {
            helperRoot = EditorUtility.CreateGameObjectWithHideFlags(k_helper_root_name, HideFlags.DontSave);
        }

        Collider[] colliders = root.transform.GetComponentsInChildren<Collider>();

        Debug.Log("Colliders found: " + colliders.Length);

        foreach (Collider c in colliders)
        {
            Debug.Log("Collider name = " + c.name);
            GameObject go;

            //deal with box colliders that represent ground and gates. Create an equal-sized cub render for them
            if (c is BoxCollider)
            {
                go = GameObject.CreatePrimitive(PrimitiveType.Cube);

                Transform t = go.transform;
                t.position = c.transform.position + (c as BoxCollider).center;
                t.rotation = c.transform.rotation;
                t.localScale = (c as BoxCollider).size;
            }
            else //deal with mesh colliders, e.g. walls. Create equal mesh renderers for them
            {
                go = Utils.NewGameObjectWithParent("").gameObject;
                MeshFilter meshFilter = go.AddComponent<MeshFilter>();
                meshFilter.sharedMesh = c.transform.GetComponent<MeshFilter>().sharedMesh;

                MeshRenderer renderer = go.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = _defaultMat;
            }

            go.transform.parent = helperRoot.transform;
            go.transform.position = c.gameObject.transform.position;
            go.transform.rotation = c.gameObject.transform.rotation;
            go.name = c.name;
            go.hideFlags = HideFlags.DontSave;

            GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.NavigationStatic);
        }
    }

    //deal with blockades that monster cannot go through
    private static void PreprocessBlockades()
    {
        GameObject root = GameObject.Find(k_level_root_name + '/' + k_blockade_root_name);
       
        Assertion.Check(root != null, "Blockade root not found!");

        GameObject helperRoot = GameObject.Find(k_helper_root_name);

        if (helperRoot == null)
        {
            helperRoot = EditorUtility.CreateGameObjectWithHideFlags(k_helper_root_name, HideFlags.DontSave);
        }

        Collider[] colliders = root.transform.GetComponentsInChildren<BoxCollider>();

        Debug.Log("Blockade Colliders found: " + colliders.Length);

        foreach (Collider c in colliders)
        {
            Debug.Log("Collider name = " + c.name);
            GameObject go;

            //deal with box colliders that represent blockades. Create an equal-sized cub render for them
            if (c.gameObject.name == "NavMeshColider")
            {
                go = GameObject.CreatePrimitive(PrimitiveType.Cube);

                Transform t = go.transform;
                t.position = c.transform.position + (c as BoxCollider).center;
                t.rotation = c.transform.rotation;
                t.localScale = (c as BoxCollider).size;

                go.transform.parent = helperRoot.transform;
                go.transform.position = c.gameObject.transform.position;
                go.transform.rotation = c.gameObject.transform.rotation;
                go.name = c.name;
                go.hideFlags = HideFlags.DontSave;

                GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.NavigationStatic);

                GameObjectUtility.SetNavMeshLayer(go, k_navLayer_not_walkable_for_monster);
            }
        }
    }

    static void BuildNaviMesh()
    {
        if (NavMeshBuilder.isRunning)
        {
            Debug.LogWarning("NaviMesh builder is already running! Aborted.");
            return;
        }

        Debug.Log("Start building NaviMesh...");

        NavMeshBuilder.ClearAllNavMeshes();
        
        NavMeshBuilder.BuildNavMesh();

        Debug.Log("NaviMesh building finished. Saving scene...");

        EditorApplication.SaveScene();
    }

    /// <summary>
    /// Remove the added game objects for navimesh generation
    /// </summary>
    static void PostProcessScene()
    {
        Debug.Log("Clearing temp objects.");
        GameObject helperRoot = GameObject.Find(k_helper_root_name);
        if (helperRoot)
        {
            GameObject.DestroyImmediate(helperRoot);
        }

        if (_defaultMat)
        {
            Object.DestroyImmediate(_defaultMat);
        }
        Debug.Log("Temp objects cleared.");
    }
}
