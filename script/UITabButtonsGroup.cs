using System;
using System.Collections.Generic;
using UnityEngine;

public class UITabButtonsGroup : MonoBehaviour
{
    public delegate void TabButtonSelectionDelegate(int index);

    public TabButtonSelectionDelegate OnSelection;

    public UITabButton[] buttons;

    private List<UITabButton> _buttonsList;
    public List<UITabButton> ButtonsList
    {
        get
        {
            if (null == _buttonsList)
            {
                _buttonsList = new List<UITabButton>(buttons);
            }
            return _buttonsList;
        }
    }

    public void ChangeSelection(int selectIndex)
    {
        if (buttons.Length > selectIndex)
        { 
            UITabButton button = buttons[selectIndex];
            OnClickButtonToSelect(button.gameObject);
        }
    }

    void Start()
    {
        foreach (UITabButton button in buttons)
        {
            UIEventListener.Get(button.gameObject).onClick = OnClickButtonToSelect;
        }
    }

    void OnClickButtonToSelect(GameObject go)
    {
        UITabButton button = go.GetComponent<UITabButton>();
        foreach (UITabButton btn in buttons)
        {
            if (btn == button)
            {
                btn.GodSelectMe();
            }
            else
            {
                btn.GodIgnoreMe();
            }
        }
        int index = ButtonsList.IndexOf(button);
        if (null != OnSelection)
        {
            OnSelection(index);
        }
    }
}
