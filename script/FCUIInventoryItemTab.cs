using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FCUIInventoryItemTab : MonoBehaviour
{
    public GameObject selection;

    public GameObject[] tabItems;
    public float screenPercent = 0.75f;

    public GameObject all;
    public GameObject weapon;
    public GameObject armor;
    public GameObject ornament;
    public GameObject vanity;
    public GameObject material;
    public GameObject tribute;
    public GameObject tattoo;
    public GameObject other;

    private Dictionary<GameObject, InventoryFilters> _tabMapping;
    private InventoryFilters _selectedTab;

    public delegate void TabselectHandlerDelegage(List<ItemType> filterList);
    public TabselectHandlerDelegage SelectHandler;

    void Awake()
    {
        UIEventListener.Get(all).onClick = OnClickTabButton;
        UIEventListener.Get(weapon).onClick = OnClickTabButton;
        UIEventListener.Get(armor).onClick = OnClickTabButton;
        UIEventListener.Get(ornament).onClick = OnClickTabButton;
        UIEventListener.Get(vanity).onClick = OnClickTabButton;
        UIEventListener.Get(material).onClick = OnClickTabButton;
        UIEventListener.Get(tribute).onClick = OnClickTabButton;
        UIEventListener.Get(other).onClick = OnClickTabButton;
        UIEventListener.Get(tattoo).onClick = OnClickTabButton;

        _tabMapping = new Dictionary<GameObject, InventoryFilters>()
        {
            {all , InventoryFilters.all},
            {weapon , InventoryFilters.weapon},
            {armor , InventoryFilters.armor},
            {ornament , InventoryFilters.ornament},
            {vanity , InventoryFilters.vanity},
            {material , InventoryFilters.material},
            {tribute , InventoryFilters.tribute},
            {tattoo, InventoryFilters.tattoo},
            {other , InventoryFilters.other}
        };

        UIRoot root = NGUITools.FindInParents<UIRoot>(gameObject);
        float designWidth = Screen.width * root.GetPixelSizeAdjustment(Screen.height);
        int totalWidth = (int)(designWidth * screenPercent);
        int perWidth = totalWidth / tabItems.Length;
        int index = 0;
        foreach (GameObject obj in tabItems)
        {
            obj.transform.localPosition = new Vector3(index * perWidth, obj.transform.localPosition.y);
            ++index;
        }
    }

    void Start()
    {
        
    }

    void Update()
    {

    }

    void OnClickTabButton(GameObject button)
    {
        InventoryFilters filter = _tabMapping[button];
        if (_selectedTab != filter)
        {
            StopCoroutine("StepToUnderSelection");
            StartCoroutine("StepToUnderSelection", button.transform.parent.transform.localPosition);
            List<ItemType> list = new List<ItemType>();
            switch (filter)
            {
                case InventoryFilters.weapon:
                    list.Add(ItemType.weapon);
                    break;
                case InventoryFilters.armor:
                    list.Add(ItemType.armor);
                    break;
                case InventoryFilters.ornament:
                    list.Add(ItemType.ornament);
                    break;
                case InventoryFilters.vanity:
                    list.Add(ItemType.vanity);
                    break;
                case InventoryFilters.material:
                    list.Add(ItemType.material);
                    break;
                case InventoryFilters.tribute:
                    list.Add(ItemType.tribute);
                    break;
                case InventoryFilters.tattoo:
                    list.Add(ItemType.tattoo);
                    break;
                case InventoryFilters.other:
                    list.Add(ItemType.potion);
                    list.Add(ItemType.gem);
                    list.Add(ItemType.recipe);
                    break;
            }
            SelectHandler(list);
            _selectedTab = filter;
        }
    }

    IEnumerator StepToUnderSelection(Vector3 dest)
    {
        do
        {
            Vector2 delta = (dest - selection.transform.localPosition) / 3;
            selection.transform.localPosition += new Vector3(delta.x, 0, 0);
            yield return null;
        }
        while (selection.transform.localPosition != dest);
    }
}

/// <summary>
/// tab button filters type.
/// </summary>
public enum InventoryFilters
{ 
    all,
    weapon,
    armor,
    ornament,
    vanity,
    material,
    tribute,
    tattoo,
    other
}