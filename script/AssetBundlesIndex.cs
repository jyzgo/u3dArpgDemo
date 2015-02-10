using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

namespace InJoy.AssetBundles
{
    using Internal;

    /// <summary>
    /// Contains entire Index (asset-to-bundle distribution) in a plain raw data,
    /// which could be easily serialized/deserialized by use of C# statements.
    /// </summary>
    [XmlTypeAttribute("Index")]
    public class Index
    {
        #region Implementation - XMLSerializable part

        [XmlTypeAttribute("AssetBundle")]
        public class AssetBundle
        {
            [XmlTypeAttribute("Asset")]
            public class Asset
            {
                public Asset()
                {
                    m_filename = null;
                    m_guid = null;
                    m_hash = null;
                }

                [XmlAttribute("Filename")]
                public string m_filename;
                [XmlAttribute("GUID")]
                public string m_guid;
                [XmlAttribute("Hash")]
                public string m_hash;
            }

            public enum Type
            {
                None,
                Asset,
                Scene,
            }

            public AssetBundle()
            {
                m_filename = "";
                m_type = Type.None;
                m_isCompressed = null;
                m_size = null;
                m_contentHash = null;
                m_urls = null;
                m_assets = new List<Asset>();
                m_assets.Clear();
                m_editableFilename = null;
                m_wasDiffProcessed = false;
                m_wasAssetsSubstituted = false;
            }

            public long Size { get { return m_size.HasValue ? m_size.Value : 0; } }

            [XmlAttribute("Filename")]
            public string m_filename;
            [XmlAttribute("Type")]
            public Type m_type;
            [XmlAttribute("Compressed")]
            public bool? m_isCompressed;
            [XmlAttribute("Size")]
            public long? m_size;
            [XmlAttribute("ContentHash")]
            public string m_contentHash;
            [XmlArrayAttribute("Urls")]
            [XmlArrayItem("URL")]
            public string[] m_urls;
            [XmlArrayAttribute("Assets")]
            [XmlArrayItem("Asset")]
            public List<Asset> m_assets;

            [XmlIgnoreAttribute()]
            public string m_editableFilename; // to modify m_filename value in text field
            [XmlIgnoreAttribute()]
            public bool m_wasDiffProcessed; // to track diffs
            [XmlIgnoreAttribute()]
            public bool m_wasAssetsSubstituted; // contained fake assets (stub)
        }

        static Index()
        {
            m_overridenFilenameMask = null;
        }

        public Index()
        {
            m_filename = "index"; // by default
            m_buildTag = null;
            m_assetBundles = new List<AssetBundle>();
            m_assetBundles.Clear();
            m_strippedIndexHash = null;
            m_xmlFilename = null;
            m_editableFilename = null;
            m_indexToCompareWith = null;
        }

        public static string m_overridenFilenameMask;
        [XmlAttribute("Filename")]
        public string m_filename;
        [XmlAttribute("BuildTag")]
        public string m_buildTag;
        [XmlArrayAttribute("AssetBundles")]
        [XmlArrayItem("AssetBundle")]
        public List<AssetBundle> m_assetBundles;
        [XmlAttribute("StrippedIndexHash")]
        public string m_strippedIndexHash; // to make a test that filled index matchs stripped index on load

        [XmlIgnoreAttribute()]
        public string m_xmlFilename; // to manage source file
        [XmlIgnoreAttribute()]
        public string m_editableFilename; // to modify m_filename value in text field
        [XmlIgnoreAttribute()]
        public Index m_indexToCompareWith;

        #endregion
        #region Interface

        public static Index CreateInstance()
        {
            //Debug.Log("CreateInstance - Started");
            Index ret = new Index();
            //Debug.Log("CreateInstance - Finished");
            return ret;
        }

        public static Index DuplicateInstance(Index srcIndex)
        {
            return DuplicateInstance(srcIndex, false);
        }

        public static Index DuplicateAndStripInstance(Index srcIndex)
        {
            return DuplicateInstance(srcIndex, true);
        }

        public static Index LoadInstance(Stream stream)
        {
            //Debug.Log("LoadInstance - Started");
            Index ret = null;
            try
            {
                ret = xmlSerializer.Deserialize(stream) as Index;
            }
            catch (Exception e)
            {
                //Debug.LogWarning(string.Format("LoadInstance - Caught exception: {0}", e.ToString()));
                ret = null;
            }
            //Debug.Log("LoadInstance - Finished");
            return ret;
        }

        public void SaveInstance(Stream stream)
        {
            //Debug.Log("SaveInstance - Started");
            using (XmlWriter xmlWriter = XmlWriter.Create(stream, xmlWriterSettings))
            {
                xmlSerializer.Serialize(xmlWriter, this);
            }
            //Debug.Log("SaveInstance - Finished");
        }

        #endregion
        #region Implementation

        private static Index DuplicateInstance(Index srcIndex, bool stripInstance)
        {
            //Debug.Log("DuplicateInstance - Started");
            Index dstIndex = null;
            if (srcIndex != null)
            {
                dstIndex = new Index();
                dstIndex.m_filename = srcIndex.m_filename;
                if (!stripInstance)
                {
                    dstIndex.m_buildTag = srcIndex.m_buildTag;
                }
                dstIndex.m_assetBundles = new List<AssetBundle>();
                dstIndex.m_assetBundles.Clear();
                foreach (Index.AssetBundle srcAssetBundle in srcIndex.m_assetBundles)
                {
                    Index.AssetBundle dstAssetBundle = new Index.AssetBundle();
                    dstAssetBundle.m_filename = srcAssetBundle.m_filename;
                    dstAssetBundle.m_type = srcAssetBundle.m_type;
                    dstAssetBundle.m_isCompressed = srcAssetBundle.m_isCompressed;
                    if (!stripInstance)
                    {
                        dstAssetBundle.m_size = srcAssetBundle.m_size;
                        dstAssetBundle.m_contentHash = srcAssetBundle.m_contentHash;
                        if (srcAssetBundle.m_urls != null)
                        {
                            dstAssetBundle.m_urls = new string[srcAssetBundle.m_urls.Length];
                            Array.Copy(srcAssetBundle.m_urls, dstAssetBundle.m_urls, srcAssetBundle.m_urls.Length);
                        }
                        else
                        {
                            dstAssetBundle.m_urls = null;
                        }
                    }
                    dstAssetBundle.m_assets = new List<Index.AssetBundle.Asset>();
                    dstAssetBundle.m_assets.Clear();
                    foreach (Index.AssetBundle.Asset srcAsset in srcAssetBundle.m_assets)
                    {
                        Index.AssetBundle.Asset dstAsset = new Index.AssetBundle.Asset();
                        dstAsset.m_filename = srcAsset.m_filename;
                        dstAsset.m_guid = srcAsset.m_guid;
                        if (!stripInstance)
                        {
                            dstAsset.m_hash = srcAsset.m_hash;
                        }
                        dstAssetBundle.m_assets.Add(dstAsset);
                    }
                    dstIndex.m_assetBundles.Add(dstAssetBundle);
                }
            }
            //Debug.Log("DuplicateInstance - Finished");
            return dstIndex;
        }

        private static XmlWriterSettings xmlWriterSettings
        {
            get
            {
                if (m_xmlWriterSettings != null)
                {
                    return m_xmlWriterSettings;
                }
                else
                {
                    m_xmlWriterSettings = new XmlWriterSettings();
                    m_xmlWriterSettings.Encoding = Encoding.UTF8;
                    m_xmlWriterSettings.Indent = true;
                    m_xmlWriterSettings.IndentChars = "\t";
                    m_xmlWriterSettings.NewLineChars = "\r\n"; // CR LF on every platform
                    m_xmlWriterSettings.NewLineHandling = NewLineHandling.None;
                    m_xmlWriterSettings.OmitXmlDeclaration = true;
                    return m_xmlWriterSettings;
                }
            }
        }

        private static XmlSerializer xmlSerializer
        {
            get
            {
                if (m_xmlSerializer != null)
                {
                    return m_xmlSerializer;
                }
                else
                {
                    m_xmlSerializer = new XmlSerializer(typeof(Index));
                    return m_xmlSerializer;
                }
            }
        }

        // to serialize/deserialize instances
        private static XmlWriterSettings m_xmlWriterSettings = null;
        private static XmlSerializer m_xmlSerializer = null;

        #endregion
    }
}
