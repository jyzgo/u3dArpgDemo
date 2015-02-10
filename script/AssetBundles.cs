using UnityEngine;

using System;
using System.Collections.Generic;
using System.IO;

namespace InJoy.AssetBundles
{
    using Internal;

    /// <summary>
    /// Allows to download, instantiate and unload asset bundles.
    /// </summary>
    public static partial class AssetBundles
    {
        #region Interface

        /// <summary>
        /// Sets handler of the situation asset bundles have been failed to load from the source.
        /// </summary>
        /// <value>
        /// Delegate.
        /// </value>
        public static Action<IndexInfo.Resolver> OnDownloadingFromSourceFailed
        {
            set { IndexInfo.OnDownloadingFromSourceFailed = value; }
        }

        /// <summary>
        /// Downloads all asset bundles from the specified URL.
        /// </summary>
        /// <returns>
        /// Info class instance or null, if an error occured.
        /// </returns>
        /// <param name='urls'>
        /// URLs to the same files from different places (mirrors).
        /// </param>
        public static IndexInfo DownloadAll(params string[] urls)
        {
            //Debug.Log("DownloadAll - Started");
            IndexInfo ret = IndexInfo.GetInstance(urls);
            //Debug.Log("DownloadAll - Finished");
            return ret;
        }

        /// <summary>
        /// Check, whether any of downloaded asset bundles contain the asset specified by assetPath.
        /// </summary>
        /// <param name='assetPath'>
        /// Asset path.
        /// </param>
        public static bool Contains(string assetPath)
        {
            //Debug.Log("Contains - Started");
            bool ret = false;
#if (UNITY_EDITOR || BUILD_MAC) && !FORCE_ASSET_BUNDLES_IN_EDITOR
            if (assetPath != null)
            {
                // in Editor just check actual place
                assetPath = assetPath.Replace('\\', '/');
                ret = File.Exists(assetPath);
                Assertion.Check(!ret || IsAssetInTheDistributions(assetPath), "You're trying to load asset \"{0}\" by use of the AssetBundles. Asset exists, but it is not included in the asset bundles pack.", assetPath);
            }
#else
			// check in asset bundles from every downloaded index
			foreach(IndexInfo indexInfo in IndexInfo.Instances)
			{
				ret = indexInfo.Contains(assetPath);
				if(ret)
				{
					break;
				}
			}
#endif
            //Debug.Log("Contains - Finished");
            return ret;
        }

        /// <summary>
        /// Load the asset specified by assetPath from any of loaded asset bundles.
        /// </summary>
        /// <param name='assetPath'>
        /// Asset path.
        /// </param>
        public static UnityEngine.Object Load(string assetPath)
        {
            //Debug.Log("Load - Started");
            UnityEngine.Object ret = null;
#if (UNITY_EDITOR || BUILD_MAC) && !FORCE_ASSET_BUNDLES_IN_EDITOR
            if (ret == null)
            {
                if (assetPath != null)
                {
                    // in Editor try to load from actual place
                    assetPath = assetPath.Replace('\\', '/');
                    ret = UnityEditor.AssetDatabase.LoadMainAssetAtPath(assetPath);
                    Assertion.Check((ret == null) || IsAssetInTheDistributions(assetPath), "You're trying to load asset \"{0}\" by use of the AssetBundles. Asset exists, but it is not included in the asset bundles pack.", assetPath);
                }
            }
#else
			if(ret == null)
			{
				// try to load from asset bundles
				foreach(IndexInfo indexInfo in IndexInfo.Instances)
				{
					ret = indexInfo.Load(assetPath);
					if(ret != null)
					{
						break;
					}
				}
			}
#endif
            if (ret == null)
            {
                //Debug.LogWarning(string.Format("Load - asset \"{0}\" was not found", assetPath));
            }
            //Debug.Log("Load - Finished");
            return ret;
        }

        /// <summary>
        /// Load the asset specified by assetPath from any of loaded asset bundles.
        /// </summary>
        /// <param name='assetPath'>
        /// Asset path.
        /// </param>
        public static T Load<T>(string assetPath) where T : UnityEngine.Object
        {
#pragma warning disable 0618
            return (T)Load(assetPath, typeof(T));
#pragma warning restore 0618
        }

        // TODO: [Obsolete("Use Load<T> instead.")]
        /// <summary>
        /// Load the asset specified by assetPath and assetType from any of loaded asset bundles.
        /// </summary>
        /// <param name='assetPath'>
        /// Asset path.
        /// </param>
        /// <param name='assetType'>
        /// Asset type.
        /// </param>
        public static UnityEngine.Object Load(string assetPath, Type assetType)
        {
            //Debug.Log("Load - Started");
            UnityEngine.Object ret = null;
#if (UNITY_EDITOR || BUILD_MAC) && !FORCE_ASSET_BUNDLES_IN_EDITOR
            if (ret == null)
            {
                if (assetPath != null)
                {
                    // in Editor try to load from actual place
                    assetPath = assetPath.Replace('\\', '/');
                    ret = UnityEditor.AssetDatabase.LoadAssetAtPath(assetPath, assetType);
                    Assertion.Check((ret == null) || IsAssetInTheDistributions(assetPath), "You're trying to load asset \"{0}\" by use of the AssetBundles. Asset exists, but it is not included in the asset bundles pack.", assetPath);
                }
            }
#else
			if(ret == null)
			{
				// try to load from asset bundles
				foreach(IndexInfo indexInfo in IndexInfo.Instances)
				{
					ret = indexInfo.Load(assetPath, assetType);
					if(ret != null)
					{
						break;
					}
				}
			}
#endif
            if (ret == null)
            {
                Debug.LogError(string.Format("Load - asset \"{0}\" of type \"{1}\" was not found", assetPath, assetType.Name));
            }
            //Debug.Log("Load - Finished");
            return ret;
        }

        /// <summary>
        /// Load asynchronously the asset specified by assetPath from any of loaded asset bundles.
        /// </summary>
        /// <param name='assetPath'>
        /// Asset path.
        /// </param>
        /// <param name='callback'>
        /// (Optionally) Callback(s) to invoke on operation being finished.
        /// </param>
        public static AssetBundlesOperation LoadAsync(string assetPath, params Action<UnityEngine.Object>[] callbacks)
        {
#pragma warning disable 0618
            // use the same solution as in case of AssetBundle.Load(string) Unity does
            // please look into assembly browser for the details
            return LoadAsync(assetPath, typeof(UnityEngine.Object), callbacks);
#pragma warning restore 0618
        }

        /// <summary>
        /// Load asynchronously the asset specified by assetPath from any of loaded asset bundles.
        /// </summary>
        /// <param name='assetPath'>
        /// Asset path.
        /// </param>
        /// <param name='callback'>
        /// (Optionally) Callback(s) to invoke on operation being finished.
        /// </param>
        public static AssetBundlesOperation<T> LoadAsync<T>(string assetPath, params Action<T>[] callbacks) where T : UnityEngine.Object
        {
            //Debug.Log("LoadAsync - Started");
            AssetBundlesOperation<T> ret = null;
            Type assetType = typeof(T);
#if (UNITY_EDITOR || BUILD_MAC) && !FORCE_ASSET_BUNDLES_IN_EDITOR
            T asset = null;
            if (ret == null)
            {
                if (assetPath != null)
                {
                    // in Editor try to load from actual place
                    assetPath = assetPath.Replace('\\', '/');
                    asset = UnityEditor.AssetDatabase.LoadAssetAtPath(assetPath, assetType) as T;
                    Assertion.Check((asset == null) || IsAssetInTheDistributions(assetPath), "You're trying to load asset \"{0}\" by use of the AssetBundles. Asset exists, but it is not included in the asset bundles pack.", assetPath);
                }
            }
            if (asset == null)
            {
                //Debug.LogWarning(string.Format("LoadAsync - asset \"{0}\" of type \"{1}\" was not found", assetPath, assetType.Name));
            }
            ret = new AssetBundlesOperation<T>(asset, callbacks);
#else
			if(ret == null)
			{
				// try to load from asset bundles
				foreach(IndexInfo indexInfo in IndexInfo.Instances)
				{
					AssetBundleRequest assetBundleRequest = indexInfo.LoadAsync(assetPath, assetType);
					if(assetBundleRequest != null)
					{
						ret = new AssetBundlesOperation<T>(assetBundleRequest, callbacks);
						break;
					}
				}
			}
			if(ret == null)
			{
				//Debug.LogWarning(string.Format("LoadAsync - asset \"{0}\" of type \"{1}\" was not found", assetPath, assetType.Name));
				ret = new AssetBundlesOperation<T>((T)null, callbacks);
			}
#endif
            Assertion.Check(ret != null, "LoadAsync must always return an instance!");
            //Debug.Log("LoadAsync - Finished");
            return ret;
        }

        // TODO: [Obsolete("Use LoadAsync<T> instead.")]
        /// <summary>
        /// Load asynchronously the asset specified by assetPath and assetType from any of loaded asset bundles.
        /// </summary>
        /// <param name='assetPath'>
        /// Asset path.
        /// </param>
        /// <param name='assetType'>
        /// Asset type.
        /// </param>
        /// <param name='callback'>
        /// (Optionally) Callback(s) to invoke on operation being finished.
        /// </param>
        public static AssetBundlesOperation LoadAsync(string assetPath, Type assetType, params Action<UnityEngine.Object>[] callbacks)
        {
            //Debug.Log("LoadAsync - Started");
            AssetBundlesOperation ret = null;
#if (UNITY_EDITOR || BUILD_MAC) && !FORCE_ASSET_BUNDLES_IN_EDITOR
            UnityEngine.Object asset = null;
            if (ret == null)
            {
                if (assetPath != null)
                {
                    // in Editor try to load from actual place
                    assetPath = assetPath.Replace('\\', '/');
                    asset = UnityEditor.AssetDatabase.LoadAssetAtPath(assetPath, assetType);
                    Assertion.Check((asset == null) || IsAssetInTheDistributions(assetPath), "You're trying to load asset \"{0}\" by use of the AssetBundles. Asset exists, but it is not included in the asset bundles pack.", assetPath);
                }
            }
            if (asset == null)
            {
                //Debug.LogWarning(string.Format("LoadAsync - asset \"{0}\" of type \"{1}\" was not found", assetPath, assetType.Name));
            }
            ret = new AssetBundlesOperation(asset, callbacks);
#else
			if(ret == null)
			{
				// try to load from asset bundles
				foreach(IndexInfo indexInfo in IndexInfo.Instances)
				{
					AssetBundleRequest assetBundleRequest = indexInfo.LoadAsync(assetPath, assetType);
					if(assetBundleRequest != null)
					{
						ret = new AssetBundlesOperation(assetBundleRequest, callbacks);
						break;
					}
				}
			}
			if(ret == null)
			{
				//Debug.LogWarning(string.Format("LoadAsync - asset \"{0}\" of type \"{1}\" was not found", assetPath, assetType.Name));
				ret = new AssetBundlesOperation((UnityEngine.Object)null, callbacks);
			}
#endif
            Assertion.Check(ret != null, "LoadAsync must always return an instance!");
            //Debug.Log("LoadAsync - Finished");
            return ret;
        }

        /// <summary>
        /// Unloads all loaded asset bundles and may unload instantiated objects by request.
        /// </summary>
        /// <param name='unloadAllLoadedObjects'>
        /// Whether all instantiated from asset bundles objects should be destroyed, too.
        /// </param>
        public static void UnloadAll(bool unloadAllLoadedObjects)
        {
            //Debug.Log("UnloadAll - Started");
            foreach (IndexInfo indexInfo in IndexInfo.Instances)
            {
                indexInfo.UnloadAll(unloadAllLoadedObjects);
            }
            //Debug.Log("UnloadAll - Finished");
        }

        #endregion
        #region Implementation

#if (UNITY_EDITOR || BUILD_MAC)
        private static bool IsAssetInTheDistributions(string assetPath)
        {
            //Debug.Log("IsAssetInTheDistributions - Started");
            if (m_assetsToBundles == null)
            {
                //Debug.Log("IsAssetInTheDistributions - Initializing list of the distributions...");
                m_assetsToBundles = new SortedList<string, string>();
                m_assetsToBundles.Clear();
                const string kDistributionsDirectory = "Assets/Editor/Conf/AssetBundles";
                string[] distributions = Directory.Exists(kDistributionsDirectory) ? Directory.GetFiles(kDistributionsDirectory, "*.xml", SearchOption.TopDirectoryOnly) : new string[] { };
                foreach (string indexFilename in distributions)
                {
                    using (FileStream fs = new FileStream(indexFilename, FileMode.Open))
                    {
                        Index index = Index.LoadInstance(fs);
                        Assertion.Check((index != null) && (index.m_assetBundles != null));
                        if ((index != null) && (index.m_assetBundles != null))
                        {
                            foreach (Index.AssetBundle assetBundle in index.m_assetBundles)
                            {
                                Assertion.Check(assetBundle.m_assets != null);
                                if (assetBundle.m_assets != null)
                                {
                                    string val = assetBundle.m_filename.ToLower();
                                    foreach (Index.AssetBundle.Asset asset in assetBundle.m_assets)
                                    {
                                        Assertion.Check((asset != null) && (!string.IsNullOrEmpty(asset.m_filename)));
                                        if ((asset != null) && (!string.IsNullOrEmpty(asset.m_filename)))
                                        {
                                            string key = asset.m_filename.Replace('\\', '/').ToLower();
                                            if (m_assetsToBundles.ContainsKey(key))
                                            {
                                                // NOTE. Commented out. To prevent in case of following situation.
                                                // Most textures in CK2 are placed into two bundles. First is a
                                                // special texture bundle. Second is a just folder, contained
                                                // different assets, that's included texture. Hope, this would be
                                                // solved with upcoming Analyzer.
                                                //Assertion.Check(false, "Asset \"{0}\" is being tried to distribute in several asset bundles:\n\n{1}\n{2}\n\nMost likely it means, that the distribution is messed up.", asset.m_filename, m_assetsToBundles[key], assetBundle.m_filename);
                                                // UPD. In ver. 0.9.7 and later thanks to Analyzer
                                                // hashes are computed correctly independing on distribution.
                                                // This assertion doesn't make any sense anymore.
                                            }
                                            else
                                            {
                                                m_assetsToBundles.Add(key, val);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                //Debug.Log("IsAssetInTheDistributions - Initialized list of the distributions");
            }
            bool ret = (assetPath != null) && m_assetsToBundles.ContainsKey(assetPath.Replace('\\', '/').ToLower());
            //Debug.Log("IsAssetInTheDistributions - Finished");
            return ret;
        }

        private static SortedList<string, string> m_assetsToBundles = null;
#endif

        #endregion
    }
}
