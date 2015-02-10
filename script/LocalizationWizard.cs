using UnityEngine;
using UnityEditor;
using System.Collections;


/// <summary>
/// Localization wizard.
/// </summary>
public class LocalizationWizard : ScriptableWizard
{	
	private LocalizationBuilder builder = new LocalizationBuilder();
	
	private string _configPath;
	public LocalizationFontConfig _configAsset;
	
	public string _toolsPath;
	
	public string _resourePath;
	
	private MultiLanguageAssetPostProcessor.GUI_PLUGIN_TYPE _GUI_Plugin = MultiLanguageAssetPostProcessor.GUI_PLUGIN_TYPE.NGUI;
	
	
	/// <summary>
	/// Creates the font.
	/// </summary>
	[MenuItem("Tools/Localization/Build All Fonts %#f")]
	static void CreateFont()
	{
		LocalizationWizard lw = (LocalizationWizard)ScriptableWizard.DisplayWizard<LocalizationWizard>("Build All Fonts", "Start Build", "Save Setting");
		
		lw.LoadSettings();
	}
	
	void LoadSettings()
	{
		_configPath = EditorPrefs.GetString("Localization._configPath");
		if (_configPath != "")
		{
			_configAsset = AssetDatabase.LoadMainAssetAtPath(_configPath) as LocalizationFontConfig;
		}
		
		_toolsPath = EditorPrefs.GetString("Localization._toolsPath");
		
		_resourePath = EditorPrefs.GetString("Localization._resourePath");
		if (_resourePath == "")
		{
			_resourePath = MultiLanguageAssetPostProcessor.MULTILANGUAGE_ASSETS_FOLDER;
		}
		
		_GUI_Plugin = (MultiLanguageAssetPostProcessor.GUI_PLUGIN_TYPE)EditorPrefs.GetInt("Localization._GUI_Plugin");
	}
	
	void SaveSettings()
	{
		_configPath = AssetDatabase.GetAssetPath(_configAsset);
		
		EditorPrefs.SetString("Localization._configPath", _configPath);
		EditorPrefs.SetString("Localization._toolsPath", _toolsPath);
		EditorPrefs.SetString("Localization._resourePath", _resourePath);
		EditorPrefs.SetInt("Localization._GUI_Plugin", (int)_GUI_Plugin);
	}
	
	bool CheckSettings()
	{
		if (_configAsset == null)
		{
			EditorUtility.DisplayDialog("Fonts Maker", "Font configure asset can't be null !", "OK");
			
			Debug.LogWarning("Localization: _configAsset == null");
			
			return false;
		}
		
		if (_toolsPath == "")
		{
			EditorUtility.DisplayDialog("Fonts Maker", "Where are the font maker tools!", "OK");
			
			Debug.LogWarning("Localization: _toolsFolder == null");
			
			return false;
		}
		else if (!System.IO.Directory.Exists(_toolsPath))
		{
			EditorUtility.DisplayDialog("Fonts Maker", "Tools path is not exist !", "OK");
			
			Debug.LogWarning("Localization: _toolsPath is no Exists");
			
			return false;
		}
		
		if (_resourePath == "")
		{
			EditorUtility.DisplayDialog("Fonts Maker", "Resoure path can't be empty !", "OK");
			
			Debug.LogWarning("Localization: _workDirectory == null");
			
			return false;
		}
		else if (!System.IO.Directory.Exists(_resourePath))
		{
			EditorUtility.DisplayDialog("Fonts Maker", "Resource path is not exist !", "OK");
			
			Debug.LogWarning("Localization: _workDirectory is no Exists");
			
			return false;
		}
		
		return true;
	}
	
	void OnEnable()
	{
		//LoadSettings();
	}
	
	void OnWizardCreate()
	{
		SaveSettings();
		
		if (CheckSettings() && builder.CheckLocalizationConfig(_configAsset))
		{
			builder.BuildFonts(_configAsset, _resourePath, _toolsPath);
			
			EditorUtility.DisplayDialog("Fonts Maker", "Fonts Building Completes!", "OK");
		}
	}
	
	void OnWizardOtherButton()
	{
		SaveSettings();
	}
}
