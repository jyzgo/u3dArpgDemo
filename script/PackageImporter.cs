using UnityEngine;
using UnityEditor;

using System;
using System.Collections.Generic;
using System.IO;

namespace InJoy.UnityBuildSystem
{
	public class PackageAssetPostprocessor : AssetPostprocessor
	{
		static string specialAttentionFolder = "Assets/Packages";
		static string specialFolder = "_PROJECT_ROOT_";
		
		const int newSpecialFolderLength = 3;
		static string[,] newSpecialFolders = new string[newSpecialFolderLength,2]{
			{"Editor Default Resources", ""},
			{"Android","Assets/Plugins/Android"},
			{"iOS","Assets/Plugins/iOS"},
//			{"iOS Resources","Assets/Resources"}
		};
		
		static bool DebugOutput = true;
		static bool ExcessDebugOutput = false;

		static void OnGeneratedCSProjectFiles ()
		{
			string[] projectFiles = Directory.GetFiles(".", "*.csproj");
			foreach (string projectFile in projectFiles)
			{
				StreamReader reader = new StreamReader(projectFile);
		        string data = reader.ReadToEnd();
		        reader.Close();
				
				bool didReplace = false;
				data = System.Text.RegularExpressions.Regex.Replace(data, "ToolsVersion=\"3.5\"", 
					a => { didReplace = true; return "ToolsVersion=\"4.0\""; });
				data = System.Text.RegularExpressions.Regex.Replace(data, "<TargetFrameworkVersion>v3.5</TargetFrameworkVersion>",
					a => { didReplace = true; return "<TargetFrameworkVersion>v4.0</TargetFrameworkVersion>"; });
				if (didReplace) 
				{
					StreamWriter writer = new StreamWriter(projectFile);
			        writer.Write(data);
			        writer.Close();
				}
			}
		}

		static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
			if(!UnityEditorInternal.InternalEditorUtility.inBatchMode)
			{
				ImportPackages();
			}
		}
		
		public static void ImportPackages()
		{
			const string packageImporterBlock = "PackageAssetPostprocessor";
			BuildLogger.OpenBlock(packageImporterBlock);
			
			// HACK: somehow NOT all newly imported assets are passed to OnPostprocessAllAssets
			// seems like only the ones imported after PackageImporter itself are (not exactly, but definitely not all)
			// Because of this our only option is to go through Assets/Packages looking for "_PROJECT_ROOT_"
			// and force their processing
			string [] specialAttentionDirs = Directory.GetDirectories(specialAttentionFolder, "_PROJECT_ROOT_", SearchOption.AllDirectories);
			
			if(DebugOutput)
			{
				BuildLogger.LogMessage("DETECTED DIRS TOTAL: " + specialAttentionDirs.Length);
				
				foreach(string path in specialAttentionDirs)
					BuildLogger.LogMessage("DETECTED DIR: " + path);
			}
			
			List<string> listToRelocate = new List<string>();
			
			foreach(string dirPath in specialAttentionDirs)
			{
				string [] specialAttentionFiles = Directory.GetFiles(dirPath, "*", SearchOption.AllDirectories);
				
				if(DebugOutput && ExcessDebugOutput)
				{
					foreach(string filePath in specialAttentionFiles)
						BuildLogger.LogMessage("DETECTED FILE: " + filePath);
				}
				
				listToRelocate.AddRange(specialAttentionFiles);
			}
			
			bool isAssetDatabaseRefreshRequired = false;
			
			//foreach(string asset in importedAssets)
			// HACK: insted of processing importedAssets we now are forced to process manually collected set of files
			foreach(string asset in listToRelocate)
			{
				if(!asset.Contains(".svn")) // cut off svn crap
				{
					FileInfo fileInfo = new FileInfo(asset);
					
					if(!fileInfo.Extension.Equals(".meta")) // cut off Unity meta files
					{
						int index = asset.IndexOf(specialFolder);
						string targetAsset = asset.Substring(index + specialFolder.Length + 1);
						
						FileInfo targetFileInfo = new FileInfo(targetAsset);
						
						// let's check if the file needs copying
						bool existanceCheck = (!targetFileInfo.Exists);
						bool lengthCheck = (targetFileInfo.Exists && (fileInfo.Length != targetFileInfo.Length));
						bool timeCheck = (targetFileInfo.Exists && (targetFileInfo.LastWriteTimeUtc.CompareTo(fileInfo.LastWriteTimeUtc) < 0));
						
						if(existanceCheck || lengthCheck || timeCheck)
						{
							if(DebugOutput)
								BuildLogger.LogMessage(
									"COPYING FILE(" +
									existanceCheck + ", " + lengthCheck + ", " + timeCheck +
									") " + fileInfo.Name +
									" FROM " + fileInfo.Directory.FullName +
									" TO " + targetFileInfo.Directory.FullName);
							
							Directory.CreateDirectory(targetFileInfo.Directory.FullName);
	
							File.Copy(asset, targetAsset, true);
							isAssetDatabaseRefreshRequired = true;
						}
						
//						CheckAndCopyFile (ref isAssetDatabaseRefreshRequired, fileInfo, targetAsset);
						CheckOtherFolders (asset, isAssetDatabaseRefreshRequired, index);
					}
				}
			}
			
			if(isAssetDatabaseRefreshRequired)
				AssetDatabase.Refresh();
		}
		
		// let's check if the file needs copying
		static void CheckAndCopyFile (ref bool isAssetDatabaseRefreshRequired, string sourceFullFilename, string targetAsset)
		{
			FileInfo fileInfo = new FileInfo(sourceFullFilename);
			CheckAndCopyFile(ref isAssetDatabaseRefreshRequired, fileInfo, targetAsset);
		}
		
		static void CheckAndCopyFile (ref bool isAssetDatabaseRefreshRequired, FileInfo fileInfo, string targetAsset)
		{
			FileInfo targetFileInfo = new FileInfo(targetAsset);
			bool existanceCheck = (!targetFileInfo.Exists);
			bool lengthCheck = (targetFileInfo.Exists && (fileInfo.Length != targetFileInfo.Length));
			targetFileInfo.Refresh();
			fileInfo.Refresh();
			bool timeCheck = (targetFileInfo.Exists && (targetFileInfo.LastWriteTimeUtc.CompareTo(fileInfo.LastWriteTimeUtc) < 0));
			
			if(existanceCheck || lengthCheck || timeCheck)
			{
				if(DebugOutput)
					//Debug.Log(
					Console.WriteLine(
						"PackageAssetPostprocessor:COPYING FILE(" + existanceCheck + ", " + lengthCheck + ", " + timeCheck + ") "
					                  + fileInfo.Name + " FROM " + fileInfo.Directory.FullName + " TO " + targetFileInfo.Directory.FullName);
				
				Directory.CreateDirectory(targetFileInfo.Directory.FullName);
			
				File.Copy(fileInfo.FullName, targetAsset, true);
				isAssetDatabaseRefreshRequired = true;
			}
		}

		static void CheckOtherFolders (string asset, bool isAssetDatabaseRefreshRequired, int index)
		{
			string parentFolder = asset.Substring(0, index);
			//prepare for Editor Default Resources folder
			string editorDefaultResourcesFolder = "Editor Default Resources";
			if(Directory.Exists( Path.Combine(parentFolder, editorDefaultResourcesFolder) ))
			{
				int packageNameStartIndex = asset.IndexOf(specialAttentionFolder) + specialAttentionFolder.Length + 1;
				string packageName = asset.Substring(packageNameStartIndex, index - packageNameStartIndex - 1);
				string rootEDRF = Path.Combine("Assets", editorDefaultResourcesFolder);
				string targetEDRF = Path.Combine(rootEDRF, packageName);
				for( int i = 0; i < newSpecialFolderLength; i ++)
				{
					if(newSpecialFolders[i,0] == editorDefaultResourcesFolder)
					{
						newSpecialFolders[i,1] = targetEDRF;
						break;
					}
				}
			}
			
			//Editor Default Resources, Android, iOS, iOS Resources
			for( int i = 0; i < newSpecialFolderLength; i ++)
			{
				string sourceAndroidFolder = Path.Combine(parentFolder, newSpecialFolders[i,0]);
				if(Directory.Exists(sourceAndroidFolder))
				{
					string[] fileList = Directory.GetFiles(sourceAndroidFolder, "*", SearchOption.AllDirectories);
					foreach(string subFullFileName in fileList)
					{
						if(subFullFileName.EndsWith(".meta"))
						{
							continue;
						}
						string targetAssetFileName = subFullFileName.Substring(sourceAndroidFolder.Length + 1);
						CheckAndCopyFile (ref isAssetDatabaseRefreshRequired, subFullFileName, Path.Combine(newSpecialFolders[i,1], targetAssetFileName));
					}
				}
			}
		}
	}
}