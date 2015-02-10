//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2013 Tasharen Entertainment
// Copyright @ caizilong. if children compoments contains UILabel, change gray color when isEnabled is false.
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Sample script showing how easy it is to implement a standard button that swaps sprites.
/// </summary>

[AddComponentMenu("NGUI/UI/Image Button")]
public class UIImageButton : MonoBehaviour
{
	public UISprite target;
	public string normalSprite;
	public string hoverSprite;
	public string pressedSprite;
	public string disabledSprite;
    public UILabel label;

    private Color _labelColor;

	public bool isEnabled
	{
		get
		{
			Collider col = collider;
			return col && col.enabled;
		}
		set
		{
			Collider col = collider;
            if (col)
            {
                if (col.enabled != value)
                {
                    col.enabled = value;
                }
            }
            UpdateImage();
		}
	}

    void Awake()
    {
        if (null != label) _labelColor = label.color;
    }

	void OnEnable ()
	{
		if (target == null) target = GetComponentInChildren<UISprite>();
	}
	
	void UpdateImage()
	{
		if (target != null)
		{
			if (isEnabled)
			{
				target.spriteName = UICamera.IsHighlighted(gameObject) ? hoverSprite : normalSprite;
                if (null != label)
                {
                    if (_labelColor == Color.clear)
                    {
                        _labelColor = label.color;
                    }
                    label.color = _labelColor;
                } 
			}
			else
			{
				target.spriteName = disabledSprite;
                //use gray color to display disabled button label.
                if(null != label)
                {
                    label.color = Color.gray;
                }
			}
			target.MakePixelPerfect();
		}
	}

	void OnHover (bool isOver)
	{
		if (isEnabled && target != null)
		{
			target.spriteName = isOver ? hoverSprite : normalSprite;
			target.MakePixelPerfect();
		}
	}

	void OnPress (bool pressed)
	{
		if (pressed)
		{
			target.spriteName = pressedSprite;
			target.MakePixelPerfect();
		}
		else UpdateImage();
	}
}
