namespace InJoy.AssetBundles
{
    using UnityEngine;
    using Internal;

    /// <summary>
    /// Allows to retrieve information whether downloaded
    /// asset bundles are up-to-date or should be updated.
    /// Instances are created by IndexInfo.CheckForUpdates.
    /// </summary>
    public class UpdateInfo
    {
        #region Interface

        /// <summary>
        /// Describes general state of the request.
        /// </summary>
        public enum State
        {
            /// <summary>
            /// Means, sending/receiving data.
            /// </summary>
            Pending,
            /// <summary>
            /// Means, offline or asset bundles have disappeared from the server.
            /// </summary>
            Unreachable,
            /// <summary>
            /// Means, actual version of asset bundles are the latest.
            /// </summary>
            UpToDate,
            /// <summary>
            /// Means, server contains newer version of asset bundles.
            /// </summary>
            Outdated,
        }

        /// <summary>
        /// Gets the state of check-for-updates request.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        public State state
        {
            get { return m_state; }
        }

        /// <summary>
        /// Creates request to check for updates. Use IndexInfo.CheckForUpdates
        /// to instantiate correct request instead.
        /// </summary>
        public static UpdateInfo CreateInstance(string[] versionUrls, string curIndexHash)
        {
            Debug.Log("CreateInstance - Started");
            UpdateInfo ret = null;
#if (UNITY_EDITOR || BUILD_MAC) && !FORCE_ASSET_BUNDLES_IN_EDITOR
            ret = new UpdateInfo(null, null);
            ret.Download(null);
#else
			Assertion.Check((versionUrls != null) && (versionUrls.Length > 0));
			Assertion.Check(!string.IsNullOrEmpty(curIndexHash));
			if((versionUrls != null) && (versionUrls.Length > 0) && !string.IsNullOrEmpty(curIndexHash))
			{
				ret = new UpdateInfo(versionUrls, curIndexHash);
				ret.Download(versionUrls);
			}
#endif
            Debug.Log("CreateInstance - Finished");
            return ret;
        }

        #endregion
        #region Implementation

        private DownloadManager.OnDownloadCallbackType onDownload
        {
            get { return (m_onDownload != null) ? m_onDownload : (m_onDownload = new DownloadManager.OnDownloadCallbackType(OnDownload)); }
        }

        private UpdateInfo(string[] versionUrls, string curIndexHash)
        {
            m_state = State.Pending;
            m_curIndexHash = curIndexHash;
        }

        private bool OnDownload(DownloadManager.Info downloadInfo)
        {
            Debug.Log("OnDownload - Started");
            bool ret = false;
            if (downloadInfo.state == DownloadManager.Info.State.Downloaded)
            {
                ret = true;
                if (m_curIndexHash != null)
                {
                    string onlineIndexHash = downloadInfo.text.Substring(0, 32);
                    if (RTUtils.IsHash(onlineIndexHash))
                    {
                        if (m_curIndexHash.Equals(onlineIndexHash))
                        {
                            m_state = State.UpToDate;
                        }
                        else
                        {
                            m_state = State.Outdated;
                        }
                    }
                    else
                    {
                        Debug.LogWarning(string.Format("OnDownload - Version file is corrupted. Hash = \"{0}\"", onlineIndexHash));
                        ret = false;
                    }
                }
            }
            else
            {
                m_state = State.Unreachable;
            }
            Debug.Log(string.Format("OnDownload - Resolved to state \"{0}\"", state));
            Debug.Log("OnDownload - Finished");
            return ret;
        }

        private void Download(string[] versionUrls)
        {
            Debug.Log("Download - Started");
#if (UNITY_EDITOR || BUILD_MAC) && !FORCE_ASSET_BUNDLES_IN_EDITOR
            m_state = State.Unreachable;
#else
			DownloadManager.LoadFromCacheOrDownloadAsync(versionUrls, DownloadManager.VERSION_FORCE_REDOWNLOAD, onDownload);
#endif
            Debug.Log("Download - Finished");
        }

        private State m_state;
        private string m_curIndexHash;
        private DownloadManager.OnDownloadCallbackType m_onDownload = null;

        #endregion
    }
}
