using UnityEngine;
using UnityEditor;
using System.Collections;

class OneKeyTools
{
    [MenuItem("NGUI/Onekey/Create Offset")]
    static public GameObject CreateEmpty()
    {
        GameObject go = Selection.activeGameObject;
        GameObject child = null;
        if (go != null)
        {
            child = new GameObject();
            child.layer = go.layer;
            child.name = "Offset";
            Transform ct = child.transform;
            ct.parent = go.transform;
            ct.localPosition = Vector3.zero;
            ct.localRotation = Quaternion.identity;
            ct.localScale = Vector3.one;
            Selection.activeGameObject = child;
        }
        return child;
    }

    [MenuItem("NGUI/Onekey/Create Anchor")]
    static public GameObject CreateAnchor()
    {
        GameObject child = CreateEmpty();
        if (child)
        {
            child.AddComponent<UIAnchor>();
        }
        return child;
    }
}

