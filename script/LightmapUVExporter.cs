using UnityEngine;
using System.Collections;
using UnityEditor;

public class LightmapUVExporter{
	
	public struct vertex
	{
		public Vector3 pos;
		public Vector2 texcoord;
		public Vector3 normal;
	};
	
	[MenuItem("Tools/Level/Export Selected Static Models", false, 4)]
	static void Export()
	{
		System.Collections.Generic.List<vertex> vertices = new System.Collections.Generic.List<vertex>();
		System.Collections.Generic.List<int> indices = new System.Collections.Generic.List<int>();
		
		for(int i = 0;i < Selection.gameObjects.Length;++i)
		{
			Renderer []renderers = Selection.gameObjects[i].GetComponentsInChildren<Renderer>();
			for(int j = 0;j < renderers.Length;++j)
			{
				MeshFilter meshfilter = renderers[j].gameObject.GetComponent<MeshFilter>();
				if(renderers[j].gameObject.isStatic && renderers[j].gameObject.activeSelf && renderers[j].enabled
					&& meshfilter != null && renderers[j].lightmapIndex != 255)
				{
					Mesh mesh = meshfilter.sharedMesh;
					if(mesh != null && mesh.uv2 != null)
					{
						Transform trans = renderers[j].gameObject.transform;
						vertex newvert;
						int start = vertices.Count;
						for(int k = 0;k < mesh.vertices.Length;++k)
						{
							newvert.pos = trans.TransformPoint(mesh.vertices[k]);
							newvert.normal = trans.TransformDirection(mesh.normals[k]);
							newvert.texcoord = mesh.uv2[k];
							newvert.texcoord.x = newvert.texcoord.x * renderers[j].lightmapTilingOffset.x + renderers[j].lightmapTilingOffset.z;
							newvert.texcoord.y = newvert.texcoord.y * renderers[j].lightmapTilingOffset.y + renderers[j].lightmapTilingOffset.w;
							vertices.Add(newvert);
						}
						for(int k = 0;k < mesh.triangles.Length;++k)
						{
							indices.Add(mesh.triangles[k] + 1 + start);
						}
					}
				}
			}
		}
		
		if(indices.Count > 0)
		{
			string []path = EditorApplication.currentScene.Split(char.Parse("."));
    		path[path.Length -1] = "obj";
			System.IO.StreamWriter file = System.IO.File.CreateText(string.Join(".", path));
			for(int i = 0;i < vertices.Count;++i)
			{
				file.WriteLine("v  " + vertices[i].pos.x + " " + vertices[i].pos.y + " " + vertices[i].pos.z);
			}
			file.WriteLine("");
			for(int i = 0;i < vertices.Count;++i)
			{
				file.WriteLine("vn " + vertices[i].normal.x + " " + vertices[i].normal.y + " " + vertices[i].normal.z);
			}
			file.WriteLine("");
			for(int i = 0;i < vertices.Count;++i)
			{
				file.WriteLine("vt " + vertices[i].texcoord.x + " " + vertices[i].texcoord.y);
			}
			file.WriteLine("");
			
			file.WriteLine("g Terrain");
			for(int i = 0;i < indices.Count;i += 3)
			{
				file.WriteLine("f " + indices[i] + "/" + indices[i] + "/" + indices[i]
					+ " " + indices[i + 1] + "/" + indices[i + 1] + "/" + indices[i + 1]
					+ " " + indices[i + 2] + "/" + indices[i + 2]);
			}
			file.WriteLine("");
			file.Close();
		}
	}
}
