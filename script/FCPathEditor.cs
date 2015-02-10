using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(FCPath))]
public class FCPathEditor : Editor
{
	FCPath _target;
	GUIStyle style = new GUIStyle();
	public static int counter = 0;
	public int nodecount = 0;
	Vector3 _basePos = Vector3.zero;
	
	void OnEnable(){
		//i like bold handle labels since I'm getting old:
		style.fontStyle = FontStyle.Bold;
		style.normal.textColor = Color.white;
		_target = (FCPath)target;
		_basePos = _target.transform.position;
	}
	
	public override void OnInspectorGUI(){		
		//path name:

		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Path Name");
		_target._pathName = EditorGUILayout.TextField(_target._pathName);
		EditorGUILayout.EndHorizontal();
		
		if(_target._pathName == ""){
			_target._pathName = "path1";
		}
		if(_target.gameObject.name != _target._pathName)
		{
			_target.gameObject.name = _target._pathName;
		}
		if(_target._nodes == null)
		{
			_target._nodes = new Vector3[2];
		}
		//path color:
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Path Color");
		_target._pathColor = EditorGUILayout.ColorField(_target._pathColor);
		EditorGUILayout.EndHorizontal();
		
		//exploration segment count control:
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Node Count");
		nodecount =  Mathf.Clamp(EditorGUILayout.IntSlider(_target._nodes.Length, 0, 10), 2,100);
		EditorGUILayout.EndHorizontal();
		if(_target._keyCount <= nodecount)
		{
			_target._keyCount = nodecount;
		}
		if(_target._keyCount == 1)
		{
			_target._keyCount = 2;
		}
		//add node?
		if(nodecount > _target._nodes.Length)
		{
			Vector3[] nodeArray = new Vector3[nodecount];
			System.Array.Copy(_target._nodes,nodeArray,_target._nodes.Length);
			_target._nodes = null;
			_target._nodes = nodeArray;
			_target.ForceRereashPath();
		}
	
		//remove node?
		if(nodecount < _target._nodes.Length){
			if(EditorUtility.DisplayDialog("Remove path node?","Realy?", "OK", "Cancel"))
			{
				Vector3[] nodeArray = new Vector3[nodecount];
				System.Array.Copy(_target._nodes,nodeArray,nodecount);
				_target._nodes = null;
				_target._nodes = nodeArray;
				_target.ForceRereashPath();
			}
		}
		

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Key Count");
		_target._keyCount =  EditorGUILayout.IntField(_target._keyCount);
		EditorGUILayout.EndHorizontal();
		
		//node display:
		EditorGUI.indentLevel = 4;
		for (int i = 0; i < _target._nodes.Length; i++) {
			_target._nodes[i] = EditorGUILayout.Vector3Field("Node " + (i+1), _target._nodes[i]);
		}
		
		if(_basePos != _target.transform.position)
		{
			for (int i = 0; i < _target._nodes.Length; i++) 
			{
				_target._nodes[i] = _target._nodes[i]-_basePos+_target.transform.position;
			}
			_basePos = _target.transform.position;
		}
		
		//update and redraw:
		if(GUI.changed){
			EditorUtility.SetDirty(_target);			
		}
	}
	
	void OnSceneGUI(){
		if(_target.enabled) { // dkoontz
			if(_target._nodes != null &&_target._nodes.Length > 0){
				//allow path adjustment undo:
				Undo.RecordObject(_target,"Adjust FC Path");
				
				//path begin and end labels:
				Handles.Label(_target._nodes[0], "'" + _target._pathName + "' Begin", style);
				Handles.Label(_target._nodes[_target._nodes.Length-1], "'" + _target._pathName + "' End", style);
				
				//node handle display:
				for (int i = 0; i < _target._nodes.Length; i++) {
					_target._nodes[i] = Handles.PositionHandle(_target._nodes[i], Quaternion.identity);
				}	
				_target.ForceRereashPath();
			}
		} // dkoontz
    }
}