using UnityEngine;
using System.Collections;

public class BoundLimitedDrag : MonoBehaviour {

    public int contentWidth = 2048;

    public int contentHeight = 2048;

    public int tweenFactor = 10;

	public GameObject content;

    private Vector2 _lastDirection;

    private long _transitionTime;

    private Vector2 _tweenVec2;

    private float _pixelSizeAdjustment;
    private float PixelSizeAdjustment 
    {
        get
        {
            if (0 == _pixelSizeAdjustment)
                _pixelSizeAdjustment = NGUITools.FindInParents<UIRoot>(gameObject).pixelSizeAdjustment;
            return _pixelSizeAdjustment;
        }
    }

	// Use this for initialization
	void Start () {
        BoxCollider bc = GetComponent<BoxCollider>();
        if (null == bc)
        {
            bc = gameObject.AddComponent<BoxCollider>();
        }
        bc.size = new Vector3(Screen.width, Screen.height, 1);
	}
	
	// Update is called once per frame
	void Update () {
        GetComponent<BoxCollider>().size = new Vector3(Screen.width, Screen.height, 1);
 	}

    #region limit border
    float BoundLeft
    {
        get { return (-Screen.width / 2) * PixelSizeAdjustment; }
    }

    float BoundTop
    {
        get { return (Screen.height / 2) * PixelSizeAdjustment - contentHeight; }
    }

    float BoundBottom
    {
        get { return (-Screen.height / 2) * PixelSizeAdjustment; }
    }

    float BoundRight
    {
        get { return Screen.width / 2 * PixelSizeAdjustment - contentWidth ; }
    }
    #endregion

    public void SetPosition(Vector3 center)
    {
        content.transform.localPosition = new Vector3(-center.x, -center.y, 0);
        limitToBound();
    }

    public void FocusOn(Vector3 center)
    {
        _tweenVec2 = new Vector3(-center.x - content.transform.localPosition.x,
            -center.y - content.transform.localPosition.y, 0);
        StartCoroutine(stepTween());
    }

    void OnPress(bool isDown)
	{
        if (!isDown)
        {
            if (_lastDirection == Vector2.zero)
            {
                SendMessage("OnClickWithoutDragging");
            }
            else
            {
                if (_lastDirection.magnitude > 10)
                {
                    _tweenVec2 = _lastDirection * tweenFactor;
                    StopAllCoroutines();
                    StartCoroutine(stepTween());
                }
                _lastDirection = Vector2.zero;
            }
        }
	}

	void OnDrag(Vector2 delta)
	{
        float ration = UIRoot.GetPixelSizeAdjustment(gameObject);
        delta *= ration;
        content.transform.localPosition +=
            new Vector3((float)delta.x, (float)delta.y, 0);
        limitToBound();
        _lastDirection = delta;
	}

    IEnumerator stepTween()
    {
        while (_tweenVec2.magnitude > 4)
        {
            Vector2 step = _tweenVec2 / 10;
            _tweenVec2 -= step;
            content.transform.localPosition += new Vector3((float)step.x, (float)step.y, 0);
            limitToBound();
            yield return null;
        }
    }

    void limitToBound()
    {
        Vector3 cp = content.transform.localPosition;
        if (cp.x > BoundLeft) { cp.x = BoundLeft; }
        if (cp.x < BoundRight) { cp.x = BoundRight; }
        if (cp.y < BoundTop) { cp.y = BoundTop; }
        if (cp.y > BoundBottom) { cp.y = BoundBottom; }
        content.transform.localPosition = cp;
    }
}
