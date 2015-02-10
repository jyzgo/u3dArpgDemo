using UnityEngine;
using System.Collections;

public class WorldMapsEntry : MonoBehaviour {

	public int nextWorldmapIndex;
	
	// Update is called once per frame
	void OnClick () {
		WorldMapController.Instance.ActiveWorldMap(nextWorldmapIndex);
	}
}
