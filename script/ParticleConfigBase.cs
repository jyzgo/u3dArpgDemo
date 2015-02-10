using UnityEngine;
using System.Collections;

public class ParticleConfigBase : MonoBehaviour {
	
	public FC_PARTICLE_BULLET_NUMBER _bulletCountType = FC_PARTICLE_BULLET_NUMBER.FC_PARTICLE_BULLET_1;
	protected Material _material = null;
	
	void OnEnable()
	{
		Config();
		ConfigShaderParams();	
	}
	
	protected virtual void Config()
	{
		Assertion.Check(_bulletCountType < FC_PARTICLE_BULLET_NUMBER.FC_PARTICLE_BULLET_COUNT);

		//get mesh fillter and set mesh by _bulletCountType
		MeshFilter mFilter = gameObject.GetComponent<MeshFilter>();
		Assertion.Check(mFilter != null);
		
		Mesh mesh = ParticleManager.Instance.GetMeshAt((int)_bulletCountType);
		mFilter.mesh = mesh;
		
		//copy meterial
		MeshRenderer mRenderer = gameObject.GetComponent<MeshRenderer>();		
		Material newMat = Utils.CloneMaterial(mRenderer.sharedMaterial);
		mRenderer.sharedMaterial = newMat;
		_material = newMat;
		

	}

	protected virtual void ConfigShaderParams()	
	{
		//set shader params
		Assertion.Check(_material != null);
		_material.SetFloat("_startTime", Time.timeSinceLevelLoad);	
	}
	
	
}
