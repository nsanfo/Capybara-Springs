using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemToggleHandler : MonoBehaviour
{
    [Header("Decor Items")]
    public GameObject[] decorItems;

    [Header("Amenity Items")]
    public GameObject[] amenityItems;

    [Header("Item Template")]
    public GameObject itemTemplate;

    private ToggleGroup toggleGroup;
    private GameObject currentItem;

    void Start()
    {
        toggleGroup = GetComponent<ToggleGroup>();
        PopulateAmenityPopOut();
    }

    public void PopulateDecorPopOut()
    {
        CleanUpContent();
        for (int i = 0; i < decorItems.Length; i++)
        {
            InstantiateItemUI(decorItems[i]);
        }
    }

    public void PopulateAmenityPopOut()
    {
        CleanUpContent();
        for (int i = 0; i < amenityItems.Length; i++)
        {
            InstantiateItemUI(amenityItems[i]);
        }
    }

    private void CleanUpContent()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void InstantiateItemUI(GameObject itemObject)
    {
        // Instantiate decor item
        GameObject prefab = Instantiate(itemObject);
        ItemInformation itemInfo = prefab.GetComponent<ItemInformation>();

        // Set item prefab to content
        GameObject item = Instantiate(itemTemplate);
        item.transform.parent = transform;
        item.GetComponent<RectTransform>().localScale = Vector3.one;
        item.GetComponent<RectTransform>().localPosition = Vector3.zero;

        // Update item display
        UpdateItemDisplay itemDisplay = item.GetComponent<UpdateItemDisplay>();
        itemDisplay.UpdateText(itemInfo.GetFormattedCost());
        itemDisplay.UpdateImage(prefab.GetComponent<CameraTextureCapture>().RetrieveTexture());

        // Destroy prefab
        Destroy(prefab);
    }

    public void UpdateCurrentItem(GameObject currentItem)
    {
        if (toggleGroup.ActiveToggles().FirstOrDefault() == null)
        {
            //currentBuildType = BuildType.None;
        }
        else
        {
            //currentBuildType = buttonBuildType.buildType;
        }
    }

    public void AllTogglesOff()
    {
        toggleGroup.SetAllTogglesOff();
    }
}
