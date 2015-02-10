using UnityEngine;
using System.Collections;



//this class is for control the "light flash" effect on button
public class Flash : MonoBehaviour {

	private Material _material;
	private UITexture _texture;
	public float _cycleTime = 3;
	private float _timer = 0;
	public float _delayTime = 3;
	private float _dTimer = 0; 
	public bool _activeFlag = false;
	
	void Start () {
		_texture = GetComponent<UITexture>();
		_material = Utils.CloneMaterial(_texture.material);	
		_texture.material = _material;
		
		_dTimer = _delayTime;
		_timer = 0;
	}
	
	void OnBeginButtonFlash()
	{
		ActiceFlash(true);	
	}
	
	void OnStopButtonFlash()
	{
		ActiceFlash(false);	
	}
	
	public void ActiceFlash(bool active)
	{
		_activeFlag =active;
	}

	void Update () {
		_dTimer -= Time.deltaTime;
		if(_dTimer <=0)
		{
			_timer += Time.deltaTime;
			if(_timer >= _cycleTime)
			{
				_timer -= _cycleTime;
				if(_activeFlag)
				{
					DoOneFlash();
					_dTimer = _delayTime;
					_timer = 0.0f;
				}
			}
		}
	}
	
	public void DoOneFlash()
	{
		_material.SetFloat("_startTime", Time.timeSinceLevelLoad);
	}
}
