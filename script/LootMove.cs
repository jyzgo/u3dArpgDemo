//#define debug_track
using UnityEngine;
using System.Collections;

public class LootMove : MonoBehaviour
{
    private float _lifeTime = 0.0f;
	
    private GameObject _lootMovePrefab = null; //a reference of move trace
    private GameObject _lootObjectPrefab = null; //a reference of loot obj
    private GameObject _lootParticlePrefab = null; //a reference of loot particle
	
    private GameObject _moveObj = null;	//an instance of move trace
	private GameObject _lootObj = null;	//an instance of loot obj
    private ParticleSystem _particle = null;	//an instance of loot particle
	
    private string _lootId;
    private int _lootCount;


    private bool _moveFlag = false;
    private bool _pickupFlag = false;

    private Transform _thisTransform;
    protected Vector3 _moveSpeed;

    private float _v0 = 0;
    private float _g = 0;

    private Transform _moveTransform = null;


    private Transform _lootTransform = null;

	private NavMeshAgent _agent = null;
	private float _time;
	private float _yOffset = 0;
	
    void Awake()
    {
        _thisTransform = transform;
		

        _pickupFlag = false;
    }


    void OnDestroy()
    {
        _lootMovePrefab = null;
        _lootObjectPrefab = null;
        _lootParticlePrefab = null;
    }

    public void SetData(GameObject lootMove, GameObject lootPrefab, GameObject lootParticle, string lootId, int count, float liftTime)
    {
        _lootMovePrefab = lootMove;
        _lootObjectPrefab = lootPrefab;
        _lootParticlePrefab = lootParticle;
        _lootId = lootId;
        _lootCount = count;
        _lifeTime = liftTime;
    }
	
	//initialize prefabs for the move, should follow setdata()
    public void PrepareMovePrefabs()
    {
		Assertion.Check(_lootMovePrefab != null);
		Assertion.Check(_lootObjectPrefab != null);
		Assertion.Check(_lootParticlePrefab != null);

        _moveObj = GameObject.Instantiate(_lootMovePrefab) as GameObject;
        _particle = _moveObj.GetComponentInChildren<ParticleSystem>();
        _moveObj.transform.parent = LootManager.Instance.LootRoot;	
		_agent = _moveObj.GetComponent<NavMeshAgent>();
		_agent.enabled = false;
		_moveObj.SetActive(false);

		_lootObj = GameObject.Instantiate(_lootObjectPrefab) as GameObject;	
		
		// for weapons, disable all weapon logic.
		MessageReciever []receivers = _lootObj.GetComponentsInChildren<MessageReciever>();
		if(receivers.Length > 0) {
			// disable all logic on weapon.
			foreach(MessageReciever r in receivers) {
				Destroy(r);
			}
			// add new loot object components.
			BoxCollider collider = _lootObj.GetComponent<BoxCollider>();
			if(collider == null) {
				collider = _lootObj.AddComponent<BoxCollider>();
			}
			collider.center = -Vector3.up *0.75f;
			collider.size = Vector3.one;
			collider.isTrigger = true;
			_lootObj.AddComponent<LootObject>();
			
			_lootObj.layer = LayerMask.NameToLayer("LOOT");
			_lootObj.transform.localRotation = Quaternion.AngleAxis(180.0f, Vector3.forward);
			
		}
		else
		{
			BoxCollider collider = _lootObj.GetComponent<BoxCollider>();
			if(collider != null) {
				collider.center = Vector3.up*0.75f;
			}
		}
		_lootObj.SetActive(false);		
        _lootObj.transform.parent = LootManager.Instance.LootRoot;	

		if (_particle != null)
		{
            _particle.Stop();
		}

    }	
	
    public void StartMove(Vector3 offset, float time)
    {
        float yOffset = offset.y;
        if (yOffset > 0)
        {
            _g = 2.0f * yOffset / (time * time * 0.25f);
            _v0 = _g * time * 0.5f;
        }else{
			_g  = 0;
			_v0 = 0;
		}

        Vector3 speed = offset / time;
		speed.y = 0;

        _moveFlag = true;
        _moveSpeed = speed;
		_moveObj.SetActive(true);
		_moveTransform = _moveObj.transform;
        _moveTransform.localPosition = _thisTransform.position;


        if (_particle != null)
        {
            _particle.Play();
        }
        else
        {
			_lootObj.SetActive(true);
            _lootTransform = _lootObj.transform;
            _lootTransform.transform.localPosition = _moveTransform.position;

            ShowShadow(false);
        }
		
		
		if(_agent != null)
		{
			_agent.enabled = true;
			_agent.destination = _moveTransform.position +  new Vector3(offset.x, 0,offset.z);
			_time = 0;
		}
		
    }	
	
	//WARNING!!!!!!!!!!!   only use for cheat drop!!!	
    public void StartMoveForCheatDrop(Vector3 offset, float time)
    {
        float yOffset = offset.y;
        if (yOffset > 0)
        {
            _g = 2.0f * yOffset / (time * time * 0.25f);
            _v0 = _g * time * 0.5f;
        }else{
			_v0 = 0;
			_g = 0;
		}

        Vector3 speed = offset / time;
		
        _moveFlag = true;
        _moveSpeed = speed;
	

        _moveObj = GameObject.Instantiate(_lootMovePrefab, _thisTransform.position, Quaternion.identity) as GameObject;
        _moveTransform = _moveObj.transform;
        _moveObj.transform.parent = LootManager.Instance.LootRoot;
        _moveObj.transform.localPosition = _thisTransform.position;
		
		_agent = _moveObj.GetComponent<NavMeshAgent>();
		if(_agent != null)
		{
			_agent.enabled = true;
			_agent.destination = _moveTransform.position +  new Vector3(offset.x, 0,offset.z);
			_time = 0;
		}
		

       // LootCollider lootCollider = _moveObj.GetComponent<LootCollider>();
        //lootCollider.Move = this;

        _lootObj = GameObject.Instantiate(_lootObjectPrefab, _thisTransform.position, Quaternion.identity) as GameObject;
		_lootTransform = _lootObj.transform;
        _lootTransform.transform.parent = LootManager.Instance.LootRoot;
        _lootTransform.transform.localPosition = _moveTransform.position;
		
        _particle = _moveObj.GetComponentInChildren<ParticleSystem>();
        if (_particle != null)
        {
            _particle.Play();
			_lootObj.SetActive(false);
        }
        else
        {
            // for weapons, disable all weapon logic.
			MessageReciever []receivers = _lootObj.GetComponentsInChildren<MessageReciever>();
			if(receivers.Length > 0) {
				// disable all logic on weapon.
				foreach(MessageReciever r in receivers) {
					Destroy(r);
				}
				// add new loot object components.
				BoxCollider collider = _lootObj.GetComponent<BoxCollider>();
				if(collider == null) {
					collider = _lootObj.AddComponent<BoxCollider>();
				}
				collider.center = -Vector3.up*0.5f;
				collider.size = Vector3.one;
				collider.isTrigger = true;
				_lootObj.AddComponent<LootObject>();
				
				_lootObj.layer = LayerMask.NameToLayer("LOOT");
			}
			
            ShowShadow(false);
        }
    }

#if debug_track
    private int _tmpGoCounter;
#endif

    void FixedUpdate()
    {
        if (!_moveFlag)
        {
            return;
        }

		
		if(_agent != null)
		{
			_time += Time.deltaTime;
			_yOffset = _v0 * _time - _g * _time * _time /2;
			if(_yOffset < 0 )
			{
				_yOffset = 0;
				StartPickup();
			}
			_agent.baseOffset = _yOffset;
			
			if (_lootTransform != null)
            {
               _lootTransform.localPosition = _moveTransform.position;
            }
		}
		
		if(_particle != null)
		{
			if(_particle.isStopped)
			{
				StartPickup();
			}
		}
    }
	
	public void StopHorizontalMove()
	{
		_moveSpeed = Vector3.zero;
	}

	//drop to the ground, can wait for pick
    public void StartPickup()
    {
        if (_pickupFlag)
        {
            return;
        }
		
        _pickupFlag = true;
        _moveFlag = false;
		
		
		//for particle objects, prevent from born too early
		//if (!_lootObj.activeSelf)
		{
            _lootObj.transform.parent = LootManager.Instance.LootRoot;
            _lootObj.transform.localPosition = _moveTransform.position;
			_lootObj.SetActive(true);
		}

        ShowShadow(true);

        LootObject lootObject = _lootObj.GetComponent<LootObject>();
      
        lootObject.LootId = _lootId;
        lootObject.LootCount = _lootCount;

        GameObject particle = GameObject.Instantiate(_lootParticlePrefab, _thisTransform.position, Quaternion.identity) as GameObject;

        particle.transform.parent = LootManager.Instance.LootRoot;
        particle.transform.localPosition = _moveTransform.position;

        lootObject.PartcileEffect = particle;
        lootObject.StartPickup(_lifeTime);
        
        Destroy(_moveObj);
        Destroy(gameObject);
    }

    private void ShowShadow(bool active)
    {
        if (_lootObj != null)
        {
            Transform shadow = _lootObj.transform.FindChild("shadow");
            if (shadow != null)
            {
                shadow.gameObject.SetActive(active);
            }
        }
    }

#if debug_track
    private void CreateTrackObject()
    {
        //debug: create a cube on current position
        GameObject tmpGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tmpGo.transform.parent = _thisTransform;
        tmpGo.transform.position = _moveTransform.position + _moveTransform.rotation * (_moveTransform.collider as BoxCollider).center;
        tmpGo.transform.rotation = _moveTransform.rotation;
        tmpGo.transform.localScale = (_moveTransform.collider as BoxCollider).size;
        Destroy(tmpGo.GetComponent<Collider>());
        tmpGo.name = "track " + _tmpGoCounter.ToString("D2");
    }
#endif
}
