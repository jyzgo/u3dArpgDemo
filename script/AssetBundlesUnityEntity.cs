using UnityEngine;

namespace InJoy.AssetBundles.Internal
{
    // HACK: class UnityEntity should be placed out of the
    // namespace in order to instantiate MonoBehaviour object
    // appropriately. The trick is to derive class from UnityEntity,
    // place it in global space, instantiate it and work with
    // it just as with instance of the base class. This would
    // work out, because derived class _is_ the base class.
    using UNITY_ENTITY = DummyUnityEntity;

    public class UnityEntity : MonoBehaviour
    {
        #region Interface

        public static UnityEntity Instance
        {
            get
            {
                if (m_instance != null)
                {
                    return m_instance;
                }
                else
                {
                    const string gameObjectName = "InJoy.AssetBundles GameObject";
                    GameObject go = GameObject.Find(gameObjectName);
                    if (go != null)
                    {
                        m_instance = go.GetComponent<UNITY_ENTITY>();
                        Assertion.Check(m_instance != null);
                        return m_instance;
                    }
                    else
                    {
                        go = new GameObject(gameObjectName);
                        go.AddComponent<UNITY_ENTITY>();
                        UnityEngine.Object.DontDestroyOnLoad(go);
                        m_instance = go.GetComponent<UNITY_ENTITY>();
                        m_instance.useGUILayout = false; // no OnGUI()
                        m_instance.enabled = false;      // no FixedUpdate(), no Update()
                        return m_instance;
                    }
                }
            }
        }

        #endregion
        #region Implementation

        // HACK: TODO: introduce public events, so client can add handlers on
#if UNITY_EDITOR
        // To ensure asset bundles will be unloaded explicitly before exit. This is
        // required in Editor to prevent it's crash. Please see for the details here:
        //   http://forum.unity3d.com/threads/43165-Deleting-persistent-object-without-writing-it-first
        // And here is a log from Unity Editor 3.5.0, these messages are _harmless_:
        //   Internal error. Persistent object is known in persistent manager
        //   Error in file: C:/BuildAgent/work/b0bcff80449a48aa/Runtime/Misc/GarbageCollectSharedAssets.cpp at line: 228
        //   Trying to reload asset from disk that is not stored on disk
        //   Error in file: C:/BuildAgent/work/b0bcff80449a48aa/Runtime/Serialize/PersistentManager.cpp at line: 1115
        private void OnApplicationQuit()
        {
            InJoy.AssetBundles.AssetBundles.UnloadAll(false);
        }
#endif

        private static UnityEntity m_instance = null;

        #endregion
    }
}
