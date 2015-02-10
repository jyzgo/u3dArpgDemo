using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("FC/Utils/Path/FCPath")]
public class FCPath : MonoBehaviour{
	
	public string _pathName ="";
	public Color _pathColor = Color.blue;
	public Vector3[] _nodes;
	public int _keyCount;
	public Vector3[] _realPathnodes;
	
	void Awake()
	{
	}
	
	public void ForceRereashPath()
	{
		if(_keyCount == _nodes.Length)
		{
			_realPathnodes = new Vector3[_keyCount];
			System.Array.Copy(_nodes,_realPathnodes,_realPathnodes.Length);
		}
		else
		{
			Vector3[] nodeTemp = PathControlPointGenerator(_nodes);
			_realPathnodes = new Vector3[_keyCount];
			_realPathnodes[0] = nodeTemp[1];
			_realPathnodes[_realPathnodes.Length-1] = nodeTemp[nodeTemp.Length-2];
			for(int i =1; i< _keyCount-1;i++)
			{
				_realPathnodes[i] = Interp(nodeTemp,(float)i/_keyCount);
			}
			nodeTemp = null;
		}
		
	}
	#region help function
	void OnDrawGizmosSelected(){
		if(enabled) 
		{
			if(_nodes != null && _nodes.Length > 0){
				DrawPathHelper(_nodes,_pathColor);
			}
		} 
	}
	
	public void PutOnPath(Transform target,float percent){
		target.position=Interp(_realPathnodes,percent);
	}
	
	public void GetPath(out Vector3[] realPath)
	{
		realPath = _realPathnodes;
	}
	private static void DrawPathHelper(Vector3[] path, Color color)
	{
		Vector3[] vector3s = PathControlPointGenerator(path);
		
		//Line Draw:
		Vector3 prevPt = Interp(vector3s,0);
		Gizmos.color=color;
		int SmoothAmount = path.Length*20;
		for (int i = 1; i <= SmoothAmount; i++) {
			float pm = (float) i / SmoothAmount;
			Vector3 currPt = Interp(vector3s,pm);
			Gizmos.DrawLine(currPt, prevPt);
			prevPt = currPt;
		}
	}	
	
	private static Vector3[] PathControlPointGenerator(Vector3[] path){
		Vector3[] suppliedPath;
		Vector3[] vector3s;
		
		//create and store path points:
		suppliedPath = path;

		//populate calculate path;
		int offset = 2;
		vector3s = new Vector3[suppliedPath.Length+offset];
		System.Array.Copy(suppliedPath,0,vector3s,1,suppliedPath.Length);
		
		//populate start and end control points:
		//vector3s[0] = vector3s[1] - vector3s[2];
		vector3s[0] = vector3s[1] + (vector3s[1] - vector3s[2]);
		vector3s[vector3s.Length-1] = vector3s[vector3s.Length-2] + (vector3s[vector3s.Length-2] - vector3s[vector3s.Length-3]);
		
		//is this a closed, continuous loop? yes? well then so let's make a continuous Catmull-Rom spline!
		if(vector3s[1] == vector3s[vector3s.Length-2]){
			Vector3[] tmpLoopSpline = new Vector3[vector3s.Length];
			System.Array.Copy(vector3s,tmpLoopSpline,vector3s.Length);
			tmpLoopSpline[0]=tmpLoopSpline[tmpLoopSpline.Length-3];
			tmpLoopSpline[tmpLoopSpline.Length-1]=tmpLoopSpline[2];
			vector3s=new Vector3[tmpLoopSpline.Length];
			System.Array.Copy(tmpLoopSpline,vector3s,tmpLoopSpline.Length);
		}	
		
		return(vector3s);
	}
	
	//andeeee from the Unity forum's steller Catmull-Rom class ( http://forum.unity3d.com/viewtopic.php?p=218400#218400 ):
	private static Vector3 Interp(Vector3[] pts, float t){
		int numSections = pts.Length - 3;
		int currPt = Mathf.Min(Mathf.FloorToInt(t * (float) numSections), numSections - 1);
		float u = t * (float) numSections - (float) currPt;
				
		Vector3 a = pts[currPt];
		Vector3 b = pts[currPt + 1];
		Vector3 c = pts[currPt + 2];
		Vector3 d = pts[currPt + 3];
		
		return .5f * (
			(-a + 3f * b - 3f * c + d) * (u * u * u)
			+ (2f * a - 5f * b + 4f * c - d) * (u * u)
			+ (-a + c) * u
			+ 2f * b
		);
	}
	#endregion
	
}
