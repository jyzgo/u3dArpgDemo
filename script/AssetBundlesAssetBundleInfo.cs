namespace InJoy.AssetBundles
{
    using UnityEngine;
    using Internal;

    /// <summary>
    /// The description of downloaded asset bundle.
    /// Instances would be created automatically during downloading.
    /// So end user should not do anything.
    /// </summary>
    public class AssetBundleInfo
    {
        # region Interface

        /// <summary>
        /// Gets information about download.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public DownloadManager.Info downloadInfo
        {
            get { return m_downloadInfo; }
        }

        /// <summary>
        /// Gets the asset bundle itself. If you want to load asset from
        /// the asset bundle, it is recommended to use AssetBundles.Load.
        /// </summary>
        /// <value>
        /// The asset bundle.
        /// </value>
        public UnityEngine.AssetBundle assetBundle
        {
            get { return downloadInfo.assetBundle; }
        }

        /// <summary>
        /// Gets original filename w/o hash suffix of the asset bundle.
        /// </summary>
        /// <value>
        /// The filename.
        /// </value>
        public string Filename
        {
            get { return m_filename; }
        }

        /// <summary>
        /// Gets size of the asset bundle.
        /// </summary>
        /// <value>
        /// The size.
        /// </value>
        public long Size
        {
            get { return m_size; }
        }

        /// <summary>
        /// Gets type of the asset bundle.
        /// </summary>
        /// <value>
        /// The type. It could be "Asset" or "Scene".
        /// </value>
        public string Type
        {
            get { return m_type; }
        }

        /// <summary>
        /// Creates instance of description of the asset bundle. It would
        /// be invoked automatically from IndexInfo class with correct parameters.
        /// So end user should not do anything.
        /// </summary>
        public AssetBundleInfo(string filename,
            long size,
            string contentHash,
            string type,
            string[] urls,
            DownloadManager.OnDownloadCallbackType onDownloadCallback)
        {
            Debug.Log("Ctor - Started");
            m_filename = filename;
            m_size = size;
            int version = RTUtils.HashToVersion(contentHash);
            m_type = type;
            m_downloadInfo = DownloadManager.LoadFromCacheOrDownloadAsync(urls, version, onDownloadCallback);
            Debug.Log("Ctor - Finished");
        }

        #endregion
        #region Implementation

        private DownloadManager.Info m_downloadInfo;
        private string m_filename;
        private string m_internalName;
        private long m_size;
        private string m_type;

        #endregion
    }
}
