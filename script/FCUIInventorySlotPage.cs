using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class FCUIInventorySlotPage : MonoBehaviour
{
    //just show in inspector
    public bool isPrimary;

    private bool _isPrimary;
    public bool IsPrimary
    {
        set 
        { 
            _isPrimary = value;
            isPrimary = _isPrimary;
        }
        get { return _isPrimary; }
    }

    private Vector3 _basePosition;
    public Vector3 BasePosition 
    {
        set 
        {
            _basePosition = value;
            _upperPosition = new Vector2(BasePosition.x, BasePosition.y + BoxCollider.size.y);
            _lowerPosition = new Vector2(BasePosition.x, BasePosition.y - BoxCollider.size.y);
        }
        get { return _basePosition; }
    }

    private bool _pageDirty = true;
    public bool pageDirty
    {
        set { _pageDirty = value; }
    }

    public int pageIndex;//just display in inspector
    private int _pageIndex = -1;
    public int PageIndex
    {
        set
        {
            if (_pageIndex != value)
            {
                _pageIndex = value;
                _pageDirty = true;
            }
            pageIndex = _pageIndex;
        }
        get
        {
            return _pageIndex;
        }
    }

    public BoxCollider BoxCollider;

    public float DragDistance 
    {
        get 
        {
            return transform.localPosition.y - BasePosition.y;
        }
    }

    public float DragPercent
    {
        get 
        {
            return (float)((decimal)DragDistance / (decimal)BoxCollider.size.y);
        }
    }

    private Vector2 _upperPosition;
    private Vector2 _lowerPosition;

    private FCUIInventorySlot[] _itemsSlots;
    public FCUIInventorySlot[] ItemsSlots
    {
        get
        {
            if(null == _itemsSlots)
            {
                List<FCUIInventorySlot> list = new List<FCUIInventorySlot>();
                foreach(Transform tf in transform)
                {
                    FCUIInventorySlot slot = tf.GetComponent<FCUIInventorySlot>();
                    slot.IsShowIconRightNow = false;
                    list.Add(slot);
                }
                list.Sort(delegate (FCUIInventorySlot slotA, FCUIInventorySlot slotB)
                    {
                        return string.Compare(slotA.gameObject.name, slotB.gameObject.name);
                    }
                );
                _itemsSlots = list.ToArray();
            }
            return _itemsSlots;
        }
    }

    private int _slotsCount;
    public int SlotsCount
    {
        get 
        {
            if (_slotsCount == 0)
                _slotsCount = ItemsSlots.Length;
            return _slotsCount;
        }
    }

    private bool _isScrolling;
    public bool IsScrolling
    {
        get { return _isScrolling; }
    }

    public void ChangeBasePositionByDirection(DragDirection direction)
    {
        if (direction == DragDirection.Upper)
            BasePosition = _upperPosition;
        else if (direction == DragDirection.Lower)
            BasePosition = _lowerPosition;
    }

    public void ForceSelect(ItemInventory newSelection)
    {
        foreach (FCUIInventorySlot slot in ItemsSlots)
        {
            if (newSelection == slot.Item)
            {
                slot.GodSelectedMe();
            }
            else
            {
                slot.GodIgnoreMe();
            }
        }
    }

    public ItemInventory OnClick(ItemInventory lastSelectedItem)
    {
        ItemInventory selectedItem = null;
        foreach(FCUIInventorySlot slot in ItemsSlots)
        {
            if (null == selectedItem && slot.TestTouch())
            {
                if (null != slot.Item && slot.Item != lastSelectedItem)
                {
                    slot.GodSelectedMe();
                    selectedItem = slot.Item;
                }
                else
                {
                    slot.GodIgnoreMe();
					selectedItem = null;
                }
            }
            else
            {
                slot.GodIgnoreMe();
            }
        }
        return selectedItem;
    }

    public void Refresh(ItemInventory selectedItem, List<ItemInventory> itemsList)
    {
        //if not in active. do nothing!(fix it sooner or later by caizilong)
        if (!_pageDirty || !gameObject.activeInHierarchy)//need not refresh
        {
            return;
        }
        int fromIndex = PageIndex * ItemsSlots.Length;
        List<FCUIInventorySlot> noIconSlots = new List<FCUIInventorySlot>();
        for (int i = 0, count = SlotsCount; i < count; ++i)
        {
            FCUIInventorySlot slot = ItemsSlots[i];
            if (itemsList.Count > i + fromIndex && fromIndex + i >= 0)
            {
                ItemInventory item = itemsList[fromIndex + i];
                slot.Item = item;
                if (!slot.RefreshComplete)
                {
                    noIconSlots.Add(slot);
                }
                slot.gameObject.SetActive(true);
                if (item == selectedItem)
                {
                    slot.GodSelectedMe();
                }
                else
                {
                    slot.GodIgnoreMe();
                }
            }
            else
            {
                slot.gameObject.SetActive(false);
                slot.Item = null;
                slot.GodIgnoreMe();
            }
        }
        StartCoroutine(StepRefreshIcons(noIconSlots));
        //Debug.Log(gameObject.name + ":refreshed!");
        _pageDirty = false;
    }

    IEnumerator StepRefreshIcons(List<FCUIInventorySlot> noIconSlots)
    {
        int index = 0;
        foreach (FCUIInventorySlot slot in noIconSlots)
        {
            slot.RefreshIcon();
            if (++index % 4 == 0)
            {
                yield return null;
            }
        }
    }

    void Start()
    {

    }

    void Update()
    {
    }

    public void TweenBack()
    {
        float deltaY = BasePosition.y - transform.localPosition.y;
        if (0 != deltaY)
        {
            StopAllCoroutines();
            _isScrolling = true;
            StartCoroutine(StepTweenDestinationPosition(deltaY));
        }
        else
        {
            transform.localPosition = BasePosition;
            _isScrolling = false;
        }
    }

    IEnumerator StepTweenDestinationPosition(float distance)
    {
        float tweenRation = 0.16666f;
        float leftDistance = distance;
        while (Mathf.Abs(leftDistance) > 1)
        {
            transform.localPosition += new Vector3(0, leftDistance * tweenRation);
            leftDistance = leftDistance * (1 - tweenRation);
            if (Mathf.Abs(leftDistance) < 10)
            {
                _isScrolling = false;
            }
            yield return null;
        }
        transform.localPosition = new Vector3(BasePosition.x, BasePosition.y, BasePosition.z);
        _isScrolling = false;
    }
}


public enum DragDirection
{ 
    Back = 0,
    Upper,
    Lower
}