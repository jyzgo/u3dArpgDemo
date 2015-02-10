using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(TownPlayerManager))]
public class TownManagerEditor : Editor {

	TownPlayerManager _myTarget;
	int _pntlength;
	
	void OnEnable() {
		_myTarget = target as TownPlayerManager;
		_pntlength = _myTarget._friendBornPoints == null ? 0 : _myTarget._friendBornPoints.Length;
	}
	
	public override void OnInspectorGUI() {
		
		DrawDefaultInspector();
		
		EditorGUILayout.Separator();
		EditorGUILayout.Separator();
		EditorGUILayout.Separator();
		
		EditorGUI.indentLevel = 0;
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.BeginVertical();
		_pntlength = EditorGUILayout.IntField("Friends Born Points", _pntlength);
		EditorGUILayout.EndVertical();
		EditorGUILayout.BeginVertical();
		if(GUILayout.Button("Reset")) {
			_myTarget.ResetFriendBornPoints(_pntlength);
		}
		EditorGUILayout.EndVertical();
		EditorGUILayout.EndHorizontal();
		if(_myTarget._friendBornPoints != null) {
			EditorGUI.indentLevel = 1;
			int pindex = 0;
			foreach(Vector3 p in _myTarget._friendBornPoints) {
				_myTarget._friendBornPoints[pindex] = EditorGUILayout.Vector3Field("friend" + (pindex + 1), p);
				++pindex;
			}
		}
		
		if(GUI.changed) {
			EditorUtility.SetDirty(this);
		}
	}
		
	void OnSceneGUI() {
		Color c = Handles.color;
		
		Handles.color = Color.red;
		
		Handles.Label(_myTarget.transform.position, "Town Manager");
		
		_myTarget._heroBornPoint = Handles.PositionHandle(_myTarget._heroBornPoint, Quaternion.identity);
		Handles.CubeCap(0, _myTarget._heroBornPoint, Quaternion.identity, 1.0f);
		Handles.Label(_myTarget._heroBornPoint, "hero");
		Handles.color = Color.blue;
		int pindex = 0;
		foreach(Vector3 p in _myTarget._friendBornPoints) {
			_myTarget._friendBornPoints[pindex] = Handles.PositionHandle(p, Quaternion.identity);
			Handles.CylinderCap(0, p, Quaternion.AngleAxis(90.0f, Vector3.left), 0.5f);
			Handles.Label(p, "friend" + (pindex + 1));
			++pindex;
		}
		
		Handles.color = c;
	}
}