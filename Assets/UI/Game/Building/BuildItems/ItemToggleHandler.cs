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

    private GameObject[] amenityPanels, decorPanels;

    void Start()
    {
        amenitiesBuilder = playerBuilding.GetComponent<AmenitiesBuilder>();
        decorBuilder = playerBuilding.GetComponent<DecorBuilder>();

        toggleGroup = GetComponent<ToggleGroup>();
        InstantiatePanelPrefabs();
    }

    private void InstantiatePanelPrefabs()
    {
        List<GameObject> panelPrefabs = new();
        for (int i = 0; i < amenityItems.Length; i++)
        {
            GameObject amenity = InstantiateItemUI(amenityItems[i]);
            amenity.SetActive(false);
            panelPrefabs.Add(amenity);
        }
        amenityPanels = panelPrefabs.ToArray();
        panelPrefabs = new();

        for (int i = 0; i < decorItems.Length; i++)
        {
            GameObject decor = InstantiateItemUI(decorItems[i]);
            decor.SetActive(false);
            panelPrefabs.Add(decor);
        }
        decorPanels = panelPrefabs.ToArray();
    }

    public void PopulateDecorPopOut()
    {
        HandleAmenity(true);
        HandleDecor(false);
    }

    public void PopulateAmenityPopOut()
    {
        HandleDecor(true);
        HandleAmenity(false);
    }

    private void HandleDecor(bool hide)
    {
        for (int i = 0; i < decorPanels.Length; i++)
        {
            if (hide)
            {
                if (decorPanels[i].activeSelf) decorPanels[i].SetActive(false);
            }
            else
            {
                if (!decorPanels[i].activeSelf) decorPanels[i].SetActive(true);
            }
        }
    }

    private void HandleAmenity(bool hide)
    {
        for (int i = 0; i < amenityPanels.Length; i++)
        {
            if (hide)
            {
                if (amenityPanels[i].activeSelf) amenityPanels[i].SetActive(false);
            }
            else
            {
                if (!amenityPanels[i].activeSelf) amenityPanels[i].SetActive(true);
            }
        }
    }

    private GameObject InstantiateItemUI(GameObject itemObject)
    {
        // Instantiate decor item
        GameObject prefab = Instantiate(itemObject);
        ItemInformation itemInfo = prefab.GetComponent<ItemInformation>();

        // Set item prefab to content
        GameObject item = Instantiate(itemTemplate);
        item.transform.SetParent(transform);
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

        return item;
    }

    public void AllTogglesOff()
    {
        toggleGroup.SetAllTogglesOff();
    }
}