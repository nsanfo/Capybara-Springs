using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemBuilding : MonoBehaviour, IPointerClickHandler
{
    public AmenitiesBuilder amenitiesBuilder;
    public DecorBuilder decorBuilder;
    public GameObject itemBlueprint;

    public BuildType buildType;

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (pointerEventData.button != PointerEventData.InputButton.Left) return;

        if (buildType == BuildType.Onsen || buildType == BuildType.Food || buildType == BuildType.Fun)
        {
            amenitiesBuilder.BuildItem(itemBlueprint, buildType);
        }
        else if (buildType == BuildType.Decor)
        {
            decorBuilder.BuildItem(itemBlueprint);
        }
    }
}
