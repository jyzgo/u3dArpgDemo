using UnityEngine;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using InJoy.UnityBuildSystem;

namespace InJoy.AssetBundles
{
    using Internal;

    /// <summary>
    /// The description of loading or loaded index (assets-to-bundles distribution).
    /// Instances would be created during the use of AssetBundles class.
    /// </summary>
    public class IndexInfo
    {
        #region Interface

        /// <summary>
        /// Describes general state of the downloading of asset bundles.
        /// </summary>
        public enum State
        {
            /// <summary>
            /// Means, downloading or trying to.
            /// </summary>
            InProgress,
            /// <summary>
            /// Successfully downloaded. To detect where asset bundles
            /// have been loaded from, use property "source".
            /// </summary>
            Succeeded,
            /// <summary>
            /// Failed to download from anywhere.
            /// </summary>
            Failed,
        }

        /// <summary>
        /// Describes source, asset bundles are being loaded from.
        /// </summary>
        public enum Source
        {
            /// <summary>
            /// From the internet.
            /// </summary>
            Online,
            /// <summary>
            /// From the cache (means, online version is unavailable).
            /// </summary>
            Cache,
            /// <summary>
            /// From the game package itself. Actually, it means something
            /// has happened with asset bundles in the cache.
            /// </summary>
            StreamingAssets,
            /// <summary>
            /// Means, every other source failed to be used.
            /// </summary>
            None,
        }

        /// <summary>
        /// Allows to make a decision, whether failed to load asset bundles should be
        /// tried one more time or should component just make a fallback to next source.
        /// </summary>
        public class Resolver
        {
            #region Interface

            public static Resolver CreateInstance(IndexInfo instanceToCatchCallback)
            {
                Resolver ret = null;
                if (instanceToCatchCallback != null)
                {
                    ret = new Resolver(instanceToCatchCallback);
                }
                return ret;
            }

            /// <summary>
            /// Call this to use next source to load asset bundles from.
            /// </summary>
            public void Fallback()
            {
                Assertion.Check(!m_isResolved);
                if (!m_isResolved)
                {
                    indexInfo.OnEntireFail_Fallback();
                    m_isResolved = true;
                }
            }

            /// <summary>
            /// Call this to retry to load asset bundles from the specified source.
            /// </summary>
            public void Retry()
            {
                Assertion.Check(!m_isResolved);
                if (!m_isResolved)
                {
                    indexInfo.OnEntireFail_Retry();
                    m_isResolved = true;
                }
            }

            #endregion
            #region Implementation

            private IndexInfo indexInfo { set; get; }

            private Resolver(IndexInfo indexInfo)
            {
                Assertion.Check(indexInfo != null);
                this.indexInfo = indexInfo;
                m_isResolved = false;
            }

            private bool m_isResolved;

            #endregion
        }

        /// <summary>
        /// Sets handler of the situation asset bundles have been failed to load from the source.
        /// </summary>
        /// <value>
        /// The delegate.
        /// </value>
        public static Action<Resolver> OnDownloadingFromSourceFailed { set; private get; }

        /// <summary>
        /// Gets the state of downloading asset bundles pack.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        public State state
        {
            get { return m_state; }
        }

        /// <summary>
        /// Gets actual source, where asset bundles are being loaded from.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public Source source
        {
            get { return m_source; }
        }

        /// <summary>
        /// Gets actual instances.
        /// </summary>
        /// <value>
        /// The array of instances.
        /// </value>
        public static IndexInfo[] Instances
        {
            get { return m_indexInfo.ToArray(); }
        }

        /// <summary>
        /// Gets the urls (mirrors), instance was instantiated with.
        /// </summary>
        /// <value>
        /// Array of the urls.
        /// </value>
        public string[] Urls
        {
            get { return m_urls; }
        }

        // TODO: [Obsolete("Use IndexDownloadInfo")]
        /// <summary>
        /// Gets information about the index (toc) file downloaded.
        /// </summary>
        /// <value>
        /// The description. Or null, if failed.
        /// </value>
        public DownloadManager.Info downloadInfo
        {
            get { return m_indexDownloadInfo; }
        }

        /// <summary>
        /// Gets build tag of the content.
        /// </summary>
        /// <value>
        /// The string contained build tag. Could be null.
        /// </value>
        public string BuildTag
        {
            get { return m_indexBuildTag; }
        }

        /// <summary>
        /// Gets hash of the content. It could be considered as a version
        /// of used asset bundles pack.
        /// </summary>
        /// <value>
        /// The string contained hash. Or null, if failed.
        /// </value>
        public string ContentHash
        {
            get { return m_indexHash; }
        }

        /// <summary>
        /// Gets hash of the previously cached content.
        /// </summary>
        /// <value>
        /// The string contained hash. Or null, if failed.
        /// </value>
        public string CachedContentHash
        {
            get { return m_cachedIndexHash; }
        }

        /// <summary>
        /// Gets information about every asset bundle from the pack.
        /// </summary>
        /// <value>
        /// Array of the descriptions.
        /// </value>
        public AssetBundleInfo[] assetBundleInfo
        {
            get { return m_assetBundleInfo.ToArray(); }
        }

        /// <summary>
        /// Retrieves the instance of the class to follow downloading
        /// of asset bundles up. If you want to start download asset bundles,
        /// then use function AssetBundles.DownloadAll.
        /// </summary>
        public static IndexInfo GetInstance(params string[] urls)
        {
            Debug.Log("GetInstance - Started");
            IndexInfo ret = null;
            if ((urls != null) && (urls.Length > 0))
            {
                // ensure, that links are valid
                for (int idx = 0; idx < urls.Length; ++idx)
                {
                    urls[idx] = urls[idx].Replace('\\', '/');
                    if (urls[idx].LastIndexOf('/') == urls[idx].Length - 1) // ensures this is not folder
                    {
                        urls[idx] = urls[idx].Remove(urls[idx].Length - 1);
                    }
                    if (urls[idx].LastIndexOf('/') < urls[idx].LastIndexOf(".")) // ensures there are no extentions
                    {
                        urls[idx] = urls[idx].Remove(urls[idx].LastIndexOf("."));
                    }
                    if (!urls[idx].Contains("://")) // ensures protocol is specified explicitly
                    {
                        urls[idx] = "file://" + urls[idx];
                    }
                }

                // retrieve index
                ret = FindIndexInfoByUrls(urls);
                if (ret == null)
                {
                    ret = new IndexInfo(urls);
                    ret.DownloadAll(); // start downloads automatically
                    Assertion.Check(ret.m_state != State.Failed);
                    m_indexInfo.Add(ret);
                    Debug.Log(string.Format("Added new index \"{0}\" to the list", urls[0]));
                }
                else
                {
                    Assertion.Check(ret.m_state != State.InProgress);
                    ret.m_requestedUrls = urls;
                    if (ret.m_state != State.InProgress)
                    {
                        ret.DownloadAll(); // start downloads automatically
                        Debug.Log(string.Format("Index from \"{0}\" is already requested, updated urls", urls[0]));
                    }
                    else
                    {
                        Debug.LogWarning("Detected attempt to change urls, while asset bundles is being downloaded");
                    }
                }
            }
            Debug.Log("GetInstance - Finished");
            return ret;
        }

        /// <summary>
        /// Starts downloading of all asset bundles.
        /// </summary>
        public void DownloadAll()
        {
            Debug.Log("DownloadAll - Started");

            // always unload asset bundles if any
            UnloadAll(false);

            // clear everything after unloading
            m_versionDownloadInfo = null;
            m_indexDownloadInfo = null;
            m_assetBundleInfo.Clear();
            m_assetToBundle.Clear();

            // update urls
            m_urls = m_requestedUrls;

            // means, no loaded asset bundles currently
            m_indexBuildTag = null;
            m_indexVersionFilename = null;
            m_indexHash = null;
            m_cachedIndexHash = null;

            // start downloads
            m_state = State.InProgress;
            m_source = Source.Online;
            DownloadVersionFile();

            Debug.Log("DownloadAll - Finished");
        }

        /// <summary>
        /// Checks for updates.
        /// </summary>
        /// <returns>
        /// The instance to retrieve result later.
        /// </returns>
        public UpdateInfo CheckForUpdates()
        {
            Debug.Log("CheckForUpdates - Started");
            UpdateInfo ret = UpdateInfo.CreateInstance(GetArrayOfVersions(), m_indexHash);
            Debug.Log("CheckForUpdates - Finished");
            return ret;
        }

        /// <summary>
        /// Check, whether downloaded asset bundles contain the asset specified by assetPath.
        /// </summary>
        /// <param name='assetPath'>
        /// Asset path.
        /// </param>
        public bool Contains(string assetPath)
        {
            Debug.Log("Contains - Started");
            bool ret = false;
            if (m_state == State.Succeeded)
            {
                Debug.Log(string.Format("Contains - Looking for \"{0}\"", assetPath ?? "null"));
                if (assetPath != null)
                {
                    // try to load asset from the destined bundle first
                    AssetBundleInfo bundleInfo = null;
                    assetPath = assetPath.Replace('\\', '/').ToLower();
                    if (m_assetToBundle.TryGetValue(assetPath, out bundleInfo))
                    {
                        Debug.Log(string.Format("Contains - Found in bundle \"{0}\"", bundleInfo.Filename));
                        if (bundleInfo != null)
                        {
                            AssetBundle assetBundle = bundleInfo.assetBundle;
                            if (assetBundle != null)
                            {
                                ret = assetBundle.Contains(assetPath);
                            }
                        }
                    }
                    if (!ret)
                    {
                        // look for the asset in all available asset bundles
                        // TODO: do we need this feature?
                        //foreach(AssetBundleInfo bundleInfo in assetBundleInfo)
                        //{
                        //	AssetBundle assetBundle = bundleInfo.assetBundle;
                        //	if(assetBundle != null)
                        //	{
                        //		ret = assetBundle.Contains(assetPath);
                        //		if(ret)
                        //		{
                        //			break;
                        //		}
                        //	}
                        //}
                    }
                }
            }
            Debug.Log("Contains - Finished");
            return ret;
        }

        /// <summary>
        /// Load the asset specified by assetPath from loaded asset bundles.
        /// </summary>
        /// <param name='assetPath'>
        /// Asset path.
        /// </param>
        public UnityEngine.Object Load(string assetPath)
        {
            Debug.Log("Load - Started");
            UnityEngine.Object ret = null;
            if (m_state == State.Succeeded)
            {
                Debug.Log(string.Format("Load - Looking for \"{0}\"", assetPath ?? "null"));
                if (assetPath != null)
                {
                    if (ret == null)
                    {
                        // try to load asset from destined bundle first
                        AssetBundleInfo bundleInfo = null;
                        assetPath = assetPath.Replace('\\', '/').ToLower();
                        if (m_assetToBundle.TryGetValue(assetPath, out bundleInfo))
                        {
                            Debug.Log(string.Format("Load - Found in bundle \"{0}\"", bundleInfo.Filename));
                            if (bundleInfo != null)
                            {
                                AssetBundle assetBundle = bundleInfo.assetBundle;
                                if (assetBundle != null)
                                {
                                    ret = assetBundle.Load(assetPath);
                                }
                            }
                            if (ret == null)
                            {
                                Debug.LogError(string.Format("Failed to load asset \"{0}\" from destinated asset bundle", assetPath));
                            }
                        }
                    }
                    if (ret == null)
                    {
                        // look for the asset in all available asset bundles
                        // TODO: do we need this feature?
                        //foreach(AssetBundleInfo bundleInfo in assetBundleInfo)
                        //{
                        //	AssetBundle assetBundle = bundleInfo.assetBundle;
                        //	if(assetBundle != null)
                        //	{
                        //		ret = assetBundle.Load(assetPath);
                        //		if(ret != null)
                        //		{
                        //			break;
                        //		}
                        //	}
                        //}
                    }
                }
            }
            Debug.Log("Load - Finished");
            return ret;
        }

        /// <summary>
        /// Load the asset specified by assetPath and assetType from loaded asset bundles.
        /// </summary>
        /// <param name='assetPath'>
        /// Asset path.
        /// </param>
        /// <param name='assetType'>
        /// Asset type.
        /// </param>
        public UnityEngine.Object Load(string assetPath, Type assetType)
        {
            Debug.Log("Load - Started");
            UnityEngine.Object ret = null;
            if (m_state == State.Succeeded)
            {
                Debug.Log(string.Format("Load - Looking for \"{0}\" of type \"{1}\"", assetPath ?? "null", (assetType != null) ? assetType.ToString() : "null"));
                if (assetPath != null)
                {
                    if (ret == null)
                    {
                        // try to load asset from destined bundle first
                        AssetBundleInfo bundleInfo = null;
                        assetPath = assetPath.Replace('\\', '/').ToLower();
                        if (m_assetToBundle.TryGetValue(assetPath, out bundleInfo))
                        {
                            Debug.Log(string.Format("Load - Found in bundle \"{0}\"", bundleInfo.Filename));
                            if (bundleInfo != null)
                            {
                                AssetBundle assetBundle = bundleInfo.assetBundle;
                                if (assetBundle != null)
                                {
                                    ret = assetBundle.Load(assetPath, assetType);
                                }
                            }
                            if (ret == null)
                            {
                                Debug.LogError(string.Format("Failed to load asset \"{0}\" of type \"{1}\" from destinated asset bundle", assetPath, assetType.Name));
                            }
                        }
                    }
                    if (ret == null)
                    {
                        // look for the asset in all available asset bundles
                        // TODO: do we need this feature?
                        //foreach(AssetBundleInfo bundleInfo in assetBundleInfo)
                        //{
                        //	AssetBundle assetBundle = bundleInfo.assetBundle;
                        //	if(assetBundle != null)
                        //	{
                        //		ret = assetBundle.Load(assetPath, assetType);
                        //		if(ret != null)
                        //		{
                        //			break;
                        //		}
                        //	}
                        //}
                    }
                }
            }
            Debug.Log("Load - Finished");
            return ret;
        }

        /// <summary>
        /// Load asynchronously the asset specified by assetPath from loaded asset bundles.
        /// </summary>
        /// <param name='assetPath'>
        /// Asset path.
        /// </param>
        public AssetBundleRequest LoadAsync(string assetPath)
        {
            // use the same solution as in case of AssetBundle.Load(string) Unity does
            // please look into assembly browser for the details
            return LoadAsync(assetPath, typeof(UnityEngine.Object));
        }

        /// <summary>
        /// Load asynchronously the asset specified by assetPath and assetType from loaded asset bundles.
        /// </summary>
        /// <param name='assetPath'>
        /// Asset path.
        /// </param>
        /// <param name='assetType'>
        /// Asset type.
        /// </param>
        public AssetBundleRequest LoadAsync(string assetPath, Type assetType)
        {
            Debug.Log("LoadAsync - Started");
            AssetBundleRequest ret = null;
            if (m_state == State.Succeeded)
            {
                Debug.Log(string.Format("LoadAsync - Looking for \"{0}\" of type \"{1}\"", assetPath ?? "null", (assetType != null) ? assetType.ToString() : "null"));
                if (assetPath != null)
                {
                    if (ret == null)
                    {
                        // try to load asset from destined bundle first
                        AssetBundleInfo bundleInfo = null;
                        assetPath = assetPath.Replace('\\', '/').ToLower();
                        if (m_assetToBundle.TryGetValue(assetPath, out bundleInfo))
                        {
                            Debug.Log(string.Format("LoadAsync - Found in bundle \"{0}\"", bundleInfo.Filename));
                            if (bundleInfo != null)
                            {
                                AssetBundle assetBundle = bundleInfo.assetBundle;
                                if (assetBundle != null)
                                {
                                    ret = assetBundle.LoadAsync(assetPath, assetType);
                                }
                            }
                            if (ret == null)
                            {
                                Debug.LogError(string.Format("Failed to load asset \"{0}\" of type \"{1}\" from destinated asset bundle", assetPath, assetType.Name));
                            }
                        }
                    }
                    if (ret == null)
                    {
                        // look for the asset in all available asset bundles
                        // TODO: is this feature even possible? LoadAsync() always return an instance
                    }
                }
            }
            Debug.Log("LoadAsync - Finished");
            return ret;
        }

        /// <summary>
        /// Unloads loaded asset bundles and may unload instantiated objects by request.
        /// </summary>
        /// <param name='unloadAllLoadedObjects'>
        /// Whether all instantiated from asset bundles objects should be destroyed, too.
        /// </param>
        public void UnloadAll(bool unloadAllLoadedObjects)
        {
            Debug.Log("UnloadAll - Started");
            // unload all loaded asset bundles and do not load them further
            foreach (AssetBundleInfo bundleInfo in assetBundleInfo)
            {
                // mark download as to be skipped
                DownloadManager.Info downloadInfo = bundleInfo.downloadInfo;
                downloadInfo.Skip();

                // unload entire asset bundle
                AssetBundle assetBundle = downloadInfo.assetBundle;
                if (assetBundle != null)
                {
                    try
                    {
                        assetBundle.Unload(unloadAllLoadedObjects);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning(string.Format("UnloadAll - On unloading asset bundle \"{0}\" caught exception: {1}", bundleInfo.Filename, e.ToString()));
                    }
                }
            }

            // index asset bundle should be unloaded, too
            {
                if (m_indexDownloadInfo != null)
                {
                    if (m_indexDownloadInfo.assetBundle != null)
                    {
                        // mark download as to be skipped
                        m_indexDownloadInfo.Skip();

                        // unload entire asset bundle
                        AssetBundle assetBundle = m_indexDownloadInfo.assetBundle;
                        if (assetBundle != null)
                        {
                            try
                            {
                                assetBundle.Unload(unloadAllLoadedObjects);
                            }
                            catch (Exception e)
                            {
                                Debug.LogWarning(string.Format("UnloadAll - On unloading index asset bundle caught exception: {0}", e.ToString()));
                            }
                        }
                    }
                }
            }
            Debug.Log("UnloadAll - Finished");
        }

        #endregion
        #region Implementation

        private static IndexInfo FindIndexInfoByUrls(params string[] urls)
        {
            Debug.Log("FindIndexByUrl - Started");
            IndexInfo ret = null;
            foreach (IndexInfo indexInfo in m_indexInfo)
            {
                foreach (string indexUrl in indexInfo.Urls)
                {
                    string indexName = Path.GetFileName(indexUrl);
                    foreach (string url in urls)
                    {
                        string filename = Path.GetFileName(url);
                        if (indexName.ToLower().Equals(filename.ToLower()))
                        {
                            ret = indexInfo;
                            break;
                        }
                    }
                    if (ret != null)
                    {
                        break;
                    }
                }
                if (ret != null)
                {
                    break;
                }
            }
            Debug.Log("FindIndexByUrl - Finished");
            return ret;
        }

        private static void ParseUrlWithCloudFront(string url, out string baseUrl, out string filename)
        {
            ParseUrl(url, out baseUrl, out filename);
            /*
                        Debug.Log("ParseUrlWithCloudFront - Started");
                        string tempBaseUrl;
                        string tempFilename;
                        ParseUrl(url, out tempBaseUrl, out tempFilename);
                        if(DynamicContentPipeline.DynamicContent.IsUseCloudFrontAddress && tempBaseUrl != null)
                        {
                            baseUrl = tempBaseUrl.Replace(DynamicContentPipeline.DynamicContent.CloudFrontBaseAddress, DynamicContentPipeline.DynamicContent.CloudFrontTargetAddress);
                        }
                        else
                        {
                            baseUrl = tempBaseUrl;
                        }
                        filename = tempFilename;
                        Debug.Log("ParseUrlWithCloudFront - New URL is " + baseUrl);
                        Debug.Log("ParseUrlWithCloudFront - end");
            */
        }

        private static void ParseUrl(string url, out string baseUrl, out string filename)
        {
            Debug.Log("ParseUrl - Started");
            if (!string.IsNullOrEmpty(url))
            {
                //url = url.Replace('\\', '/');
                int idx = url.LastIndexOf('/');
                baseUrl = url.Remove(idx + 1);
                filename = url.Substring(idx + 1);
                idx = filename.IndexOf("?");
                if (idx >= 0)
                {
                    filename = filename.Remove(idx);
                }
            }
            else
            {
                baseUrl = null;
                filename = null;
            }
            Debug.Log("ParseUrl - Finished");
        }

        private static string GetPathToCache()
        {
            Debug.Log("GetPathToCache - Started");
            string ret = Application.temporaryCachePath;
            if (string.IsNullOrEmpty(ret))
            {
                ret = ".";
            }
            ret += '/';
            if (!string.IsNullOrEmpty(BuildInfo.buildTag))
            {
                ret += (BuildInfo.buildTag + '/');
            }
            Debug.Log(string.Format("GetPathToCache returns \"{0}\"", ret));
            Debug.Log("GetPathToCache - Finished");
            return ret;
        }

        private static string GetBaseUrlToCache()
        {
            Debug.Log("GetBaseUrlToCache - Started");
            string ret = "file://" + GetPathToCache();
            Debug.Log("GetBaseUrlToCache - Finished");
            return ret;
        }

        private static string GetBaseUrlToStreamingAssets()
        {
            Debug.Log("GetBaseUrlToStreamingAssets - Started");
            string ret = null;
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    ret = "jar:file://" + Application.dataPath + "!/assets/AssetBundles/";
                    break;
                case RuntimePlatform.IPhonePlayer:
                    ret = "file://" + Application.dataPath + "/Raw/AssetBundles/";
                    break;
                case RuntimePlatform.OSXPlayer:
                    ret = "file://" + Application.dataPath + "/Data/StreamingAssets/AssetBundles/";
                    break;
                case RuntimePlatform.WindowsPlayer:
                    ret = "file://" + Application.dataPath + "/StreamingAssets/AssetBundles/";
                    break;
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.WindowsEditor:
                    ret = "file://" + Application.dataPath + "/StreamingAssets/AssetBundles/";
                    break;
                default:
                    // SANITY CHECK
                    Assertion.Check(false, "Internal class error: platform \"{0}\" isn't supported", Application.platform);
                    break;
            }
            Debug.Log("GetBaseUrlToStreamingAssets - Finished");
            return ret;
        }

        private static string[] AdjustUrlsToSource(Source source, params string[] urls)
        {
            Debug.Log("AdjustUrlsToSource - Started");
            string[] ret = null;
            string baseUrl;
            string filename;
            switch (source)
            {
                case Source.Online:
                    ret = urls;
                    break;
                case Source.Cache:
                    ParseUrl(urls[0], out baseUrl, out filename);
                    ret = new string[] { GetBaseUrlToCache() + filename };
                    break;
                case Source.StreamingAssets:
                    ParseUrl(urls[0], out baseUrl, out filename);
                    ret = new string[] { GetBaseUrlToStreamingAssets() + filename };
                    break;
                case Source.None:
                    ret = urls;
                    break;
                default:
                    // SANITY CHECK
                    Assertion.Check(false, "Internal class error: current source \"{0}\" is unknown", source);
                    break;
            }
            Debug.Log("AdjustUrlsToSource - Finished");
            return ret;
        }

        private IndexInfo(string[] urls)
        {
            Debug.Log("Ctor - Started");
            m_requestedUrls = urls;
            m_urls = null;
            m_versionDownloadInfo = null;
            m_indexDownloadInfo = null;
            m_assetBundleInfo = new List<AssetBundleInfo>();
            m_assetBundleInfo.Clear();
            m_state = State.Failed;
            m_source = Source.None;
            m_indexBuildTag = null;
            m_indexVersionFilename = null;
            m_indexHash = null;
            m_cachedIndexHash = null;
            m_assetToBundle = new Dictionary<string, AssetBundleInfo>();
            m_assetToBundle.Clear();
            Debug.Log("Ctor - Finished");
        }

        private string[] GetArrayOfAssetBundles(string relativePath)
        {
            Debug.Log("GetArrayOfAssetBundles - Started");
            string[] assetBundlesUrls = new string[m_urls.Length];
            for (int idx = 0; idx < m_urls.Length; ++idx)
            {
                string baseUrl;
                string indexFilename;
                ParseUrlWithCloudFront(m_urls[idx], out baseUrl, out indexFilename);
                assetBundlesUrls[idx] = baseUrl + relativePath;
                //assetBundlesUrls[idx*2] = AdjustUrlsToSource(Source.Cache, new string[]{baseUrl + relativePath})[0];
            }
            Debug.Log("GetArrayOfAssetBundles - Finished");
            return assetBundlesUrls;
        }

        private string[] GetArrayOfIndices()
        {
            Debug.Log("GetArrayOfIndices - Started");
            string[] indexUrls = new string[m_urls.Length];
            for (int idx = 0; idx < m_urls.Length; ++idx)
            {
                indexUrls[idx] = m_urls[idx] + "_" + m_indexHash + ".unity3d";
            }
            Debug.Log("GetArrayOfIndices - Finished");
            return indexUrls;
        }

        private string[] GetArrayOfVersions()
        {
            Debug.Log("GetArrayOfVersions - Started");
            string[] versionUrls = new string[m_urls.Length];
            for (int idx = 0; idx < m_urls.Length; ++idx)
            {
                versionUrls[idx] = m_urls[idx] + ".version";
            }
            Debug.Log("GetArrayOfVersions - Finished");
            return versionUrls;
        }

        private void SaveIndexVersionToCache()
        {
            Debug.Log("SaveIndexVersionToCache - Started");
            Assertion.Check(m_indexHash != null);
            if (m_indexHash != null)
            {
                try
                {
                    string filename = GetPathToCache() + m_indexVersionFilename;
                    Debug.Log(string.Format("SaveIndexVersionToCache - Save to \"{0}\"", filename));
                    string directory = Path.GetDirectoryName(filename);
                    if (!Directory.Exists(directory))
                    {
                        Debug.Log(string.Format("SaveIndexVersionToCache - No directory \"{0}\", creating...", directory));
                        try { Directory.CreateDirectory(directory); }
                        catch (Exception) { }
                    }
                    byte[] bytes = new ASCIIEncoding().GetBytes(m_indexHash);
                    StorageManager.WriteToLocation(filename, filename, bytes);
#if UNITY_IPHONE && !UNITY_EDITOR
					// File must not be removed by iOS. So the plan is to either
					// 1) place file into <Documents> folder without iCloud/iTunes backup
					// 2) place file into <Caches> folder without iCloud/iTunes backup (side effect)
					iPhone.SetNoBackupFlag(filename); // 2nd way
					string checkFilename = filename + ".check"; // HACK: hardcoded
					if(File.Exists(checkFilename))
					{
						iPhone.SetNoBackupFlag(checkFilename);
					}
#endif
                }
                catch (Exception e)
                {
                    Debug.LogError(string.Format("SaveIndexVersionToCache - Caught exception: {0}", e.ToString()));
                }
            }
            Debug.Log("SaveIndexVersionToCache - Finished");
        }

        private DownloadManager.OnDownloadCallbackType onDownloadVersionFile
        {
            get { return (m_onDownloadVersionFile != null) ? m_onDownloadVersionFile : (m_onDownloadVersionFile = new DownloadManager.OnDownloadCallbackType(OnDownloadVersionFile)); }
        }

        private DownloadManager.OnDownloadCallbackType onDownloadIndexFile
        {
            get { return (m_onDownloadIndexFile != null) ? m_onDownloadIndexFile : (m_onDownloadIndexFile = new DownloadManager.OnDownloadCallbackType(OnDownloadIndexFile)); }
        }

        private DownloadManager.OnDownloadCallbackType onDownloadAssetBundleFile
        {
            get { return (m_onDownloadAssetBundleFile != null) ? m_onDownloadAssetBundleFile : (m_onDownloadAssetBundleFile = new DownloadManager.OnDownloadCallbackType(OnDownloadAssetBundleFile)); }
        }

        private void OnEntireSuccess()
        {
            Debug.Log("OnEntireSuccess - Started");
            m_versionDownloadInfo = null; // unnecessary
            if (m_source != Source.Cache) // save version file to the cache, if it was not loaded from
            {
                SaveIndexVersionToCache();
            }
            m_state = State.Succeeded;
            Debug.Log("Index has been loaded and deployed successfully");
            Debug.Log("OnEntireSuccess - Finished");
        }

        private void OnEntireFail()
        {
            Debug.Log("OnEntireFail - Started");
            // IMPORTANT. This function is invoked on missed *.version file as well.
            // If no asset bundles are deployed online, then handler would be invoked.
            // To prevent from that, check, whether fail happened on asset bundle.
            // And only if so, then invoke callback function.
            bool isFailOnAssetBundle = !string.IsNullOrEmpty(m_indexHash);

            // unload all loaded asset bundles
            UnloadAll(true);

            // clear everything after unloading
            m_indexDownloadInfo = null;
            m_assetBundleInfo.Clear();
            m_assetToBundle.Clear();

            // no valid-version-on-sight anymore
            m_indexBuildTag = null;
            m_indexVersionFilename = null;
            m_indexHash = null;
            // m_cachedIndexHash must not be reset

            // choose next source to load index and asset bundles
            if (isFailOnAssetBundle && (m_source == Source.Online) && (OnDownloadingFromSourceFailed != null))
            {
                try
                {
                    OnDownloadingFromSourceFailed(Resolver.CreateInstance(this));
                }
                catch (Exception e)
                {
                    Debug.LogError(string.Format("OnEntireFail - OnDownloadingFromSourceFailed thrown exception: {0}", e.ToString()));
                    // do not re-throw exception here, do nothing
                }
            }
            else
            {
                OnEntireFail_Fallback();
            }

            Debug.Log("OnEntireFail - Finished");
        }

        private void OnEntireFail_Fallback()
        {
            Debug.Log("OnEntireFail_Fallback - Started");
            m_versionDownloadInfo = null; // unnecessary
            switch (m_source)
            {
                case Source.Online:
                //m_source = Source.Cache;
                //DownloadVersionFile();
                //break;
                case Source.Cache:
                //m_source = Source.StreamingAssets;
                //DownloadVersionFile();
                //break;
                case Source.StreamingAssets:
                    // here are our last words to failed-to-download index
                    m_source = Source.None;
                    m_state = State.Failed;
                    Debug.LogWarning("Index has been failed to download");
                    break;
                case Source.None:
                default:
                    // SANITY CHECK
                    Assertion.Check(false, "Internal class error: current source \"{0}\" is unknown", m_source);
                    break;
            }
            Debug.Log("OnEntireFail_Fallback - Finished");
        }

        private void OnEntireFail_Retry()
        {
            Debug.Log("OnEntireFail_Retry - Started");
            bool ret = OnDownloadVersionFile(m_versionDownloadInfo);
            Assertion.Check(ret);
            if (!ret)
            {
                OnEntireFail_Fallback();
            }
            Debug.Log("OnEntireFail_Retry - Finished");
        }

        private bool OnDownloadAssetBundleFile(DownloadManager.Info downloadInfo)
        {
            Debug.Log("OnDownloadAssetBundleFile - Started");
            Debug.Log(string.Format("OnDownloadAssetBundleFile - Source is \"{0}\"", m_source));
            bool ret = false;
            if (downloadInfo.state == DownloadManager.Info.State.Downloaded)
            {
                Debug.Log("OnDownloadAssetBundleFile - Downloaded successfully");
                ret = true; // confirms success
            }
            else
            {
                Debug.Log("OnDownloadAssetBundleFile - Failed to download, skipping other asset bundles");
                foreach (AssetBundleInfo bundleInfo in assetBundleInfo)
                {
                    bundleInfo.downloadInfo.Skip();
                }
            }
            // check, whether all requested asset bundles were processed
            bool finished = true;
            bool succeeded = true;
            foreach (AssetBundleInfo bundleInfo in assetBundleInfo)
            {
                switch (bundleInfo.downloadInfo.state)
                {
                    // is some asset bundle still in progress?
                    case DownloadManager.Info.State.NotStarted:
                    case DownloadManager.Info.State.Pending:
                    case DownloadManager.Info.State.Sending:
                    case DownloadManager.Info.State.Receiving:
                    case DownloadManager.Info.State.WaitingBeforeRetry:
                        finished = false;
                        succeeded = false;
                        break;
                    // is asset bundle failed?
                    case DownloadManager.Info.State.Abandoned:
                    case DownloadManager.Info.State.DoNotDownload:
                        succeeded = false;
                        break;
                    // is asset bundle succeeded?
                    case DownloadManager.Info.State.Downloaded:
                        // do nothing
                        break;
                    default:
                        Assertion.Check(false); // SANITY CHECK
                        break;
                }
            }
            Debug.Log(string.Format("OnDownloadAssetBundleFile - FinishedAll: {0}, SucceededAll: {1}", finished, succeeded));
            if (finished)
            {
                Assertion.Check(m_indexHash != null);
                if (succeeded)
                {
                    OnEntireSuccess();
                }
                else
                {
                    OnEntireFail();
                }
            }
            Debug.Log("OnDownloadAssetBundleFile - Finished");
            return ret;
        }

        private bool OnDownloadIndexFile(DownloadManager.Info downloadInfo)
        {
            Debug.Log("OnDownloadIndexFile - Started");
            Debug.Log(string.Format("OnDownloadIndexFile - Source is \"{0}\"", m_source));
            bool ret = false;
            if (downloadInfo.state == DownloadManager.Info.State.Downloaded)
            {
                Debug.Log("OnDownloadIndexFile - Downloaded successfully");
                Assertion.Check(m_assetBundleInfo.Count == 0); //m_assetBundleInfo.Clear();
                Assertion.Check(m_assetToBundle.Count == 0); //m_assetToBundle.Clear();
                string baseUrl;
                string indexFilename;
                //ParseUrl(downloadInfo.LatestUrl, out baseUrl, out indexFilename);
                ParseUrlWithCloudFront(downloadInfo.LatestUrl, out baseUrl, out indexFilename);
                Index index = null;
                if (downloadInfo.assetBundle != null)
                {
                    try
                    {
                        TextAsset textAsset = downloadInfo.assetBundle.mainAsset as TextAsset;
                        if (textAsset != null)
                        {
                            using (Stream stream = new MemoryStream(textAsset.bytes))
                            {
                                index = Index.LoadInstance(stream);
                            }
                        }
                        downloadInfo.assetBundle.Unload(true);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning(string.Format("OnDownloadIndexFile - Caught exception: {0}", e.ToString()));
                    }
                }
                if (index != null)
                {
                    Debug.Log("OnDownloadIndexFile - Deserialized successfully");

                    m_indexBuildTag = index.m_buildTag;
                    Debug.Log(string.Format("OnDownloadIndexFile - Got build tag: \"{0}\"", m_indexBuildTag ?? "<null>"));

                    foreach (Index.AssetBundle assetBundle in index.m_assetBundles)
                    {
                        // create URLs depending on source target
                        List<string> urls = new List<string>(assetBundle.m_urls.Length + 1);
                        urls.Clear();

                        // first URL should be to the StreamingAssets to allow to catch uncompressed asset bundle out
                        urls.AddRange(AdjustUrlsToSource(Source.StreamingAssets, assetBundle.m_urls));

                        foreach (string url in assetBundle.m_urls)
                        {
                            if (url.IndexOf(".") == 0)
                            {
                                // first try lately used URL
                                /*if(!string.IsNullOrEmpty(baseUrl)) // skip it, if WWW is null, i.e. if it is to uncompressed asset bundle in the StreamingAssets
                                {
                                    string bestCandidate = baseUrl + url.Substring(2);
                                    if(!urls.Contains(bestCandidate))
                                    {
                                        urls.Add(bestCandidate);
                                    }
                                }*/

                                // try all urls provided by user
                                foreach (string candidate in GetArrayOfAssetBundles(url.Substring(2)))
                                {
                                    if (!urls.Contains(candidate))
                                    {
                                        urls.Add(candidate);
                                    }
                                }
                            }
                            else
                            {
                                // then absolute urls
                                if (!urls.Contains(url))
                                {
                                    urls.Add(url);
                                }
                            }
                        }

                        // init download and add asset bundles to index
                        AssetBundleInfo assetBundleInfo = new AssetBundleInfo(assetBundle.m_filename,
                            assetBundle.Size,
                            assetBundle.m_contentHash,
                            assetBundle.m_type.ToString(),
                            urls.ToArray(),
                            onDownloadAssetBundleFile);
                        m_assetBundleInfo.Add(assetBundleInfo);

                        // get assets included in asset bundle
                        foreach (Index.AssetBundle.Asset asset in assetBundle.m_assets)
                        {
                            try
                            {
                                string key = asset.m_filename.ToLower();
                                if (m_assetToBundle.ContainsKey(key))
                                {
                                    m_assetToBundle[key] = assetBundleInfo;
                                }
                                else
                                {
                                    m_assetToBundle.Add(key, assetBundleInfo);
                                }
                            }
                            catch (Exception e)
                            {
                                Debug.LogError(string.Format("Couldn't map asset \"{0}\" to the bundle \"{1}\" due to error: {2}", asset.m_filename, assetBundleInfo.Filename, e.ToString()));
                            }
                        }
                    }

                    // confirm success
                    ret = true;
                }
            }
            else
            {
                OnEntireFail();
            }
            Debug.Log("OnDownloadIndexFile - Finished");
            return ret;
        }

        private bool OnDownloadVersionFile(DownloadManager.Info downloadInfo)
        {
            Debug.Log("OnDownloadVersionFile - Started");
            Debug.Log(string.Format("OnDownloadVersionFile - Source is \"{0}\"", m_source));
            bool ret = false;
            if (downloadInfo.state == DownloadManager.Info.State.Downloaded)
            {
                Debug.Log("OnDownloadVersionFile - Downloaded successfully");
                string baseUrl;
                string indexVersionFilename;
                Assertion.Check(!string.IsNullOrEmpty(downloadInfo.LatestUrl));
                //ParseUrl(downloadInfo.LatestUrl, out baseUrl, out indexVersionFilename);
                ParseUrlWithCloudFront(downloadInfo.LatestUrl, out baseUrl, out indexVersionFilename);
                string indexHash = downloadInfo.text.Substring(0, 32);
                if (RTUtils.IsHash(indexHash))
                {
                    // to allow retrying
                    m_versionDownloadInfo = downloadInfo;

                    // version file should be saved at the end (after success)
                    m_indexVersionFilename = indexVersionFilename;
                    m_indexHash = indexHash;

                    // retrieve version of the cached pack, if any
                    try
                    {
                        string filename = GetPathToCache() + m_indexVersionFilename;
                        byte[] bytes = StorageManager.ReadFromLocation(filename);
                        if ((bytes != null) && (bytes.Length > 0))
                        {
                            m_cachedIndexHash = new ASCIIEncoding().GetString(bytes);
                        }
                    }
                    catch (Exception)
                    {
                    }

                    // get urls to index files
                    List<string> onlineIndexUrls = new List<string>();
                    onlineIndexUrls.AddRange(GetArrayOfIndices());
                    if ((m_source == Source.Online) && (onlineIndexUrls.Count > 1))
                    {
                        // URL used previously to download version file should be first in that list
                        for (int idx = 0; idx < onlineIndexUrls.Count; ++idx)
                        {
                            if (onlineIndexUrls[idx].StartsWith(baseUrl))
                            {
                                string usedUrl = onlineIndexUrls[idx];
                                onlineIndexUrls.RemoveAt(idx);
                                onlineIndexUrls.Insert(0, usedUrl);
                                break;
                            }
                        }
                    }

                    // create URLs depending on source target
                    List<string> indexUrls = new List<string>(onlineIndexUrls.Count + 1);
                    indexUrls.Clear();
                    // first URL should be to the StreamingAssets to allow to catch uncompressed index out
                    indexUrls.AddRange(AdjustUrlsToSource(Source.StreamingAssets, onlineIndexUrls.ToArray()));
                    // try all URLs provided by user
                    indexUrls.AddRange(onlineIndexUrls);

                    // add index file to downloads
                    int version = RTUtils.HashToVersion(indexHash);
                    m_indexDownloadInfo = DownloadManager.LoadFromCacheOrDownloadAsync(indexUrls.ToArray(), version, onDownloadIndexFile);

                    // confirm success
                    ret = true;
                }
                else
                {
                    Debug.LogWarning(string.Format("OnDownloadVersionFile - Version file is corrupted. Hash = \"{0}\"", indexHash));
                }
            }
            else
            {
                OnEntireFail();
            }
            Debug.Log("OnDownloadVersionFile - Finished");
            return ret;
        }

        private void DownloadVersionFile()
        {
            Debug.Log("DownloadVersionFile - Started");
            Assertion.Check(m_source != Source.None);
            if (m_source != Source.None)
            {
                // get urls to version files
                string[] versionUrls = GetArrayOfVersions();

                // modify URLs depending on source
                versionUrls = AdjustUrlsToSource(m_source, versionUrls);

#if (UNITY_EDITOR || BUILD_MAC) && !FORCE_ASSET_BUNDLES_IN_EDITOR
                m_state = State.Succeeded;
#else
				// add version file to downloads
				DownloadManager.LoadFromCacheOrDownloadAsync(versionUrls, DownloadManager.VERSION_FORCE_REDOWNLOAD, onDownloadVersionFile);
#endif
            }
            Debug.Log("DownloadVersionFile - Finished");
        }

        private static List<IndexInfo> m_indexInfo = new List<IndexInfo>();
        private string[] m_requestedUrls;
        private string[] m_urls;
        private DownloadManager.Info m_versionDownloadInfo;
        private DownloadManager.Info m_indexDownloadInfo;
        private List<AssetBundleInfo> m_assetBundleInfo;
        private State m_state;
        private Source m_source;
        private string m_indexBuildTag;
        private string m_indexVersionFilename;
        private string m_indexHash;
        private string m_cachedIndexHash;
        private DownloadManager.OnDownloadCallbackType m_onDownloadAssetBundleFile = null;
        private DownloadManager.OnDownloadCallbackType m_onDownloadIndexFile = null;
        private DownloadManager.OnDownloadCallbackType m_onDownloadVersionFile = null;
        private Dictionary<string, AssetBundleInfo> m_assetToBundle;

        #endregion
    }
}
