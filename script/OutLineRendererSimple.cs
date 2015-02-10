using UnityEngine;
using System.Collections;

public class OutLineRendererSimple : MonoBehaviour {
	
	public float _spread = 0.003f;
	public Color _outLineColor = new Color(1.0f,0.9f,0.5f,1f);
	
	private Material _material;
	private Shader _originalShader;
	private Shader _outlineShader;
	
	void Start () {
		
		_material = gameObject.renderer.material;
		_originalShader = Shader.Find(_material.shader.name);
		_outlineShader = Shader.Find( _material.shader.name + "(OutLine)" );
		if(!_outlineShader)
		{
            Debug.LogError("Not found " + _outlineShader + " !");
		}
	}
	
	
	void OnDisable()
    {
		OnRevertOriginalShader();
	}
	
	
	[ContextMenu("replace")]
	void OnReplaceByOutlineShader() 
	{
		if(!_outlineShader) return;
		_material.shader = _outlineShader;
		_material.SetColor("_OutlineColor" , _outLineColor);
		_material.SetFloat("_OutlineSpread" , _spread );
	}
	
	[ContextMenu("revert")]
	void OnRevertOriginalShader() 
	{
		if(!_outlineShader) return;
		_material.shader = _originalShader;
	}
	
}
