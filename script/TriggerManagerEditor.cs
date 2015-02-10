using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(TriggerManager))]
public class TriggerManagerEditor : Editor {

	TriggerManager _myTarget;
	
	void OnEnable() {
		_myTarget = target as TriggerManager;
	}
		
	void OnSceneGUI() {
		Color c = Handles.color;
		
		Handles.color = Color.red;
		_myTarget._player1BornPoint = Handles.PositionHandle(_myTarget._player1BornPoint, Quaternion.Euler(0, _myTarget._player1RotationY, 0));
		Handles.Label(_myTarget._player1BornPoint, "player1");
        _myTarget._player2BornPoint = Handles.PositionHandle(_myTarget._player2BornPoint, Quaternion.Euler(0, _myTarget._player2RotationY, 0));
		Handles.Label(_myTarget._player2BornPoint, "player2");
        _myTarget._player3BornPoint = Handles.PositionHandle(_myTarget._player3BornPoint, Quaternion.Euler(0, _myTarget._player3RotationY, 0));
		Handles.Label(_myTarget._player3BornPoint, "player3");
        _myTarget._player4BornPoint = Handles.PositionHandle(_myTarget._player4BornPoint, Quaternion.Euler(0, _myTarget._player4RotationY, 0));
		Handles.Label(_myTarget._player4BornPoint, "player4");
		
		Handles.color = c;
	}
}
