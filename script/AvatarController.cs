using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AvatarController : MonoBehaviour
{
    public enum ItemName
    {
        ItemName_Weapon = 0,
        ItemName_Count
    }

    // animation type.
    public enum AnimType
    {
        AnimType_Normal = 0,
        AnimType_Move,
        AnimType_Attack,
        AnimType_SpecialAttack,
        AnimType_Charge,
        AnimType_Hurt,
        AnimType_Count
    }

    public UIHPController _uiHPController;


    public MaterialBuilder _materialBuilder;
    public bool _dynamicShadow = true;
    public Material _baseMaterial;
    public CharacterGraphicsQuality _quality;
    public Renderer[] _characterRenderers; // material will be uniformly replaced after created.
    // material and texture reference.
    MaterialInst _materialInst = null;
    public MaterialInst materialInst
    {
        get { return _materialInst; }
    }

    ActionController _actionCtrl;
    // animation component.
    Animator _animator;
    AnimStateMachine[] _animStateMachines = null;
    AnimType _currentAnimStateMachine;
    int _resetStateCounter = -1;
    protected float _preAniPlayTime = 0;
    protected int _preAniOverCount = 0;

    protected float _deltaSpeedScale = 1;
    protected bool _controlByAni = false;

    protected float _tinyHurtTime = 0;

    public FCAnimationInfo _animationInfos = null;
    private FCAnimationInfo _originalAnimationInfos;   //a clone

    public FCAnimationInfo _animationInfos_PVP = null;

    protected Rigidbody _thisRigidBody = null;

    int _characterClass = -1;

    public RuntimeAnimatorController animatorCtl;
    public RuntimeAnimatorController animatorCtl_PVP;
    public bool _allowPetAssembled = false;

    public float DeltaSpeedScale
    {
        set
        {
            _deltaSpeedScale = value;
        }
        get
        {
            return _deltaSpeedScale;
        }
    }

    public bool ControlByAni
    {
        set
        {

            _controlByAni = value;
        }
        get
        {
            return _controlByAni;
        }
    }

    protected Transform _thisTransform;
    public Transform myTransform
    {
        get { return _thisTransform; }
    }

    protected GameObject _thisObject;
    public GameObject ThisObject
    {
        get { return _thisObject; }
    }

    //protected bool _showAnimationIdx = false;


    private Transform[] _nodesMapping = new Transform[(int)EnumEquipSlot.MAX];
    private GameObject[] _equipmentMapping = new GameObject[(int)EnumEquipSlot.MAX];
    private GameObject[] _upgradeNodeMapping = new GameObject[(int)EnumEquipSlot.MAX];

    private System.Collections.Generic.List<GameObject> _nonReplacedEquipments =
                                                        new System.Collections.Generic.List<GameObject>();

    public Transform GetSlotNode(EnumEquipSlot slot)
    {
        return _nodesMapping[(int)slot];
    }

    protected float _timeCountForNewAni = 0;

    private Renderer[] _renderers = null;

    public void InitRenderers()
    {
        _renderers = gameObject.GetComponentsInChildren<Renderer>();
    }

    public void ChangeMeshRenderers(bool show)
    {
        foreach (Renderer renderer in _renderers)
        {
            renderer.enabled = show;
        }
    }

    void Awake()
    {
        _animator = GetComponent<Animator>();

        if (GameManager.Instance.GameState == EnumGameState.InBattle)
        {
            if (GameManager.Instance.IsPVPMode)
            {
                _animator.runtimeAnimatorController = animatorCtl_PVP;
            }
        }

        _thisTransform = transform;
        _thisObject = gameObject;
        _characterController = GetComponent<CharacterController>();


        for (int i = 0; i < (int)_nodesMapping.Length; ++i)
        {
            _nodesMapping[i] = Utils.FindTransformByNodeName(gameObject.transform, FCEquipmentsBase.GetNodeByEquipSlot((EnumEquipSlot)i));
        }
    }

    //refresh all ports
    public void RefreshPorts(GameObject root)
    {
        for (int i = 0; i < (int)_nodesMapping.Length; ++i)
        {
            _nodesMapping[i] = Utils.FindTransformByNodeName(root.transform, FCEquipmentsBase.GetNodeByEquipSlot((EnumEquipSlot)i));
        }
    }

    //refresh only damage ports, which is on weapon
    //this should be executed after RefreshPorts()
    public void RefreshDemagePorts(GameObject root)
    {
        _nodesMapping[(int)EnumEquipSlot.damage_main_A] =
            Utils.FindTransformByNodeName(root.transform, FCEquipmentsBase.GetNodeByEquipSlot(EnumEquipSlot.damage_main_A));

        _nodesMapping[(int)EnumEquipSlot.damage_main_B] =
            Utils.FindTransformByNodeName(root.transform, FCEquipmentsBase.GetNodeByEquipSlot(EnumEquipSlot.damage_main_B));

    }


    public void Init(string nickName, string guildName, int characterClass)
    {
        _characterClass = characterClass;
        _uiHPController = GetComponentInChildren<UIHPController>();

        // get hp display components.
        if (_uiHPController != null)
        {
            _uiHPController.DisplayText = nickName;
            _uiHPController.GuildName = guildName;
            SetHPDisplay(1.0f, 0, false, false);
            SetEnergyDisplay(1);
        }
        // init material.
        // TODO: remove this protection that not necessary.
        if (_materialBuilder != null)
        {
            _materialInst = _materialBuilder.CreateMaterialForBody(_characterRenderers, _baseMaterial, _quality);
            if (_dynamicShadow)
            {
                foreach (Renderer r in _characterRenderers)
                {
                    r.gameObject.layer = LayerMask.NameToLayer("CHARACTER");
                }
            }
        }

        // init animation state machine.
        _animStateMachines = new AnimStateMachine[(int)AnimType.AnimType_Count];
        _animStateMachines[(int)AnimType.AnimType_Normal] = new NormalAnimStateMachine();
        _animStateMachines[(int)AnimType.AnimType_Move] = new MoveAnimStateMachine();
        _animStateMachines[(int)AnimType.AnimType_Attack] = new AttackAnimStateMachine();
        _animStateMachines[(int)AnimType.AnimType_SpecialAttack] = new SpecialAttackStateMachine();
        _animStateMachines[(int)AnimType.AnimType_Charge] = new ChargeStateMachine();
        _animStateMachines[(int)AnimType.AnimType_Hurt] = new HurtAnimStateMachine();

        _actionCtrl = GetComponent<ActionController>();

        foreach (AnimStateMachine asm in _animStateMachines)
        {
            asm.Init(_actionCtrl, _animator);
        }
        _currentAnimStateMachine = AnimType.AnimType_Normal;

        _thisRigidBody = rigidbody;
    }

    public RenderTexture _icon = null;
    public void TakeIcon()
    {
        if (_icon != null)
        {
            return;
        }
        if (_characterClass < 0 && _actionCtrl != null)
        {
            string charID = _actionCtrl.Data.characterId;
            if (charID.StartsWith("mage"))
            {
                _characterClass = 0;
            }
            else if (charID.StartsWith("warrior"))
            {
                _characterClass = 1;
            }
            else if (charID.StartsWith("monk"))
            {
                _characterClass = 2;
            }
        }
        if (_characterClass < 0)
        {
            return;
        }
        _icon = RenderTexture.GetTemporary(128, 128, 32, RenderTextureFormat.ARGB32);
        string[] cameraPaths = new string[]{"Assets/Characters/Common/IconCamera_mage.prefab",
											"Assets/Characters/Common/IconCamera_warrior.prefab",
											"Assets/Characters/Common/IconCamera_monk.prefab"};
        GameObject go = InJoy.AssetBundles.AssetBundles.Load(cameraPaths[_characterClass], typeof(GameObject)) as GameObject;
        go = GameObject.Instantiate(go) as GameObject;
        go.transform.parent = transform;
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;

        Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
        int[] layers = new int[renderers.Length];
        for (int i = 0; i < renderers.Length; ++i)
        {
            layers[i] = renderers[i].gameObject.layer;
            if (renderers[i].gameObject.layer == LayerMask.NameToLayer("CHARACTER")
                || renderers[i].gameObject.layer == LayerMask.NameToLayer("Default"))
            {
                renderers[i].gameObject.layer = LayerMask.NameToLayer("SCRIPT");
            }
        }

        IconRenderer iRenderer = go.GetComponent<IconRenderer>();
        iRenderer.Render(_icon);
        Destroy(go);

        for (int i = 0; i < renderers.Length; ++i)
        {
            renderers[i].gameObject.layer = layers[i];
        }
    }

    public void RemoveWeapons()
    {
        materialInst.ClearWeapon();
    }

    public void RemoveCurrentEquipments()
    {
        //destroy game objects on mounting points
        for (int i = 0; i < (int)(int)EnumEquipSlot.MAX; ++i)
        {
            GameObject.Destroy(_equipmentMapping[i]);
            _equipmentMapping[i] = null;
            GameObject.Destroy(_upgradeNodeMapping[i]);
            _upgradeNodeMapping[i] = null;
        }
        // remove none-replacable equipments.
        foreach (GameObject go in _nonReplacedEquipments)
        {
            Destroy(go);
        }
        _nonReplacedEquipments.Clear();

        if (_actionCtrl != null)
        {
            _actionCtrl.ClearWeaponList();
        }
        RemoveWeapons();
    }

    //Refresh avatar with the equipments listed in the equipmentsRoot object
    public void RefreshEquipments(Transform equipmentRoot)
    {
        RemoveCurrentEquipments();

        // FCEquipmentsBase[] equipments = equipmentRoot.GetComponentsInChildren<FCEquipmentsBase>();

        List<FCEquipmentsBase> equipments = new List<FCEquipmentsBase>();
        foreach (Transform child in equipmentRoot)
        {
            FCEquipmentsBase childEquip = child.GetComponent<FCEquipmentsBase>();
            if (childEquip != null)
            {
                equipments.Add(childEquip);
            }
        }


        foreach (FCEquipmentsBase equipment in equipments)
        {
            equipment.SetOwner(_actionCtrl);

            EquipmentAssembler.Singleton.Assemble(equipment, this);
        }

        InitRenderers();
        if (_actionCtrl != null
            && _actionCtrl.AIUse != null && _actionCtrl.AIUse._isInTown
            && _actionCtrl.ThisObject.name.Contains("warrior"))
        {
            Debug.Log("----------Equipment from the backpack-----------");
            _actionCtrl.SwitchWeaponTo(EnumEquipSlot.weapon_hang, _actionCtrl.AIUse._defaultWeaponType);
        }
    }

    public void SetChargeDisplay(float percent)
    {
        if (_uiHPController != null)
        {
            _uiHPController.ChantProgress(percent);
        }
    }

    //diaplay HP
    public void SetHPDisplay(float percent, int reduceValue, bool isCriticalHit, bool isFromSkill)
    {
        if (_uiHPController != null && _actionCtrl != null && _actionCtrl.Data != null)
        {
            _uiHPController.HP = percent;

            _uiHPController.ChangeHP(reduceValue, isCriticalHit, _actionCtrl.Data.eliteType, isFromSkill);
        }
    }

    //display text, now is the name
    public void SetTextDisplay(string text)
    {
        if (_uiHPController != null)
        {

        }

    }

    //diaplay Energy
    public void SetEnergyDisplay(float percent)
    {
        if (_uiHPController != null)
        {
            _uiHPController.Energy = percent;
        }
    }

    void OnDestroy()
    {
        if (_materialInst != null)
        {
            _materialInst.Destroy();
        }
        if (_icon != null)
        {
            RenderTexture.ReleaseTemporary(_icon);
        }
    }

    void RecoverToNormalSpeed(int nameHash)
    {
        if (_animationInfos != null)
        {
            foreach (FCAnimationInfoDetails eids in _animationInfos._animationInfo)
            {
                if (eids._nameHashCode == nameHash)
                {
                    _animator.speed = eids._speed;
                    break;
                }
            }
        }
        else
        {
            _animator.speed = 1;
        }
    }

    public void RecoverToNormalSpeed()
    {
        int nh = 0;
        if (!_animator.IsInTransition(0))
        {
            nh = _animator.GetCurrentAnimatorStateInfo(0).nameHash;
        }
        else
        {
            nh = _animator.GetNextAnimatorStateInfo(0).nameHash;
        }
        RecoverToNormalSpeed(nh);
    }
#if EVENT_FIXED_UPDATE
	void LateUpdate()
	{
		if(_resetStateCounter == FC_CONST.MAX_SAFE_FRAMECOUNT)
		{
			_animator.SetInteger("state", -1);
			_resetStateCounter = -1;
			if(_actionCtrl != null)
			{
				_preAniPlayTime = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
				_actionCtrl.AniIsStart();
				
			}
		}
		if(!_animator.IsInTransition(0) && _resetStateCounter >0)
		{
			_animator.SetInteger("state", -1);
			//_showAnimationIdx = true;
			
			_resetStateCounter = -1;
			//_animator.SetInteger("state", -1);
			if(_actionCtrl != null)
			{
				_preAniPlayTime = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
				_actionCtrl.AniIsStart();
				
			}
		}
	}
    void FixedUpdate()
    {
		if (_animator != null)
        {
            // rest state.
            --_resetStateCounter;
			
			if(_tinyHurtTime >0)
			{
				_tinyHurtTime -= Time.deltaTime;
				if(_tinyHurtTime <=0)
				{
					_animator.SetInteger("SubState", 0);
					_animator.SetLayerWeight(1, 0);
				}
			}

			
            // update state machine.
			if(_resetStateCounter<=0)
			{
				_animStateMachines[(int)_currentAnimStateMachine].Update();
			}
            // loop count
			float animTime = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
			float round = Mathf.Round(animTime);
            int round1 = Mathf.CeilToInt(_preAniPlayTime);
			int round2 = Mathf.CeilToInt(animTime);
           
           
            if (_actionCtrl != null && !_animator.IsInTransition(0) && (round - animTime > 0.0f && round - animTime < 0.05f || (round2 - round1 > 0 && round1 >0)) && _resetStateCounter<0)
            {
                _actionCtrl.AniIsOver((int)round);
            }
			_preAniPlayTime = animTime;
        }
    }
	
	void Update()
	{
		_materialInst.Update();
	}
#else

    protected float _preAnimationTime = 0;
    void Update()
    {
        if (_animator != null && !_isWaitingForPlay && !GameManager.Instance.GamePaused)
        {
            //_animator.SetInteger("state", -1);
            // rest state.
            if (_resetStateCounter > -10)
            {
                --_resetStateCounter;
            }
            _timeCountForNewAni += Time.deltaTime;
            if (_tinyHurtTime > 0)
            {
                _tinyHurtTime -= Time.deltaTime;
                if (_tinyHurtTime <= 0)
                {
                    _animator.SetInteger("SubState", 0);
                    if (_animator.layerCount >= 2)
                    {
                        _animator.SetLayerWeight(1, 0);
                    }
                }
            }

            if (_resetStateCounter == FCConst.MAX_SAFE_FRAMECOUNT && _actionCtrl != null)
            {
                _actionCtrl.AniIsStart();
            }
            else if (!_animator.IsInTransition(0) && _resetStateCounter < FCConst.MAX_SAFE_FRAMECOUNT)
            {
                if (_resetStateCounter >= -10)
                {
                    _animator.SetInteger("state", -1);
                    _resetStateCounter = -11;
                }
                //_showAnimationIdx = true;
            }
            // update state machine.
            if (_resetStateCounter <= 0)
            {
                _animStateMachines[(int)_currentAnimStateMachine].Update();
            }
            // loop count
            float animTime = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            float round = Mathf.Round(animTime);

            //if (_actionCtrl != null && !_animator.IsInTransition(0) && (round - animTime > 0.0f 
            //	&& (round - animTime < 0.01f || ((int)round - (int)animTime) >= 1)) && _resetStateCounter<0)
            //&& round - animTime < 0.01f) && _resetStateCounter<0)
            if (_actionCtrl != null && !_animator.IsInTransition(0))
            {
                if (animTime >= _preAniPlayTime && (int)round > _preAniOverCount
                    && (((Mathf.Floor(animTime) - Mathf.Floor(_preAniPlayTime)) >= 1)
                    || (round - animTime > 0.0f && round - animTime <= 0.05f)))
                {
                    _actionCtrl.AniIsOver((int)round);
                    _preAniOverCount = (int)round;
                }
            }
            _preAniPlayTime = animTime;
            if (_preAniPlayTime <= 0.5f && _preAniOverCount >= 1)
            {
                _preAniOverCount = 0;
            }
        }
        _materialInst.Update();

		//if (_actionCtrl != null && _actionCtrl.IsPlayerSelf && TownHUD._hasChangeNickname)
		//{
		//    _uiHPController.GuildName = PlayerInfo.Instance.GuildName;

		//    TownHUD._hasChangeNickname = false;
		//}
    }

#endif
    public void SetAnimation(AniSwitch parameter)
    {
        if (this.gameObject.activeInHierarchy && _animStateMachines != null && _animator != null)
        {
            StartCoroutine(SetAnimationThread(parameter));
        }
    }

    public void SetAnimationSpeed(float aniSpeed)
    {
        _animator.speed = aniSpeed;
    }

    public float GetAnimationSpeed()
    {
        return _animator.speed;
    }

    public float GetAnimationNormalizedTime()
    {
        float timel = 0;
        if (_resetStateCounter <= 0)
        {
            timel = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        }
        else
        {
            if (_animator.GetNextAnimationClipState(0) != null
                && _animator.GetNextAnimationClipState(0).Length > 0
                && _animator.GetNextAnimationClipState(0)[0].clip != null)
            {
                float timeTotal = _animator.GetNextAnimationClipState(0)[0].clip.length;
                timel = Mathf.Clamp(_timeCountForNewAni / timeTotal * _animator.speed, 0, 1f);
            }
            else if (_animator.GetCurrentAnimationClipState(0) != null && _animator.GetCurrentAnimationClipState(0).Length > 0)
            {
                float timeTotal = _animator.GetCurrentAnimationClipState(0)[0].clip.length;
                timel = Mathf.Clamp(_timeCountForNewAni / timeTotal * _animator.speed, 0, 1f);
            }
        }

        return timel;
    }

    public void SetAnimation(string param, int pValue)
    {
        if (_actionCtrl.IsPlayer)
        {
            _animator.SetLayerWeight(1, 1f);
            _tinyHurtTime = 0.3f;
        }
        else
        {
            if (_animator.layerCount >= 2)
            {
                _animator.SetLayerWeight(1, 1f);
                _tinyHurtTime = 0.6f;
            }
        }
        _animator.SetInteger(param, pValue);

    }
    public float GetAnimationLength()
    {
        float timel = 0;
        if (!_animator.IsInTransition(0))
        {
            timel = _animator.GetCurrentAnimatorStateInfo(0).length;
        }
        else
        {
            timel = _animator.GetNextAnimatorStateInfo(0).length;
        }
        return timel;
    }
    //return origenal speed of the animation 
    public float GetAnimationOrgSpeed()
    {
        int nh = 0;
        if (!_animator.IsInTransition(0))
        {
            nh = _animator.GetCurrentAnimatorStateInfo(0).nameHash;
        }
        else
        {
            nh = _animator.GetNextAnimatorStateInfo(0).nameHash;
        }
        return GetAnimationOrgSpeed(nh);
    }

    float GetAnimationOrgSpeed(int nameHash)
    {
        float speed = 1;
        if (_animationInfos != null)
        {
            foreach (FCAnimationInfoDetails eids in _animationInfos._animationInfo)
            {
                if (eids._nameHashCode == nameHash)
                {
                    speed = eids._speed;
                    break;
                }
            }
        }
        return speed;
    }

    protected bool _isWaitingForPlay;
    IEnumerator SetAnimationThread(AniSwitch parameter)
    {
        _timeCountForNewAni = 0;
        _isWaitingForPlay = true;
        while (_animator.IsInTransition(0))
        {
            yield return null;
        }
        _isWaitingForPlay = false;
        _animStateMachines[(int)parameter._type].Enter(parameter);
        _currentAnimStateMachine = parameter._type;
        _resetStateCounter = FCConst.MAX_BLEND_FRAMECOUNT;
        _preAniPlayTime = 0;
        _preAniOverCount = 0;
        //_showAnimationIdx = false;

    }


    public void PlayEffect(int part)
    {
        if (part >= FCConst.GLOBAL_EFFECT_START)
        {
            //I am playing a global effect
            //play global effect from part id
            FC_GLOBAL_EFFECT effId = (FC_GLOBAL_EFFECT)(part - (int)FCConst.GLOBAL_EFFECT_START);

            switch (effId)
            {
                case FC_GLOBAL_EFFECT.BASH:

                    //get blade track pos
                    Transform trans = Utils.FindTransformByNodeName(_thisTransform, "A");
                    if (trans == null)
                        Debug.LogError("no weapon blade track!(" + gameObject.name + ")");
                    Vector3 pos = trans.position;
                    //set height from my foot
                    pos.y = _thisTransform.position.y;

                    GlobalEffectManager.Instance.PlayEffect(effId, pos);
                    break;

                case FC_GLOBAL_EFFECT.BORN:
                    // from current character's position.
                    Vector3 bornPos = _thisTransform.localPosition;
                    GlobalEffectManager.Instance.PlayEffect(effId, bornPos);
                    break;

                case FC_GLOBAL_EFFECT.BLOOD:
                    // from current character's position.
                    Vector3 bloodPos = GetSlotNode(EnumEquipSlot.belt).position;
                    GlobalEffectManager.Instance.PlayEffect(effId, bloodPos);
                    break;

                default:
                    Assertion.Check(false, "try to play a invalid global effect");
                    break;
            }



        }

    }


    //to scale the move speed by animation



    private CharacterController _characterController = null;

    void OnAnimatorMove()
    {
        if (_actionCtrl != null)
        {
            if (_controlByAni && !_actionCtrl.MoveIsPause)
            {
                //rigidbody.position += _animator.deltaPosition *_deltaSpeedScale;
                Vector3 vG = _actionCtrl.SelfMoveAgent.GetGOffset();
                if (_characterController != null)
                {
                    Vector3 move = _animator.deltaPosition * _deltaSpeedScale + vG;
                    if (float.IsNaN(move.x) || float.IsNaN(move.y) || float.IsNaN(move.z)
                        || move.x >= Mathf.Infinity || move.x <= Mathf.NegativeInfinity
                        || move.y >= Mathf.Infinity || move.y <= Mathf.NegativeInfinity
                        || move.z >= Mathf.Infinity || move.z <= Mathf.NegativeInfinity)
                    {
                        Debug.LogError("Character Controller error:" + gameObject.name + " attampts to move to " + move);
                    }
                    else
                    {
                        _characterController.Move(move);
                    }
                }
                else
                {
                    float scale = Mathf.Min(_actionCtrl.ThisTransform.localScale.x, 1.0f);
                    Vector3 move = (_animator.deltaPosition * _deltaSpeedScale + vG) * scale;
                    if (float.IsNaN(move.x) || float.IsNaN(move.y) || float.IsNaN(move.z)
                        || move.x >= Mathf.Infinity || move.x <= Mathf.NegativeInfinity
                        || move.y >= Mathf.Infinity || move.y <= Mathf.NegativeInfinity
                        || move.z >= Mathf.Infinity || move.z <= Mathf.NegativeInfinity)
                    {
                        Debug.LogError("Avatar Controller error:" + gameObject.name + " attampts to move to " + move);
                    }
                    else
                    {
                        _actionCtrl.ThisTransform.localPosition += move;
                    }
                }
            }
        }
    }

    public void EnableAnimationPhysics()
    {
        //_animator.animatePhysics = true;
    }

    /// <summary>
    /// Add the game object to equipment node list
    /// </summary>
    public void AddEquipmentNode(EnumEquipSlot slot, GameObject node, bool replaceOther)
    {
        if (replaceOther)
        {
            _nonReplacedEquipments.Add(node);
            return;
        }
        if (_equipmentMapping[(int)slot] != null)
        {
            Destroy(_equipmentMapping[(int)slot]);
            _equipmentMapping[(int)slot] = null;
        }
        _equipmentMapping[(int)slot] = node;
        // for slot on weapons, find child node.
        if (slot == EnumEquipSlot.damage_main_A)
        {
            Transform transSlot = Utils.FindTransformByNodeName(node.transform, FCEquipmentsBase.GetNodeByEquipSlot(slot));
            if (transSlot != null)
            {
                _equipmentMapping[(int)slot] = transSlot.gameObject;
            }
        }
    }

    public void AddEquipmentGradeEffect(EnumEquipSlot slot, GameObject node)
    {
        if (_upgradeNodeMapping[(int)slot] != null)
        {
            Destroy(_upgradeNodeMapping[(int)slot]);
            _upgradeNodeMapping[(int)slot] = null;
        }
        _upgradeNodeMapping[(int)slot] = node;
    }

    public void UpdateShadowMatrix(Vector3 lightDir, float pointLight, Vector4 shadowRange)
    {
        Matrix4x4 shadowMatrix = ShadowRenderer.UpdateShadowMatrix(Vector3.up, -_thisTransform.position.y, lightDir, pointLight);
        _materialInst.SetShadowMatrix(shadowMatrix, shadowRange);
    }

    // material related.
    public void HurtColor(float duration)
    {
        _materialInst.SetState(1, duration);
    }

    public void IceHurtColor(float duration)
    {
        _materialInst.SetState(2, duration);
    }

    public void SuperManColor(float duration)
    {
        _materialInst.SetState(3, duration);
    }

    public void RimFlashColor(bool enable)
    {
        // timer > 0: enable
        // timer < 0: disable
        float timer = (enable ? 1.0f : -1.0f);
        _materialInst.SetState(4, timer);
    }

    public void RageFlashColor(bool enable)
    {
        float timer = (enable ? 1.0f : -1.0f);
        _materialInst.SetState(5, timer);
    }

    public void UnDestoryableColor(bool enable)
    {
        float timer = (enable ? 1.0f : -1.0f);
        _materialInst.SetState(6, timer);
    }

    public void GodDownColor(float duration)
    {
        _materialInst.SetState(7, duration);
    }

    public void InvincibleColor(bool enable)
    {
        _materialInst.SetState(7, enable);
    }

    public void PlayDyingEffect()
    {
        Material deadMat = MaterialManager.Instance._deadMat;
        deadMat.SetFloat("_maxHeight", _renderers[0].bounds.size.y);
        deadMat.SetFloat("_startTime", Time.timeSinceLevelLoad);
        deadMat.SetVector("_worldOrigin", myTransform.position);
        materialInst.SetDeadMaterial(deadMat);

        int layer = LayerMask.NameToLayer("TransparentFX");
        foreach (Renderer r in _characterRenderers)
        {
            r.gameObject.layer = layer;
        }
    }

    [ContextMenu("Get destination")]
    void GetDestination()
    {
        NavMeshAgent nma = GetComponent<NavMeshAgent>();
        if (nma != null)
        {
            Debug.Log("Navmesh's destination is " + nma.destination);
        }
    }
}
