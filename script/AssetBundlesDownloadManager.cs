using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace InJoy.AssetBundles
{
    using Internal;

    /// <summary>
    /// Allows to request and follow up downloads. Also allows to
    /// tune specific options. If you want to initiate downloading
    /// of asset bundles, please see for the AssetBundles class.
    /// </summary>
    public static class DownloadManager
    {
        #region Interface

        public const int VERSION_FORCE_REDOWNLOAD = (int)-0x80000000;

        /// <summary>
        /// Contains information about requested download.
        /// </summary>
        public class Info
        {
            #region Interface

            /// <summary>
            /// Describes general state of the download.
            /// </summary>
            public enum State
            {
                /// <summary>
                /// Means, request is on the list. Initial value.
                /// </summary>
                NotStarted,
                /// <summary>
                /// Means, downloading has been just started.
                /// </summary>
                Pending,
                /// <summary>
                /// Means, request to the server is being sent.
                /// </summary>
                Sending,
                /// <summary>
                /// Means, downloading is in progress.
                /// </summary>
                Receiving,
                /// <summary>
                /// Means, downloading failed, and will retry.
                /// </summary>
                WaitingBeforeRetry,
                /// <summary>
                /// Means, file has been successfully downloaded.
                /// </summary>
                Downloaded,
                /// <summary>
                /// Means, no success on all attempts. Dropped.
                /// </summary>
                Abandoned,
                /// <summary>
                /// Means, user explicitly invoked function Skip.
                /// </summary>
                DoNotDownload,
            }

            /// <summary>
            /// Describes cache status for the download.
            /// </summary>
            public enum CacheStatus
            {
                /// <summary>
                /// Means, cache status is unknown. Either download has been just added
                /// to the list, or the request is not asset bundle at all.
                /// </summary>
                Undefined,
                /// <summary>
                /// Means, failed to download and cache compressed asset bundle.
                /// Or this is just uncompressed asset bundle from the StreamingAssets.
                /// </summary>
                NotCached,
                /// <summary>
                /// Means, downloaded asset bundle successfully, but failed to cache it.
                /// Most likely device is out of free space.
                /// </summary>
                Breakdown,
                /// <summary>
                /// Means, asset bundle has been downloaded and cached.
                /// </summary>
                JustCached,
                /// <summary>
                /// Means, asset bundle was loaded from the cache.
                /// </summary>
                AlreadyCached,
            }

            /// <summary>
            /// Returns general state of the download.
            /// </summary>
            /// <value>
            /// The state.
            /// </value>
            public State state
            {
                get
                {
                    State s = State.NotStarted;
                    switch (m_download.state)
                    {
                        case Download.State.NotStarted:
                            s = State.NotStarted;
                            break;
                        case Download.State.Loading:
                            s = (www != null) ? ((www.progress > 0) ? State.Receiving : ((www.uploadProgress > 0) ? State.Sending : State.Pending)) : State.Pending;
                            break;
                        case Download.State.WaitingBeforeRetry:
                            s = State.WaitingBeforeRetry;
                            break;
                        case Download.State.Downloaded:
                            s = State.Downloaded;
                            break;
                        case Download.State.Abandoned:
                            s = State.Abandoned;
                            break;
                        case Download.State.DoNotDownload:
                            s = State.DoNotDownload;
                            break;
                        default:
                            // SANITY CHECK
                            Assertion.Check(false, "Internal class error: could not convert internal state \"{0}\" to DownloadManager.Info state", m_download.state);
                            break;
                    }
                    return s;
                }
            }

            /// <summary>
            /// Returns cache status of the download.
            /// </summary>
            /// <value>
            /// The cache status.
            /// </value>
            public CacheStatus cacheStatus
            {
                get
                {
                    CacheStatus ret = CacheStatus.NotCached;
                    switch (m_download.cacheStatus)
                    {
                        case Download.CacheStatus.Undefined:
                            ret = CacheStatus.Undefined;
                            break;
                        case Download.CacheStatus.NotCached:
                            ret = CacheStatus.NotCached;
                            break;
                        case Download.CacheStatus.Breakdown:
                            ret = CacheStatus.Breakdown;
                            break;
                        case Download.CacheStatus.JustCached:
                            ret = CacheStatus.JustCached;
                            break;
                        case Download.CacheStatus.AlreadyCached:
                            ret = CacheStatus.AlreadyCached;
                            break;
                        default:
                            // SANITY CHECK
                            Assertion.Check(false, "Internal class error: could not convert internal cache status \"{0}\" to DownloadManager.Info cache status", m_download.cacheStatus);
                            break;
                    }
                    return ret;
                }
            }

            /// <summary>
            /// Returns the list of used mirrors to download file.
            /// </summary>
            /// <value>
            /// The urls.
            /// </value>
            public string[] Urls
            {
                get { return m_download.Urls; }
            }

            public Download.DownloadSource CurDownloadSource
            {
                get { return m_download.CurDownloadSource; }
            }

            /// <summary>
            /// Returns uploading progress.
            /// </summary>
            /// <value>
            /// Number between 0 and 1.
            /// </value>
            public float SendingProgress
            {
                // TODO: could returned value be logically incorrect?
                get { return (www != null) ? www.uploadProgress : 0; }
            }

            /// <summary>
            /// Returns downloading progress.
            /// </summary>
            /// <value>
            /// Number between 0 and 1.
            /// </value>
            public float ReceivingProgress
            {
                get { return (state == State.Downloaded) ? 1 : ((www != null) ? www.progress : 0); }
            }

            /// <summary>
            /// Returns the latest tried link.
            /// </summary>
            /// <value>
            /// The URL.
            /// </value>
            public string LatestUrl
            {
                get { return (www != null) ? www.url.Replace('\\', '/') : null; } // Replace is because of HackUrl
            }

            /// <summary>
            /// Returns the latest error happened during downloading.
            /// </summary>
            /// <value>
            /// The string. Or null, if no errors happened.
            /// </value>
            public string LatestError
            {
                get { return (www != null) ? www.error : null; }
            }

            /// <summary>
            /// Returns downloaded file as an asset bundle.
            /// </summary>
            /// <value>
            /// The asset bundle. Or null, if it not downloaded yet.
            /// </value>
            public AssetBundle assetBundle
            {
                get
                {
                    AssetBundle ret = null;
                    if (state == State.Downloaded)
                    {
                        if (m_download.OfflineAssetBundle != null)
                        {
                            ret = m_download.OfflineAssetBundle;
                        }
                        else
                        {
                            ret = ((www != null) ? www.assetBundle : null);
                        }
                    }
                    return ret;
                }
            }

            /// <summary>
            /// Returns downloaded file as byte array.
            /// </summary>
            /// <value>
            /// The byte array. Or null, if it not downloaded yet.
            /// </value>
            public byte[] bytes
            {
                get { return ((state == State.Downloaded) && (version < 0)) ? www.bytes : null; }
            }

            /// <summary>
            /// Returns downloaded file as a plain text.
            /// </summary>
            /// <value>
            /// The text string. Or null, if it not downloaded yet.
            /// </value>
            public string text
            {
                get { return ((state == State.Downloaded) && (version < 0)) ? www.text : null; }
            }

            /// <summary>
            /// Allows to retrieve instance by the specified GUID.
            /// </summary>
            /// <param name='guid'>
            /// GUID of the request.
            /// </param>
            public static Info Retrieve(string guid)
            {
                Debug.Log("Retrieve - Started");
                Info ret = null;
                Download download = FindDownloadByGuid(guid);
                if (download != null)
                {
                    ret = new Info(download);
                }
                Debug.Log("Retrieve - Finished");
                return ret;
            }

            /// <summary>
            /// Do not download the file, if downloading hasn't been started yet.
            /// </summary>
            public void Skip()
            {
                Debug.Log("Skip - Started");
                m_download.Skip();
                Debug.Log("Skip - Finished");
            }

            #endregion
            #region Implementation

            private Info(Download download)
            {
                m_download = download;
            }

            private int version
            {
                get { return m_download.version; }
            }

            private WWW www
            {
                get { return m_download.www; }
            }

            private Download m_download;

            #endregion
        }

        /// <summary>
        /// Type of callback function, which would be invoked once download status be resolved.
        /// </summary>
        public delegate bool OnDownloadCallbackType(Info downloadInfo);

        /// <summary>
        /// Gets or sets the thread count to download files with.
        /// </summary>
        /// <value>
        /// The thread count.
        /// </value>
        public static int ThreadCount
        {
            set { m_threadCount = value; }
            get { return m_threadCount; }
        }

        /// <summary>
        /// Gets the active threads count at the time.
        /// </summary>
        /// <value>
        /// The amount of active threads.
        /// </value>
        public static int ActiveThreads
        {
            get { return m_downloadCoroutines.Count; }
        }

        /// <summary>
        /// Gets or sets how much tries download manager should do to download file.
        /// </summary>
        /// <value>
        /// The tries count.
        /// </value>
        public static int TriesCount
        {
            set { m_triesCount = value; }
            get { return m_triesCount; }
        }

        /// <summary>
        /// Gets or sets the delay download manager should do before it will retry failed download.
        /// </summary>
        /// <value>
        /// The delay in seconds.
        /// </value>
        public static int DelayBeforeRetry
        {
            set { m_delayBeforeRetry = value; }
            get { return m_delayBeforeRetry; }
        }

        /// <summary>
        /// Adds a file specified by urls and version to downloads.
        /// </summary>
        /// <param name='urls'>
        /// An array of URLs (mirrors) to the same file. Download Manager would try
        /// to load file from them subsequently. End files MUST have the same name
        /// and extension, paths don't matter.
        /// </param>
        /// <param name='version'>
        /// Version of the file to download. If file with the specified version has
        /// been already downloaded, it would be loaded from the cache. Version might
        /// be positive integer value. If the file should be downloaded forcefully
        /// from the server, set version to VERSION_FORCE_REDOWNLOAD.
        /// </param>
        /// <param name='onDownloadCallback'>
        /// Function, which would be invoked once download be resolved. Set null, if
        /// no callback function should be executed.
        /// </param>
        public static Info LoadFromCacheOrDownloadAsync(string[] urls, int version, OnDownloadCallbackType onDownloadCallback)
        {
            Debug.Log("LoadFromCacheOrDownloadAsync - Started");
            Assertion.Check((urls != null) && (urls.Length > 0));
            foreach (string url in urls)
            {
                Assertion.Check(url.IndexOf("://") >= 0, "Protocol should be specified explicitly (\"file://\", \"http://\" etc.) for URL \"{0}\"", url);
            }

            // log urls
            string list = "";
            foreach (string url in urls)
            {
                list += string.Format("{0}{1}", string.IsNullOrEmpty(list) ? "" : "\n", url);
            }
            Debug.Log(string.Format("LoadFromCacheOrDownloadAsync - [in] urls:\n==>\n{0}\n<==", list));

            Info ret = null;
            if ((urls != null) && (urls.Length > 0))
            {
                for (int idx = 0; idx < urls.Length; ++idx)
                {
                    urls[idx] = urls[idx].Replace('\\', '/'); // prefers slash over backslash
                }

                Download download = FindDownloadByUrl(urls[0]);
                if (download == null)
                {
                    Debug.Log("LoadFromCacheOrDownloadAsync - Create new download");
                    download = new Download(urls, version, onDownloadCallback);
                    m_downloads.Add(download);
                    Debug.Log(string.Format("LoadFromCacheOrDownloadAsync - Added new download \"{0}\" to the list", urls[0]));
                }
                else
                {
                    Debug.LogWarning(string.Format("LoadFromCacheOrDownloadAsync - Download \"{0}\" is already requested", urls[0]));
                }

                ret = Info.Retrieve(download.GUID);
                EnsureDownloadCoroutines();
            }

            Assertion.Check(ret != null);
            if (ret == null)
            {
                Debug.LogWarning("LoadFromCacheOrDownloadAsync - Nothing to download");
            }

            Debug.Log("LoadFromCacheOrDownloadAsync - Finished");
            return ret;
        }

        /// <summary>
        /// Determines whether a file specified by URL and version is downloaded and cached.
        /// </summary>
        /// <returns>
        /// <c>true</c> if file could be loaded from cache; otherwise, <c>false</c>.
        /// </returns>
        /// <param name='filenameOrUrl'>
        /// Presence of file is determined by its name and extension. Path is irrelevant.
        /// However entire url can be specified, function would truncate it to filename
        /// automatically.
        /// </param>
        /// <param name='version'>
        /// Several versions could be stored in the cache. Use version to specify exact
        /// version of the file. It should positive integer number.
        /// </param>
        public static bool IsCached(string filenameOrUrl, int version)
        {
            Debug.Log("IsCached - Started");
            Assertion.Check((version == VERSION_FORCE_REDOWNLOAD) || (version >= 0)); // otherwise not implemented
            bool ret = false;
            if (version >= 0)
            {
                ret = Caching.IsVersionCached(filenameOrUrl, version);
            }
            Debug.Log("IsCached - Finished");
            return ret;
        }

        #endregion
        #region Implementation

        public class Download
        {
            public enum State
            {
                NotStarted,
                Loading,
                WaitingBeforeRetry,
                Downloaded,
                Abandoned,
                DoNotDownload,
            }

            public enum CacheStatus
            {
                Undefined,
                NotCached,
                Breakdown,
                JustCached,
                AlreadyCached,
            }

            public enum DownloadSource
            {
                None,
                Online,
                Cache,
            }


            public string GUID
            {
                get { return m_guid; }
            }

            public string[] Urls
            {
                get { return m_urls.ToArray(); }
            }

            public DownloadSource CurDownloadSource
            { set; get; }

            public int version
            {
                get { return m_version; }
            }

            public OnDownloadCallbackType onDownloadCallback
            {
                get { return m_onDownloadCallback; }
            }

            public WWW www
            {
                set { m_www = value; }
                get { return m_www; }
            }

            public AssetBundle OfflineAssetBundle
            {
                set { m_offlineAssetBundle = value; }
                get { return m_offlineAssetBundle; }
            }

            public State state
            {
                set { m_state = value; }
                get { return m_state; }
            }

            public CacheStatus cacheStatus
            {
                set { m_cacheStatus = value; }
                get { return m_cacheStatus; }
            }

            public Download(string[] urls, int version, OnDownloadCallbackType onDownloadCallback)
            {
                Debug.Log("Ctor - Started");
                m_guid = Guid.NewGuid().ToString();
                m_urls = new List<string>(urls.Length);
                m_urls.Clear();
                foreach (string url in urls)
                {
                    m_urls.Add(url);
                }
                Assertion.Check((version == VERSION_FORCE_REDOWNLOAD) || (version >= 0)); // otherwise not implemented
                m_version = version;
                m_onDownloadCallback = onDownloadCallback;
                m_www = null;
                m_offlineAssetBundle = null;
                m_state = State.NotStarted;
                m_cacheStatus = CacheStatus.Undefined;
                CurDownloadSource = DownloadSource.None;
                Debug.Log("Ctor - Finished");
            }

            public void Skip()
            {
                Debug.Log("Skip - Started");
                Debug.Log(string.Format("Skip - (in) State is \"{0}\"", state));
                if (state == State.NotStarted)
                {
                    m_state = State.DoNotDownload;
                }
                Debug.Log(string.Format("Skip - (out) State is \"{0}\"", state));
                Debug.Log("Skip - Finished");
            }

            private string m_guid;
            private List<string> m_urls;
            private int m_version;
            private OnDownloadCallbackType m_onDownloadCallback;
            private WWW m_www;
            private AssetBundle m_offlineAssetBundle;
            private State m_state;
            private CacheStatus m_cacheStatus;
        }

#if UNITY_EDITOR
        // To clean cache in Editor before the use of AssetBundles
        static DownloadManager()
        {
            // HACK: tricky way to allow run-time scripts to detect,
            // whether cache should be cleaned or not.
            bool cleanCache = File.Exists("Assets/Editor/Conf/AssetBundles/DefaultShellSettings_CleanCache~");
            if (cleanCache)
            {
                m_isCacheCleaned = false;
                UnityEntity.Instance.StartCoroutine(CleanCacheRoutine());
            }
            else
            {
                m_isCacheCleaned = true;
            }
        }

        private static IEnumerator CleanCacheRoutine()
        {
            while (!Caching.ready)
            {
                yield return null;
            }
            bool success = Caching.CleanCache();
            if (!success)
            {
                Debug.LogWarning("CleanCacheRoutine - Failed to clean cache");
            }
            m_isCacheCleaned = true;
        }
#endif

        private static void EnsureDownloadCoroutines()
        {
            Debug.Log("EnsureDownloadCoroutines - Started");
            // ensure, DownloadCoroutines are actual at the time
            for (int idx = 0; idx < ThreadCount; ++idx)
            {
                bool works;
                if (m_downloadCoroutines.TryGetValue(idx, out works))
                {
                    Assertion.Check(works);
                    if (!works)
                    {
                        // if key exists, its value could be "true" only!
                        Debug.LogWarning("EnsureDownloadCoroutines - Repair wrong behaviour");
                        m_downloadCoroutines.Remove(idx);
                    }
                }
                else
                {
                    works = false;
                }
                if (!works)
                {
                    Debug.Log(string.Format("EnsureDownloadCoroutines - Starting DownloadCoroutine #{0}...", idx));
                    // mark, that coroutine is going to start its work
                    m_downloadCoroutines.Add(idx, true);
                    UnityEntity.Instance.StartCoroutine(DownloadCoroutine(idx));
                }
            }
            Debug.Log("EnsureDownloadCoroutines - Finished");
        }

        private static string HackUrl(string url)
        {
#if UNITY_STANDALONE_WIN
			// HACK: on win32 force backslash for local files only,
			// because of the bug in WWW class
			const string kFileProtocol = "file://";
			if((url.ToLower().IndexOf(kFileProtocol) == 0) && (url.IndexOf("//", kFileProtocol.Length) < 0))
			{
				url = url.Substring(0, kFileProtocol.Length) + url.Substring(kFileProtocol.Length).Replace('/', '\\');
			}
			Debug.Log("HackUrl - \"{0}\"", url);
#endif
            return url;
        }

        private static bool ShouldAssetBundleBeTriedAsUncompressed(string filename)
        {
            bool ret = true;
#if UNITY_EDITOR
            // to avoid error pause in the Editor, check for the asset bundle header
            try
            {
                using (StreamReader sr = new StreamReader(filename))
                {
                    const string HEADER = "UnityRaw"; // hardcoded
                    char[] chars = new char[HEADER.Length];
                    sr.ReadBlock(chars, 0, HEADER.Length);
                    string header = new string(chars);
                    ret = !string.IsNullOrEmpty(header) && header.Equals(HEADER);
                }
            }
            catch (Exception)
            {
            }
#endif
            return ret;
        }

        private static Download FindDownloadByGuid(string guid)
        {
            Debug.Log("FindDownloadByGuid - Started");
            Download ret = null;
            for (int idx = m_downloads.Count - 1; idx >= 0; --idx)
            {
                Download download = m_downloads[idx];
                if (download.GUID.Equals(guid))
                {
                    ret = download;
                    break;
                }
            }
            Debug.Log("FindDownloadByGuid - Finished");
            return ret;
        }

        private static Download FindDownloadByUrl(string url)
        {
            Debug.Log("FindDownloadByUrl - Started");
            Download ret = null;
            for (int idx = m_downloads.Count - 1; idx >= 0; --idx)
            {
                Download download = m_downloads[idx];
                foreach (string downloadUrl in download.Urls)
                {
                    if (downloadUrl.Equals(url))
                    {
                        ret = download;
                        break;
                    }
                }
                if (ret != null)
                {
                    break;
                }
            }
            Debug.Log("FindDownloadByUrl - Finished");
            return ret;
        }

        private static IEnumerator DownloadCoroutine(int threadNumber)
        {
            Debug.Log(string.Format("DownloadCoroutine #{0} - Started", threadNumber));

            // wait for a frame - to allow functions in call stack
            // (invoked this coroutine) finish their current work
            yield return null;

            // process entire downloads list
            int idx = 0;
            while (idx < m_downloads.Count)
            {
                // pick only unresolved downloads, i.e. marked as NotStarted
                Download download = m_downloads[idx];
                Debug.Log(string.Format("DownloadCoroutine #{0} - Processing download with state {1}", threadNumber, download.state));
                if (download.state == Download.State.NotStarted)
                {
                    Debug.Log(string.Format("DownloadCoroutine #{0} - Found new task, GUID = \"{1}\"", threadNumber, download.GUID));

                    bool downloaded = false;
                    bool cacheBreakdown = false;
                    int urlIndex = 0;
                    for (urlIndex = 0; urlIndex < download.Urls.Length; ++urlIndex)
                    {
                        Debug.Log(string.Format("DownloadCoroutine #{0} - Got URL #{1}: \"{2}\" (ver. {3})", threadNumber, urlIndex, download.Urls[urlIndex], download.version));
                        if (download.Urls[urlIndex].Contains("http"))
                        {
                            download.CurDownloadSource = Download.DownloadSource.Online;
                        }
                        else
                        {
                            download.CurDownloadSource = Download.DownloadSource.Cache;

                        }
                        int attempt = 0;
                        for (attempt = 0; attempt < TriesCount; ++attempt) // attempt to get required file
                        {
                            // if we're going to request the same file twice, lets make a pause
                            if (/*(urlIndex > 0) || */(attempt > 0))
                            {
                                Debug.Log(string.Format("DownloadCoroutine #{0} - Waiting for the delay ({1} seconds), because previous attempt has failed", threadNumber, DelayBeforeRetry));
                                yield return new WaitForSeconds(DelayBeforeRetry);
                            }

                            if (RTUtils.UncompressedAssetBundlesAllowed)
                            {
                                // try to catch asset bundle out from the StreamingAssets
                                try
                                {
                                    string filename = download.Urls[urlIndex].Substring(download.Urls[urlIndex].IndexOf("://") + 3);
                                    if ((download.version >= 0) && File.Exists(filename) && ShouldAssetBundleBeTriedAsUncompressed(filename))
                                    {
                                        download.OfflineAssetBundle = AssetBundle.CreateFromFile(filename);
                                        downloaded = (download.OfflineAssetBundle != null);
                                        Debug.Log(string.Format("DownloadCoroutine #{0} - CreateFromFile - Created from the StreamingAssets with result: {1}", threadNumber, downloaded));
                                        if (downloaded)
                                        {
                                            download.cacheStatus = Download.CacheStatus.NotCached;
                                            break;
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    Debug.Log(string.Format("DownloadCoroutine #{0} - CreateFromFile - Caught exception: {1}", threadNumber, e.ToString()));
                                }
                            }

                            // try to download. Check, whether version has been specified
                            Debug.Log(string.Format("DownloadCoroutine #{0} - Trying to download (attempt #{1})", threadNumber, attempt));
                            if (download.version >= 0)
                            {
                                // file could be downloaded from server as well as from cache
                                Debug.Log(string.Format("DownloadCoroutine #{0} - LoadFromCacheOrDownload", threadNumber));
                                download.state = Download.State.Loading;

                                bool warned = false;
#if UNITY_EDITOR
                                // wait until cache would be cleaned
                                warned = false;
                                while (!m_isCacheCleaned)
                                {
                                    if (!warned)
                                    {
                                        Debug.Log(string.Format("DownloadCoroutine #{0} - Waiting until cache would be cleaned", threadNumber));
                                        warned = true;
                                    }
                                    yield return null;
                                }
#endif

                                // cache could be in invalid state, wait until it would be validated
                                warned = false;
                                while (!Caching.ready)
                                {
                                    if (!warned)
                                    {
                                        Debug.Log(string.Format("DownloadCoroutine #{0} - Waiting until Caching.ready would be true", threadNumber));
                                        warned = true;
                                    }
                                    yield return null;
                                }

                                bool alreadyCached = Caching.IsVersionCached(download.Urls[urlIndex], download.version);
                                download.www = WWW.LoadFromCacheOrDownload(SpecializeURL(download.Urls[urlIndex]), download.version);
                                yield return UnityEntity.Instance.StartCoroutine(WWWCoroutine(download.www));
                                if (WWWSucceeded(download.www))
                                {
                                    Debug.Log(string.Format("DownloadCoroutine #{0} - Downloaded successfully", threadNumber));
                                    if (Caching.IsVersionCached(download.Urls[urlIndex], download.version))
                                    {
                                        Debug.Log(string.Format("DownloadCoroutine #{0} - Cached successfully", threadNumber));
                                        download.cacheStatus = alreadyCached ? Download.CacheStatus.AlreadyCached : Download.CacheStatus.JustCached;
                                        downloaded = true;
                                        break;
                                    }
                                    else
                                    {
                                        // most likely this means, no free space on HDD
                                        Debug.LogWarning(string.Format("DownloadCoroutine #{0} - Failed to cache", threadNumber));
                                        download.cacheStatus = Download.CacheStatus.Breakdown;
                                        cacheBreakdown = true;
                                    }
                                }
                                else
                                {
                                    download.cacheStatus = Download.CacheStatus.NotCached;
                                }

                                Assertion.Check(!downloaded); // SANITY CHECK
                                Debug.Log(string.Format("DownloadCoroutine #{0} - Failed to download and cache", threadNumber));
                                Debug.Log(string.Format("DownloadCoroutine #{0} - WWW properties are URL:\"{1}\", isDone:{2}, error:\"{3}\", progress sent/received:{4}/{5}", threadNumber, download.www.url, download.www.isDone, download.www.error, download.www.uploadProgress, download.www.progress));
                                Debug.Log(string.Format("DownloadCoroutine #{0} - IsVersionCached:{1}", threadNumber, Caching.IsVersionCached(download.Urls[urlIndex], download.version)));
                                Debug.Log(string.Format("DownloadCoroutine #{0} - Caching enabled:{1}, ready:{2}, expirationDelay:{3}", threadNumber, Caching.enabled, Caching.ready, Caching.expirationDelay));


                                //patch
                                if (!String.IsNullOrEmpty(download.www.error) && download.www.error.Contains("Cannot load cached AssetBundle."))
                                {
                                    PlayerPrefs.SetInt("ForceCleanCache", 1);
                                    PlayerPrefs.Save();
                                }

#if !UNITY_EDITOR
								// FIXME: this doesn't help. Remove it later
								// HACK: to try to catch unbelievable www.error, to be exact:
								// "Cannot load cached AssetBundle. A file of the same name is already loaded from another AssetBundle."
								try
								{
									AssetBundle assetBundle = download.www.assetBundle;
									if(assetBundle != null)
									{
										Debug.LogWarning(string.Format("DownloadCoroutine #{0} - Asset bundle failed to load was instantiated! Unloading it immediately!", threadNumber));
										assetBundle.Unload(true);
									}
									else
									{
										Debug.Log(string.Format("DownloadCoroutine #{0} - AssetBundle is null", threadNumber));
									}
								}
								catch(Exception e)
								{
									Debug.LogWarning(string.Format("DownloadCoroutine #{0} - Caught exception: {1}", threadNumber, e.ToString()));
								}
#endif
                            }
                            else if (download.version == VERSION_FORCE_REDOWNLOAD)
                            {
                                // file should be downloaded from server
                                Debug.Log(string.Format("DownloadCoroutine #{0} - Download", threadNumber));
                                download.state = Download.State.Loading;
                                download.www = new WWW(SpecializeURL(HackUrl(download.Urls[urlIndex])));
                                yield return UnityEntity.Instance.StartCoroutine(WWWCoroutine(download.www));
                                if (WWWSucceeded(download.www))
                                {
                                    Debug.Log(string.Format("DownloadCoroutine #{0} - Downloaded successfully", threadNumber));
                                    downloaded = true;
                                    break;
                                }

                                Assertion.Check(!downloaded); // SANITY CHECK
                                Debug.Log(string.Format("DownloadCoroutine #{0} - Failed to download", threadNumber));
                                Debug.Log(string.Format("DownloadCoroutine #{0} - WWW properties are URL:\"{1}\", isDone:{2}, error:\"{3}\", progress sent/received:{4}/{5}", threadNumber, download.www.url, download.www.isDone, download.www.error, download.www.uploadProgress, download.www.progress));
                            }
                            else
                            {
                                // do nothing, and download will be just failed
                                Debug.LogWarning(string.Format("DownloadCoroutine #{0} - Not implemented", threadNumber));
                            }

                            Assertion.Check(!downloaded); // SANITY CHECK
                            if (download.www != null)
                            {
                                try
                                {
                                    // HACK: to try to dismiss connection failed because of fired timeout
                                    download.www.Dispose();
                                }
                                catch (Exception e)
                                {
                                    Debug.LogWarning(string.Format("DownloadCoroutine #{0} - Caught exception: {1}", threadNumber, e.ToString()));
                                }
                                finally
                                {
                                    Debug.Log(string.Format("DownloadCoroutine #{0} - Reseting WWW to null", threadNumber));
                                    download.www = null;
                                }
                            }

                            // at this point attempt failed
                            download.state = Download.State.WaitingBeforeRetry;
                            Debug.Log(string.Format("DownloadCoroutine #{0} - Failed to download", threadNumber));

                            if (cacheBreakdown)
                            {
                                ++attempt; // just to match actual count on leave
                                break;
                            }
                        }

                        if (downloaded) // succeeded to download a file
                        {
                            // mark download as finished (successfully finished)
                            download.state = Download.State.Downloaded;
                            Debug.Log(string.Format("DownloadCoroutine #{0} - Downloaded successfully", threadNumber));

                            // let postprocess delegate know result
                            if (download.onDownloadCallback != null)
                            {
                                try
                                {
                                    Debug.Log(string.Format("DownloadCoroutine #{0} - Invoking OnDownloadCallback", threadNumber));
                                    downloaded = download.onDownloadCallback(Info.Retrieve(download.GUID));
                                }
                                catch (Exception e)
                                {
                                    Debug.LogError(string.Format("DownloadCoroutine #{0} - Caught exception, thrown by callback: {1}", threadNumber, e.ToString()));
                                    downloaded = false;
                                }
                            }

                            if (downloaded) // succeeded to download a file
                            {
                                break;
                            }
                            else
                            {
                                Debug.LogWarning(string.Format("DownloadCoroutine #{0} - Forced to fail download", threadNumber));
                            }
                        }

                        if (!downloaded) // all our attempts to download from current URL were without effect
                        {
                            download.state = Download.State.WaitingBeforeRetry;
                            Debug.Log(string.Format("DownloadCoroutine #{0} - Tried URL up to {1} times with no success", threadNumber, attempt));
                        }

                        if (cacheBreakdown)
                        {
                            ++urlIndex; // just to match actual count on leave
                            break;
                        }
                    }

                    if (!downloaded) // all our attempts to download from all URLS were without effect
                    {
                        // mark download as finished (failed)
                        download.state = Download.State.Abandoned;
                        Debug.Log(string.Format("DownloadCoroutine #{0} - Tried {1} links and failed in the end", threadNumber, urlIndex));

                        // let postprocess delegate know result
                        if (download.onDownloadCallback != null)
                        {
                            try
                            {
                                Debug.Log(string.Format("DownloadCoroutine #{0} - Invoking OnDownloadCallback", threadNumber));
                                download.onDownloadCallback(Info.Retrieve(download.GUID));
                            }
                            catch (Exception e)
                            {
                                Debug.LogError(string.Format("DownloadCoroutine #{0} - Caught exception, thrown by callback: {1}", threadNumber, e.ToString()));
                            }
                        }
                    }

                    // remove resolved download from list
                    Debug.Log(string.Format("DownloadCoroutine #{0} - Remove task from the list, GUID = \"{1}\"", threadNumber, download.GUID));
                    m_downloads.Remove(download);
                    idx = 0;
                }
                else if (download.state == Download.State.DoNotDownload)
                {
                    // remove skipped download from list
                    m_downloads.RemoveAt(idx);
                    Debug.Log(string.Format("DownloadCoroutine #{0} - Removed skipped task, GUID = \"{1}\"", threadNumber, download.GUID));
                }
                else
                {
                    // look for next download in list
                    Debug.Log(string.Format("DownloadCoroutine #{0} - Look for next download", threadNumber));
                    ++idx;
                }

                // if suddenly granted limit of threads was decreased during downloading, lets finish freed coroutine
                // TODO: choose the coroutine to be stopped in more flexible way
                if (threadNumber >= ThreadCount)
                {
                    Debug.Log(string.Format("DownloadCoroutine #{0} - Allowed thread count has decreased (actually, {1}). Exitting", threadNumber, ThreadCount));
                    break;
                }

                // following pause is required for the reasons
                // 1) downloads list could contain a lot of files,
                // make a pause to allow Unity to process its tasks;
                // 2) more important, uncompressed asset bundles
                // must not be loaded all in one frame.
                yield return null;
            }

            // mark, that coroutine is going to finish its work
            m_downloadCoroutines.Remove(threadNumber);
            Debug.Log(string.Format("DownloadCoroutine #{0} - Finished", threadNumber));
        }

        private static string SpecializeURL(string url) // hardcoded
        {
#if DCP_ADD_RANDOM_PARAMETER_TO_URL_QUERY_STRING
			bool isLocalScheme = url.ToLower().Contains("file://");
			if(!isLocalScheme)
			{
				bool isQueryString = url.Contains("?");
				url += string.Format("{0}ref={1}", isQueryString ? "&" : "?", Guid.NewGuid().ToString("N"));
			}
#endif
            return url;
        }

        private static IEnumerator WWWCoroutine(WWW www)
        {
#if WWW_ISDONE_BUG_FIXED
			yield return www;
#else
            // HACK: to drop hopeless downloading
            double timeOut = DateTime.MaxValue.Second;// by GC: to remove time out. this will casue download state error.
            //FCDownloadManager.Instance.IsBackgroundMode ? DateTime.MaxValue.Second:30.0;//System.Convert.ToInt32(DataManager.Instance.CurGlobalConfig.getConfig("download_background_time").ToString()):5.0; // in seconds
            Debug.Log("WWW Coroutine Time Out Is " + timeOut);
            DateTime dateTime = DateTime.Now;
            float progress = www.progress;
            while (true)
            {
                // detect WWW request is resolved
                if (www.isDone || !string.IsNullOrEmpty(www.error))
                {
                    break;
                }

                if (!www.url.ToLower().StartsWith("file://"))
                {
                    // detect loading is hung on (don't do that on local files!)
                    if (www.progress > progress)
                    {
                        dateTime = DateTime.Now;
                        progress = www.progress;
                    }
                    else if ((www.progress < 1.0f) && ((DateTime.Now - dateTime).TotalSeconds > timeOut))
                    {
                        Debug.LogWarning(string.Format("WWW timeout has been fired. isDone: {0}, error: \"{1}\", progress: {2}", www.isDone, www.error, www.progress));
                        break;
                    }
                }

                yield return null;
            }
#endif
        }

        private static bool WWWSucceeded(WWW www)
        {
            bool ret = www.isDone && string.IsNullOrEmpty(www.error);
            if (ret)
            {
                try
                {
                    Dictionary<string, string> response = www.responseHeaders;
                    string statusLine = response["STATUS"];
                    string[] info = statusLine.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (info[0].ToUpper().Contains("HTTP"))
                    {
                        int statusCode = (int)Convert.ChangeType(info[1], typeof(int));
                        Debug.Log(string.Format("WWW status code: {0}", statusCode));
                        ret = (statusCode < 400); // 1xx, 2xx, 3xx means success. 4xx, 5xx means fail
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(string.Format("WWW status code cannot be retrieved. Caught exception: {0}", e.ToString()));
                }
            }
            return ret;
        }

        private static int m_threadCount = 1;
        private static int m_triesCount = 3;
        private static int m_delayBeforeRetry = 1;
        private static List<Download> m_downloads = new List<Download>();
        // TODO: dictionary contains value-based pairs. Is it OK against JIT on iOS?
        private static Dictionary<int, bool> m_downloadCoroutines = new Dictionary<int, bool>();
#if UNITY_EDITOR
        private static bool m_isCacheCleaned = false;
#endif

        #endregion
    }
}
