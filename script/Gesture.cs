using UnityEngine;
using System.Collections;

public class Gesture
{
    public enum Directions
    {
        Up = 0,
        UpRight,
        Right,
        DownRight,
        Down,
        DownLeft,
        Left,
        UpLeft,
        COUNT,
        None
    }

    public struct GestureData
    {
        public Directions direction;
        public float angle;

        public GestureData(Directions direction, float angle, float distance)
        {
            this.direction = direction;
            this.angle = angle;
        }
    }

    private Directions _currentDirection;
    private Vector2 _initialPosition;
    private GestureProcessor _processor;

    // Tweak minimum movement amplitude considered a gesture for all gestures.
    // will be overwrite by GestureController
    private static float _minimumAmplitude = 100.0f;
    public static float MinimumAmplitude
    {
        get { return _minimumAmplitude; }
        set { _minimumAmplitude = value; }
    }

    private static float[] _directionRange;
    public static float[] DirectionRange
    {
        get { return _directionRange; }
        set { _directionRange = value; }
    }

    private bool _started;      //if a gesture begins. Only touches inside the input zone are taken as effective.
    public bool HasStarted
    {
        get { return _started; }
    }

    public Gesture()
    {
        _currentDirection = Directions.None;
    }

    public void SetProcessor(GestureProcessor processor)
    {
        _processor = processor;
    }

    public void Start(Vector2 position)
    {
        if (_processor.InputZone.Contains(position))
        {
            _initialPosition = position;
            _started = true;
        }
    }

    public void Process(Vector2 position)
    {
        if (!_processor.InputZone.Contains(position))
        {
            Finish();
        }
        else if (_currentDirection == Directions.None)
        {
            float distance = Vector2.Distance(_initialPosition, position);
            if (distance > _minimumAmplitude)
            {
                GestureData data = GetGestureData(_initialPosition, position);
                _currentDirection = data.direction;

                // handle gesture via interface
                if (_processor != null)
                {
                    _processor.ProcessGesture(data, distance);
                }
            }
        }
    }
	
	public void Process(Touch touch)
    {
        if (!_processor.InputZone.Contains(touch.position))
        {
            Finish();
        }
        else
        {
			float distance = Vector2.Distance(_initialPosition, touch.position);
			
            if (distance > _minimumAmplitude)
            {
				GestureData data = GetGestureData(_initialPosition, touch.position);
				
				
                // handle gesture via interface
                if (_processor != null)
                {
                    if (_processor.ProcessGesture(data.direction, distance))
					{
						_initialPosition = touch.position;
					}
                }
            }
        }
    }

    public void Finish()
    {
        _currentDirection = Directions.None;
        _started = false;
    }

    private GestureData GetGestureData(Vector2 p0, Vector2 p1)
    {
        // Due to Camera y-base reference y will be considered the reference for angles
        float angle = Mathf.Atan2(p1.x - p0.x, p1.y - p0.y) * Mathf.Rad2Deg;

        GestureData data;
        data.direction = Directions.None;
        data.angle = angle;

        // angle range: [-180, 180] degrees
        if (angle > _directionRange[(int)Directions.Up] && angle <= _directionRange[(int)Directions.Up + 1])
        {
            data.direction = Directions.Up;
        }
        else if (angle > _directionRange[(int)Directions.UpRight] && angle <= _directionRange[(int)Directions.UpRight + 1])
        {
            data.direction = Directions.UpRight;
        }
        else if (angle > _directionRange[(int)Directions.Right] && angle <= _directionRange[(int)Directions.Right + 1])
        {
            data.direction = Directions.Right;
        }
        else if (angle > _directionRange[(int)Directions.DownRight] && angle <= _directionRange[(int)Directions.DownRight + 1])
        {
            data.direction = Directions.DownRight;
        }
        else if (angle > _directionRange[(int)Directions.Down] && angle <= 180 || angle > -180 && angle < _directionRange[(int)Directions.Down + 1])
        {
            data.direction = Directions.Down;
        }
        else if (angle > _directionRange[(int)Directions.DownLeft] && angle <= _directionRange[(int)Directions.DownLeft + 1])
        {
            data.direction = Directions.DownLeft;
        }
        else if (angle > _directionRange[(int)Directions.Left] && angle <= _directionRange[(int)Directions.Left + 1])
        {
            data.direction = Directions.Left;
        }
        else if (angle > _directionRange[(int)Directions.UpLeft] && angle <= _directionRange[(int)Directions.UpLeft + 1])
        {
            data.direction = Directions.UpLeft;
        }

        return data;
    }
}