using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;

namespace InJoy.AssetBundles
{
    using Internal;

    /// <summary>
    /// Allows to follow up asynchronous downloading of the asset from asset bundles.
    /// </summary>
    public class AssetBundlesOperation : AssetBundlesOperation<UnityEngine.Object>
    {
        public AssetBundlesOperation(AssetBundleRequest assetBundleRequest, params Action<UnityEngine.Object>[] callbacks)
            : base(assetBundleRequest, callbacks)
        {
        }

        public AssetBundlesOperation(UnityEngine.Object asset, params Action<UnityEngine.Object>[] callbacks)
            : base(asset, callbacks)
        {
        }
    }

    /// <summary>
    /// Allows to follow up asynchronous downloading of the asset from asset bundles.
    /// </summary>
    public class AssetBundlesOperation<T> where T : UnityEngine.Object
    {
        #region Interface

        public static implicit operator YieldInstruction(AssetBundlesOperation<T> assetBundlesOperation)
        {
            if (assetBundlesOperation.m_assetBundleRequest != null)
            {
                return assetBundlesOperation.m_assetBundleRequest;
            }
            else
            {
                return null;
            }
        }

        public bool isDone
        {
            get
            {
                if (m_assetBundleRequest != null)
                {
                    return m_assetBundleRequest.isDone;
                }
                else
                {
                    return m_isDone;
                }
            }
        }

        public float progress
        {
            get
            {
                if (m_assetBundleRequest != null)
                {
                    return m_assetBundleRequest.progress;
                }
                else
                {
                    return m_progress;
                }
            }
        }


        public int priority
        {
            set
            {
                if (m_assetBundleRequest != null)
                {
                    m_assetBundleRequest.priority = value;
                }
                else
                {
                    m_priority = value;
                }
            }
            get
            {
                if (m_assetBundleRequest != null)
                {
                    return m_assetBundleRequest.priority;
                }
                else
                {
                    return m_priority;
                }
            }
        }


        public T asset
        {
            get
            {
                if (m_assetBundleRequest != null)
                {
                    return m_assetBundleRequest.asset as T; // could be null
                }
                else
                {
                    return m_asset;
                }
            }
        }

        public AssetBundlesOperation(AssetBundleRequest assetBundleRequest, params Action<T>[] callbacks)
        {
            Debug.Log("Ctor - Started");
            Assertion.Check(assetBundleRequest != null); // SANITY CHECK
            m_assetBundleRequest = assetBundleRequest;
            RegisterCallback(callbacks);
            ProcessRequest();
            Debug.Log("Ctor - Finished");
        }

        public AssetBundlesOperation(T asset, params Action<T>[] callbacks)
        {
            Debug.Log("Ctor - Started");
            m_isDone = true;
            m_progress = 1.0f;
            m_priority = 0;
            m_asset = asset;
            RegisterCallback(callbacks);
            ProcessRequest();
            Debug.Log("Ctor - Finished");
        }

        /// <summary>
        /// Returns instance which can be yielded on.
        /// Do not yield directly.
        /// </summary>
        /// <returns>
        /// The YieldInstruction.
        /// </returns>
        public YieldInstruction ToYieldInstruction()
        {
            return this;
        }

        #endregion
        #region Implementation

        private static IEnumerator WaitForResultRoutine(AssetBundlesOperation<T> self)
        {
            Debug.Log("WaitForResultRoutine - Started");
            yield return self.ToYieldInstruction();
            self.InvokeRegisteredCallbacks();
            Debug.Log("WaitForResultRoutine - Finished");
            yield break;
        }

        private List<Action<T>> Callbacks
        {
            get
            {
                return m_callbacks;
            }
        }

        private void RegisterCallback(params Action<T>[] callbacks)
        {
            Debug.Log("RegisterCallback - Started");
            foreach (Action<T> callback in callbacks)
            {
                if (callback != null)
                {
                    Callbacks.Add(callback);
                }
            }
            Debug.Log("RegisterCallback - Finished");
        }

        private void ProcessRequest()
        {
            Debug.Log("ProcessRequest - Started");
            if (Callbacks.Count > 0)
            {
                Debug.Log("ProcessRequest - Starting coroutine...");
                UnityEntity.Instance.StartCoroutine(WaitForResultRoutine(this));
            }
            Debug.Log("ProcessRequest - Finished");
        }

        private void InvokeRegisteredCallbacks()
        {
            Debug.Log("InvokeRegisteredCallbacks - Started");
            try
            {
                foreach (Action<T> callback in Callbacks)
                {
                    Assertion.Check(callback != null);
                    try
                    {
                        callback(asset);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(string.Format("InvokeRegisteredCallbacks - Caught exception: {0}", e.ToString()));
                    }
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                Callbacks.Clear();
            }
            Debug.Log("InvokeRegisteredCallbacks - Finished");
        }

        private List<Action<T>> m_callbacks = new List<Action<T>>();
        private AssetBundleRequest m_assetBundleRequest;
        private bool m_isDone;
        private float m_progress;
        private int m_priority;
        private T m_asset;

        #endregion
    }
}
