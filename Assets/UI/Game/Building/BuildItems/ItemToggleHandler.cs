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

    [Header("Player Building Object")]
    public GameObject playerBuilding;
    private AmenitiesBuilder amenitiesBuilder;
    private DecorBuilder decorBuilder;

    private ToggleGroup toggleGroup;

    void Start()
    {
        amenitiesBuilder = playerBuilding.GetComponent<AmenitiesBuilder>();
        decorBuilder = playerBuilding.GetComponent<DecorBuilder>();

        toggleGroup = GetComponent<ToggleGroup>();
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

        // Update item building
        ItemBuilding itemBuilding = item.GetComponent<ItemBuilding>();
        itemBuilding.amenitiesBuilder = amenitiesBuilder;
        itemBuilding.decorBuilder = decorBuilder;
        itemBuilding.buildType = itemInfo.itemType;
        itemBuilding.itemBlueprint = itemInfo.itemBlueprint;

        // Destroy prefab
        Destroy(prefab);
    }

    public void AllTogglesOff()
    {
        toggleGroup.SetAllTogglesOff();
    }
}
