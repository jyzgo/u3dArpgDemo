using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;



// Base class of language database converter
public abstract class LanguageDataBaseConverter
{		
	// Where to output dictionay file and language database
	public string _outputDirectory;
	
	public void Convert(string file, string outputFolder)
	{
		Debug.Log(outputFolder);
		
		_outputDirectory = outputFolder;

		ReadXml(file);
	}
	

	public  void ReadXml(string fileName)
	{
		string sheetName = "Sheet1";
		Workbook workbook = Workbook.CreatWorkbook(fileName,sheetName);
		Sheet sheet = workbook._sheet;
		if(sheet == null)
		{
			Debug.LogError("Can't find " + sheetName + " in " + fileName);
			return;
		}
		
		Dictionary<string, List<LanguageTextEntry>>  languages = new Dictionary<string, List<LanguageTextEntry>>();
		
		List<string> langKeys = sheet._rows[0]._cells;
		for(int i = 1; i< langKeys.Count; i++)
		{
			string lang  = langKeys[i];		
			languages.Add(lang, new List<LanguageTextEntry>());
		}
		
	
		for(int i = 1; i< sheet._rows.Count; i++)
		{
			Row row = sheet._rows[i];
			string IDS = row._cells[0];
			for(int j =1; j < row._cells.Count; j++)
			{
				LanguageTextEntry textEntry = new LanguageTextEntry();
				
				textEntry.key = IDS;
				textEntry.text = row._cells[j];
				
				string lang = row._key[j];
				List<LanguageTextEntry> textEntryList = languages[lang];
				textEntryList.Add(textEntry);
			}
		}
		
		
		for(int i = 1; i< langKeys.Count; i++)
		{
			string lang  = langKeys[i];	
			List<LanguageTextEntry> textEntryList = languages[lang];
			
			string path = MultiLanguageAssetPostProcessor.MULTILANGUAGE_ASSETS_FOLDER;
			LocalizationEditorUtils.CreateDirectoryIfNotExist(path, "Fonts", lang);
			
			// Create config file that contain key-value strings.
			CreateLanguageDataBase(lang, textEntryList);	
			
			// Create config file that contain uv and size information.
			CreateLanguageDictionary(lang, textEntryList);
		}
	}
	
	// Create language database interface, class derive from this must override
	public abstract void CreateLanguageDataBase(string language_name, List<LanguageTextEntry> text_list);
	
	// Characters used in this language
	public void CreateLanguageDictionary(string language_name, List<LanguageTextEntry> text_list)
	{		
		HashSet<char> allCharSet = new HashSet<char>();
		
		for (int i=0; i<text_list.Count; i++)
		{
			allCharSet.UnionWith(text_list[i].text.ToCharArray());
		}
	
		List<char> allCharList = new List<char>(allCharSet);
		allCharList.Sort();		
		
		string path = MultiLanguageAssetPostProcessor.MULTILANGUAGE_COMMON_FOLDER;
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
		
		path = System.IO.Path.Combine(path, language_name + "_dict.txt");
		
		
		TextWriter tw = new StreamWriter(path);
		
		tw.Write(allCharList.ToArray());
		tw.Close();
	}
}
