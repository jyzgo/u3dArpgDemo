using UnityEngine;
using System.Collections;

public class DeferredShadingRenderer : MonoBehaviour {
	
	public Camera _GBufferCamera;
	public Camera _TBufferCamera;
	public Camera _LBufferCamera;
	public Shader _gbufferShader;
	public Texture _depthTex;
	
	public Material _downSampleMat;
	public Material _filterMat;
	public Material _blurMat;
	public Material _bloomMat;
	public float _threhold = 1.0f;
	public float brightness = 1.0f;
	
	public AnimationCurve phaseAnim = new AnimationCurve();
	public Material _phaseMat;
	bool _phaseEnabled;
	
	bool _postprocessEnabled = false;
	
	static RenderTexture s_gbuffer = null;
	static RenderTexture s_lbuffer = null;
	static RenderTexture s_tbuffer = null;
	static RenderTexture s_fbuffer = null;
	static Matrix4x4 s_inverseProjectMatrix = Matrix4x4.identity;
	static Matrix4x4 s_cameraMatrix = Matrix4x4.identity;
	
	static DeferredShadingRenderer _inst = null;
	static public DeferredShadingRenderer Singleton {
		get {return _inst;}
	}
	
	static public RenderTexture GBuffer {
		get {return s_gbuffer;}
	}
	static public RenderTexture TBuffer {
		get {return s_tbuffer;}
	}
	static public RenderTexture LBuffer {
		get {return s_lbuffer;}
	}
	static public RenderTexture FBuffer {
		get {return s_fbuffer;}
	}
	static public Matrix4x4 InverseProjectMatrix {
		get {return s_inverseProjectMatrix;}
		set {s_inverseProjectMatrix = value;}
	}
	static public Matrix4x4 CameraMatrix {
		get {return s_cameraMatrix;}
		set {s_cameraMatrix = value;}
	}
	
	void Awake() {
		Shader.SetGlobalTexture("_DepthTex", _depthTex);
		_postprocessEnabled = GameSettings.Instance.IsPostProcessActived();
	}
	
	void CreateRenderTextures () {
		if(s_gbuffer == null) {
#if UNITY_STANDALONE
			s_gbuffer = RenderTexture.GetTemporary(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
#else
			s_gbuffer = RenderTexture.GetTemporary(Screen.width / 2, Screen.height / 2, 24, RenderTextureFormat.ARGB32);
#endif
			s_gbuffer.name = "gBuffer";
			s_gbuffer.filterMode = FilterMode.Point;
			Shader.SetGlobalTexture("_GBuffer", s_gbuffer);
		}
		if(s_tbuffer == null) {
			s_tbuffer = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
			s_tbuffer.name = "tBuffer";
			s_tbuffer.filterMode = FilterMode.Point;
			Shader.SetGlobalTexture("_TBuffer", s_tbuffer);
		}
		if(s_lbuffer == null) {
#if UNITY_STANDALONE
			s_lbuffer = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
#else
			s_lbuffer = RenderTexture.GetTemporary(Screen.width / 2, Screen.height / 2, 0, RenderTextureFormat.ARGB32);
#endif
			s_lbuffer.name = "lBuffer";
			s_lbuffer.filterMode = FilterMode.Trilinear;
			Shader.SetGlobalTexture("_LBuffer", s_lbuffer);
		}
		if(s_fbuffer == null) {
			s_fbuffer = RenderTexture.GetTemporary(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
			s_fbuffer.name = "fBuffer";
			s_fbuffer.filterMode = FilterMode.Point;
			Shader.SetGlobalTexture("_FBuffer", s_fbuffer);
		}
		_GBufferCamera.SetTargetBuffers(s_gbuffer.colorBuffer, s_gbuffer.depthBuffer);
		_GBufferCamera.SetReplacementShader(_gbufferShader, "");
		_TBufferCamera.SetTargetBuffers(s_tbuffer.colorBuffer, s_fbuffer.depthBuffer);
		_LBufferCamera.SetTargetBuffers(s_lbuffer.colorBuffer, s_gbuffer.depthBuffer);
		camera.SetTargetBuffers(s_fbuffer.colorBuffer, s_fbuffer.depthBuffer);
	}
	
	void OnEnable() {
		if(_inst != null) {
			Debug.LogError("Duplicated " + GetType() + ". Destroy this " + gameObject.name);
			GameObject.Destroy(this);
			return;
		}
		_inst = this;
		CreateRenderTextures();
	}
	
	void OnDisable() {
		if(_inst == this) {
			_inst = null;
		}
	}
	
	void OnDestroy() {
		if(_inst == this) {
			_inst = null;
		}
		DestroyRenderTextures();
	}
	
	void DestroyRenderTextures() {
		if(s_gbuffer != null) {
			RenderTexture.ReleaseTemporary(s_gbuffer);
			s_gbuffer = null;
		}
		if(s_tbuffer != null) {
			RenderTexture.ReleaseTemporary(s_tbuffer);
			s_tbuffer = null;
		}
		if(s_fbuffer != null) {
			RenderTexture.ReleaseTemporary(s_fbuffer);
			s_fbuffer = null;
		}
		if(s_lbuffer != null) {
			RenderTexture.ReleaseTemporary(s_lbuffer);
			s_lbuffer = null;
		}
	}
	
	void OnPostRender()
	{
		if(_postprocessEnabled) {
			RenderTexture rtHalf = RenderTexture.GetTemporary(Screen.width / 2, Screen.height / 2, 0, RenderTextureFormat.ARGB32);
			rtHalf.filterMode = FilterMode.Bilinear;
			RenderTexture rtQuad = RenderTexture.GetTemporary(Screen.width / 4, Screen.height / 4, 0, RenderTextureFormat.ARGB32);
			rtQuad.filterMode = FilterMode.Bilinear;
			RenderTexture rtQuad2 = RenderTexture.GetTemporary(Screen.width / 4, Screen.height / 4, 0, RenderTextureFormat.ARGB32);
			rtQuad2.filterMode = FilterMode.Bilinear;
			
			Vector4 downSample = new Vector4(1.0f / Screen.width, 1.0f / Screen.height, 2.0f / Screen.width, 2.0f / Screen.height);
			_downSampleMat.SetVector("_TexelSize", downSample);
			Graphics.Blit(s_fbuffer, rtHalf, _downSampleMat);
			downSample = new Vector4(2.0f / Screen.width, 2.0f / Screen.height, 4.0f / Screen.width, 4.0f / Screen.height);
			_downSampleMat.SetVector("_TexelSize", downSample);
			Graphics.Blit(rtHalf, rtQuad, _downSampleMat);
			_filterMat.SetFloat("_Threhold", _threhold);
			Graphics.Blit(rtQuad, rtQuad2, _filterMat);
			Vector4 filter = new Vector4(4.0f / Screen.width, 0.0f, brightness, 0.0f);
			_blurMat.SetVector("_Filter", filter);
			Graphics.Blit(rtQuad2, rtQuad, _blurMat);
			filter = new Vector4(0.0f, 4.0f / Screen.height, brightness, 0.0f);
			_blurMat.SetVector("_Filter", filter);
			Graphics.Blit(rtQuad, rtQuad2, _blurMat);
			_bloomMat.SetVector("_TexelSize", new Vector4(4.0f / Screen.width, 4.0f / Screen.height, 1.0f / Screen.width, 1.0f / Screen.height));
			RenderTexture rtFull = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
			_bloomMat.SetTexture("_OriginTex", s_fbuffer);
			Graphics.Blit(rtQuad2, rtFull, _bloomMat);
			bool NeedMove = true;
			if(_phaseEnabled) {
				Graphics.Blit(rtFull, s_fbuffer, _phaseMat);
				NeedMove = false;
			}
			if(NeedMove) {
				Graphics.Blit(rtFull, s_fbuffer);
			}
			
			RenderTexture.ReleaseTemporary(rtHalf);
			RenderTexture.ReleaseTemporary(rtQuad);
			RenderTexture.ReleaseTemporary(rtQuad2);
			RenderTexture.ReleaseTemporary(rtFull);
		}
	}
	
	IEnumerator PhaseEffect() {
		float time = 0.0f;
		float maxTime = phaseAnim.keys[phaseAnim.keys.Length - 1].time;
		_phaseEnabled = true;
		while(time <= maxTime) {
			float v = phaseAnim.Evaluate(time);
			_phaseMat.SetVector("_Filter", new Vector4(1.0f / Screen.width, 1.0f / Screen.height, v, 0.0f));
			time += Time.deltaTime;
			yield return null;
		}
		_phaseEnabled = false;
	}
	
	public void StartPhasEffect() {
		StartCoroutine(PhaseEffect());
	}
	
	void OnApplicationPause(bool pause) {
		if(!pause) {
			Debug.Log("Application resumed");
			
			CreateRenderTextures();
		}
		else {
			DestroyRenderTextures();
			_GBufferCamera.targetTexture = null;
			_GBufferCamera.ResetReplacementShader();
			_TBufferCamera.targetTexture = null;
			_LBufferCamera.targetTexture = null;
			camera.targetTexture = null;
		}
	}

}
