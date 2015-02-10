using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class FCUIInventorySlotPageController : MonoBehaviour
{
    public delegate void ItemSelectionDelegate(ItemInventory item);
    public ItemSelectionDelegate ItemSelectionHandler;

    public FCUIInventorySlotPageProgress prograssBar;
    public FCUIInventorySlotPage mainPage;

    private FCUIInventorySlotPage _secondPage;
    public FCUIInventorySlotPage SecondPage
    {
        get
        {
            if (null == _secondPage)
            {
                _secondPage = NGUITools.AddChild(gameObject, mainPage.gameObject).GetComponent<FCUIInventorySlotPage>();
                _secondPage.IsPrimary = false;
            }
            return _secondPage;
        }
        set 
        {
            _secondPage = value;
        }
    }

    private ItemInventory _selectedItem;
    public ItemInventory SelectedItem
    {
        set
        { 
            _selectedItem = value;
            if (null != ItemSelectionHandler)
            {
                ItemSelectionHandler(_selectedItem);
            }
        }
        get { return _selectedItem; }
    }

    private int _currentPageIndex;

    private float _lastDragStrength;

    private float _lastDragTime;

    private float _totalDragOffset;

    private Vector3 _mainBasePosition;

    private List<ItemInventory> _itemsList;

    private int PagesCount
    {
        get
        {
            int itemsCount = _itemsList.Count;
            return Mathf.CeilToInt((float)((decimal)itemsCount / (decimal)mainPage.SlotsCount));
        }
    }

    void OnGUI()
    { 
		if(null != _itemsList)
		{
			if(GUI.Button(new Rect(Screen.width - 100, Screen.height - 30, 100, 30),
			              new GUIContent("page:" + (_currentPageIndex + 1) + "/" + PagesCount)))
			{
				mainPage.Refresh(_selectedItem, _itemsList);
            }
        }
    }

    void Start()
    {
        _currentPageIndex = 0;

        _mainBasePosition = mainPage.BasePosition = mainPage.transform.localPosition;
        mainPage.PageIndex = _currentPageIndex;
        mainPage.IsPrimary = true;
        prograssBar.SepPagePercent(_currentPageIndex, PagesCount);
    }

    void OnDrag(Vector2 delta)
    {
        float ration = UIRoot.GetPixelSizeAdjustment(gameObject);
        float offsetY = delta.y * ration;
        _lastDragStrength = offsetY;
        _totalDragOffset += offsetY;
        mainPage.transform.localPosition += new Vector3(0, offsetY, 0);
        _lastDragTime = UnityEngine.Time.realtimeSinceStartup;
    }

    void OnPress(bool press)
    {
        if (!press)
        {
            if (_lastDragStrength != 0 || _totalDragOffset != 0)//touch
            {
                DragDirection direction = CalculateDragDirection();
                if (direction != DragDirection.Back)
                {
                    ExchangePrimary(direction);
                    mainPage.pageDirty = true;
                }
                mainPage.TweenBack();
                mainPage.Refresh(_selectedItem, _itemsList);
            }
            else//drag
            {
                mainPage.TweenBack();
            }
            _lastDragStrength = 0;
            _totalDragOffset = 0;
        }
        else
        {
            mainPage.StopAllCoroutines();
        }
    }

    public void ForceSelectItem(ItemInventory item)
    {
        mainPage.ForceSelect(item);
        SelectedItem = item;
    }

    void OnClick()
    {
        if (!mainPage.IsScrolling)
        {
            SelectedItem = mainPage.OnClick(SelectedItem);
        }
    }

    void LateUpdate()
    {
        SecondFollowPrimary();
    }

    void SecondFollowPrimary()
    {
        if (mainPage.DragPercent > 0)
        {
            SecondPage.BasePosition = mainPage.BasePosition -
                new Vector3(0, mainPage.BoxCollider.size.y, 0);
            SecondPage.PageIndex = _currentPageIndex + 1;
        }
        else if (mainPage.DragPercent < 0)
        {
            SecondPage.BasePosition = mainPage.BasePosition +
                new Vector3(0, mainPage.BoxCollider.size.y, 0);
            SecondPage.PageIndex = _currentPageIndex - 1;
        }
        else
        {
            SecondPage.BasePosition = mainPage.BasePosition -
                new Vector3(0, mainPage.BoxCollider.size.y, 0);
            SecondPage.PageIndex = -1;
        }
        if (_itemsList != null)
        {
            SecondPage.Refresh(SelectedItem, _itemsList);
        }

        if (SecondPage.PageIndex < 0 || SecondPage.PageIndex + 1 > PagesCount)
        {
            SecondPage.transform.localPosition = new Vector3(1000, 0, 0);
        }
        else
        {
            SecondPage.transform.localPosition = new Vector3(SecondPage.BasePosition.x,
                SecondPage.BasePosition.y + mainPage.DragDistance, SecondPage.BasePosition.z);
        }
    }

    void ExchangePrimary(DragDirection direction)
    {
        FCUIInventorySlotPage temp = mainPage;
        mainPage = SecondPage;
        SecondPage = temp;
        mainPage.IsPrimary = true;
        SecondPage.IsPrimary = false;
        mainPage.BasePosition = _mainBasePosition;
        SecondPage.ChangeBasePositionByDirection(direction);
        if (direction == DragDirection.Upper)
        {
            _currentPageIndex++;
        }
        else if (direction == DragDirection.Lower)
        {
            _currentPageIndex--;
        }
        mainPage.PageIndex = _currentPageIndex;
        prograssBar.SepPagePercent(_currentPageIndex, PagesCount);
    }

    public void Show(List<ItemInventory> filterList, bool backToFirstPage = true)
    {
        _itemsList = filterList;
        mainPage.PageIndex = _currentPageIndex = backToFirstPage ? 0 : _currentPageIndex;
        mainPage.pageDirty = true;
        RefreshMainPage();
        prograssBar.SepPagePercent(_currentPageIndex, PagesCount);
    }

    public void RefreshMainPage()
    {
        mainPage.pageDirty = true;
        mainPage.Refresh(_selectedItem, _itemsList);
    }

    public void ClearSelection()
    {
        _selectedItem = null;
    }

    public DragDirection CalculateDragDirection()
    {
        int minDragOffset = 100;
        int strengthThreshold = 1;
        float deltaTime = UnityEngine.Time.realtimeSinceStartup - _lastDragTime;
        if (deltaTime > 0.1f)
            _lastDragStrength = 0;

        DragDirection direction = DragDirection.Back;
        float DragPercent = _totalDragOffset / GetComponent<BoxCollider>().size.y;

        if (_lastDragStrength >= strengthThreshold && Mathf.Abs(_totalDragOffset) > minDragOffset)
        {
            if (DragPercent < 0)
            {
                direction = DragDirection.Back;
            }
            else
            {
                direction = DragDirection.Upper;
            }
        }
        else if (_lastDragStrength <= -strengthThreshold && Mathf.Abs(_totalDragOffset) > minDragOffset)
        {
            if (DragPercent > 0)
            {
                direction = DragDirection.Back;
            }
            else
            {
                direction = DragDirection.Lower;
            }
        }
        else
        {
            if (DragPercent > 0.5f)
            {
                direction = DragDirection.Upper;
            }
            else if (DragPercent < -0.5f)
            {
                direction = DragDirection.Lower;
            }
        }
        if (direction == DragDirection.Lower && _currentPageIndex == 0)
            direction = DragDirection.Back;
        else if (direction == DragDirection.Upper && _currentPageIndex + 1 >= PagesCount)
            direction = DragDirection.Back;
        return direction;
    }
}
