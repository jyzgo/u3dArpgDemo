using UnityEngine;
using System.Collections;

public class ModifiableMaterialInst : MaterialInst
{
	private Material _mat;
	private Material []_matArray;
	private RenderTexture _tex = null;
	private RenderTexture _normalTex = null;
	
	public Material SharedMaterial {
		get {return _mat;}
	}
	
	public RenderTexture TargetTexture {
		get {return _tex;}
	}
	
	public RenderTexture TargetNormalTexture {
		get {
			if(_normalTex == null) {
				_normalTex = RenderTexture.GetTemporary(_tex.width, _tex.height);
				_normalTex.name = "atlas normal texture";
				_mat.SetTexture("_NormalTex", _normalTex);
			}
			return _normalTex;
		}
	}
	
	protected override Material[] GetMaterials ()
	{
		return _matArray;
	}
	
	public override void Destroy ()
	{
		base.Destroy();
		GameObject.Destroy(_mat);
		_mat = null;
		_matArray = null;
		RenderTexture.ReleaseTemporary(_tex);
		if(_normalTex != null) {
			RenderTexture.ReleaseTemporary(_normalTex);
		}
	}
	
	public void InitWithSizeAndMat(int width, int height, Material mat) {
		_mat = Utils.CloneMaterial(mat);
		_tex = RenderTexture.GetTemporary(width, height);
		_tex.name = "atlas texture";
		_mat.mainTexture = _tex;
		_matArray = new Material[1]{_mat};
	}
}

public class ModifiableMaterialBuilder : MaterialBuilder
{
	static float instanceHeight = 0.0f;
    public override MaterialInst CreateMaterialForBody(Renderer[] renderers, Material origin, CharacterGraphicsQuality quality)
    {
        // need to instantiate material builder to use camera in it.
        GameObject go = GameObject.Instantiate(gameObject) as GameObject;
		go.transform.localPosition = Vector3.up * instanceHeight;
		instanceHeight += 100.0f;
        ModifiableMaterialInst inst = new ModifiableMaterialInst();

        // allocate a piece of texture.
        int size = QualityInformation.GetCharacterTextureSize(quality);
        inst.InitWithSizeAndMat(size, size, origin);

        foreach (Renderer r in renderers)
        {
            r.sharedMaterial = inst.SharedMaterial;
        }

        // get mesh renderers and camera.
        Camera cam = go.GetComponentInChildren<Camera>();
        MeshRenderer[] rendererArray = go.GetComponentsInChildren<MeshRenderer>();

        // disable all mesh pieces.
        foreach (MeshRenderer mr in rendererArray)
        {
            mr.enabled = false;
        }

        // set target
        cam.targetTexture = inst.TargetTexture;

        // init a black texture.
        cam.Render();

        cam.targetTexture = null;
        Destroy(go);

        return inst;
    }

    public override void UpdateMaterialOfEquipment(MaterialInst inst, Renderer[] equipments, string[] avatarPartNames, Texture2D source, Texture2D normal, Color blendColor, CharacterGraphicsQuality quality)
    {
        // inst must be type of ModifiableMaterialInst.
        ModifiableMaterialInst mmi = inst as ModifiableMaterialInst;
        Assertion.Check(mmi != null);
        Material sharedMat = mmi.SharedMaterial;
        foreach (Renderer r in equipments)
        {
            r.sharedMaterial = sharedMat;
        }

        // need to instantiate material builder to use camera in it.
        GameObject go = GameObject.Instantiate(gameObject) as GameObject;
		go.transform.localPosition = Vector3.up * instanceHeight;
		instanceHeight += 100.0f;
		
        Camera cam = go.GetComponentInChildren<Camera>();

        MeshRenderer[] renderers = go.GetComponentsInChildren<MeshRenderer>();
        Assertion.Check(renderers.Length > 0);
        Material renderMat = renderers[0].sharedMaterial;
        // enable certain pieces by name
        foreach (MeshRenderer r in renderers)
        {
            r.enabled = false;
            foreach (string s in avatarPartNames)
            {
                if (s == r.gameObject.name)
                {
                    r.enabled = true;
                    Assertion.Check(r.sharedMaterial == renderMat); // all pieces use only 1 uniformed material.
					break;
                }
            }
        }

        // render color texture.
        renderMat.SetTexture("_MainTex", source);
		renderMat.SetTexture("_AlphaTex", null);
        renderMat.SetColor("_BlendColor", blendColor);
        // set target.
        cam.targetTexture = mmi.TargetTexture;
        cam.Render();

        // render normal texture.
        if (normal != null)
        {
            renderMat.SetTexture("_MainTex", normal);
            renderMat.SetTexture("_AlphaTex", null);
            cam.targetTexture = mmi.TargetNormalTexture;
            cam.Render();
        }
		
        // clean up
		renderMat.SetTexture("_MainTex", null);
		renderMat.SetTexture("_AlphaTex", null);
		cam.targetTexture = null;
        Destroy(go);
    }	
	
}
