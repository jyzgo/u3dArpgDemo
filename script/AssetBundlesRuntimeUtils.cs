using UnityEngine;

using System;
using System.Text;

namespace InJoy.AssetBundles.Internal
{
    /// <summary>
    /// Utils for the Loader. It is not designed to be used outside of AssetBundles space.
    /// </summary>
    public static class RTUtils
    {
        #region Interface

        public static Version UnityVersion
        {
            get
            {
                if (m_unityVersion != null)
                {
                    return m_unityVersion;
                }
                else
                {
                    string[] unityVersionNumbers = Application.unityVersion.Split('.');
                    int[] numbers = new int[unityVersionNumbers.Length];
                    for (int idx = 0; idx < numbers.Length; ++idx)
                    {
                        int length = unityVersionNumbers[idx].Length;
                        while (length > 0)
                        {
                            try
                            {
                                numbers[idx] = (int)Convert.ChangeType(unityVersionNumbers[idx].Substring(0, length), typeof(int));
                                break;
                            }
                            catch (Exception)
                            {
                                --length;
                            }
                        }
                    }
                    int major = numbers[0];
                    int minor = numbers[1];
                    int micro = numbers[2];
                    m_unityVersion = new Version(major, minor, micro);
                    return m_unityVersion;
                }
            }
        }

        public static bool UncompressedAssetBundlesAllowed
        {
            get
            {
                return true;
            }
        }

        public static byte[] HashToBytes(string hash)
        {
            byte[] bytes = null;
            if (!string.IsNullOrEmpty(hash))
            {
                int size = hash.Length / 2;
                bytes = new byte[size];
                for (int idx = 0; idx < size; ++idx)
                {
                    bytes[idx] = Convert.ToByte(hash.Substring(idx * 2, 2), 16);
                }
            }
            return bytes;
        }

        public static int HashToVersion(string hash)
        {
            Debug.Log("HashToVersion - Started");
            Debug.Log(string.Format("HashToVersion - Hash is \"{0}\"", hash ?? "null"));
            int ret = 0;
            if (!string.IsNullOrEmpty(hash))
            {
                try
                {
                    byte[] bytes = HashToBytes(hash);
                    Assertion.Check(bytes.Length % 4 == 0);
                    const int kLength = 4;
                    for (int idx = 0; idx < kLength; ++idx)
                    {
                        for (int offset = kLength; offset + idx < bytes.Length; offset += kLength)
                        {
                            bytes[idx] ^= bytes[idx + offset];
                        }
                    }
                    ret = (((bytes[0] & 0x7F) << 24) |
                           ((bytes[1] & 0xFF) << 16) |
                           ((bytes[2] & 0xFF) << 8) |
                            (bytes[3] & 0xFF));
                }
                catch (Exception e)
                {
                    Debug.LogError(string.Format("HashToVersion - Hash \"{0}\" could not be converted to version. Caught exception: {1}", hash ?? "null", e.ToString()));
                }
                Debug.Log("HashToVersion - Finished");
            }
            return ret;
        }

        public static bool IsHash(string hash)
        {
            bool ret = false;
            if (!string.IsNullOrEmpty(hash))
            {
                hash = hash.ToLower();
                ret = true;
                for (int i = 0; i < hash.Length; ++i)
                {
                    switch (hash[i])
                    {
                        case '0':
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                        case 'a':
                        case 'b':
                        case 'c':
                        case 'd':
                        case 'e':
                        case 'f':
                            break;
                        default:
                            ret = false;
                            break;
                    }
                    if (!ret)
                    {
                        break;
                    }
                }
            }
            return ret;
        }

        #endregion
        #region Implementation

        private static Version m_unityVersion = null;

        #endregion
    }
}
