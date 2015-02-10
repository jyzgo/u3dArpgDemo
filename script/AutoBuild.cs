using UnityEngine;
using UnityEditor;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace InJoy.UnityBuildSystem
{
	public interface IBuildConfig
	{
		void Build(List<string> scenesToBuild, BuildTarget buildTarget, BuildOptions buildOptions);
	}
	
	public class DefaultIosBuildConfig : IBuildConfig
	{
		public virtual void Build (System.Collections.Generic.List<string> scenesToBuild, UnityEditor.BuildTarget buildTarget, UnityEditor.BuildOptions buildOptions)
		{
			AutoBuild.UnityBuildPlayer(scenesToBuild, buildTarget, buildOptions);
		}
	}
	
	public class AutoBuild
	{
		private static List<string> GetScenesToBuild()
		{
			List<string> scenePaths = new List<string>();
			foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
			{
				if (scene.enabled && File.Exists(scene.path))
					scenePaths.Add(scene.path);
			}
			
			return scenePaths;
		}
		
		static string outDir = "GeneratedProject";
		static string configTypeName = null;
		static string buildTag = "LOCALCONF_" + DateTime.Now.ToString("yyyyMdd_HHmm");

		public static void Build()
		{
			const string autoBuildBlock = "AutoBuild.Build()";
			BuildLogger.OpenBlock(autoBuildBlock);
			
			BuildLogger.LogMessage("Invoke PackageImporter explicitly");
			PackageAssetPostprocessor.ImportPackages();
			
			BuildLogger.LogMessage("Parsing additional params");
			var args = Environment.GetCommandLineArgs();

            UnityEditor.BuildOptions buildOptions = BuildOptions.None;
	        
	        for(int i = 0; i < args.Length; i++)
	        {
	            if(args[i] == "+outdir" && args.Length > i + 1)
	            {
	                outDir = args[i + 1];
					BuildLogger.LogMessage("outDir: " + outDir);
				}
				else if(args[i] == "+configTypeName" && args.Length > i + 1)
				{
					configTypeName = args[i + 1];
					BuildLogger.LogMessage("configTypeName: " + configTypeName);
					if(!configTypeName.Equals(configTypeName.Trim())) // to detect 'CR' at the end
					{
						BuildLogger.LogWarning("Build configurator type contains whitespaces (+configTypeName length = " + configTypeName.Length + ")!");
					}
	            }
				else if(args[i] == "+buildTag" && args.Length > i + 1)
				{
					buildTag = args[i + 1];
					BuildLogger.LogMessage("buildTag: " + buildTag);
	            }
                else if (args[i] == "+configuration" && args.Length > i + 1)        //Test = development build; Release = None
                {
                    if (args[i + 1] == "Test")
                    {
                        buildOptions = BuildOptions.Development;
						BuildLogger.LogMessage("configuration: Test(Development build)");
                    }
                    else
                    {
						BuildLogger.LogMessage("configuration: Release");
                    }
	            }
	        }
	        
			// produce Assets/Resources/build_tag.txt
			if(buildTag != null)
			{
				if (!Directory.Exists("Assets/Resources"))
				{
					BuildLogger.LogMessage("Creating Assets/Resources");
					Directory.CreateDirectory("Assets/Resources");
				}
				
				BuildLogger.LogMessage(string.Format("Writing Assets/Resources/{0}.txt: {1}", BuildInfo.k_build_tag, buildTag));
				
				Stream file = File.Create(string.Format("Assets/Resources/{0}.txt", BuildInfo.k_build_tag));
				StreamWriter sw = new StreamWriter(file);
				sw.Write(buildTag);
				sw.Close();
				file.Close();
				
				AssetDatabase.Refresh();
			}
			
			// prepare default build params
			BuildLogger.LogMessage("Preparing default build params");
			List<string> scenesToBuild = GetScenesToBuild();
			UnityEditor.BuildTarget buildTarget = BuildTarget.iPhone;
			BuildLogger.LogMessage("Default scenes to build:");
			foreach (string sceneName in scenesToBuild)
				BuildLogger.LogMessage('\t' + sceneName);
			BuildLogger.LogMessage("Default buildTarget=" + buildTarget.ToString());
			BuildLogger.LogMessage("Default buildOptions=" + buildOptions.ToString());
			
			// run custom builder (or fall back to default)
			Type configType = null;
			if(configTypeName != null)
				configType = Type.GetType(configTypeName);
			
			IBuildConfig buildConfig = null;
			if(configType != null)
			{
				if(configType.GetInterface("IBuildConfig") != null)
				{
					ConstructorInfo defaultConstructorInfo = configType.GetConstructor(new Type [0]);
					if(defaultConstructorInfo != null)
					{
						buildConfig = defaultConstructorInfo.Invoke(new object [0]) as IBuildConfig;
						if(buildConfig != null)
						{
							BuildLogger.LogMessage("Using build configurator \"" + buildConfig.ToString() + "\"");
						}
						else
						{
							BuildLogger.LogWarning("Failed to construct an instance of build configurator type (+configTypeName \"" + configTypeName + "\")");
						}
					}
					else
					{
						BuildLogger.LogWarning("Build configurator type (+configTypeName \"" + configTypeName + "\") does NOT have a default constructor");
					}
				}
				else
				{
					BuildLogger.LogWarning("Build configurator type (+configTypeName \"" + configTypeName + "\") does NOT implement IBuildConfig");
				}
			}
			else
			{
				BuildLogger.LogWarning("Build configurator type NOT found (+configTypeName \"" + configTypeName + "\")");
			}
			if(buildConfig != null)
			{
				string block = string.Format("{0}.Build()", buildConfig.GetType().Name);
				BuildLogger.OpenBlock(block);
				buildConfig.Build(scenesToBuild, buildTarget, buildOptions);
				BuildLogger.CloseBlock(block);
			}
			else
			{
				BuildLogger.LogError("Unable to configure build for " + configTypeName);
				throw new ApplicationException("Error: unable to configure build for " + configTypeName);
			}
			
			BuildLogger.CloseBlock(autoBuildBlock);
		}
		
		public static void UnityBuildPlayer(List<string> scenesToBuild, BuildTarget buildTarget, BuildOptions buildOptions)
		{
			const string funcBlock = "AutoBuild.UnityBuildPlayer()";
			BuildLogger.OpenBlock(funcBlock);
			
			BuildLogger.LogMessage("Scenes to build:");
			foreach (string sceneName in scenesToBuild)
				BuildLogger.LogMessage('\t' + sceneName);
			BuildLogger.LogMessage("BuildTarget=" + buildTarget.ToString());
			BuildLogger.LogMessage("BuildOptions=" + buildOptions.ToString());
			
			if(buildTarget == BuildTarget.Android)
				outDir = outDir + "/" + PlayerSettings.bundleIdentifier + ".apk";
			
			string buildPlayerBlock = string.Format("BuildPipeline.BuildPlayer (\"{0}\")", Path.GetFileName(outDir));
			BuildLogger.OpenBlock(buildPlayerBlock);
			
			string errorMessage = BuildPipeline.BuildPlayer(scenesToBuild.ToArray(), outDir, buildTarget, buildOptions);
			
			BuildLogger.CloseBlock(buildPlayerBlock);
			
			if(errorMessage != null && !errorMessage.Equals("") && !(errorMessage.Length == 0))
			{
				throw new ApplicationException(errorMessage);
			}
			
			BuildLogger.CloseBlock(funcBlock);
		}
	}
}