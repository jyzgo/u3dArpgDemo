using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class EditorUtils{

	public static T CreateScriptable<T>() where T: ScriptableObject
	{
		T newScriptable = ScriptableObject.CreateInstance<T>();
		string path = AssetDatabase.GetAssetPath(Selection.activeObject);
		if(path.Length == 0)
		{
			path = "Assets/";
		}
		string className = typeof(T).Name;
		path = AssetDatabase.GenerateUniqueAssetPath(path+"/new"+className+".asset");
		AssetDatabase.CreateAsset(newScriptable,path);
		return newScriptable;
	}
	
	
	[MenuItem("Tools/Create Data Asset/DynamicConst")]
	public static void CreateDynamicConst()
	{
		EditorUtils.CreateScriptable<DynamicConst>();
	}
	
	[MenuItem("Tools/Create Data Asset/Actor DataList")]
	public static void CreateAcDataList()
	{
		EditorUtils.CreateScriptable<AcDataList>();
	}
	
	[MenuItem("Tools/Create Data Asset/Tutorial/TutorialLevel")]
	public static void CreateTutorialLevel()
	{
		EditorUtils.CreateScriptable<TutorialLevelList>();
	}
	
	[MenuItem("Tools/Create Data Asset/Tutorial/TutorialTown")]
	public static void CreateTutorialTown()
	{
		EditorUtils.CreateScriptable<TutorialTownList>();
	}
	
	
	[MenuItem("Tools/Create Data Asset/Actor PrefabList")]
	public static void CreateAcPrefabList()
	{
		EditorUtils.CreateScriptable<AcPrefabList>();
	}
	
	[MenuItem("Tools/Create Data Asset/SkillUpgradeData")]
	public static void CreateSkillUpgradeData()
	{
		EditorUtils.CreateScriptable<SkillUpgradeDataList>();
	}
	
	[MenuItem("Tools/Create Data Asset/InitInformation")]
	public static void CreateInitInformation()
	{
		EditorUtils.CreateScriptable<InitInformation>();
	}
	
	[MenuItem("Tools/Sound/Create SoundClipList")]
	public static void CreateSoundClipList()
	{
		EditorUtils.CreateScriptable<SoundClipList>();
	}
	
	[MenuItem("Tools/Create Data Asset/Create Equipment Upgrade Data")]
	public static void CreateUpgradeConfigData()
	{
		EditorUtils.CreateScriptable<UpgradeConfigData>();
	}
	
	[MenuItem("Tools/Create Data Asset/Create StoreDataList")]
	public static void CreateStoreDataList()
	{
		EditorUtils.CreateScriptable<StoreDataList>();
	}
	
	[MenuItem("Tools/Create Data Asset/Create StoreDataConfig")]
	public static void CreateStoreDataConfig()
	{
		EditorUtils.CreateScriptable<StoreDataConfig>();
	}

	
	
	[MenuItem("Tools/Sound/Update SoundClipList")]
	public static void UpdateSoundClipList()
	{
		string path = "Assets/Data/Sound/SoundClipList.asset";
		SoundClipList list = AssetDatabase.LoadAssetAtPath(path, typeof(SoundClipList)) as SoundClipList;
		foreach(SoundClip sc in list._soundList)
		{
			string scPath = AssetDatabase.GetAssetPath(sc.Clip);
			sc._clipPath = scPath.Substring(16);
			Debug.Log(sc._clipPath);
		}
		EditorUtility.SetDirty(list);	
	}
	

    [MenuItem("Tools/Helper/Get asset name", false, 4)]
    static void HelperGetAssetName()
    {
        Debug.Log(AssetDatabase.GetAssetPath(Selection.activeObject));
    }
	
	[MenuItem("Tools/Helper/Create AnimationInfo", false, 4)]
    static void CreateAnimationInfo()
    {
       EditorUtils.CreateScriptable<FCAnimationInfo>();
    }

    /// <summary>
    /// Check the dependencies of a folder to see if they are outside the folder.
    /// </summary>
    [MenuItem("Tools/Pwt/check external dependencies", false, 5)]
    public static void CheckExternalDependencies()
    {
        if (Selection.activeObject)
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (Directory.Exists(path))
            {
                CheckFolderDependency(path);
            }
        }
    }

    /// <summary>
    /// Find the dependent external files of a folder, used to ensure the resources under a folder is self-content.
    /// </summary>
    /// <param name="folderName"></param>
    /// <returns></returns>
    private static bool CheckFolderDependency(string folderName)
    {
        Debug.Log("===Folder dependency check started. Folder = " + folderName);

        List<string> externalPathList = new List<string>();

        string[] assetFiles = Directory.GetFiles(folderName, "*.*", SearchOption.AllDirectories);

        Debug.Log("Files under folder: " + assetFiles.Length);

        string[] dependentFiles = AssetDatabase.GetDependencies(assetFiles);

        Debug.Log("Dependencies found: " + dependentFiles.Length);

        foreach (string objPath in dependentFiles)
        {
            if (!objPath.StartsWith(folderName) &&
                !objPath.EndsWith(".cs") &&
                !objPath.StartsWith("Assets/Shaders")
                )
            {
                externalPathList.Add(objPath);
            }
        }

        if (externalPathList.Count > 0)
        {
            Debug.LogError("External assets found: " + externalPathList.Count);
            foreach (string s in externalPathList)
            {
                Debug.Log("\t\t" + s);
            }
        }
        else
        {
            Debug.Log("No external assets found. Great!");
        }

        Debug.Log("===Folder dependency check done. Folder = " + folderName);

        return externalPathList.Count == 0;
    }

    #region check the components of scenes
    /// <summary>
    /// List all the component type names of the current scene and write to a list
    /// </summary>
    /// <returns>a name list of user component names</returns>
    private static List<string> GetComponentsOfCurrentScene()
    {
        GameObject[] goAll = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];

        List<string> nameList = new List<string>();

        foreach (GameObject go in goAll)
        {
            Component[] components = go.GetComponents<Component>();
            foreach (Component component in components)
            {
                string typeName = component.GetType().ToString();
                if (!typeName.StartsWith("UnityEngine.") && !nameList.Contains(typeName))
                {
                    nameList.Add(typeName);
                }
            }
        }
        nameList.Sort();
        return nameList;
    }

    /// <summary>
    /// enumerate all the user component names of the scenes under specified folders.
    /// </summary>
    [MenuItem("Tools/Check components of scenes")]
    static void GetComponentsOfScenes()
    {
        string[] folders_to_scan = new string[]
        {
            "Assets/Levels"
        };

        string[] scenes_to_skip = new string[]
        {
            "Assets/Levels/scene-to-skip.unity"
        };

        List<string> skipList = new List<string>(scenes_to_skip);

        //remember the current scene
        string savedSceneName = EditorApplication.currentScene;

        EditorApplication.SaveCurrentSceneIfUserWantsTo();

        List<string> list = new List<string>();
		List<string> sceneNameList = new List<string>();

        foreach (string dir in folders_to_scan)
        {
            Debug.Log("Processing scene folder -- " + dir);
            foreach (string sceneName in Directory.GetFiles(dir, "*.unity", SearchOption.AllDirectories))
            {
                string sceneName2 = sceneName.Replace("\\", "/");
                if (!skipList.Contains(sceneName2))
                {
                    Debug.Log("\t\tProcessing scene -- " + sceneName);
                    EditorApplication.OpenScene(sceneName);
                    list.Add("User components of scene: " + sceneName);
					sceneNameList.Add(sceneName);

                    List<string> typeNames = GetComponentsOfCurrentScene();

                    foreach (string typeName in typeNames)
                    {
                        list.Add("\t\t" + typeName);
                    }
                }
                else
                {
                    Debug.LogWarning("\t\tSkipped!! scene -- " + sceneName);
                }
            }
        }

        using (StreamWriter sw = new StreamWriter("user_components_in_scenes.txt"))
        {
            foreach (string s in list)
            {
                sw.WriteLine(s);
            }
			sw.WriteLine("");
			foreach (string s in sceneNameList)
			{
				sw.WriteLine(s);
			}
        }

        Debug.Log("All scene folders processed. Log file written to user_components_in_scenes.txt");

        EditorApplication.OpenScene(savedSceneName);
    }
    #endregion

	#region check if any illegale scripts in scenes
	static string[] scriptNames=
	{
		"Blockade",
		"BornPoint",
		"EnemySpot",
		"LevelLightInfo",
		"SoundTrigger",
		"StaticObjectSpawner",
		"TriggerManager",
		"FrameAnimation",
		"FeatureCamera"
	};
	
	static string[] skipScenes=
	{
		"Assets/Levels/Global/Scenes/character_selection.unity",
		"Assets/Levels/Global/Scenes/town.unity",
		"Assets/Levels/scene-to-skip.unity",
		"Assets/Levels/Greece/Scenes/test_ai_sandbox.unity"
	};
	
	static string LevelFolder = "Assets/Levels";
	
	[MenuItem("Tools/Check scripts of scenes")]
	public static void CheckScriptsOfScenes()
	{
		System.Console.WriteLine("Begin check scripts");
        //remember the current scene
        string savedSceneName = EditorApplication.currentScene;

        EditorApplication.SaveCurrentSceneIfUserWantsTo();

        List<string> list = new List<string>();
		List<string> skipScenesList = new List<string>(skipScenes);
		List<string> scriptList = new List<string>(scriptNames);
		
        foreach (string sceneName in Directory.GetFiles(LevelFolder, "*.unity", SearchOption.AllDirectories))
        {
			System.Console.WriteLine("Check Scene : " + sceneName);
            string sceneName2 = sceneName.Replace("\\", "/");
            if (!skipScenesList.Contains(sceneName2))
			{
	         	EditorApplication.OpenScene(sceneName2);
	
	            List<string> typeNames = GetComponentsOfCurrentScene();
				foreach(string curTypeName in typeNames)
				{
					if(!scriptList.Contains(curTypeName))
					{
						string errorInfo = "ERROR! Found Illegal Script: " + curTypeName + " In Scene: " + sceneName2;
						list.Add(errorInfo);
					}
				}
			}
    	}
		
		foreach(string errorInfo in list)
		{
        	System.Console.WriteLine(errorInfo);
		}
		System.Console.WriteLine("Check Scripts Finished!");

        EditorApplication.OpenScene(savedSceneName);
	}
	
	#endregion
}
