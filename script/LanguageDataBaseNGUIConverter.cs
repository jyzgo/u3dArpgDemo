using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

// NGUI language database converter
public class LanguageDataBaseNGUIConverter : LanguageDataBaseConverter
{
	public static readonly string GUISystem = "NGUI";
	private LocalizationFontConfig _fontsConfig;
	
	public override void CreateLanguageDataBase(string language_name, List<LanguageTextEntry> text_list)
	{
		string assetPath = MultiLanguageAssetPostProcessor.MULTILANGUAGE_COMMON_FOLDER;
		string file = MultiLanguageAssetPostProcessor.MULTILANGUAGE_CONFIGFILE;
		assetPath = System.IO.Path.Combine(assetPath, file);
		
		_fontsConfig = UnityEditor.AssetDatabase.LoadAssetAtPath(assetPath, typeof(LocalizationFontConfig)) as LocalizationFontConfig;
		
		
		string path = System.IO.Path.Combine(_outputDirectory, GUISystem);
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
		
		path = System.IO.Path.Combine(path, language_name + ".txt" );
		
		System.IO.TextWriter tw = new StreamWriter(path);
		
		for (int i=0; i<text_list.Count; i++)
		{
			string text = text_list[i].text;
			
			text = ReplacePatternMatch(text);
			
			string line = text_list[i].key + " = " + text;
			
			tw.WriteLine(line);
		}
		
		tw.Close();
		
		Debug.Log(language_name + " Successful!");
	}
	
	// Replace predefined string with color tag. For example, [red] to [ff0000]
	string ReplacePatternMatch(string text)
	{
		foreach (LanguageTextEntry pattern in _fontsConfig._colorTags)
		{
			text = text.Replace(pattern.key, pattern.text);
		}
		
		return text;
	}
}
