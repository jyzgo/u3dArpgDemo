using UnityEditor;
using UnityEngine;
using System.Collections.Generic;


public class DefinesWindow : EditorWindow
{
	[MenuItem ("Window/Defines")]
	static void Init()
	{
		// Get existing open window or if none, make a new one:
		EditorWindow.GetWindow(typeof(DefinesWindow));
		
	}
	
	private string _title = "Defines Editor";
	private string _addDefine = "";
	private Vector2 _scrollPosition = Vector2.zero;
	void OnGUI ()
	{
		title = _title;
		_scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, false, false);
		foreach (string define in CSharpCompilerOptions.GetDefines())
		{
			EditorGUILayout.BeginHorizontal();
			
			EditorGUILayout.LabelField(define);
			if(GUILayout.Button("Del"))
				CSharpCompilerOptions.Undefine(define);
			
			EditorGUILayout.EndHorizontal();
		}
		EditorGUILayout.EndScrollView();

		EditorGUILayout.Space();
		EditorGUILayout.BeginHorizontal();
		_addDefine = EditorGUILayout.TextField(_addDefine);
		if(GUILayout.Button("Add"))
		{
			CSharpCompilerOptions.Define(_addDefine);
		}
		
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.Space();
		EditorGUILayout.BeginHorizontal();
		if(GUILayout.Button("Undefine all"))
			CSharpCompilerOptions.UndefineAll();
		if(GUILayout.Button("Reimport C# scripts"))
		{
			CSharpCompilerOptions.ReimportScripts();
		}
		EditorGUILayout.EndHorizontal();
	}
	
}
