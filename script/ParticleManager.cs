using UnityEngine;
using System.Collections;

public class ParticleManager : MonoBehaviour {
	
	static ParticleManager _inst;
	static public ParticleManager Instance {
		get {
			return _inst;
		}
	}	
	
	void Awake() {
		if(_inst != null)
		{
            Debug.LogError("ParticleManager: detected singleton instance has existed. Destroy this one " + gameObject.name);
			Destroy(this);
			return;
		}
		
		_inst = this;
	}	
	
	private Mesh[] _particleMeshBuffer = null;
	private int[] _particleMeshSize = new int[]{1, 5, 10, 20};
	private int _meshBufferCount = (int)FC_PARTICLE_BULLET_NUMBER.FC_PARTICLE_BULLET_COUNT;
	
	public Mesh GetMeshAt(int meshIndex)
	{
		Assertion.Check(meshIndex < _meshBufferCount);
		return _particleMeshBuffer[meshIndex];
	}
	
	private void InitMeshBuffers() 
	{
		_particleMeshBuffer = new Mesh[_meshBufferCount];

		//build mesh buffers
		for (int i=0; i<_meshBufferCount; i++)
		{
			//new mesh
			_particleMeshBuffer[i] = new Mesh();
			
			//build vertices
			Vector3[] vertices = new Vector3[4 * _particleMeshSize[i]];
			for (int j=0; j<_particleMeshSize[i]; j++)
			{
				// 1 2
				// 0 3
				vertices[0 + 4*j] = new Vector3(-0.5f, -0.5f, 0);
				vertices[1 + 4*j] = new Vector3(-0.5f, 0.5f, 0);
				vertices[2 + 4*j] = new Vector3(0.5f, 0.5f, 0);
				vertices[3 + 4*j] = new Vector3(0.5f, -0.5f, 0);
				Quaternion rot = Quaternion.AngleAxis(0.0f, Vector3.forward);
				vertices[0 + 4 * j] = rot * vertices[0 + 4*j];
				vertices[1 + 4 * j] = rot * vertices[1 + 4*j];
				vertices[2 + 4 * j] = rot * vertices[2 + 4*j];
				vertices[3 + 4 * j] = rot * vertices[3 + 4*j];
			}
			_particleMeshBuffer[i].vertices = vertices;
			
			
			//build index
			int[] indices = new int[6 * _particleMeshSize[i]];
			for (int j=0; j<_particleMeshSize[i]; j++)
			{
				//0-1-2, 0-2-3
				indices[0 + 6*j] = 0 + 4*j;
				indices[1 + 6*j] = 1 + 4*j;
				indices[2 + 6*j] = 2 + 4*j;
				indices[3 + 6*j] = 0 + 4*j;
				indices[4 + 6*j] = 2 + 4*j;
				indices[5 + 6*j] = 3 + 4*j;
			}
			_particleMeshBuffer[i].triangles = indices;
			
	
			//build UVs
			Vector2[] uv = new Vector2[4 * _particleMeshSize[i]];
			for (int j=0; j<_particleMeshSize[i]; j++)
			{
				uv[0 + 4*j] = new Vector2(0, 0);
				uv[1 + 4*j] = new Vector2(0, 1);
				uv[2 + 4*j] = new Vector2(1, 1);
				uv[3 + 4*j] = new Vector2(1, 0);
			}
			_particleMeshBuffer[i].uv = uv;		
			
			//build color
			Color[] colors = new Color[4 * _particleMeshSize[i]];
			for (int j=0; j<_particleMeshSize[i]; j++)
			{
				//use r,g,b channel is the x,y,z vector
				colors[0 + 4*j].r = colors[1 + 4*j].r = colors[2 + 4*j].r = colors[3 + 4*j].r = 
					Random.Range(0.0f, 1.0f);
				colors[0 + 4*j].g = colors[1 + 4*j].g = colors[2 + 4*j].g = colors[3 + 4*j].g = 
					Random.Range(0.0f, 1.0f);
				colors[0 + 4*j].b = colors[1 + 4*j].b = colors[2 + 4*j].b = colors[3 + 4*j].b = 
					Random.Range(0.0f, 1.0f);
				colors[0 + 4*j].a = colors[1 + 4*j].a = colors[2 + 4*j].a = colors[3 + 4*j].a = 
					Random.Range(0.0f, 1.0f);
			}
			
			
			_particleMeshBuffer[i].colors = colors;
		}
	}
	
	
	// Use this for initialization
	void Start () {
	
		Assertion.Check(_particleMeshSize.Length == _meshBufferCount);
		InitMeshBuffers();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
