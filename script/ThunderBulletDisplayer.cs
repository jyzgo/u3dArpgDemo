using UnityEngine;
using System.Collections;

public class ThunderBulletDisplayer : BulletDisplayer {
	
	public GameObject _chainRes;
	public int _chainMaxLength = 1;
	
	public Material _chainMat;
	Material _chainMatInst = null;
	public Material _sparkMat;
	
	public GameObject _spark;
			
	public class pointInfo
	{
		public float time;
		public Vector3 worldPos;
		public Transform chainTrans;
		//public Transform sparkTrans;
		//public ParticleSystem sparkParticle;
		//public GameObject spark;
		public GameObject chain;
		//public Material sparkMat;
		public pointInfo previous = null;
		public pointInfo next = null;
		
		public void Active(float startTime) {
			time = startTime;
			chain.SetActive(true);
			//spark.SetActive(true);
			//sparkMat.SetFloat("_startTime", Time.timeSinceLevelLoad);
		}
		
		public void Deactive() {
			chain.SetActive(false);
			//spark.SetActive(false);
		}
		
		public void Update(Vector3 from, Vector3 to) {
			worldPos = from;
			chainTrans.localPosition = from;
			chainTrans.LookAt(to);
			Vector3 s = chainTrans.localScale;
			s.z = (from - to).magnitude;
			s.x = 1.0f;//s.z;
			chainTrans.localScale = s;
			
			//sparkTrans.localPosition = to;
		}
	};
	
	System.Collections.Generic.List<pointInfo> _pointList;
	pointInfo _endPoint;
	int _head = 0;
	int _activeCnt = 0;
	
	void Awake() {
		
		_pointList = new System.Collections.Generic.List<pointInfo>();
		for(int i = 0;i < _chainMaxLength;++i) {
			_pointList.Add(CreatePointInfo(Vector3.zero));
		}
		_head = 0;
		_activeCnt = 0;
		_endPoint = null;
	}
	
	public void SetChainLength(int count) {
		_chainMaxLength = count;
	}
	
	public void PlayBlindSpark(Vector3 position) {
		GlobalEffectManager.Instance.PlayEffect(FC_GLOBAL_EFFECT.THUNDER_NO_TARGET, position);
	}
	
	pointInfo CreatePointInfo(Vector3 worldPos) {
		// create material instance first.
		if(_chainMatInst == null) {
			_chainMatInst = Utils.CloneMaterial(_chainMat);
			//_chainMatInst.mainTexture = _thunderTexture.TexturePtr;
		}
		
		pointInfo pi = new pointInfo();
		GameObject go = GameObject.Instantiate(_chainRes) as GameObject;
		pi.chain = go;
		pi.chain.GetComponentInChildren<Renderer>().sharedMaterial = _chainMatInst;
		pi.chainTrans = go.transform;
		pi.chainTrans.parent = LevelManager.Singleton.BulletRoot;
		
		//pi.spark = GameObject.Instantiate(_spark) as GameObject;
		//pi.sparkParticle = pi.spark.GetComponent<ParticleSystem>();
		//Material newMat = new Material(_sparkMat.shader);
		//newMat.CopyPropertiesFromMaterial(_sparkMat);
		//newMat.hideFlags = HideFlags.DontSave;
		//pi.spark.gameObject.GetComponentInChildren<Renderer>().sharedMaterial = newMat;
		//pi.sparkMat = newMat;
		//pi.sparkTrans = pi.spark.transform;
		//pi.sparkTrans.parent = LevelManager.Singleton.BulletRoot;
		
		pi.worldPos = worldPos;
		pi.Deactive();
		return pi;
	}	
	
	public int AddPoint(Vector3 worldPos) {
		
		// extend
		int tail = (_head + _activeCnt) % _pointList.Count;
		if(_activeCnt == _pointList.Count) {
			_pointList.Insert(tail, CreatePointInfo(worldPos));
		}
		pointInfo pi = _pointList[tail];
		pi.worldPos = worldPos;
		if(_endPoint != null) {
			_endPoint.next = pi;
			pi.previous = _endPoint;
			_endPoint.Active(Time.timeSinceLevelLoad);
			_endPoint.Update(_endPoint.worldPos, worldPos);
		}
		_endPoint = pi;
		++_activeCnt;
		
		return tail;
	}
	
	public void UpdatePos(int idx, Vector3 pos)
	{
		pointInfo current = _pointList[idx];
		pointInfo previous = _pointList[idx].previous;
		pointInfo next = _pointList[idx].next;
		current.worldPos = pos;
		if(previous != null) {
			previous.Update(previous.worldPos, pos);
		}
		if(next != null) {
			current.Update(pos, next.worldPos);
		}
	}
	
	public void Clear() {
		while( _activeCnt > 1) {
			RemoveFirst();
		}
		pointInfo pi = _pointList[_head];
		pi.Deactive();
		pi.previous = null;
		pi.next = null;
		_activeCnt = 0;
		_endPoint = null;
	}
	
	public void RemoveFirst() {
		if(_activeCnt > 0) {
			pointInfo pi = _pointList[_head];
			pi.Deactive();
			if(pi.previous != null) {
				pi.previous.next = null;
				pi.previous = null;
			}
			if(pi.next != null) {
				pi.next.previous = null;
				pi.next = null;
			}
			_head = ((_head + 1) % _pointList.Count);
			--_activeCnt;
		}
	}
}
