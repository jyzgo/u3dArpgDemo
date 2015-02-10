using UnityEngine;
using UnityEditor;
using System.Collections;

public class ToolsEx
{
	[MenuItem("NGUI/Helper/Create Panel with select")]
	static public void AddPanelWithSelection ()
	{
		GameObject go = Selection.activeGameObject;

		if (go != null && EditorUtility.DisplayDialog("Create a panel without undo?",
					"Its not offical function, be careful!",
					"Continue", "Cancel"))
		{
			GameObject child = new GameObject(GetName<UIPanel>());
			child.layer = go.layer;

			Transform ct = child.transform;
			ct.parent = go.transform;
			ct.localPosition = Vector3.zero;
			ct.localRotation = Quaternion.identity;
			ct.localScale = Vector3.one;

			child.AddComponent<UIPanel>();
			Selection.activeGameObject = child;
		}
	}
	
	static public string GetName<T> () where T : Component
	{
		string s = typeof(T).ToString();
		if (s.StartsWith("UI")) s = s.Substring(2);
		else if (s.StartsWith("UnityEngine.")) s = s.Substring(12);
		return s;
	}

}
