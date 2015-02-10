using UnityEngine;
using System.Collections;

/// <summary>
/// Receive and process gesture events. Should be attached to the same game object of GestureController
/// </summary>
public class GestureProcessor : MonoBehaviour
{
    //only touch points inside the zone will be processed. Zone is in screen space.
    protected Rect _inputZone = new Rect(0, 0, Screen.width, Screen.height);

    public Rect InputZone
    {
        get { return _inputZone; }
    }

    void Start()
    {
        SetInputZone();
    }

    public virtual void SetInputZone()
    {
    }
	
	public virtual void ProcessGesture(Gesture.GestureData data)
    {

    }

    public virtual void ProcessGesture(Gesture.GestureData data, float delta)
    {

    }
	
	public virtual bool ProcessGesture(Gesture.Directions dir, float delta)
    {
		return true;
    }
}
