using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
public class StaticObjectSpawner : MonoBehaviour
{
	public StaticObjectType type;
	public static string[] StaticObjectPaths = new string[]
	{
		"Assets/Levels/StaticObjects/Prefabs/pottery_01.prefab",//pottery
		"Assets/Levels/StaticObjects/Prefabs/barrel_01.prefab",//barrel
		"Assets/Levels/StaticObjects/Prefabs/crate_01.prefab",//crate
		"Assets/Levels/StaticObjects/Prefabs/chest_01.prefab",//chest
		"Assets/Levels/StaticObjects/Prefabs/gold_chest_01.prefab",//gold chest
	};

#if UNITY_EDITOR
	private GameObject _prefab;	//for each static object type

    private GameObject _hiddenRoot;     //the root for all hidden game objects

    private Dictionary<Transform, Transform> _dict = new Dictionary<Transform, Transform>();    //actual -- hidden

    private const HideFlags k_hide_flag = HideFlags.HideAndDontSave;
#endif

    static int staticObjCount;   //static object counter

#if UNITY_EDITOR
    void Start()
    {
		this.name = type.ToString().ToLower();

        if (!Application.isPlaying)
        {
            InstantiateHiddenPrefabs();
        }
    }
#endif

#if UNITY_EDITOR
    private const string k_hidden_root_name = "Hidden Root";
    /// <summary>
    /// When in Editor, create hidden game objects to show all possibile spawn points.
    /// </summary>
    private void InstantiateHiddenPrefabs()
    {
        if (!_hiddenRoot)
        {
            _hiddenRoot = EditorUtility.CreateGameObjectWithHideFlags(k_hidden_root_name + '_' + this.name, k_hide_flag);
        }

        if (!_prefab)
        {
			string prefabPath = StaticObjectPaths[(int)type];

            _prefab = AssetDatabase.LoadMainAssetAtPath(prefabPath) as GameObject;
        }

        foreach (Transform t in transform)
        {
            AddHiddenChild(t);
        }
    }

    private void AddHiddenChild(Transform spawnPointTrans)
    {
        if (_prefab)
        {
            GameObject go = PrefabUtility.InstantiatePrefab(_prefab) as GameObject;

            go.transform.parent = _hiddenRoot.transform;
            go.transform.position = spawnPointTrans.position;
            go.transform.rotation = spawnPointTrans.rotation;
            go.transform.localScale = spawnPointTrans.localScale;
            go.hideFlags = k_hide_flag;
            go.name = spawnPointTrans.name + "  hidden";

            _dict.Add(spawnPointTrans, go.transform);

            //set hide flags of child nodes
            Transform[] children = go.GetComponentsInChildren<Transform>();
            foreach (Transform t in children)
            {
                t.gameObject.hideFlags = k_hide_flag;
            }

        }
        else
        {
            Debug.LogWarning(string.Format("[StaticObjectSpawner] Prefab not found {0}. Failed to add hidden child for {1}.", type.ToString().ToLower(), spawnPointTrans.name));
        }
    }
#endif

    /// <summary>
    /// Randomly choose one of the spots to instantiate the prefabs by asset paths.
    /// </summary>
	public void InstantiateNormalPrefab()
	{
		if (!LevelManager.Singleton.staticObjMapping.ContainsKey(type))
		{
			Destroy(this.gameObject);
			return;
		}

		List<StaticObjectInfo> list = LevelManager.Singleton.staticObjMapping[type];

		string prefabPath = StaticObjectPaths[(int)type];

		GameObject prefab = InJoy.AssetBundles.AssetBundles.Load(prefabPath) as GameObject;

		if (prefab != null)
		{
			Transform[] spots = GetComponentsInChildren<Transform>();

			List<Transform> availableSpots = new List<Transform>(spots);

			availableSpots.Remove(this.transform);

			foreach (StaticObjectInfo info in list)
			{
				if (availableSpots.Count == 0)
				{
					Debug.LogError(string.Format("[Static object spawner] There are more {0} than needed. Ignored.", type));

					break;
				}
				//Randomly select one spawn point to instantiate
				Transform selectedSpot = availableSpots[Random.Range(0, availableSpots.Count)];

				GameObject go = UnityEngine.Object.Instantiate(prefab) as GameObject;
				go.transform.parent = this.transform.parent;
				go.transform.position = selectedSpot.position;
				go.transform.rotation = selectedSpot.rotation;
				go.transform.localScale = selectedSpot.localScale;
				go.name = this.name + "--" + selectedSpot.name + "_" + staticObjCount.ToString("D2");

				staticObjCount++;

				go.GetComponentInChildren<NavMeshObstacle>().enabled = false;

				go.GetComponentInChildren<StaticObjectLoot>().lootTable = info.lootTable;

				availableSpots.Remove(selectedSpot);
			}
		}
		Destroy(this.gameObject);
	}

#if UNITY_EDITOR
    //Run in Editor only
    void Update()
    {
        if (!Application.isPlaying)
        {
            //add new nodes
            foreach (Transform t in transform)
            {
                if (!_dict.ContainsKey(t))
                {
                    AddHiddenChild(t);
                }
            }

            //remove redundant nodes
            List<Transform> list = new List<Transform>();
            foreach (Transform t in _dict.Keys)
            {
                bool found = false;
                foreach (Transform tt in transform)
                {
                    if (t == tt)
                    {
                        found = true;
                    }
                }
                if (!found)
                {
                    DestroyImmediate(_dict[t].gameObject);
                    list.Add(t);
                }
            }

            foreach (Transform t in list)
            {
                _dict.Remove(t);
            }

            //align the spawn points and display objects
            foreach (KeyValuePair<Transform, Transform> kv in _dict)
            {
                if (kv.Key.position != kv.Value.position)
                {
                    kv.Value.position = kv.Key.position;
                }
                if (kv.Key.rotation != kv.Value.rotation)
                {
                    kv.Value.rotation = kv.Key.rotation;
                }
                if (kv.Key.localScale != kv.Value.localScale)
                {
                    kv.Value.localScale = kv.Key.localScale;
                }
            }
        }
    }
#endif

#if UNITY_EDITOR
	void OnDestroy()
	{
		if (!Application.isPlaying)
		{
			if (_hiddenRoot)
			{
				DestroyImmediate(_hiddenRoot);
			}
			
			if (_dict != null)
			{
				_dict.Clear();
				_dict = null;
			}
		}
	}
#endif
}

public struct StaticObjectInfo
{
	public StaticObjectType type;

	public List<LootObjData> lootTable;
}