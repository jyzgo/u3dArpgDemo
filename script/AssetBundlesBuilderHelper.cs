using UnityEngine;
using UnityEditor;

using System;
using System.Collections.Generic;
using System.IO;

using InJoy.UnityBuildSystem;

namespace InJoy.AssetBundles
{
	using Internal;
	
	/// <summary>
	/// Provides with functions to allow to build asset bundles automatically
	/// (e.g. on the build machines).
	/// </summary>
	public static class BuilderHelper
	{
		#region Interface
		
		/// <summary>
		/// Gets or sets the scene. If it is not null or empty, then functions
		/// Build*Player() would make it the first in the BuildSettings list.
		/// This is optional property, default value is null.
		/// </summary>
		/// <value>
		/// The scene filename.
		/// </value>
		public static string BootSceneFilename { set; get; }
		
		/// <summary>
		/// Builds asset bundles and put them inside the project,
		/// so they would be included in the application package.
		/// If asset bundles already exist, they would be removed.
		/// Built asset bundles could be compressed or not.
		/// </summary>
		public static void BuildInsidePlayer(
			ref List<string> scenesToBuild,
			ref BuildTarget buildTarget,
			ref BuildOptions buildOptions,
			IBuildConfig buildConfig)
		{
			CleanStreamingAssets();
			Build(ref scenesToBuild, ref buildTarget, ref buildOptions, buildConfig, null, null, null);
			CopyToStreamingAssets();
		}
		
		/// <summary>
		/// Builds asset bundles and put them outside of the project,
		/// so they would not be included in the application package.
		/// If asset bundles already exist, they would be removed.
		/// Built asset bundles will be always compressed.
		/// </summary>
		public static void BuildOutsideOfPlayer(
			ref List<string> scenesToBuild,
			ref BuildTarget buildTarget,
			ref BuildOptions buildOptions,
			IBuildConfig buildConfig)
		{
			CleanStreamingAssets();
			Build(ref scenesToBuild, ref buildTarget, ref buildOptions, buildConfig, null, null, true);
            SeparateBundles();
		}

        /// <summary>
        /// Builds asset bundles and put them outside the project, and split the bundles into two folders,
        /// one for compressed, another for uncompressed. Uncompressed bundles will be used for release, compressed for 
        /// download.
        /// </summary>
        public static void BuildOutsidePlayer2(
            ref List<string> scenesToBuild,
            ref BuildTarget buildTarget,
            ref BuildOptions buildOptions,
            IBuildConfig buildConfig)
        {
            CleanStreamingAssets();
            Build(ref scenesToBuild, ref buildTarget, ref buildOptions, buildConfig, null, null, null);
            //copy bundles to different folders by their compressed property
            SeparateBundles();
        }
		
		/// <summary>
		/// Cleans the project from generated asset bundles.
		/// </summary>
		[Obsolete("Use CleanStreamingAssets")]
		public static void CleanProject()
		{
			CleanStreamingAssets();
		}
		
		/// <summary>
		/// Cleans the StreamingAssets folder from generated asset bundles.
		/// </summary>
		public static void CleanStreamingAssets()
		{
			ETUtils.DeleteDirectory(Builder.TARGET_IN_STREAMING_ASSETS);
			ETUtils.CreateDirectory(Builder.TARGET_IN_STREAMING_ASSETS);
			AssetDatabase.Refresh();
		}
		
		/// <summary>
		/// Builds asset bundles.
		/// </summary>
		public static void Build(
			ref List<string> scenesToBuild,
			ref BuildTarget buildTarget,
			ref BuildOptions buildOptions,
			IBuildConfig buildConfig,
			string producedIndexFilename)
		{
			Build(ref scenesToBuild, ref buildTarget, ref buildOptions, buildConfig, null, producedIndexFilename, null);
		}
		
		/// <summary>
		/// Builds asset bundles.
		/// </summary>
		public static void Build(
			ref List<string> scenesToBuild,
			ref BuildTarget buildTarget,
			ref BuildOptions buildOptions,
			IBuildConfig buildConfig,
			string producedIndexFilename,
			bool? compressAssetBundles)
		{
			Build(ref scenesToBuild, ref buildTarget, ref buildOptions, buildConfig, null, producedIndexFilename, compressAssetBundles);
		}
		
		/// <summary>
		/// Builds asset bundles.
		/// </summary>
		public static void Build(
			ref List<string> scenesToBuild,
			ref BuildTarget buildTarget,
			ref BuildOptions buildOptions,
			IBuildConfig buildConfig,
			string originalIndexFilename,
			string producedIndexFilename)
		{
			Build(ref scenesToBuild, ref buildTarget, ref buildOptions, buildConfig, originalIndexFilename, producedIndexFilename, null);
		}

		/// <summary>
		/// Builds asset bundles.
		/// </summary>
		public static void Build(
			ref List<string> scenesToBuild,
			ref BuildTarget buildTarget,
			ref BuildOptions buildOptions,
			IBuildConfig buildConfig,
			string originalIndexFilename,
			string producedIndexFilename,
			bool? compressAssetBundles)
		{
			// set output folder
			Assertion.Check(buildConfig != null, "BuildConfig must be specified!");
			InitDistDirectory(buildConfig);
			
			// configure builder
			Index index = Builder.index;
			Builder.index = null; // means, build all found indices
			if(!string.IsNullOrEmpty(originalIndexFilename))
			{
				bool found = false;
				Index[] indices = Builder.GetIndexInstances();
				Builder.index = index; // restores original value
				foreach(Index i in indices)
				{
					if(i.m_filename.ToLower().Equals(originalIndexFilename.ToLower()))
					{
						Builder.index = i;
						found = true;
						break;
					}
				}
				Assertion.Check(found, "Index \"{0}\" has not been found!", originalIndexFilename);
				if(!found)
				{
					throw new NotSupportedException(string.Format("Index \"{0}\" has not been found. No asset bundles will be built.", originalIndexFilename));
				}
			}
			Index.m_overridenFilenameMask = producedIndexFilename;
			bool buildReadableIndex = Builder.BuildReadableIndex;
			Builder.BuildReadableIndex = ETUtils.IsUnityInBatchMode(); // means, on buildmachines only
			Builder.AssetBundlesCompressionOverriding overrideAssetBundlesCompression = Builder.OverrideAssetBundlesCompression;
			if((compressAssetBundles != null) && compressAssetBundles.HasValue)
			{
				Builder.OverrideAssetBundlesCompression = compressAssetBundles.Value ? Builder.AssetBundlesCompressionOverriding.Compressed : Builder.AssetBundlesCompressionOverriding.Uncompressed;
			}
			else
			{
				Builder.OverrideAssetBundlesCompression = Builder.AssetBundlesCompressionOverriding.DoNotOverride;
			}
			
			// produce bundles
			UpdateScenes(ref scenesToBuild);
			Builder.BuildAssetBundles(DistDirectory, buildTarget, buildOptions);
			
			// restore builder
			Builder.index = index;
			Index.m_overridenFilenameMask = null;
			Builder.BuildReadableIndex = buildReadableIndex;
			Builder.OverrideAssetBundlesCompression = overrideAssetBundlesCompression;
		}
		
		/// <summary>
		/// Copies built asset bundles to the StreamingAssets folder.
		/// </summary>
		public static void CopyToStreamingAssets()
		{
			if(!string.IsNullOrEmpty(DistDirectory))
			{
				try
				{
					string srcDirectory = DistDirectory;
					string[] files = Directory.GetFiles(srcDirectory);
					if(srcDirectory.StartsWith("./"))
					{
						srcDirectory = srcDirectory.Substring(2); // makes entire string indifferent to absolute and relative paths
					}
					foreach(string sourceFilename in files)
					{
						string destFileName = Builder.TARGET_IN_STREAMING_ASSETS + sourceFilename.Substring(sourceFilename.IndexOf(srcDirectory) + srcDirectory.Length);
						ETUtils.CreateDirectoryForFile(destFileName);
						File.Copy(sourceFilename, destFileName, true);
					}
				}
				catch(Exception e)
				{
					// TODO: introduce logger like in the Builder
					// - to replace all Console.Write*;
					// - to inform TeamCity about the error (otherwise throw an exception).
					Console.WriteLine("Caught exception, while copying files to StreamingAssets: " + e.ToString());
				}
				AssetDatabase.Refresh(); // makes sure asset bundles would be included in the build
			}
			else
			{
				Console.WriteLine("WARNING. Can't copy artifacts, because nothing has been built by use of BuilderHelper.");
			}
		}

        /// <summary>
        /// Separate bundles into two folders: compressed and uncompressed
        /// </summary>
        public static void SeparateBundles()
        {
            if (!string.IsNullOrEmpty(DistDirectory))
            {
                try
                {
                    string srcDirectory = DistDirectory;
                    string[] files = Directory.GetFiles(srcDirectory);
                    if (srcDirectory.StartsWith("./"))
                    {
                        srcDirectory = srcDirectory.Substring(2); // makes entire string indifferent to absolute and relative paths
                    }

                    string destUnCompressedDir = srcDirectory + "/Uncompressed";

                    string destCompressedDir = srcDirectory + "/Compressed";

                    foreach (string sourceFilename in files)
                    {
                        string destFileName;
                        string srcFileName = Path.GetFileName(sourceFilename);
                        Console.WriteLine("Source file name = " + srcFileName);
                        if (Builder.uncompressedBundleList.Contains(srcFileName))
                        {
                            destFileName = Path.Combine(destUnCompressedDir, srcFileName);
                        }
                        else
                        {
                            destFileName = Path.Combine(destCompressedDir, srcFileName);
                        }

                        //index files are not in list, but they must exist in both folders.
                        if (srcFileName.StartsWith("index"))
                        {
                            string fn = Path.Combine(destUnCompressedDir, srcFileName);
                            ETUtils.CreateDirectoryForFile(fn);
                            File.Copy(sourceFilename, fn);
                        }

                        ETUtils.CreateDirectoryForFile(destFileName);
                        File.Move(sourceFilename, destFileName);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Caught exception, while copying files to StreamingAssets: " + e.Message);
                }
                AssetDatabase.Refresh(); // makes sure asset bundles would be included in the build
            }
            else
            {
                Console.WriteLine("WARNING. Can't copy artifacts, because nothing has been built by use of BuilderHelper.");
            }
        }
        #endregion
		#region Implementation
		
		private static string DistDirectory { set; get; }
		
		private static void InitDistDirectory(IBuildConfig buildConfig)
		{
			DistDirectory = "./dist/" + buildConfig.GetType().Name + "/AssetBundles";
		}
		
		private static void UpdateScenes(ref List<string> scenesToBuild)
		{
			List<string> scenes = new List<string>();
			scenes.Clear();
			scenes.AddRange(Builder.ExcludeScenesInAssetBundlesFrom(scenesToBuild.ToArray()));
			if(!string.IsNullOrEmpty(BootSceneFilename))
			{
				int idx = 0;
				while(idx < scenes.Count)
				{
					if(scenes[idx].ToLower().Equals(BootSceneFilename.ToLower()))
					{
						scenes.RemoveAt(idx);
					}
					else
					{
						++idx;
					}
				}
			}
			scenesToBuild.Clear();
			if(!string.IsNullOrEmpty(BootSceneFilename) && File.Exists(BootSceneFilename))
			{
				scenesToBuild.Add(BootSceneFilename);
			}
			scenesToBuild.AddRange(scenes);
		}
		
		#endregion
	}
}
