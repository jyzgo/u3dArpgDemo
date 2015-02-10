using UnityEngine;
using System.Collections;
using System.Collections.Generic;
 
public class NGUICoverFlow : MonoBehaviour 
{
    protected List<GameObject> photos = new List<GameObject>();
    protected int photosCount;
 
    protected int currentIndex;
    private float MARGIN_X = 3f;  //Internal between planes
    private float ITEM_W = 2f;   //plane size 10M
 
    public delegate void OnCoverChanged(int index);
	public OnCoverChanged _OnCoverChanged;
	

	void Start()
    {
    }
	
	public virtual void Initialize()
	{
		currentIndex = -1;
		
		LoadImages();	
	}
	
	public virtual void Clear()
	{
		photos.Clear();
	}
	
	protected virtual void OnCoverFlowed(int index)
	{
		if (_OnCoverChanged != null)
		{
			_OnCoverChanged(index);
		}
	}
	
	protected virtual void LoadImages()
	{        
		MoveSlider(photos.Count / 2);
    }
 
    public void MoveSlider(int id)
    {
        if (currentIndex == id)
            return;
		
		currentIndex = id;
		
		OnCoverFlowed(currentIndex);
 
        for (int i = 0; i < photosCount; i++)
        {
			// Parent's world position
            float targetX = transform.position.x;
            float targetZ = transform.position.z;
            float targetRot = 0f;
 
            targetX += MARGIN_X * (i - id);
			
            //left slides
            if (i < id)
            {
                targetX -= ITEM_W * 0.6f;
                targetZ = ITEM_W * 3f / 4;
                targetRot = -30f;
 
            }
            //right slides
            else if (i > id)
            {
                targetX += ITEM_W * 0.6f;
                targetZ = ITEM_W * 3f / 4;
                targetRot = 30f;
            }
            else
            {
                targetX += 0f;
                targetZ = 0f;
                targetRot = 0f;
            }
 
            GameObject photo = photos[i];
            float ys = photo.transform.position.y;
            Vector3 ea = photo.transform.eulerAngles;
			
            iTween.MoveTo(photo, new Vector3(targetX, ys, targetZ), 1f);
            iTween.RotateTo(photo, new Vector3(ea.x, targetRot, targetZ), 1f);
        }
    }
	
	// Flow By Slider.
    public void OnSliderChange(float value)
    {
        MoveSlider((int)(value * photosCount));
    }
	
	// Flow by Touch.
	public void FlowCover(int delta)
	{
		int val = currentIndex + delta;
		
		val = Mathf.Min(val, photos.Count-1);
		val = Mathf.Max(val, 0);
		
		if ((val>=0) && (val<photos.Count))
		{
			MoveSlider(val);
		}
	}
	
	public int GetCurrentCover()
	{
		return currentIndex;
	}
}
