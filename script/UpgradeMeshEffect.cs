using UnityEngine;
using System.Collections;

public class UpgradeMeshEffect : UpgradeEffect {
	
	public GameObject []_grades;
		
	public override void SetGrade (int grade)
	{
		// disable it temperarily.
		grade = 0;
		// @TODO, clamp level to 2.
		grade = Mathf.Clamp(grade, 0, 2);
		Assertion.Check(grade >= 0 && grade < _grades.Length);
		foreach(GameObject go in _grades) {
			if(go != null) {
				go.SetActive(false);
			}
		}
		if(_grades[grade] != null) {
			_grades[grade].SetActive(true);
		}
	}
}
