using UnityEngine;
using System.Collections;

public class AnimBackup : ScriptableObject {
	
	[System.Serializable]
	public class AnimBackupData {
		public string _name;
		public string _curve;
	}
	
	public AnimBackupData []_data;
	
	public string GetAnimationCurveData(string animName) {
		foreach(AnimBackupData abd in _data) {
			if(abd._name == animName) {
				return abd._curve;
			}
		}
		return null;
	}
}
