using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SharedTextureMaterialInst : MaterialInst
{
    private System.Collections.Generic.List<Material> _mats;
	private Material []_matArray;
	
	protected override Material[] GetMaterials ()
	{
		return _matArray;
	}
	
	public override void Destroy ()
	{
		base.Destroy();
		foreach(Material mat in _matArray) {
			GameObject.Destroy(mat);
		}
		_mats = null;
		_matArray = null;
	}

    // note: this parameter material should not be modified, this is reference from project.
    public void InitWithTexAndMat(Material mat)
    {
        _mats = new System.Collections.Generic.List<Material>();
		_matArray = _mats.ToArray();
    }

    //add the cloned materials to _mats
    public void SetMaterialOfRenderer(Renderer renderer)
    {
		Material m = renderer.sharedMaterial;
		int index = 0;
		foreach(Material mat in _mats) {
			if(mat == m) {
				renderer.sharedMaterial = _matArray[index];
				return;
			}
			++index;
		}
		_mats.Add(m);
		Material newMat = Utils.CloneMaterial(m);
		renderer.sharedMaterial = newMat;
		
		Material []materials = new Material[_matArray.Length + 1];
		for(int i = 0;i < _matArray.Length;++i) {
			materials[i] = _matArray[i];
		}
		materials[_matArray.Length] = newMat;
		_matArray = materials;
    }
}

public class SimpleMaterialBuilder : MaterialBuilder
{
	public override MaterialInst CreateMaterialForBody (Renderer[] renderers, Material origin, CharacterGraphicsQuality quality)
	{
		SharedTextureMaterialInst inst = new SharedTextureMaterialInst();
		inst.InitWithTexAndMat(origin);
		foreach(Renderer r in renderers) {
			inst.SetMaterialOfRenderer(r);
		}
		return inst;
	}
	
	public override void UpdateMaterialOfEquipment (MaterialInst inst, Renderer[] equipments, string[] avatarPartNames, Texture2D source, Texture2D normal, Color blendColor, CharacterGraphicsQuality quality)
	{
		SharedTextureMaterialInst stmi = inst as SharedTextureMaterialInst;
		Assertion.Check(stmi != null);
		
		foreach(Renderer r in equipments) {
			stmi.SetMaterialOfRenderer(r);
		}
	}
}
