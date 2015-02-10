using System;
using UnityEngine;
using UnityEditor;

using System.Collections.Generic;
using System.IO;

/// <summary>
/// Multi language asset post processor.
/// 
/// If Languages.xml was modified, the language database and dictionary file will be regenerated.
/// </summary>
public class SplashTextImporter : AssetPostprocessor 
{
	public enum GUI_PLUGIN_TYPE
	{
		EZGUI = 0,
		NGUI = 1,
	}
	
	public static readonly string MULTILANGUAGE_XML_FOLDER = "Assets/Data/Localization/TextRes";
	public static readonly string MULTILANGUAGE_ASSETS_FOLDER = "Assets/Resources/SplashText";
	public static readonly string MULTILANGUAGE_COMMON_FOLDER = "Assets/Data/Localization/Common";
	public static readonly string MULTILANGUAGE_LANGUAGESXML = "SplashText.xml";
	
	private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, 
	                                           string[] movedAssets, string[] movedFromPath)
	{
		if (CheckResModified(importedAssets) || CheckResModified(deletedAssets) || CheckResModified(movedAssets))
		{
			ImportLanguage();
		}
	}
	
	
	[MenuItem("Tools/Localization/Import Multilanguage")]
	public static void ImportLanguage()
	{
			string path = System.IO.Path.Combine(MULTILANGUAGE_XML_FOLDER, MULTILANGUAGE_LANGUAGESXML);
			
			LanguageDataBaseConverter _languageDatabaseConverter;
				
			// Load multi languages xml file(Languages.xml) and create languages database
			//if (GetGUIPlugin() == (int)MultiLanguageAssetPostProcessor.GUI_PLUGIN_TYPE.NGUI)
			//{
				Debug.Log("NGUI");
				_languageDatabaseConverter = new LanguageDataBaseNGUIConverter();
			//}
			//else
			//{
			//	Debug.Log("EZGUI");
			//	_languageDatabaseConverter = new LanguageDataBaseEZGUIConverter();
			//}
			
			_languageDatabaseConverter.Convert(path, MULTILANGUAGE_ASSETS_FOLDER);
		
	}
		
		

	private static bool CheckResModified(string[] files)
	{
		bool fileModified = false;
		
		foreach (string file in files)
		{
			if (file.Contains(MULTILANGUAGE_XML_FOLDER))
			{
				fileModified = true;	
				
				break;
			}
		}
		
		return fileModified;
	}
	
	private static int GetGUIPlugin()
	{
		return EditorPrefs.GetInt("Localization._GUI_Plugin", 1);
	}
}
