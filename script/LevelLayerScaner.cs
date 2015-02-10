using UnityEditor;
using UnityEngine;
using System.Collections;

public class LevelLayerScaner : MonoBehaviour {

	[MenuItem("Tools/Level/Scan Light level", false, 4)]
	static void Scan()
	{
		Renderer []renderers = GameObject.FindObjectsOfType(typeof(Renderer)) as Renderer[];
		foreach(Renderer r in renderers) {
			if(r.sharedMaterial.shader.name == "InJoy/scene/lightmap" || r.sharedMaterial.shader.name == "InJoy/scene/reflect") {
				r.gameObject.layer = LayerMask.NameToLayer("NONELIGHT");
			}
		}
	}
}
