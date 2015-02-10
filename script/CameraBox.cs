using UnityEngine;
using System.Collections;

public class CameraBox : MonoBehaviour
{
    private const float k_road_edge = 4.32f;    //the coordinate Z of road edge

    private const float k_wall_width = 0.5f;

    public enum EnumCameraBoxState
    {
        following,
        stoppedLeft,
        stoppedRight,
    }

    private Transform _target;  //hero for camera box to follow

    private EnumCameraBoxState _state = EnumCameraBoxState.following;

    private Transform _myTransform;

    public Transform MyTransform { get { return _myTransform; } }

    private BoxCollider _myCollider;

    private float _defaultZ;        //do not move in Z

    private GameObject _blocker;    //the blocker that stops camera box, at most 1

    public void SetTarget(Transform target, FC2Camera ew2Camera)
    {
        _target = target;

        _myTransform.localRotation = _target.localRotation;

        float fovVertical = ew2Camera.fieldOfView;

        float fovHorizontal = fovVertical * Screen.width / Screen.height;

        float distanceToRoadEdge = ew2Camera.translation.z + CameraController.Instance._target.transform.localPosition.z - k_road_edge;

        float halfSize = distanceToRoadEdge * Mathf.Tan(fovHorizontal / 2 * Mathf.Deg2Rad);

        _myCollider.size = _target.localRotation * (new Vector3(halfSize * 2 - k_wall_width * 2, 1, 1));

        _defaultZ = target.transform.localPosition.z;
    }


    void Awake()
    {
        _myTransform = transform;

        _myCollider = collider as BoxCollider;
    }

    // Update is called once per frame
    void Update()
    {
        if (_target == null) return;

        switch (_state)
        {
            case EnumCameraBoxState.following:
                SyncPosition();
                break;

            case EnumCameraBoxState.stoppedLeft:
                if (_target.localPosition.x > _myTransform.localPosition.x || !_blocker.collider.enabled)
                {
                    RestoreFollowState();
                }
                break;

            case EnumCameraBoxState.stoppedRight:
                if (_target.localPosition.x < _myTransform.localPosition.x || !_blocker.collider.enabled)
                {
                    RestoreFollowState();
                }
                break;
        }
    }

    private void RestoreFollowState()
    {
        _blocker = null;

        SyncPosition();

        _state = EnumCameraBoxState.following;
    }

    private void SyncPosition()
    {
        if (_target != null && _myTransform.localPosition.x != _target.localPosition.x)
        {
            Vector3 targetPos = _target.localPosition;

            targetPos.z = _defaultZ;

            _myTransform.localPosition = targetPos;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.transform.localPosition.x < _myTransform.localPosition.x)  //on my left
        {
            _state = EnumCameraBoxState.stoppedLeft;
        }
        else
        {
            _state = EnumCameraBoxState.stoppedRight;
        }

        _blocker = other.gameObject;

        Debug.Log(string.Format("Trigger entered: {0}    State = {1}", other.gameObject.name, _state));
    }

    void OnTriggerExit(Collider other)
    {
        _state = EnumCameraBoxState.following;

        _blocker = null;

        Debug.Log(string.Format("Trigger exited: {0}", other.gameObject.name));
    }
}
