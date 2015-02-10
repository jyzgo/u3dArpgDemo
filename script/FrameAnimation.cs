using UnityEngine;
using System.Collections;

public class FrameAnimation : MonoBehaviour {
	
	public bool randomStart = false;
	public int xFrames;
	public int yFrames;
	
	private Material material;
	public float Step;
	public int playTimes;
	
	private Vector4 originOffsetTiling;
	private float m_lastTime;
	
	private float uvStepX;
	private float uvStepY;
	
	private int _horiSteps;
	private int _vertSteps;
	
	private int _playedTime;
	
	// Use this for initialization
	void Start () {
		material = GetComponent<MeshRenderer>().material;
		//material.hideFlags = HideFlags.DontSave;
		
		if(material != null && material.mainTexture != null)
		{
			originOffsetTiling.z = material.mainTextureOffset.x;
			originOffsetTiling.w = material.mainTextureOffset.y;
			originOffsetTiling.x = material.mainTextureScale.x;
			originOffsetTiling.y = material.mainTextureScale.y;
			m_lastTime = 0.0f;
			uvStepX = 1.0f / xFrames;
			uvStepY = 1.0f / yFrames;
			_horiSteps = 0;
			_vertSteps = 0;
			Vector2 tile;
			tile.x = uvStepX;
			tile.y = uvStepY;
			material.mainTextureScale = tile;
			Vector2 newUV;
			newUV.x = uvStepX * _horiSteps;
			newUV.y = 1.0f - uvStepY * (_vertSteps + 1);
			material.mainTextureOffset = newUV;
			
			_playedTime = 1;
			
			if(randomStart)
			{
				int startIndex  = UnityEngine.Random.Range(0,xFrames* yFrames);
				_horiSteps = startIndex % xFrames;
				_vertSteps = startIndex /xFrames;
			}
			
			
		}
		this.enabled = (material != null && material.mainTexture != null);
	}
	
	// Update is called once per frame
	void Update () {
		m_lastTime += Time.deltaTime;
		if(m_lastTime >= Step)
		{
			_horiSteps = _horiSteps + 1;
			_vertSteps = (_vertSteps + _horiSteps / xFrames) % yFrames;
			_horiSteps = _horiSteps % xFrames;
			
			Vector2 newUV;
			newUV.x = uvStepX * _horiSteps;
			newUV.y = 1.0f - uvStepY * (_vertSteps + 1);
			material.mainTextureOffset = newUV;
			
			m_lastTime = 0.0f;
			
			_playedTime += 1;
			if(_playedTime == xFrames * yFrames * playTimes)
			{
				this.enabled = false;
			}
		}
	}
	
	void Reset()
	{
		if(material != null && material.mainTexture != null)
		{
			Vector2 param;
			param.x = originOffsetTiling.z;
			param.y = originOffsetTiling.w;
			material.mainTextureOffset = param;
			param.x = originOffsetTiling.x;
			param.y = originOffsetTiling.y;
			material.mainTextureScale = param;
		}
	}
	
	void OnDestroy()
	{
		Reset();
	}
	
	
	public void ChaneColor(Color color)
	{
		material.color = color;
	}
	
}
