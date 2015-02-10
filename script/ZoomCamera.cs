using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class ZoomCamera : MonoBehaviour
{	
	#region Editor fields
	public float _zoomSpeed = 10.0f;
	public float _zoomRatio = 0.01f;
	public float _initialZoomLevel = 0.5f;
	public float _orthoMinZoomSize = 10.0f;
	public float _orthoMaxZoomSize = 70.0f;
	#endregion
	
	private int _numPressedPointers;
	private int[] _pressedPointersId;
	private Vector3[] _pointerPositions;
	private Vector3[] _lastPointerPositions;
	private const int maxPointers = 12;
	private float _zoomLevel;
	private float _targetZoomLevel;
	private bool _zoomEnabled = true;
	
	public bool ZoomEnabled {
		get {
			return this._zoomEnabled;
		}
		set {
			_zoomEnabled = value;
		}
	}

	void Awake ()
	{
		_zoomLevel = Mathf.Clamp01 (_initialZoomLevel);
		_targetZoomLevel = _zoomLevel;
		
		_numPressedPointers = 0;
		_pointerPositions = new Vector3[maxPointers];
		_lastPointerPositions = new Vector3[maxPointers];
		_pressedPointersId = new int[2];

	}
	
	void Start ()
	{

	}

	public void ZoomTo (float zoom)
	{
		_targetZoomLevel = zoom;
	}

	public void ReceiveInput ()
	{
		int count = Input.touchCount;
		for (int i = 0; i < count; i++) {
			Touch touch = Input.GetTouch (i);
			
			_pointerPositions [touch.fingerId] = touch.position;
			if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) {
				_numPressedPointers--;
				if (_numPressedPointers < 0)
					_numPressedPointers = 0;
				
			} else {
				if (_numPressedPointers < _pressedPointersId.Length)
					_pressedPointersId [_numPressedPointers] = touch.fingerId;
				
				_numPressedPointers++;
				
				for (int k = 0; k < maxPointers; k++) {
					_lastPointerPositions [k] = _pointerPositions [k];
				}
			}
		}		
	}
	
	void Update ()
	{
		//ReceiveInput ();
		ControlUpdate ();
		UpdateZoom ();		
	}
	
	private void ControlUpdate ()
	{

		if (_numPressedPointers == 2) {
			// Pinch zoom
			Vector3 lastDif = _lastPointerPositions [_pressedPointersId [1]] - _lastPointerPositions [_pressedPointersId [0]];
			Vector3 currentDif = _pointerPositions [_pressedPointersId [1]] - _pointerPositions [_pressedPointersId [0]];
				
			float lastDifLength = lastDif.magnitude;
			float currentDifLength = currentDif.magnitude;
				
			if ((lastDifLength > 0.01f) && (_zoomEnabled)) {
				float zoomRatio = currentDifLength - lastDifLength;
					
				_targetZoomLevel = Mathf.Clamp01 (_targetZoomLevel + zoomRatio * _zoomRatio);
			}
				
			_lastPointerPositions [_pressedPointersId [0]] = _pointerPositions [_pressedPointersId [0]];
			_lastPointerPositions [_pressedPointersId [1]] = _pointerPositions [_pressedPointersId [1]];
		}
		
		
		#if UNITY_EDITOR
		if (_zoomEnabled){
			float zoom = Input.GetAxis("Mouse ScrollWheel");
			if (Input.GetMouseButton(1))
					zoom += Input.GetAxis("Mouse Y");
					
			_targetZoomLevel = Mathf.Clamp01(_targetZoomLevel + zoom);
		}
		#endif
		
	
	}
	
	private void UpdateZoom ()
	{
		_zoomLevel = Mathf.SmoothStep (_zoomLevel, _targetZoomLevel, _zoomSpeed * Time.deltaTime);
		camera.fieldOfView = Mathf.Lerp (_orthoMinZoomSize, _orthoMaxZoomSize, _zoomLevel);	
	}
	

}

