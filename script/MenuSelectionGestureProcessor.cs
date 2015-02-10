using UnityEngine;
using System.Collections;

/// <summary>
/// Receive and process gesture events. Should be attached to the same game object of GestureController
/// </summary>
public class MenuSelectionGestureProcessor : GestureProcessor
{
    public override void SetInputZone()
    {
        float screenRatio = 1f * Screen.width / Screen.height;

        int index;

        if (screenRatio < 1.34f)    // 4:3
        {
            index = 0;
        }
        else if (screenRatio < 1.51f)  // 3:2
        {
            index = 1;
        }
        else if (screenRatio < 1.78f)  //16:9
        {
            index = 2;
        }
        else //others
        {
            index = 2;
        }

        float[,] pos =
        {
            { 0.1f, 0.1f, 0.680f, 0.884f },
            { 0.1f, 0.1f, 0.680f, 0.884f },
            { 0.1f, 0.1f, 0.578f, 0.881f }
        };

        _inputZone = new Rect(pos[index, 0] * Screen.width, pos[index, 1] * Screen.height, 
            (pos[index, 2] - pos[index, 0]) * Screen.width, (pos[index, 3] - pos[index, 1]) * Screen.height);

        //Debug.Log("Input zone = " + _inputZone);
    }

    public override bool ProcessGesture(Gesture.Directions direction, float delta)
    {
        if (direction == Gesture.Directions.Left || direction == Gesture.Directions.DownLeft || direction == Gesture.Directions.UpLeft)
        {
            this.GetComponent<CharacterSelector>().ShowPreviousSelection();
        }
        else if (direction == Gesture.Directions.Right || direction == Gesture.Directions.DownRight || direction == Gesture.Directions.UpRight)
        {
            this.GetComponent<CharacterSelector>().ShowNextSelection();
        }
		
		return true;
    }
	
	public override void ProcessGesture(Gesture.GestureData data, float distance)
    {
        if (data.direction == Gesture.Directions.Left || data.direction == Gesture.Directions.DownLeft || data.direction == Gesture.Directions.UpLeft)
        {
            this.GetComponent<CharacterSelector>().ShowPreviousSelection();
        }
        else if (data.direction == Gesture.Directions.Right || data.direction == Gesture.Directions.DownRight || data.direction == Gesture.Directions.UpRight)
        {
            this.GetComponent<CharacterSelector>().ShowNextSelection();
        }
    }
}
