using UnityEngine;
using System.Collections;

public class CreateCharacters : MonoBehaviour {
	// Use this for initialization
	
	void OnGUI() {
		if(GUI.Button(new Rect(0.0f, 0.0f, 100.0f, 60.0f), "attack")) {
			GetComponent<Animator>().SetInteger("state", 2);
		}
		if(GUI.Button(new Rect(100.0f, 0.0f, 100.0f, 60.0f), "run")) {
			GetComponent<Animator>().SetInteger("state", 1);
		}
	}
	
	void Update() {
		Animator animator = GetComponent<Animator>();
		float weapon = animator.GetFloat("weaponActivity");
		if(weapon > 0.5f) {
            Debug.Log(weapon.ToString());
		}
	}
}
