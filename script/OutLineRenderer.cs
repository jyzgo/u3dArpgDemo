using UnityEngine;
using System.Collections;

public class OutLineRenderer : MonoBehaviour {
	
	public int _iterations = 3;
	public float _spread = 0.7f;
	public Color _outLineColor = new Color(0.133f,1,0,1);
	
	public Camera _outLineCamera;
	public Shader _compositeShader;
    Material _compositeMaterial = null;
	protected Material CompositeMaterial {
		get {
			if (_compositeMaterial == null) {
                _compositeMaterial = new Material(_compositeShader);
				_compositeMaterial.hideFlags = HideFlags.HideAndDontSave;
			}
			return _compositeMaterial;
		} 
	}
	
	public Shader _blurShader;
    Material _blurMaterial = null;
	protected Material BlurMaterial {
		get {
			if (_blurMaterial == null) {
                _blurMaterial = new Material(_blurShader);
                _blurMaterial.hideFlags = HideFlags.HideAndDontSave;
			}
			return _blurMaterial;
		} 
	}
	
	
	public Shader _cutoffShader;
	Material _cutoffMaterial = null;
	protected Material CutoffMaterial {
		get {
			if (_cutoffMaterial == null) {
				_cutoffMaterial = new Material( _cutoffShader );
				_cutoffMaterial.hideFlags = HideFlags.HideAndDontSave;
			}
			return _cutoffMaterial;
		} 
	}
	
	public Shader _outLineShader = null;
	Material _outLineMat = null;
	protected Material OutLineMat{
		get{
			if(_outLineMat == null){
				_outLineMat = new Material(_outLineShader);
				_outLineMat.hideFlags = HideFlags.HideAndDontSave;
				//_outLineMat.shader.hideFlags =  HideFlags.HideAndDontSave; 
			}
			
			return _outLineMat;
		}
	}
	
	private RenderTexture _outLineTexture = null;
	protected RenderTexture OutLineTexture{
		get{
			if(!_outLineTexture)
			{
				_outLineTexture =  new RenderTexture( (int)camera.pixelWidth,(int)camera.pixelHeight, 16 );
				_outLineTexture.hideFlags = HideFlags.DontSave;
			}
			return _outLineTexture;
		}
	}

	void Start () {
		if (!SystemInfo.supportsImageEffects)
		{
			enabled = false;
			return;
		}
		
		if( !BlurMaterial.shader.isSupported )
			enabled = false;
		if( !CompositeMaterial.shader.isSupported )
			enabled = false;
		if( !CutoffMaterial.shader.isSupported )
			enabled = false;
		if( !OutLineMat.shader.isSupported )
			enabled = false;
	}
	
	
	void OnDisable()
    {
		if(_outLineTexture)
		{
			DestroyImmediate(_outLineTexture);
			_outLineTexture = null;
		}
		if( _compositeMaterial ) {
			DestroyImmediate( _compositeMaterial );
		}
		if( _blurMaterial ) {
			DestroyImmediate( _blurMaterial );
		}
		if(_outLineMat) {
			DestroyImmediate (_outLineMat);
		}
	}
	
	public void FourTapCone (RenderTexture source, RenderTexture dest, int iteration)
	{
		float off = 0.5f + iteration*_spread;
		Graphics.BlitMultiTap (source, dest, BlurMaterial,
            new Vector2( off, off),
			new Vector2(-off, off),
            new Vector2( off,-off),
            new Vector2(-off,-off)
		);
	}
	
	void OnPreRender() 
	{
		_outLineCamera.fieldOfView = camera.fieldOfView;
		Shader.SetGlobalColor("_OutLineColor" , _outLineColor);
		_outLineCamera.targetTexture = OutLineTexture;
		_outLineCamera.RenderWithShader(OutLineMat.shader,"OutLineCaster");
	}
	
	void OnRenderImage (RenderTexture source, RenderTexture destination)
	{
		_iterations = Mathf.Clamp( _iterations, 0, 15 );
		_spread = Mathf.Clamp( _spread, 0.5f, 6.0f );
		
		RenderTexture buffer = RenderTexture.GetTemporary(OutLineTexture.width, OutLineTexture.height, 0);
		RenderTexture buffer2 = RenderTexture.GetTemporary(OutLineTexture.width, OutLineTexture.height, 0);
		
		Graphics.Blit(OutLineTexture, buffer);

		bool oddEven = true;
		for(int i = 0; i < _iterations; i++)
		{
			if( oddEven )
				FourTapCone (buffer, buffer2, i);
			else
				FourTapCone (buffer2, buffer, i);
			oddEven = !oddEven;
		}
		Graphics.Blit(source,destination);
		
		if( oddEven ){
			Graphics.Blit(OutLineTexture,buffer, CutoffMaterial);			
			Graphics.Blit(buffer,destination,CompositeMaterial);
		}
		else{
			Graphics.Blit(OutLineTexture,buffer2, CutoffMaterial);
			Graphics.Blit(buffer2,destination,CompositeMaterial);
		}	
		
		RenderTexture.ReleaseTemporary(buffer);
		RenderTexture.ReleaseTemporary(buffer2);
	}
}
