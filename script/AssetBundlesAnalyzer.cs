using UnityEditor;
using UnityEngine;

using System;
using System.Collections.Generic;
using System.IO;

namespace InJoy.AssetBundles
{
	using Internal;
	
	public static class Analyzer
	{
		#region Interface
		
		// TODO: comment it: produces a log for every index
		public static void LogAssetDependencies()
		{
			Index temp = Builder.index;
			Builder.index = null;
			Index[] indices = Builder.GetIndexInstances();
			Builder.index = temp;
			
			using(ETUtils.ProgressBar pb = new ETUtils.ProgressBar(null, "Please wait."))
			{
				for(int idx = 0; idx < indices.Length; ++idx)
				{
					Index index = indices[idx];
					string indexName = !string.IsNullOrEmpty(index.m_filename) ? Path.GetFileNameWithoutExtension(index.m_filename) : Path.GetFileNameWithoutExtension(index.m_xmlFilename);
					pb.Title = string.Format("Logging dependencies for the index \"{0}\"", indexName);
					pb.Progress = ((float)idx) / indices.Length;
					string filename = string.Format("dependenciesLog_{0}.txt", indexName);
					LogAssetDependencies(index, filename);
					++idx;
				}
			}
		}
		
		// TODO: comment it: gets distribution (which assets in which bundles) in reality
		public static Index ReorganizeDistribution(Index index)
		{
			Dictionary<string, Index.AssetBundle> userAssetsToBundles = GetUserDistribution(index);
			Dictionary<string, Index.AssetBundle> realAssetsToBundles = GetRealDistribution(userAssetsToBundles);
			Dictionary<string, List<string>> dist = new Dictionary<string, List<string>>();
			foreach(KeyValuePair<string, Index.AssetBundle> kvp in realAssetsToBundles)
			{
				if(!dist.ContainsKey(kvp.Value.m_filename))
				{
					dist.Add(kvp.Value.m_filename, new List<string>());
				}
				dist[kvp.Value.m_filename].Add(kvp.Key);
			}
			index = Index.DuplicateInstance(index);
			HashSet<Index.AssetBundle> checkedAssetBundles = new HashSet<Index.AssetBundle>();
			HashSet<Index.AssetBundle.Asset> checkedAssets = new HashSet<Index.AssetBundle.Asset>();
			foreach(KeyValuePair<string, List<string>> kvp in dist)
			{
				Index.AssetBundle assetBundle = null;
				foreach(Index.AssetBundle ab in index.m_assetBundles)
				{
					if(ab.m_filename.Equals(kvp.Key))
					{
						assetBundle = ab;
						break;
					}
				}
				Assertion.Check(assetBundle != null);
				if(assetBundle != null)
				{
					foreach(string val in kvp.Value)
					{
						Index.AssetBundle.Asset asset = null;
						foreach(Index.AssetBundle.Asset a in assetBundle.m_assets)
						{
							if(a.m_filename.Equals(val))
							{
								asset = a;
								break;
							}
						}
						if(asset == null)
						{
							asset = new Index.AssetBundle.Asset();
							asset.m_filename = val;
							asset.m_guid = AssetDatabase.AssetPathToGUID(asset.m_filename);
							assetBundle.m_assets.Add(asset);
						}
						asset.m_hash = File.Exists(asset.m_filename) ? ETUtils.CreateHashForAsset(asset.m_filename) : null;
						checkedAssets.Add(asset);
					}
					checkedAssetBundles.Add(assetBundle);
				}
			}
			foreach(Index.AssetBundle assetBundle in index.m_assetBundles)
			{
				if(checkedAssetBundles.Contains(assetBundle))
				{
					for(int idx = assetBundle.m_assets.Count - 1; idx >= 0; --idx)
					{
						if(!checkedAssets.Contains(assetBundle.m_assets[idx]))
						{
							assetBundle.m_assets.RemoveAt(idx);
						}
					}
				}
				else
				{
					assetBundle.m_assets.Clear();
				}
			}
			return index;
		}
		
		// TODO: comment it: computes hash of the asset bundle based on actually included assets only (not references!)
		public static string ComputeAssetBundleHashDependingOnRealDistribution(Index index, Index.AssetBundle assetBundle)
		{
			// reorganize distribution, so hash would be computed as they should,
			// i.e. based on real assets (and not their references) in the bundle
			Index userIndex = index;
			Index realIndex = ReorganizeDistribution(userIndex);
			return ComputeAssetBundleHashDependingOnProvidedDistribution(realIndex, assetBundle);
		}
		
		// TODO: comment it: computes hash of the asset bundle based on the specified assets
		public static string ComputeAssetBundleHashDependingOnProvidedDistribution(Index index, Index.AssetBundle assetBundle)
		{
			Index.AssetBundle userAssetBundle = assetBundle;
			Index.AssetBundle realAssetBundle = null;
			foreach(Index.AssetBundle ab in index.m_assetBundles)
			{
				if(ab.m_filename.Equals(userAssetBundle.m_filename))
				{
					realAssetBundle = ab;
					break;
				}
			}
			Assertion.Check(realAssetBundle != null); // SANITY CHECK
			
			// create a list contained user-defined assets (they could become just references)
			List<string> references = new List<string>();
			foreach(Index.AssetBundle.Asset asset in userAssetBundle.m_assets)
			{
				references.Add(asset.m_filename);
			}
			
			// now compute hash for the asset bundle
			string ret = "";
			foreach(Index.AssetBundle.Asset asset in realAssetBundle.m_assets)
			{
				string hash = File.Exists(asset.m_filename) ? ETUtils.CreateHashForAsset(asset.m_filename) : null; // hash of the content
				ret = ETUtils.HashXorHash(ret, hash);
			}
			foreach(string assetFilepath in references)
			{
				string hash = ETUtils.CreateHashForString(assetFilepath); // hash of explicit asset name
				ret = ETUtils.HashXorHash(ret, hash);
			}
			return ret; // return null to compute hashes with base (incorrent) algorithm
		}
		
		#endregion
		#region Implementation
		
		private static void LogAssetDependencies(Index index, string dependenciesLogFile)
		{
			try
			{
				using(StreamWriter streamWriter = new StreamWriter(dependenciesLogFile))
				{
					streamWriter.WriteLine("Report contains asset bundles content and asset dependencies");
					streamWriter.WriteLine("Conventional signs:");
					streamWriter.WriteLine("+  -  resource is actually in that bundle and was placed here in assets-to-bundle distribution");
					streamWriter.WriteLine("-  -  additional resource added by reference to that bundle");
					streamWriter.WriteLine("*  -  resource is in another bundle");
					streamWriter.WriteLine("#  -  resource is in bundle different from where it was placed in assets-to-bundle distribution");
					streamWriter.WriteLine();
					streamWriter.WriteLine();
					streamWriter.WriteLine();
					
					Dictionary<string, Index.AssetBundle> userAssetsToBundles = GetUserDistribution(index);
					Dictionary<string, Index.AssetBundle> realAssetsToBundles = GetRealDistribution(userAssetsToBundles);
					
					//fill dict that contains actual bundling
					Dictionary<string, List<string>> actualBundlesLayout = new Dictionary<string, List<string>>();
					foreach(KeyValuePair<string, Index.AssetBundle> kvp in realAssetsToBundles)
					{
						if(!actualBundlesLayout.ContainsKey(kvp.Value.m_filename))
							actualBundlesLayout.Add(kvp.Value.m_filename, new List<string>());
						
						actualBundlesLayout[kvp.Value.m_filename].Add(kvp.Key);
					}
					foreach(KeyValuePair<string, Index.AssetBundle> kvp in userAssetsToBundles)
					{
						if(!actualBundlesLayout.ContainsKey(kvp.Value.m_filename))
							actualBundlesLayout.Add(kvp.Value.m_filename, new List<string>());
						
						if(!actualBundlesLayout[kvp.Value.m_filename].Contains(kvp.Key))
							actualBundlesLayout[kvp.Value.m_filename].Add(kvp.Key);
					}
					
					//write info to file
					foreach(KeyValuePair<string, List<string>> kvp in actualBundlesLayout)
					{
						streamWriter.WriteLine("===" + kvp.Key + "===");
						foreach(string assetPath in kvp.Value)
						{
							string prefix = "";
							if(realAssetsToBundles[assetPath].m_filename.Equals(kvp.Key))
							{
								if(userAssetsToBundles.ContainsKey(assetPath) && (userAssetsToBundles[assetPath].m_filename.Equals(kvp.Key)))
									prefix = "+";
								else
									prefix = "-";
							}
							else
							{
								prefix = "#[" + realAssetsToBundles[assetPath].m_filename + "]";
							}
							WriteResourceInfo(streamWriter, prefix, assetPath);
							
							string[] dependencies = AssetDatabase.GetDependencies(new string[] { assetPath });
							foreach(string dependency in dependencies)
							{
								if(!dependency.Equals(assetPath) && MustBeInDependencyList(dependency))
								{
									if(!userAssetsToBundles.ContainsKey(dependency))
									{//not specified in asset bundles list
										WriteDependencyInfo(streamWriter, "-", dependency);
									}
									else
									{
										if(!userAssetsToBundles[dependency].m_filename.Equals(realAssetsToBundles[dependency].m_filename))
										{//actually is in another bundle than it was specified
											WriteDependencyInfo(streamWriter, "#[" + realAssetsToBundles[dependency].m_filename + "]", dependency);
										}
										else if(!userAssetsToBundles[dependency].m_filename.Equals(kvp.Key))
										{//not from current bundle but was specified correctly
											WriteDependencyInfo(streamWriter, "*[" + userAssetsToBundles[dependency].m_filename + "]", dependency);
										}
										else
										{//is in the same bundle
											WriteDependencyInfo(streamWriter, "+", dependency);
										}
									}
								}
							}
						}
					}
				}
				Debug.Log("Report saved to dependenciesLog.txt");
				AssetDatabase.Refresh();
			}
			catch(Exception e)
			{
				Debug.LogError(string.Format("Failed to log dependencies -  Caught exception: {0}", e.Message));
			}
		}
		
		private static Dictionary<string, Index.AssetBundle> GetRealDistribution(Dictionary<string, Index.AssetBundle> userAssetsToBundles)
		{
			Dictionary<string, Index.AssetBundle> realAssetsToBundles = new Dictionary<string, Index.AssetBundle>();
			
			//collect info about actual bundling
			foreach(KeyValuePair<string, Index.AssetBundle> kvp in userAssetsToBundles)
			{
				if(!realAssetsToBundles.ContainsKey(kvp.Key))
				{
					realAssetsToBundles.Add(kvp.Key, kvp.Value);
					
					List<string> childrenDependenciesList = new List<string>();
					CollectDependenciesRecursively(AssetDatabase.GetDependencies(new string[] { kvp.Key }), childrenDependenciesList);
					foreach(string asset in childrenDependenciesList)
					{
						if(!realAssetsToBundles.ContainsKey(asset))
						{
							realAssetsToBundles.Add(asset, kvp.Value);
						}
					}
				}
			}
			
			return realAssetsToBundles;
		}
		
		private static void CollectDependenciesRecursively(string[] dependencies, List<string> dependenciesCollector)
		{
			foreach(string dependency in dependencies)
			{
				if(!dependenciesCollector.Contains(dependency))
				{
					dependenciesCollector.Add(dependency);
					CollectDependenciesRecursively(AssetDatabase.GetDependencies(new string[] { dependency }), dependenciesCollector);
				}
			}
		}
		
		private static bool MustBeInDependencyList(string assetFilename)
		{
			return !assetFilename.EndsWith(".cs");
		}
		
		private static void WriteResourceInfo(StreamWriter streamWriter, string prefix, string path)
		{
			streamWriter.Write(prefix);
			streamWriter.Write(path);
			streamWriter.WriteLine();
		}
		
		private static void WriteDependencyInfo(StreamWriter streamWriter, string prefix, string path)
		{
			streamWriter.Write("\t");
			if(prefix != null)
				streamWriter.Write(prefix);
			streamWriter.Write(path);
			streamWriter.WriteLine();
		}
		
		private static Dictionary<string, Index.AssetBundle> GetUserDistribution(Index index)
		{
			Assertion.Check(index != null);
			Dictionary<string, Index.AssetBundle> assetsToBundles = new Dictionary<string, Index.AssetBundle>();
			
			foreach(Index.AssetBundle assetBundle in index.m_assetBundles)
			{
				foreach(Index.AssetBundle.Asset asset in assetBundle.m_assets)
				{
					if(!assetsToBundles.ContainsKey(asset.m_filename))
						assetsToBundles.Add(asset.m_filename, assetBundle);
				}
			}
			
			return assetsToBundles;
		}
		
		#endregion
	}
}
