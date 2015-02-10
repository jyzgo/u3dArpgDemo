using UnityEngine;
using System.Collections;

public class TestParticle : MonoBehaviour {
	
	public GameObject _particleTemplate;
	
    void OnGUI() {
        if (GUILayout.Button("Generate Particle")) {
			GameObject testParticle = GameObject.Instantiate(_particleTemplate) as GameObject;
			Debug.Log("Generate particle " + testParticle.name);
		}
    }	
	
	
	// Use this for initialization
	void Start () {
		
		
		
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
