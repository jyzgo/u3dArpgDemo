using UnityEngine;
using System.Collections;

public class DrawGizmos : MonoBehaviour {

	#if UNITY_EDITOR
	Transform[] _pathPoint = null;
	void OnDrawGizmos ()
	{
		if(_pathPoint == null || _pathPoint.Length == 0)
		{
			_pathPoint = transform.GetComponentsInChildren<Transform>();
		}
		if(_pathPoint != null && _pathPoint.Length != 0)
		{
			foreach(Transform tf in _pathPoint)
			{
				Gizmos.color = new Color(1f, 0.4f, 0f);
				Gizmos.DrawWireCube(tf.position, new Vector3(1, 1, 1));
			}
		}
			
	}
#endif
}
