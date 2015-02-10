using UnityEngine;
using UnityEditor;
using System.Collections;


/// <summary>
/// Localization builder.
/// </summary>
public class LocalizationBuilder
{
	// settings
	private LocalizationFontConfig _fontConfig;
	private string _workDirectory;
	private string _toolDirectory;
	
	// tools
	private readonly string ToolTTF2Images = "ttf2img";
	private readonly string ToolCustomImages = "replace_char.py";
	private readonly string ToolAtlasMaker = "pack.py";
	private readonly string ToolFntMaker = "packfnt.py";
	
	// progress
	private float _buildProgress = 0.0f;
	private float _buildstep = 0.0f;
	
	
	/// <summary>
	/// Builds the fonts.
	/// </summary>
	/// <param name='config'>
	/// Config.
	/// </param>
	public void BuildFonts(LocalizationFontConfig config, string workDir, string toolsDir)
	{
		LocalizationCmdProcessor.GetInstance().Initialize();
		
		_fontConfig = config;
		_workDirectory = workDir;
		_toolDirectory = toolsDir;
		_buildstep = 1.0f/(_fontConfig._languages.Length*4);
		
		EditorUtility.DisplayCancelableProgressBar("Fonts maker", "Start to build fonts", 0.0f);
			
		for (int i=0; i<_fontConfig._languages.Length; i++)
		{
			bool ret = BuildLanguage(_fontConfig._languages[i]);
			if (!ret)
			{
				break;
			}
		}
		
		LocalizationCmdProcessor.GetInstance().Clean();
		
		EditorUtility.DisplayCancelableProgressBar("Fonts maker", "fonts building complete!", 1.0f);
		EditorUtility.ClearProgressBar();
	}
	
	protected bool BuildLanguage(LocalizationFontConfig.LanguageConfig langConfig)
	{
		Debug.Log("Localization: " + langConfig._languageName + " build begin...");
		
		if (!CheckLanguage(langConfig))
		{
			
			Debug.LogWarning("Localization: " + langConfig._languageName + " config is not valid!");
			
			return false;
		}
		
		for (int j=0; j<langConfig._fonts.Length; j++)
		{
			bool ret = BuildFont(langConfig._languageName, langConfig._charSetFile, langConfig._fonts[j]);
			
			if (!ret)
			{
				return false;
			}
		}
		
		Debug.Log("Localization: " + langConfig._languageName + " build end...");
		
		return true;
	}
	
	protected bool BuildFont(string lang, TextAsset charsetAsset, LocalizationFontConfig.FontConfig config)
	{
		Debug.Log("Localization: " + lang + " " + config._fontName + " build begin...");
		
		if (!CheckFont(config))
		{
			Debug.LogWarning("Localization: " + lang + " " + config._fontName + " config is not valid!");
			
			return false;
		}
		
		string path = System.IO.Path.Combine(_workDirectory, "Fonts");
		path = System.IO.Path.Combine(path, lang);
		
		if (!System.IO.Directory.Exists(path))
		{
			System.IO.Directory.CreateDirectory(path);
		}
		
		string charsetFile = System.IO.Path.Combine(MultiLanguageAssetPostProcessor.MULTILANGUAGE_COMMON_FOLDER, lang + "_dict.txt");
		if (!System.IO.File.Exists(charsetFile))
		{
			EditorUtility.DisplayDialog("Fonts Maker", charsetFile + " is not exist!", "OK");
			
			return false;
		}
		
		
		CreateImages(lang, ref config, charsetFile);
		
		ReplaceCustomImages(lang, ref config);
				
		MakeAtlas(lang, ref config);
		
		GenerateFnt(lang, ref config);
		
		Debug.Log("Localization: " + lang + " " + config._fontName + " build end...");
		
		return true;
	}
	
	public bool CheckLocalizationConfig(LocalizationFontConfig config)
	{
		if (config._languages.Length == 0)
		{
			EditorUtility.DisplayDialog("Fonts Maker", "There is no language to be made font !", "OK");
			
			Debug.LogWarning("Localization: config._languages.Length == 0");
			
			return false;
		}
		
		return true;
	}
	
	protected bool CheckLanguage(LocalizationFontConfig.LanguageConfig langConfig)
	{
		if (langConfig._languageName == "")
		{
			EditorUtility.DisplayDialog("Fonts Maker", "Language Name Can't be empty!", "OK");
			
			Debug.LogWarning("Localization: langConfig._languageName == null");
			
			return false;
		}
		
//		if (langConfig._charSetFile == null)
//		{
//			EditorUtility.DisplayDialog("Fonts Maker", "Char set file Can't be null!", "OK");
//			
//			Debug.LogWarning("Localization: langConfig._charSetFile == null");
//			
//			return false;
//		}
		
		if (langConfig._fonts.Length == 0)
		{
			EditorUtility.DisplayDialog("Fonts Maker", langConfig._languageName+" has no font to make!", "OK");
			
			Debug.LogWarning("Localization: langConfig._fonts.Length == 0");
			
			return false;
		}
		
		return true;
	}
	
	protected bool CheckFont(LocalizationFontConfig.FontConfig fontConfig)
	{
		if (fontConfig._fontName == "")
		{
			EditorUtility.DisplayDialog("Fonts Maker", "Font Name Can't be empty!", "OK");
			
			Debug.LogWarning("Localization: fontConfig._fontName == null");
			
			return false;
		}
		
		if (fontConfig._ttfFile == "")
		{
			EditorUtility.DisplayDialog("Fonts Maker", "True Type file Can't be empty!", "OK");
			
			Debug.LogWarning("Localization: fontConfig._ttfFile == null");
			
			return false;
		}
		
		if (!System.IO.File.Exists(fontConfig._ttfFile))
		{
			EditorUtility.DisplayDialog("Fonts Maker", "True Type file is not exist!", "OK");
			
			Debug.LogWarning("Localization: fontConfig._ttfFile is not Exists");
			
			return false;
		}
		
		if (fontConfig._fontSize <= 0)
		{
			EditorUtility.DisplayDialog("Fonts Maker", "Font size must be bigger than 0 !", "OK");
			
			Debug.LogWarning("Localization: fontConfig._fontSize must be bigger than 0");
			
			return false;
		}
		
		return true;
	}
	
	/// <summary>
	/// Creates the images.
	/// </summary>
	protected void CreateImages(string lang, ref LocalizationFontConfig.FontConfig config, string charsetFile)
	{		
		string filename = System.IO.Path.Combine(_toolDirectory, ToolTTF2Images);
		
		LocalizationCommand cmd = new LocalizationCommand(filename);
		
		config._imagesDir = LocalizationEditorUtils.CreateDirectoryIfNotExist(_toolDirectory, "Temp", lang, config._fontName);
		config._font_info = System.IO.Path.Combine(config._imagesDir, "font_info.txt");
				
		cmd.SetRequiredParams(config._ttfFile, config._fontSize, config._imagesDir);
		
		cmd.AddOptionalParams("-p", config._border);
		cmd.AddOptionalParams("-C", charsetFile);
		
		cmd.Execute();
		
				
		_buildProgress += _buildstep;
		EditorUtility.DisplayCancelableProgressBar("Fonts maker", "Building...", _buildProgress);
	}
	
	protected void ReplaceCustomImages(string lang, ref LocalizationFontConfig.FontConfig config)
	{
		if (config._customImagesPath == "")
		{
			return;
		}
		
		if (!System.IO.Directory.Exists(config._customImagesPath))
		{
			return;
		}
		
		string filename = System.IO.Path.Combine(_toolDirectory, ToolCustomImages);
		LocalizationCommand cmd = new LocalizationCommand("python");
		
		cmd.SetRequiredParams(filename, config._imagesDir, config._imagesDir, config._customImagesPath);
		
		cmd.Execute();
		
		
		_buildProgress += _buildstep;
		EditorUtility.DisplayCancelableProgressBar("Fonts maker", "Building...", _buildProgress);
	}
	
	/// <summary>
	/// Makes the atlas.
	/// </summary>
	protected void MakeAtlas(string lang, ref LocalizationFontConfig.FontConfig config)
	{
		string filename = System.IO.Path.Combine(_toolDirectory, ToolAtlasMaker);
		string path = LocalizationEditorUtils.CreateDirectoryIfNotExist(_workDirectory, "Fonts", lang);
		config._atlasFile = System.IO.Path.Combine(path, config._fontName+".png");
		
		LocalizationCommand cmd = new LocalizationCommand("python");
				
		cmd.SetRequiredParams(filename, config._imagesDir, config._atlasFile);
		cmd.AddOptionalParams("-p", config._padding);
		
		cmd.Execute();
		
		
		_buildProgress += _buildstep;
		EditorUtility.DisplayCancelableProgressBar("Fonts maker", "Building...", _buildProgress);
	}
	
	/// <summary>
	/// Generates the fnt.
	/// </summary>
	protected void GenerateFnt(string lang, ref LocalizationFontConfig.FontConfig config)
	{
		string filename = System.IO.Path.Combine(_toolDirectory, ToolFntMaker);
		string path = LocalizationEditorUtils.CreateDirectoryIfNotExist(_workDirectory, "Fonts", lang);
		string fntFile = System.IO.Path.Combine(path, config._fontName+".fnt");
		string txfFile = System.IO.Path.Combine(path, config._fontName+".txt");
		
		path = System.IO.Path.Combine(path, config._fontName+".config");
		
		LocalizationCommand cmd = new LocalizationCommand("python");
		cmd.SetRequiredParams(filename, path, config._atlasFile, config._font_info);
		
		cmd.Execute();
		
		// Rename *.fnt to *.txt
		System.IO.File.Delete(txfFile);
		System.IO.File.Move(fntFile, txfFile);
		
		_buildProgress += _buildstep;
		EditorUtility.DisplayCancelableProgressBar("Fonts maker", "Building...", _buildProgress);
	}
}
