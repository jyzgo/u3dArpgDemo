using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UIImageButton))]
public class UITabButton : MonoBehaviour
{
    private UIImageButton _imageButton;
    public UIImageButton ImageButton
    { 
        get
        {
            if (null == _imageButton)
            {
                _imageButton = GetComponent<UIImageButton>();
            }
            return _imageButton;
        }
    }

    private bool _isSelected = false;

    public UISprite selectionSprite;

    public int labelFontSize;

    public int labelFontSizeSelection;

    void Awake()
    {
        if (!_isSelected)
        {
            selectionSprite.gameObject.SetActive(false);
        }
    }

    void Start()
    {
    }

    void OnEnable()
    {
        
    }

    public void GodSelectMe()
    {
        _isSelected = true;
        if (null != ImageButton.label)
        {
            ImageButton.label.transform.localScale = new Vector3(labelFontSizeSelection,
                labelFontSizeSelection, 1);
        }
        selectionSprite.gameObject.SetActive(true);
    }

    public void GodIgnoreMe()
    {
        _isSelected = false;
        if (null != ImageButton.label)
        {
            ImageButton.label.transform.localScale = new Vector3(labelFontSize,
                labelFontSize, 1);
        }
        selectionSprite.gameObject.SetActive(false);
    }
}
