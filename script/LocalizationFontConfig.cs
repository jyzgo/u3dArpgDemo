using UnityEngine;
using UnityEditor;
using System.Collections;


/// <summary>
/// Localization config.
/// </summary>
public class LocalizationFontConfig : ScriptableObject 
{
	[System.Serializable]
	public class FontConfig
	{
		// font name
		public string _fontName;
	
		// ttfFile
		[System.ComponentModel.Description("TrueType file path, such as \"Assets/Data/Localization/TrueType\"")]
		public string _ttfFile = "*.ttf";
		
		// font size
		[InJoy.IntMinMaxAttribute(12,64)]
		public int _fontSize;
		
		// border to the pictures
		public int _border;
		
		// padding between two pictures
		public int _padding;
		
		public string _customImagesPath;
		
		// images directory
		[System.NonSerializedAttribute]
		public string _imagesDir;
			
		// atlas file
		[System.NonSerializedAttribute]
		public string _atlasFile = "*.png";
		
		// font info
		[System.NonSerializedAttribute]
		public string _font_info;
	}
	
	[System.Serializable]
	public class LanguageConfig
	{
		public string _languageName;
		
		// configFile
		[System.NonSerializedAttribute]
		public TextAsset _charSetFile;
		
			
		public FontConfig[] _fonts;
	}
	
	public LanguageConfig[] _languages;
	
	public LanguageTextEntry[] _colorTags;
}
