//#define debug_camera
//#define test_camera_effect
using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    public float damping = 5;

    private class ShakeCamera
    {
        protected Animation _anim;
        protected float _duration;
        protected float _tick;
        protected string _idle;

        public void SetAnim(Animation anim, string defaultAnim)
        {
            _anim = anim;
            _idle = defaultAnim;
        }

        public void Shake(string anim, float duration, float scale)
        {
            _anim.gameObject.transform.localScale = Vector3.one * scale;
            _anim.Play(anim);
            _duration = duration;
            _tick = 0.0f;
        }

        public void Update(float deltaTime)
        {
            if (_duration > 0.0f)
            {
                _tick += deltaTime;
                if (_duration <= _tick)
                {
                    StopShake();
                }
            }
        }

        public void StopShake()
        {
            _duration = -1.0f;
            _anim.gameObject.transform.localScale = Vector3.one;
            _anim.Play(_idle);
            //Debug.LogError("Shake stopped.");
        }
    }

    public enum CameraMode
    {
        Standard = 0,
        Follow,
        Follow2,
        PVP,
        PushingBox,
    };

    private Camera _coreCamera;
    public string _forwardCameraPath;
    public string _deferredCameraPath;
    public Transform _cameraParent;
    private Transform _transform;

    public FC2Camera CurrentCamera { get { return _currentCamera as FC2Camera; } }

    private FCCamera _currentCamera = null;
    private FCCamera _oldCamera = null;
    public FCCamera[] _cameraModes;
    public Transform _target;
    public Transform _target2;
    private bool _needChasingTarget = true;   //control camera mode: whether to chase target

    private float _timer;
    private float _duration;
    private GameObject _callbackTarget = null;
    private string _callbackname = "";
    float _fovFactor;
    public SkillCameraCache _skillCameraCache;
    private SkillCamera _skillCamera;
    private bool _forceHold;        //if the current camera effect should be held after fading in

    private float _skillTime;
    public SkillCameraList skillCameraList;
    private ShakeCamera _shakeCamera;
    private Animation _effectAnimation;
    private Vector3 _previousTargetPos;
    private Vector3 _previousPos;
    private static CameraController _instance = null;

    public static CameraController Instance
    {
        get { return _instance; }
    }

    void Awake()
    {
        _instance = this;
    }

    void OnDestroy()
    {
        _instance = null;
        if (_shadowRT != null)
        {
            RenderTexture.ReleaseTemporary(_shadowRT);
            _shadowRT = null;
        }
    }

    public Camera MainCamera
    {
        get { return _coreCamera; }
    }

    public void StartPreserveEffect()
    {
        _effectAnimation.Play("shake3_loop");
    }

    public void StopPreserveEffect()
    {
        _effectAnimation.Stop();
    }
    
    public GameObject _shadowCamera;
    Transform _shadowCameraTransform;
    Camera _shadowCameraInstance;
    ShadowRenderer _shadowRenderer;
    RenderTexture _shadowRT;
    public Shader _shadowShader;
    public Texture _shadowBorder;

    // Use this for initialization
    void Start()
    {
        _transform = transform;

        _effectAnimation = gameObject.transform.FindChild("CameraEffects").gameObject.GetComponent<Animation>();

        string cameraPath = _forwardCameraPath;

        if (GameSettings.Instance.IsDeferredShadingActived())
        {
            cameraPath = _deferredCameraPath;
        }
        GameObject camObj = InJoy.AssetBundles.AssetBundles.Load(cameraPath, typeof(GameObject)) as GameObject;
        camObj = GameObject.Instantiate(camObj) as GameObject;
        camObj.transform.parent = _cameraParent;
        camObj.transform.localPosition = Vector3.zero;
        camObj.transform.localRotation = Quaternion.identity;
        camObj.transform.localScale = Vector3.one;
        _coreCamera = camObj.gameObject.GetComponent<Camera>();
        _timer = 0.0f;
        _duration = 1000.0f;
        _fovFactor = GameSettings.Instance.FovFactor;

        _shakeCamera = new ShakeCamera();
        _shakeCamera.SetAnim(_effectAnimation, _effectAnimation.clip.name);

        if (_currentCamera == null)
        {
            Assertion.Check(_cameraModes.Length > 0);
            SetCurrentCamera(CameraMode.Standard);
        }

        _oldCamera = null;

        _coreCamera.backgroundColor = RenderSettings.fogColor;
        
        LevelLightInfo level = GameObject.FindObjectOfType(typeof(LevelLightInfo)) as LevelLightInfo;
        Renderer [] sceneRenderers = new Renderer[0];
        if (level != null)
        {
            sceneRenderers = level.GetComponentsInChildren<Renderer>();
        }
        if (!GameSettings.Instance.IsFullSceneDisplay())
        {
            int layer = LayerMask.NameToLayer("NONE");
            foreach (Renderer r in sceneRenderers)
            {
                string shaderName = r.sharedMaterial.shader.name;
                if (shaderName.Contains("(noshadow)"))
                {
                    r.gameObject.layer = layer;
                }
            }
        }
        
        if (GameSettings.Instance.IsDirectionalShadowActived())
        {
            _shadowRT = RenderTexture.GetTemporary(800, 800, 0, RenderTextureFormat.ARGB32);
            GameObject cameraObj = GameObject.Instantiate(_shadowCamera) as GameObject;
            _shadowCameraInstance = cameraObj.GetComponentInChildren<Camera>();
            if (_shadowCameraInstance != null)
            {
                _shadowCameraInstance.targetTexture = _shadowRT;
                _shadowCameraInstance.enabled = false;
                _shadowCameraInstance.SetReplacementShader(_shadowShader, "");
                _shadowCameraInstance.Render();
                _shadowCameraInstance.aspect = 1.0f;
                
                foreach (Renderer r in sceneRenderers)
                {
                    if (null != r && null != r.sharedMaterial)
                    {
                        r.sharedMaterial.SetTexture("_shadowTex", _shadowRT);
                    }
                }
            }
            _shadowCameraTransform = cameraObj.GetComponent<Transform>();
            _shadowRenderer = _shadowCameraInstance.GetComponentInChildren<ShadowRenderer>();
            GameObject mainLight = GameObject.FindWithTag("MainLight");
            if (mainLight != null && _shadowRenderer != null)
            {
                _shadowRenderer.SetDirectionalLight(mainLight.transform);
            }
            Shader.SetGlobalTexture("_shadowBorder", _shadowBorder);
        }
    }

    void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            if (_shadowCameraInstance != null && _shadowRT != null)
            {
                Debug.Log("destroy shadow texture");
                _shadowCameraInstance.targetTexture = null;
                RenderTexture.ReleaseTemporary(_shadowRT);
                _shadowCameraInstance.ResetReplacementShader();
                _shadowRT = null;
            }
        }
        if (!pause)
        {
            Debug.Log("Application resumed");
            
            if (_shadowCameraInstance != null && _shadowRT == null)
            {
                Debug.Log("recreate shadow texture.");
                _shadowRT = RenderTexture.GetTemporary(800, 800, 0, RenderTextureFormat.ARGB32);
                _shadowCameraInstance.targetTexture = _shadowRT;
                _shadowCameraInstance.SetReplacementShader(_shadowShader, "");
            }
        }
    }
#if debug_camera
    private float _lastFov;
    private float _lastDistance;
#endif
    public void CameraUpdate()
    {
        if (_needChasingTarget && _currentCamera != null && _target != null)
        {
            UpdateInternal(Time.deltaTime);
        }
        _shakeCamera.Update(Time.deltaTime);
#if debug_camera
        if (Mathf.Abs(_lastFov - _coreCamera.fov) > 0.1f)
        {
            _lastFov = _coreCamera.fov;
            //Debug.Log("==== Fov: " + _lastFov);
        }

        float distance = (_coreCamera.transform.position - _currentCamera.target.position).magnitude;
        if (Mathf.Abs(_lastDistance - distance) > 0.2f)
        {
            _lastDistance = distance;
            Debug.Log("========== Distance: " + distance.ToString("f2"));
        }
#endif
    }

    void UpdateInternal(float deltaTime)
    {
#if UNITY_EDITOR
        _coreCamera.backgroundColor = RenderSettings.fogColor;
#endif
                
        Vector3 pos = _previousPos;
        Vector3 focus = _previousTargetPos;
        float fov = _currentCamera.fieldOfView + _currentCamera.fovFactorOnPhone * _fovFactor;
        Vector2 clipPlane = _currentCamera.clipPlanes;
        
        if (_oldCamera != null)
        {
            _currentCamera.UpdatePosAndRotation(_target, _target2, true, ref pos, ref focus);
            _timer += deltaTime;
            float percent = Mathf.Min(_timer / _duration, 1.0f);
            Vector3 pos2 = _previousPos;
            Vector3 focus2 = _previousTargetPos;
            _oldCamera.UpdatePosAndRotation(_target, _target2, false, ref pos2, ref focus2);
            
            pos = Vector3.Lerp(pos2, pos, percent);
            focus = Vector3.Lerp(focus2, focus, percent);
            fov = Mathf.Lerp(_oldCamera.fieldOfView + _oldCamera.fovFactorOnPhone * _fovFactor, fov, percent);
            clipPlane = Vector2.Lerp(_oldCamera.clipPlanes, clipPlane, percent);
            
            if (percent >= 1.0f)
            {
                _oldCamera = null;
                if (_callbackTarget != null)
                {
                    _callbackTarget.SendMessage(_callbackname);
                }
            }
        } else
        {
            _currentCamera.UpdatePosAndRotation(_target, _target2, true, ref pos, ref focus);
        }

        if (_skillCamera != null)
        {
            _skillTime += deltaTime;
            _skillCamera.Update(_skillTime, _skillCameraCache);
            
            if (_skillTime >= _skillCamera.duration && !_forceHold)
            {
                _skillCamera = null;
                _shakeCamera.StopShake();
            }
        } else
        {
            _skillCameraCache.SetParameter(0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f);
        }
        
        _previousPos = pos;
        _previousTargetPos = focus;
        
        Vector3 originPos = pos - focus;

        Vector3 normalizeOriginPos = originPos.normalized;
        originPos += normalizeOriginPos * _skillCameraCache.distance;

        Vector3 virPos = Vector3.Cross(Vector3.left, Vector3.forward).normalized;
        originPos += virPos * _skillCameraCache.vertical;

        if (0 != _skillCameraCache.vertical)
        {
            Debug.Log(string.Format("originPos = {0}", originPos));
        }

        Vector3 horiPos = Vector3.Cross(Vector3.up, Vector3.forward).normalized;
        originPos += horiPos * _skillCameraCache.horizontal;

		Quaternion rot = Quaternion.AngleAxis(_skillCameraCache.verticalRotation, Vector3.Cross(Vector3.up, normalizeOriginPos));
		originPos = rot * originPos;
		rot = Quaternion.AngleAxis(_skillCameraCache.horizontalRotation, Vector3.up);
		originPos = rot * originPos;
        pos = originPos + focus;
        
        _transform.position = pos;
        _transform.LookAt(focus);
        
        _coreCamera.fieldOfView = fov + _skillCameraCache.fieldOfView;
        _coreCamera.nearClipPlane = clipPlane.x;
        _coreCamera.farClipPlane = clipPlane.y;
        
        // shadow camera update.
        if (_shadowCameraTransform != null)
        {
            _shadowCameraTransform.position = focus;
        }
    }

    private void SetSkillCamera(SkillCamera camera, bool forceHold)
    {
        _skillCamera = camera;
        _skillTime = 0.0f;
        _forceHold = forceHold;
    }

    /// <summary>
    /// Invoke a camera effect by index. The feature camera list will be used to locate the actual camera setting: fov, rotation, etc.
    /// </summary>
    /// <param name="index"></param>
    private void StartCameraEffect(int index, EnumShakeEffect shakeEffect = EnumShakeEffect.none, bool forceHold = false)
    {
        SkillCamera cam = null; 
        if (index >= 0)
        {
            cam = skillCameraList._skillCameraList [index];

            Debug.Log("Starting camera effect: " + index + " Name: " + cam.cameraName);

            _shakeCamera.StopShake();
            SetSkillCamera(cam, forceHold);
        }

        if (shakeEffect != EnumShakeEffect.none)
        {
            _shakeCamera.Shake(shakeEffect.ToString(), shakeEffect == EnumShakeEffect.shake3_loop ? -1f : 0.4f, 1.0f);
        }

    }

    /// <summary>
    /// start the camera effect by id, using the specified shake effect. If force to hold, the camera effect will ignore its lasting time
    /// and keep holding on the settings after fading in until StopCameraEffect() is called.
    /// </summary>
    /// <param name="effectID"></param>
    /// <param name="shakeEffect"></param>
    /// <param name="forceHold"></param>
    public void StartCameraEffect(EnumCameraEffect effectID, EnumShakeEffect shakeEffect = EnumShakeEffect.none, bool forceHold = false)
    {
        StartCameraEffect((int)effectID, shakeEffect, forceHold);
    }

    /// <summary>
    /// Stop the current camera effect by setting the elapsed time, can be used for restoring the default camera.
    /// </summary>
    public void StopCameraEffect()
    {
        //Debug.Log("Camera effect stopped. Camera restored to default.");

        _skillCamera = null;
        _forceHold = false;
        _shakeCamera.StopShake();
    }

    public void SetCurrentCamera(CameraMode mode)
    {
        FCCamera fcamera = _cameraModes [(int)mode];
        SetCurrentCamera(fcamera);

        if (mode == CameraMode.PushingBox)
        {
            SetTarget2(null);
        }
    }

    void SetCurrentCamera(FCCamera fcamera)
    {
        _currentCamera = fcamera;
    }

    public void SetFeatureCamera(FCCamera camera, float duration, GameObject target, string CALLBACK)
    {
        _oldCamera = _currentCamera;
        _currentCamera = camera;
        _duration = duration;
        _timer = 0.0f;
        _callbackTarget = target;
        _callbackname = CALLBACK;
    }

    public void PreviewSkillCamera(int index)
    {
        StartCameraEffect(index, EnumShakeEffect.none, false);
    }

    public void PreviewUpdate(float deltaTime)
    {
        UpdateInternal(deltaTime);
    }

    public void SetTarget(Transform target)
    {
        _target = target;

        if (target != null)
        {
            _previousTargetPos = target.position;
        }
        FCCamera ewcam = (_currentCamera != null ? _currentCamera : _cameraModes [0]);
        //ewcam.OnSetTarget(target, ref _target2);
        ewcam.UpdatePosAndRotation(target, _target2, false, ref _previousPos, ref _previousTargetPos);
        LevelLightInfo.s_completeCamera = null;
    }

    public void SetTarget2(Transform target2)
    {
        _target2 = target2;
    }
    
    public void DisableRotate()
    {
        if (_currentCamera != null)
        {
            _currentCamera.NeedRotateBlend = false;
        }
    }
    //disable main camera
    public void Freeze()
    {
        _coreCamera.enabled = false;
    }

    //enable main camera
    public void Defreeze()
    {
        _coreCamera.enabled = true;
    }

    /// <summary>
    /// Stop chasing target
    /// </summary>
    public void StopChaseTarget()
    {
        _needChasingTarget = false;

        Debug.Log(" ==== Camera stopped chasing target.");
    }

    //Resume chasing target. Make sure these two methods are called in pairs.
    public void ResumeChaseTarget()
    {
        _needChasingTarget = true;

        Debug.Log(" ==== Camera resumed chasing target.");
    }
    
    public void DrawShadow()
    {
        if (_shadowCameraInstance != null && _shadowRenderer != null)
        {
            _shadowRenderer.DoClear();
            _shadowRenderer.DrawShadow();
            Vector3 cameraPos = _shadowCameraTransform.position;
            float width = _shadowCameraInstance.orthographicSize;
            Vector4 shadowOffset = Vector4.one;
            shadowOffset.x = cameraPos.x - width;
            shadowOffset.y = cameraPos.z - width;
            shadowOffset.z = width * 2.0f;
            shadowOffset.w = width * 2.0f;
            Shader.SetGlobalVector("_shadowTexOffset", shadowOffset);
        }
    }
    
    void LateUpdate()
    {
        DrawShadow();
    }
    
    public void AddCharacterForShadow(AvatarController inst)
    {
        if (_shadowRenderer != null)
        {
            _shadowRenderer.AddShadowInstance(inst);
        }
    }
    
    public void RemoveCharacterForShadow(AvatarController inst)
    {
        if (_shadowRenderer != null)
        {
            _shadowRenderer.RemoveShadowInstance(inst);
        }
    }
    
    public void AddPointLight(Transform newLight)
    {
        if (_shadowRenderer != null && GameSettings.Instance.IsPointShadowActived())
        {
            _shadowRenderer.AddPointLight(newLight);
        }
    }
    
    public void RemovePointLight(Transform l)
    {
        if (_shadowRenderer != null && GameSettings.Instance.IsPointShadowActived())
        {
            _shadowRenderer.RemovePointLight(l);
        }
    }

#if test_camera_effect
    string _effectIDString = "0";
    string _shakeId = "0";
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(30, 50, 100, 200));

        GUILayout.Label("Effect Id:");

        _effectIDString = GUILayout.TextField(_effectIDString);

        GUILayout.Label("Shake Id:");

        _shakeId = GUILayout.TextField(_shakeId);

        if (GUILayout.Button("Test"))
        {
            int effectId, shakeId;

            if (int.TryParse(_effectIDString, out effectId) && int.TryParse(_shakeId, out shakeId))
            {
                StartCameraEffect(effectId, (EnumShakeEffect)shakeId, false);
            }
        }
        GUILayout.EndArea();
    }
#endif //test_camera_effect
}

public enum EnumCameraEffect
{
    none = -1,
    effect_0,
    effect_1,
    effect_2,
    effect_3,
    effect_4,
    effect_5,
    effect_6,
    effect_7,
    effect_8,
    effect_9,
    effect_10,
    effect_11,
    effect_12,
    effect_13,
    effect_14,
    effect_15,
    effect_16,
    effect_17,
}

//each enumration has a corresponding camera animation
public enum EnumShakeEffect
{
    none,
    shake1,
    shake2,
    shake3_loop,
    shake4,
    shake5,
    shake6
}