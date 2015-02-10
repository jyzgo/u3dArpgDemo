using UnityEngine;
using System.Collections;


public class QuadMeshFactory {

	static System.Collections.Generic.Dictionary<int, Mesh> _particleMeshLibrary = null;
	
	static public Mesh GetParticleMesh(int count) {
		if(_particleMeshLibrary == null) {
			_particleMeshLibrary = new System.Collections.Generic.Dictionary<int, Mesh>();
		}
		Mesh mesh = null;
		if(!_particleMeshLibrary.TryGetValue(count, out mesh)) {
			mesh = new Mesh();
			mesh.name = "particle mesh(" + count + ")";
			mesh.Clear(false);
			Vector3 []posTemplate = new Vector3[]{new Vector3(-0.5f, 0.0f, 0.5f), new Vector3(0.5f, 0.0f, 0.5f), new Vector3(-0.5f, 0.0f, -0.5f), new Vector3(0.5f, 0.0f, -0.5f)};
			Vector2 []texcoordTemplate = new Vector2[]{new Vector2(0.0f, 1.0f), new Vector2(1.0f, 1.0f), new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.0f)};
			Vector3 []posArray = new Vector3[count * 4];
			Vector2 []texArray = new Vector2[count * 4];
			Color []colorArray = new Color[count * 4];
			int []indice = new int[count * 6];
			for(int i = 0;i < count;++i) {
				posArray[i * 4] = posTemplate[0];
				posArray[i * 4 + 1] = posTemplate[1];
				posArray[i * 4 + 2] = posTemplate[2];
				posArray[i * 4 + 3] = posTemplate[3];
				
				texArray[i * 4] = texcoordTemplate[0];
				texArray[i * 4 + 1] = texcoordTemplate[1];
				texArray[i * 4 + 2] = texcoordTemplate[2];
				texArray[i * 4 + 3] = texcoordTemplate[3];
				
				Color color = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 
											Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
				colorArray[i * 4] = color;
				colorArray[i * 4 + 1] = color;
				colorArray[i * 4 + 2] = color;
				colorArray[i * 4 + 3] = color;
				
				indice[i * 6] = 0;
				indice[i * 6 + 1] = 1;
				indice[i * 6 + 2] = 3;
				indice[i * 6 + 3] = 0;
				indice[i * 6 + 4] = 3;
				indice[i * 6 + 5] = 2;				
			}
			mesh.vertices = posArray;
			mesh.uv = texcoordTemplate;
			mesh.colors = colorArray;
			mesh.triangles = indice;
			
			_particleMeshLibrary[count] = mesh;
		}
		return mesh;
	}
	
	static Mesh _centralMesh = null;
	static Mesh _leftAlignedMesh = null;
	
	static public Mesh GetCentralMesh() {
		if(_centralMesh == null) {
			_centralMesh = new Mesh();
			_centralMesh.hideFlags = HideFlags.HideAndDontSave;
			_centralMesh.name = "billboard mesh (central)";
			_centralMesh.Clear();
			_centralMesh.vertices = new Vector3[]{new Vector3(-0.5f, 0.0f, 0.5f), new Vector3(0.5f, 0.0f, 0.5f), new Vector3(-0.5f, 0.0f, -0.5f), new Vector3(0.5f, 0.0f, -0.5f)};
			_centralMesh.uv = new Vector2[]{new Vector2(0.0f, 1.0f), new Vector2(1.0f, 1.0f), new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.0f)};
			_centralMesh.triangles = new int[]{0, 1, 3, 0, 3, 2};
		}
		return _centralMesh;
	}
	static public Mesh GetLeftAlignedMesh() {
		if(_leftAlignedMesh == null) {
			_leftAlignedMesh = new Mesh();
			_leftAlignedMesh.name = "billboard mesh (left-aligned)";
			_leftAlignedMesh.Clear();
			_leftAlignedMesh.vertices = new Vector3[]{new Vector3(0.0f, 0.0f, 0.5f), new Vector3(1.0f, 0.0f, 0.5f), new Vector3(0.0f, 0.0f, -0.5f), new Vector3(1.0f, 0.0f, -0.5f)};
			_leftAlignedMesh.uv = new Vector2[]{new Vector2(0.0f, 1.0f), new Vector2(1.0f, 1.0f), new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.0f)};
			_leftAlignedMesh.triangles = new int[]{0, 1, 3, 0, 3, 2};
		}
		return _leftAlignedMesh;
	}
}
