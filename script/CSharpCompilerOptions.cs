using UnityEngine;
using UnityEditor;

using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using ConditionalAttribute = System.Diagnostics.ConditionalAttribute;

[InitializeOnLoad]
public class CSharpCompilerOptions
{
	private static bool autoSave = true;
	
	private class CSharpDefines
	{
		private string _smcsPath = "Assets/smcs.rsp";
		private string _gmcsPath = "Assets/gmcs.rsp";
		private string _prefix = "-define:";
		private string _configDir = "Assets/Editor/Conf/CSharpComplerOptions/";
		private string _configPath = "Assets/Editor/Conf/CSharpComplerOptions/defines.xml";
		private string _packagesDir = "Assets/Packages/";
		
		
		private HashSet<string> _packages = new HashSet<string>();
		private HashSet<string> _defines = new HashSet<string>();
		private Regex _properDefine = new Regex("^[a-z|A-Z|_][a-z|A-Z|_|0-9]*$");
		
		private XmlSerializer _serializer = new XmlSerializer(typeof(HashSet<string>));
		
		
		public CSharpDefines(): base() 
		{
			if (!Directory.Exists(_configDir))
			{
				DeleteMeta(_configDir);
				Directory.CreateDirectory(_configDir);
			}
			Reset();
		}
		
		private bool CheckDefine(string define)
		{
			Match m = _properDefine.Match(define);
			return m.Success;
		}
		
		private void SaveConfig()
		{
			try
			{
				StreamWriter configFile = new StreamWriter(_configPath);
				_serializer.Serialize(configFile, _defines);
				configFile.Close();
			}
			catch
			{
				Debug.LogWarning("CSharpCompilerOptions: Failed to save config file");
			}
		}
		
		[Conditional("UNITY_3")]
		private void DeleteMeta(string filename)
		{
			DeleteMeta(filename, ".meta");
		}
		
		[Conditional("UNITY_3")]
		private void DeleteMeta(string filename, string suffix)
		{
			string metaname = filename + suffix;
			if (File.Exists(metaname))
			{
				File.Delete(metaname);
			}
		}
				
		private void LoadConfig()
		{
			_defines.Clear();
			try
			{
				if (File.Exists(_configPath))
				{
					StreamReader configFile = new StreamReader(_configPath);
					_defines = (HashSet<string>) _serializer.Deserialize(configFile);
					configFile.Close();
				}
				else
				{
					DeleteMeta(_configPath);
					SaveConfig();
				}
			}
			catch
			{
				Debug.LogWarning("CSharpCompilerOptions: Failed to load config file");
			}
		}
		
		private void DetectPackages()
		{
			_packages.Clear();
			DirectoryInfo packageDirs = new DirectoryInfo(_packagesDir);
			foreach (DirectoryInfo directory in packageDirs.GetDirectories())
			{
				string define = directory.Name.ToUpper().Trim();
				if (CheckDefine(define))
				{
					_packages.Add(define);
				}
				else if (define[0] != '.')
				{
					Debug.LogError(directory.Name + " is not a valid package name, define can not be created"); 
				}
			}
		}
		
		private bool WriteRSP(string file, string defineLine)
		{
			bool fileCreated = true;
			StringBuilder stringBuilder = new StringBuilder();
			if (File.Exists(file))
			{
				fileCreated = false;
				using (StreamReader streamReader = new StreamReader(file))
				{
					string line = null;
					while ((line = streamReader.ReadLine()) != null)
					{
						if (!line.StartsWith(_prefix))
						{
							stringBuilder.AppendLine(line);
						}
					}
				}
			}
			else
			{
				DeleteMeta(file);
			}
			
			stringBuilder.AppendLine(defineLine);
			using (StreamWriter streamWriter = new StreamWriter(file))
			{
				streamWriter.Write(stringBuilder.ToString());
			}
			return fileCreated;
		}
		
		private void WriteRSPs()
		{		
			StringBuilder stringBuilder = new StringBuilder();
			if (_defines.Count + _packages.Count > 0)
			{
				stringBuilder.Append(_prefix);
				foreach (string define in _defines)
				{
					stringBuilder.Append(define);
					stringBuilder.Append(";");
				}
				foreach (string define in _packages)
				{
					stringBuilder.Append(define);
					stringBuilder.Append(";");
				}
			}
			
			string defineString = stringBuilder.ToString().TrimEnd(';');
			//sync to project
			PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, defineString);			
			
			
			bool refreshNeeded = WriteRSP(_smcsPath, defineString);
			refreshNeeded |= WriteRSP(_gmcsPath, defineString);
			
			if (refreshNeeded)
			{
				AssetDatabase.Refresh();
			}
		}

		
		public void SaveDefines()
		{
			SaveConfig();
			DetectPackages();
			WriteRSPs();
		}
		
		public void Reset()
		{
			LoadConfig();
			DetectPackages();
			WriteRSPs();
		}
	
		public bool Add(string define)
		{
			string trimmedDefine = define.Trim();
			if (CheckDefine(trimmedDefine))
			{
				return _defines.Add(trimmedDefine);
			}
			return false;
		}
		
		public bool Contains(string define)
		{
			return _defines.Contains(define);
		}
		
		public bool Remove(string define)
		{
			return _defines.Remove(define);
		}
		
		public void Clear()
		{
			_defines.Clear();
		}
		
		public HashSet<string> GetDefines()
		{
			return new HashSet<string>(_defines);
		}
	}
	
	private static CSharpDefines _defines = new CSharpDefines();
	
	private static void AutoSave()
	{
		if (autoSave)
			WriteDefines();
	}
	
	public void SetAutoSave(bool isAutosaved)
	// Sets if Compile options should be saved on every change or not
	{
		autoSave = isAutosaved;
		AutoSave();
	}
	
	public static void WriteDefines()
	// Saves changes into *.rsp files
	{
		_defines.SaveDefines();
	}
	
	public static void ResetChanges()
	// Restore defines to last saved state
	{
		_defines.Reset();
	}
	
	public static bool IsDefined(string define)
	// Checks if specified string is defined
    {
		return _defines.Contains(define);
	}
	
	public static void Define(string define)
	// Adds a string to define
    {
		if (_defines.Add(define))
			AutoSave();
	}
	
	public static void Undefine(string define)
	// Removes define if exists
    {
		if (_defines.Contains(define))
		{
			_defines.Remove(define);
			AutoSave();
		}
	}
	
	public static HashSet<string> GetDefines()
	// Returns a copy of internal HashSet of defines
	{
		return _defines.GetDefines();
	}
	
	[MenuItem("Tools/C# compiler options/Undefine all")]
	public static void UndefineAll()
	// Undefines all define
    {
		_defines.Clear();
		AutoSave();
	}
	
	[MenuItem("Tools/C# compiler options/Reimport C# scripts")]
	public static void ReimportScripts()
	// Triggers script recompilation in unity, not needed in batch mode
	{
		AutoSave();
		AssetDatabase.StartAssetEditing();

		string[] scriptMasks = new string[]
		{
			"*.cs",
		};

		foreach (var mask in scriptMasks)
		{
			string[] scripts = System.IO.Directory.GetFiles("Assets", mask, System.IO.SearchOption.AllDirectories);
			foreach (var s in scripts)
			{
				AssetDatabase.ImportAsset(s);
			}
		}

		AssetDatabase.StopAssetEditing();
	}
	

	/*
	[MenuItem("Config/_TestReadDefines")]
	public static void TestRead()
	{
		HashSet<string> defines = ReadDefines();
		foreach(string define in defines)
			Debug.Log(define);
	}

	[MenuItem("Config/_TestWriteDefines")]
	public static void TestWrite()
	{
		HashSet<string> defines = ReadDefines();
		defines.Remove("AAA");
		defines.Add("BBB");
		defines.Add("CCC");
		WriteDefines(defines);
	}

	[MenuItem("Config/_TestPreprocessorDefines")]
	public static void TestPreprocessorDefines()
	{
		Debug.Log(CSharpCompilerOptions.IsDefined("BBB"));
		CSharpCompilerOptions.Undefine("CCC");
		CSharpCompilerOptions.Define("YYY");
		CSharpCompilerOptions.Define("ZZZ");
	}
	*/
}
