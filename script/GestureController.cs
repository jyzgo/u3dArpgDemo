using UnityEngine;
using System.Collections;

public class GestureController : MonoBehaviour
{
#if UNITY_EDITOR || !(UNITY_IPHONE || UNITY_ANDROID)
    private const int MAX_TOUCHES = 1;
#else
	private const int MAX_TOUCHES = 5;
#endif

    private bool _pressed = false;
    private Gesture[] _gestures; // Used to handle gestures from mouse or mobile multitouch

    // Tweak minimum movement amplitude considered a gesture for all gestures.
    public float _minimumAmplitude = 15.0f;
    public float _horizontalAngleRange = 45.0f;
    public float _verticalAngleRange = 45.0f;

    public float MinimumAmplitude
    {
        get { return _minimumAmplitude; }
        set { _minimumAmplitude = value; }
    }

    public float HorizontalAngleRange
    {
        get { return _horizontalAngleRange; }
        set { _horizontalAngleRange = value; }
    }

    public float VerticalAngleRange
    {
        get { return _verticalAngleRange; }
        set { _verticalAngleRange = value; }
    }
	
    void Init()
    {
        Gesture.MinimumAmplitude = _minimumAmplitude; // A way to externalize the value Gesture class minimumAmplitude

        float[] gestureDirectionRange = new float[(int)Gesture.Directions.COUNT * 2];

        float horizontalAngleRangeHalf = _horizontalAngleRange * 0.5f;
        float verticalAngleRangeHalf = _verticalAngleRange * 0.5f;
        float diagonalAngleRangeHalf = (360.0f - (_verticalAngleRange + _horizontalAngleRange) * 2.0f) * 0.125f;

        gestureDirectionRange[(int)Gesture.Directions.Up] = 0 - verticalAngleRangeHalf;
        gestureDirectionRange[(int)Gesture.Directions.Up + 1] = 0 + verticalAngleRangeHalf;

        gestureDirectionRange[(int)Gesture.Directions.UpRight] = 45 - diagonalAngleRangeHalf;
        gestureDirectionRange[(int)Gesture.Directions.UpRight + 1] = 45 + diagonalAngleRangeHalf;

        gestureDirectionRange[(int)Gesture.Directions.Right] = 90 - horizontalAngleRangeHalf;
        gestureDirectionRange[(int)Gesture.Directions.Right + 1] = 90 + horizontalAngleRangeHalf;

        gestureDirectionRange[(int)Gesture.Directions.DownRight] = 135 - diagonalAngleRangeHalf;
        gestureDirectionRange[(int)Gesture.Directions.DownRight + 1] = 135 + diagonalAngleRangeHalf;

        gestureDirectionRange[(int)Gesture.Directions.Down] = 180 - verticalAngleRangeHalf;
        gestureDirectionRange[(int)Gesture.Directions.Down + 1] = -180 + verticalAngleRangeHalf;

        gestureDirectionRange[(int)Gesture.Directions.DownLeft] = -135 - diagonalAngleRangeHalf;
        gestureDirectionRange[(int)Gesture.Directions.DownLeft + 1] = -135 + diagonalAngleRangeHalf;

        gestureDirectionRange[(int)Gesture.Directions.Left] = -90 - horizontalAngleRangeHalf;
        gestureDirectionRange[(int)Gesture.Directions.Left + 1] = -90 + horizontalAngleRangeHalf;

        gestureDirectionRange[(int)Gesture.Directions.UpLeft] = -45 - diagonalAngleRangeHalf;
        gestureDirectionRange[(int)Gesture.Directions.UpLeft + 1] = -45 + diagonalAngleRangeHalf;

        Gesture.DirectionRange = gestureDirectionRange;
    }

    // Must be called after Start method
    public void SetInputInterface(GestureProcessor processor)
    {
        if (_gestures != null)
        {
            for (int i = 0; i < _gestures.Length; i++)
            {
                _gestures[i].SetProcessor(processor);
            }
        }
    }

    // Use this for initialization
    public void Awake()
    {
        Init();

        _gestures = new Gesture[MAX_TOUCHES];
        for (int i = 0; i < MAX_TOUCHES; i++)
        {
            _gestures[i] = new Gesture();
            _gestures[i].SetProcessor(GetComponent<GestureProcessor>());
        }
    }

    // Update is called once per frame
    public void Update()
    {
#if UNITY_EDITOR || !(UNITY_IPHONE || UNITY_ANDROID)
        HandleMouse();
#else
		HandleMultiTouch();
#endif
    }

    // Handle gesture from mouse
    private void HandleMouse()
    {
        if (Input.GetButton("Fire1"))
        {
            Vector2 position;
            position.x = Input.mousePosition.x;
            position.y = Input.mousePosition.y;

            if (!_gestures[0].HasStarted)
            {
                _gestures[0].Start(position);       //try to start it
            }
            else
            {
                _gestures[0].Process(position);
            }
            _pressed = true;
        }
        else if (_pressed == true)
        {
            _pressed = false;
            _gestures[0].Finish();
        }
    }

    // Handle gesture from multitouch
    private void HandleMultiTouch()
    {
        if (Input.touchCount > 0) // Do we have any input?
        {
            int id = 0;
            Touch touch;

            //Ignores inputs above the count limit (MAX_TOUCHES)
            for (int i = 0; (i < Input.touchCount) && (i < MAX_TOUCHES); i++)
            {
                touch = Input.touches[i];
                id = touch.fingerId;

                //TODO: Find a better solution for this
                //Added the "if" to avoid System.IndexOutOfRangeException when accumulating touch events
                //and the finger id returns a value greater than MAX_TOUCHES
                if (id < MAX_TOUCHES)
                {
                    if (touch.phase == TouchPhase.Began)
                    {
                        //textMultitouch.guiText.text = "Began";
						if (!_gestures[id].HasStarted)
						{						
                        	_gestures[id].Start(touch.position);
						}
                    }
                    else if (touch.phase == TouchPhase.Moved)
                    {
                        //textMultitouch.guiText.text = "Moved";
						if (!_gestures[id].HasStarted)
						{
							_gestures[id].Start(touch.position);
						}
						else
						{
							_gestures[id].Process(touch);
						}
                    }
                    else if (touch.phase != TouchPhase.Stationary)
                    {
                        //textMultitouch.guiText.text = "Finish";
						if (_gestures[id].HasStarted)
						{
                        	_gestures[id].Finish();
						}
                    }
                }
            }
        }
    }

    //The method that will run when a gesture event occours
    private void handleGesture(object sender, Gesture.Directions e)
    {
        GameObject text = GameObject.Find("Debug");
        text.guiText.text = "direction: " + e;
    }
}
