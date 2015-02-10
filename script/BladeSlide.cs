using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CycleList
{
	private int _start;
	private int _count;
	private int _max;
	
	public int Count
	{
		get {return _count;}
		set {_count = value;}
	}
	
	public int Remaining
	{
		get {return _max - _count;}
	}
	
	public CycleList(int count, int max)
	{
		_start = 0;
		_count = 0;
		_max = max;
		AddPoints(count);
	}
	public void RemoveHead(int count)
	{
		_start = (_start + count) % _max;
		_count = Mathf.Max(0, _count - count);
	}
	public void RemoveTail(int count)
	{
		_count = Mathf.Max(0, _count - count);
	}
	
	public int AddPoints(int count)
	{
		if(_count + count > _max)
			count = _max - _count;
		_count += count;
		return count;
	}
	public int GetIndex(int count)
	{
		if(count < _count)
		{
			return (_start + count) % _max;
		}
		return 0;
	}
};

public class BladeSlide : MonoBehaviour
{
    public int Fragments = 30; // fragment count of 1-time blade slide.
    public float Duration = 0.15f; // duration of a blade fragment.

    public Transform PointA;
    public Transform PointB;

    public float subdivision = 0.5f;

    public float EndWidth = 1.0f;

    public bool ForceDisappear = false;

    protected Vector3[] _position;
    protected Vector2[] _texcoord;
    protected int[] _indices;
    protected float[] _vertAnim;
    protected CycleList _vertlist;

    protected Vector3[] _refPoints;
    protected int _nextResampling;

    protected Mesh _mesh;
    public Transform Parent;
    public Material BladeMaterial;
    public FightShadow _fightShadow;

    public void SetMaterial(Material newMat)
    {
        BladeMaterial = newMat;
        if (_renderer != null)
        {
            _renderer.material = BladeMaterial;
        }
    }

    protected bool _effectOn = false;
    public bool SetEffectEnable
    {
        get { return _effectOn; }
        set { _effectOn = isEffectOn && value; }
    }

    public static bool isEffectOn = true;

    public bool Emit
    {
        set
        {
            SetEffectEnable = value;

            if (SetEffectEnable && _fightShadow != null)
            {
                _fightShadow.Show();
            }
        }
    }

    void Awake()
    {
        PointA = transform.Find("A");
        PointB = transform.Find("B");
    }


    //private int FragmentStart = 0;
    //private int FragmentCount = 0;
    private MeshRenderer _renderer = null;

    void Start()
    {
        GameObject rootGo = GameObject.Find("BladeRoot");

        if (rootGo == null)
        {
            rootGo = new GameObject("BladeRoot");
            rootGo.transform.position = Vector3.zero;
        }

        Parent = rootGo.transform;

        // objects and mesh creation.
        GameObject go = new GameObject(gameObject.name + "_trail");
        go.layer = LayerMask.NameToLayer("TransparentFX");
        go.transform.parent = Parent;
        go.transform.position = Vector3.zero;
        go.transform.rotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        MeshFilter filter = go.AddComponent<MeshFilter>();
        _renderer = go.AddComponent<MeshRenderer>();
        SetMaterial(BladeMaterial);

        _mesh = new Mesh();
        _mesh.name = gameObject.name + "_TrailMesh";
        filter.mesh = _mesh;

        // create vertex buffers map.
        if (Fragments <= 0)
        {
            Debug.Log("Error blade effect parameter:" + gameObject.transform.parent.parent.gameObject.name);
            Fragments = 20;
        }
        _position = new Vector3[Fragments * 2];
        _texcoord = new Vector2[Fragments * 2];
        _vertAnim = new float[Fragments];
        _indices = new int[Fragments * 6];
        for (int i = 0; i < Fragments; ++i)
        {
            _indices[i * 6] = i * 2;
            _indices[i * 6 + 1] = i * 2 + 1;
            _indices[i * 6 + 2] = i * 2 + 2;
            _indices[i * 6 + 3] = i * 2 + 2;
            _indices[i * 6 + 4] = i * 2 + 1;
            _indices[i * 6 + 5] = i * 2 + 3;
        }
        _indices[(Fragments - 1) * 6 + 2] = 0;
        _indices[(Fragments - 1) * 6 + 3] = 0;
        _indices[(Fragments - 1) * 6 + 5] = 1;

        _vertlist = new CycleList(0, Fragments);

        _refPoints = new Vector3[4];
        _nextResampling = 0;
    }

    void LateUpdate()
    {
        // update position and uv depending on time.
        int fragmentCnt = _vertlist.Count;
        float inverseEnd = 1.0f - EndWidth;
        for (int i = 0; i < fragmentCnt; ++i)
        {
            int idx = _vertlist.GetIndex(i);
            float factor = _vertAnim[idx];
            factor -= Time.deltaTime;
            float scale = factor / Duration;
            float width = scale * inverseEnd + EndWidth;
            _position[idx * 2 + 1] = _position[idx * 2 + 1] * width + _position[idx * 2] * (1.0f - width);
            _texcoord[idx * 2].x = scale;
            _texcoord[idx * 2 + 1].x = scale;
            _vertAnim[idx] = factor;
        }
        // update eslaped time.
        while (_vertlist.Count > 0 && _vertAnim[_vertlist.GetIndex(0)] <= 0)
        {
            _vertlist.RemoveHead(1);
            --_nextResampling;
        }
        // generate new strip.
        if (_effectOn)
        {
            AddNewFragment();
        }
        else if (ForceDisappear)
        {
            _vertlist.Count = 0;
        }
        // commit vertices.
        if (_vertlist.Count > 1)
        {
            int[] indices = new int[(_vertlist.Count - 1) * 6];
            for (int i = 0, s = _vertlist.GetIndex(0); i < _vertlist.Count - 1; ++i)
            {
                int i6 = i * 6;
                int s6 = s * 6;
                indices[i6] = _indices[s6];
                indices[i6 + 1] = _indices[s6 + 1];
                indices[i6 + 2] = _indices[s6 + 2];
                indices[i6 + 3] = _indices[s6 + 3];
                indices[i6 + 4] = _indices[s6 + 4];
                indices[i6 + 5] = _indices[s6 + 5];
                s = (s + 1) % Fragments;
            }
            _mesh.Clear();
            _mesh.vertices = _position;
            _mesh.uv = _texcoord;
            _mesh.triangles = indices;
            _renderer.enabled = true;
        }
        else
        {
            _renderer.enabled = false;
        }
    }

    void AddNewFragment()
    {
        Vector3 a = PointA.transform.position;
        Vector3 b = PointB.transform.position;

        //if(_tempPontCount >= 2 && FragmentCount >= 2)
        if (_vertlist.Count >= 2)
        {
            int index1 = _vertlist.GetIndex(_nextResampling);
            int index2 = _vertlist.GetIndex(_vertlist.Count - 1);

            if (index1 < 0 || index1 >= _position.Length)
            {
                Debug.Log("BladeSlide:array length is " + _position.Length + ",index1 is " + index1 + ",_nextResampling is " + _nextResampling + "Count is " + _vertlist.Count + "remaining is " + _vertlist.Remaining);
                index1 = 0;
            }
            if (index2 < 0 || index2 >= _position.Length)
            {
                Debug.Log("BladeSlide:array length is " + _position.Length + ",index2 is " + index2 + ",_nextResampling is " + _nextResampling + "Count is " + _vertlist.Count + "remaining is " + _vertlist.Remaining);
                index2 = 0;
            }

            Vector3 p00 = _position[index1 * 2];
            Vector3 p10 = _position[index1 * 2 + 1];
            Vector3 p01 = _position[index2 * 2];
            Vector3 p11 = _position[index2 * 2 + 1];

            float startUv = _texcoord[index1 * 2].x;
            float startTime = _vertAnim[index1];
            _vertlist.RemoveTail(_vertlist.Count - _nextResampling);

            float distance = Vector3.Distance(p00, p01) + Vector3.Distance(p01, a);
            int addpointcnt = Mathf.Max(3, (int)(distance / subdivision));

            if (addpointcnt > Fragments - 1)
                addpointcnt = Fragments - 1;
            int sub = _vertlist.Count + addpointcnt - Fragments + 1;
            if (sub > 0)
            {
                _vertlist.RemoveHead(sub);
            }

            for (int i = 0; i <= addpointcnt; ++i)
            {
                float dev = i / (float)(addpointcnt);
                _vertlist.AddPoints(1);
                int idx1 = _vertlist.GetIndex(_vertlist.Count - 1);
                _position[idx1 * 2] = p00 * ((1.0f - dev) * (1.0f - dev)) + p01 * (2 * dev * (1 - dev)) + a * (dev * dev);
                _position[idx1 * 2 + 1] = p10 * ((1.0f - dev) * (1.0f - dev)) + p11 * (2 * dev * (1 - dev)) + b * (dev * dev);
                _texcoord[idx1 * 2].x = startUv + (1 - startUv) * dev;
                _texcoord[idx1 * 2].y = 0.0f;
                _texcoord[idx1 * 2 + 1].x = _texcoord[idx1 * 2].x;
                _texcoord[idx1 * 2 + 1].y = 1.0f;

                _vertAnim[idx1] = Duration * dev + startTime * (1.0f - dev);
            }

            _nextResampling = _vertlist.Count - 1 - addpointcnt / 2;
            _refPoints[0] = _refPoints[2];
            _refPoints[2] = a;
            _refPoints[1] = _refPoints[3];
            _refPoints[3] = b;
        }
        else
        {
            _vertlist.AddPoints(1);
            int idx1 = _vertlist.GetIndex(_vertlist.Count - 1);
            _position[idx1 * 2] = a;
            _position[idx1 * 2 + 1] = b;

            _texcoord[idx1 * 2].x = 1.0f;
            _texcoord[idx1 * 2].y = 0.0f;
            _texcoord[idx1 * 2 + 1].x = 1.0f;
            _texcoord[idx1 * 2 + 1].y = 1.0f;

            _vertAnim[idx1] = Duration;

            _refPoints[0] = _refPoints[2];
            _refPoints[2] = a;
            _refPoints[1] = _refPoints[3];
            _refPoints[3] = b;
            _nextResampling = 0;
        }
    }

    void OnDisable()
    {
        if (_renderer != null)
        {
            _renderer.enabled = false;
        }
    }

    void OnEnable()
    {
        if (_renderer != null)
        {
            _renderer.enabled = true;
        }
    }

    void OnDestroy()
    {
        if (_renderer != null)
        {
            Destroy(_renderer.gameObject);
        }

        if (_mesh != null)
        {
            Destroy(_mesh);
        }
    }
}
