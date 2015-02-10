using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// vertical list
/// </summary>
public class FCUIMenuSelectionList : MonoBehaviour
{
    private FCUIMenuSelection[] _list;
    public FCUIMenuSelection[] List
    {
        set { _list = value; refresh(); }
        get { return _list; }
    }

    /// <summary>
    /// original to clone
    /// </summary>
    public GameObject SelectionMenuItem;

    /// <summary>
    /// resizebel board
    /// </summary>
    public GameObject SelectionBoard;

    public GameObject offset;

    private List<GameObject> _cacheList = new List<GameObject>();

    private Dictionary<GameObject, FCUIMenuSelection> _menuSelectionMapping;

    private float _startLayoutY;

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void ShowAt(Vector3 snapGlobalPosition)
    {
        gameObject.SetActive(true);
        offset.transform.position = new Vector3(snapGlobalPosition.x, snapGlobalPosition.y, 0);
        offset.transform.localPosition -= new Vector3(-SelectionBoard.transform.localScale.x / 2,
            SelectionBoard.transform.localScale.y / 2,
            offset.transform.localPosition.z);

        //layout
        Vector3 offsetPosition = UICamera.currentCamera.WorldToScreenPoint(offset.transform.position);
        float deltaY = offsetPosition.y - SelectionBoard.transform.localScale.y / 2 - SelectionMenuItem.transform.localScale.y;
        if (deltaY < 0)
        {
            offset.transform.localPosition -= new Vector3(0, deltaY, 0);
        }
    }

    void Start()
    {
        SelectionMenuItem.SetActive(false);
    }

    void refresh()
    {
        if (null == _menuSelectionMapping)
            _menuSelectionMapping = new Dictionary<GameObject, FCUIMenuSelection>();
        recycleSelectionItems();
        float itemHeight = SelectionMenuItem.transform.localScale.y * 1.1f;
        float totalHeight = _list.Length * itemHeight;
        _startLayoutY = (itemHeight - totalHeight) / 2;
        int index = 0;
        foreach (FCUIMenuSelection menu in _list)
        {
            GameObject menuItem = CloneMenuSelectionItem(menu.Label, _startLayoutY + index * itemHeight);
            UIEventListener.Get(menuItem).onClick = OnClickedMenuItem;
            UIEventListener.Get(menuItem).onPress = OnPressMenuItem;
            _menuSelectionMapping[menuItem] = menu;
            index++;
        }
        SelectionBoard.transform.localScale = new Vector3(SelectionBoard.transform.localScale.x, totalHeight + itemHeight, 1);
    }

    void OnPressMenuItem(GameObject go, bool isPress)
    {
        UILabel label = go.GetComponent<UILabel>();
        label.color = isPress ? Color.yellow : Color.white;
    }

    void OnClickedMenuItem(GameObject go)
    {
        FCUIMenuSelection selection = _menuSelectionMapping[go];
        selection.SelectionHandler(selection.Label);
        Hide();
    }

    GameObject CloneMenuSelectionItem(string text, float layoutY)
    {
        GameObject menuItem = null;
        if (_cacheList.Count > 0)
        {
            menuItem = _cacheList[_cacheList.Count - 1];
            _cacheList.RemoveAt(_cacheList.Count - 1);
        }
        else
        {
            menuItem = GameObject.Instantiate(SelectionMenuItem) as GameObject;
        }
        menuItem.GetComponent<UILabel>().text = text;
        menuItem.transform.parent = offset.transform;
        menuItem.transform.localPosition = new Vector3(0, layoutY, 0);
        menuItem.transform.localScale = SelectionMenuItem.transform.localScale;
        menuItem.name = text;
        menuItem.SetActive(true);
        return menuItem;
    }

    void recycleSelectionItems()
    {
        List<GameObject> removeGoList = new List<GameObject>();
        foreach (GameObject go in _menuSelectionMapping.Keys)
        {
            removeGoList.Add(go);
        }
        foreach (GameObject go in removeGoList)
        {
            recycleItem(go);
            _menuSelectionMapping.Remove(go);
        }
    }

    void recycleItem(GameObject menuItem)
    {
        UIEventListener.Get(menuItem).onClick = null;
        menuItem.SetActive(false);
        _cacheList.Add(menuItem);
    }
}

public class FCUIMenuSelection
{
    public object arg;
    public string Label;
    public delegate void SelectionHandlerDelegate(string label);
    public SelectionHandlerDelegate SelectionHandler;

    public FCUIMenuSelection(string label, SelectionHandlerDelegate callback)
    {
        this.Label = label;
        this.SelectionHandler = callback;
    }
}
