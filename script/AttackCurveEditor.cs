using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(AttackBase),true)]
public class AttackCurveEditor : Editor {
	
	private static bool _m_haveData;
	private static Keyframe[] _targetKeyFrames = null;
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		AttackBase ab = (AttackBase)target;
		bool pasteWasPressed = false;
		EditorGUIUtility.LookLikeControls();
		EditorGUI.indentLevel = 0;
		Keyframe[] km = null;
		EditorGUILayout.BeginHorizontal();
		{
			if(GUILayout.Button("Copy"))
			{
				_m_haveData = true;
				km = new Keyframe[ab._speedCurve.length];
				Keyframe[] km1 = ab._speedCurve.keys;
				System.Array.Copy(km1, km ,ab._speedCurve.length);
				_targetKeyFrames = km;
				
			}
			GUI.enabled = _m_haveData;
			if(GUILayout.Button("Paste"))
			{
				if(_targetKeyFrames != null)
				{
					pasteWasPressed = true;
				}
			}
			GUI.enabled = true;
		}
		
		EditorGUILayout.EndHorizontal();
		//EditorGUIUtility.LookLikeInspector();
		if(pasteWasPressed)
		{
			ab._speedCurve.keys = _targetKeyFrames;
			EditorUtility.SetDirty(ab);
			pasteWasPressed = false;
		}
	}
}
