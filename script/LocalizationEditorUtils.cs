using UnityEngine;
using UnityEditor;


public static class LocalizationEditorUtils
{
	public static T CreateScriptable<T>() where T: ScriptableObject
	{
		T newScriptable = ScriptableObject.CreateInstance<T>();
		string path = AssetDatabase.GetAssetPath(Selection.activeObject);

		if(path.Length == 0)
		{
			Debug.LogError("didn't select a folder from \'Project View\'");
			return null;
		}
		
		if(System.IO.File.Exists(path))
		{
			path = System.IO.Path.GetDirectoryName(path);
		}
		
		string className = typeof(T).Name;
		path = AssetDatabase.GenerateUniqueAssetPath(path + "/new" + className+".asset");

		AssetDatabase.CreateAsset(newScriptable, path);
		
		return newScriptable;
	}
	
//	//[MenuItem("Tools/Localization/EZGUI-Create Language Data Set")]
//	public static void CreateLanguageDataSet()
//	{
//		LocalizationEditorUtils.CreateScriptable<LanguageDataSet>();
//	}
	
	[MenuItem("Tools/Localization/Create LocalizationFontDataSet")]
	public static void CreateLanguageFontSet()
	{
		LocalizationEditorUtils.CreateScriptable<LocalizationFontDataSet>();
	}
	
	[MenuItem("Tools/Localization/Create LocalizationFontConfig")]
	public static void CreateFontsConfig()
	{
		LocalizationEditorUtils.CreateScriptable<LocalizationFontConfig>();
	}
	
	/// <summary>
	/// Creates the directory if not exist.
	/// </summary>
	/// <returns>
	/// The directory if not exist.
	/// </returns>
	/// <param name='list'>
	/// List.
	/// </param>
	public static  string CreateDirectoryIfNotExist(params string[] list)
	{
		string dir = "";
		
		foreach (string sub in list)
		{
			dir = System.IO.Path.Combine(dir, sub);
		}
		
		try
		{		
			if (!System.IO.Directory.Exists(dir))
			{
				System.IO.Directory.CreateDirectory(dir);
			}
		}
		catch (System.Exception e)
		{
			Debug.LogWarning("Localization: " + e.ToString());
		}
		
		return dir;
	}
}

