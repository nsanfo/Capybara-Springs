using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AnimateBuildMenu))]
public class BuildingToggleButton : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite selectedSprite;
    private Sprite originalSprite;

    [Header("Hover Animation")]
    public AnimateBuildHover buildHover;

    [Header("PopOut Animation")]
    public AnimateBuildingPopOut buildPopOut;

    [Header("Building Toggles")]
    public BuildToggleHandler buildToggles;

    [Header("Item Select Toggles")]
    public ItemToggleHandler itemToggles;

    [Header("Player Building")]
    public BuildingUpgrade playerBuilding;

    private Toggle toggle;
    private Image background;

    private AudioSource clickSound;

    void Start()
    {
        toggle = GetComponent<Toggle>();
        background = transform.Find("Background").GetComponent<Image>();
        originalSprite = background.sprite;

        clickSound = GameObject.Find("UISounds").transform.GetChild(0).GetComponent<AudioSource>();
    }

    public void ToggleBuilding()
    {
        clickSound.Play();

        bool toggleState = toggle.isOn;
        GetComponent<AnimateBuildMenu>().UpdateAnimation(toggleState);
        buildHover.UpdateAnimation(toggleState);

        if (toggleState)
        {
            background.sprite = selectedSprite;
        }
        else
        {
            background.sprite = originalSprite;
            buildPopOut.UpdateAnimation(false);
            buildToggles.AllTogglesOff();
            itemToggles.AllTogglesOff();
        }

        // Update building
        playerBuilding.buildingModes.enableBuild = toggleState;
    }
}
