using UnityEngine;
using System.Collections;

public class UpgradeShadowEffect : UpgradeEffect {
	
	GameObject _modelCopy = null;
	Renderer _modelCopyRenderer = null;
	
	public Renderer _reference;
	public Material []_grades;
	
	public override void SetGrade (int grade)
	{
		// @TODO, clamp level to 2.
		grade = Mathf.Clamp(grade, 0, 2);
		Assertion.Check(grade >= 0 && grade < _grades.Length);
		
		Material mat = _grades[grade];
		// item on this grade should have no effect.
		if(mat == null) {
			if(_modelCopy != null) {
				if(_modelCopyRenderer.sharedMaterial != null) {
					GameObject.Destroy(_modelCopyRenderer.sharedMaterial);
				}
				GameObject.Destroy(_modelCopy);
				_modelCopy = null;
				_modelCopyRenderer = null;
			}
		}
		else {
			// create model.
			if(_modelCopy == null) {
				_modelCopy = new GameObject("upgrade_shadow");
				_modelCopy.layer = LayerMask.NameToLayer("TransparentFX");
				Transform t = _modelCopy.transform;
				t.parent = _reference.gameObject.transform;
				t.localPosition = Vector3.zero;
				t.localRotation = Quaternion.identity;
				t.localScale = Vector3.one;
				MeshFilter mf = _modelCopy.AddComponent<MeshFilter>();
				MeshFilter refMf = _reference.gameObject.GetComponent<MeshFilter>();
				Assertion.Check(refMf != null);
				mf.sharedMesh = refMf.sharedMesh;
				_modelCopyRenderer = _modelCopy.AddComponent<MeshRenderer>();
				_modelCopyRenderer.sharedMaterial = null;
			}
			// set material.
			Material oldMat = _modelCopyRenderer.sharedMaterial;
			if(oldMat != null) {
				GameObject.Destroy(oldMat);
			}
			oldMat = Utils.CloneMaterial(mat);
			_modelCopyRenderer.sharedMaterial = oldMat;
		}
	}
}
