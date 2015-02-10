using UnityEngine;
using System.Collections;

public class SkillCameraEditorScene : MonoBehaviour {
	
	public CameraController Controller;
	public string PrevewSkill;
	
	protected bool isStarted = false;
	
	void Start()
	{
		isStarted = false;
	}
	// Use this for initialization
	void Update () {
		if(!isStarted && Controller != null && Controller.skillCameraList != null && Controller.skillCameraList._skillCameraList != null)
		{
			int idx = Controller.skillCameraList.GetIndex(PrevewSkill);
			Controller.PreviewSkillCamera(idx);
			isStarted = true;
		}
	}
}
