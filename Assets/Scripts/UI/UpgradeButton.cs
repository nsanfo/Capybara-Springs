using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeButton : MonoBehaviour
{
    Balance balanceScript;
    UIManager uiManagerScript;
    public GameObject previousObject;
    public AmenityUpgrade upgradeScript;
    GameObject tutorial;

    void Start()
    {
        GameObject stats = GameObject.Find("Stats");
        balanceScript = stats.GetComponent<Balance>();
        GameObject uiManager = GameObject.Find("UI Manager");
        uiManagerScript = uiManager.GetComponent<UIManager>();
        tutorial = GameObject.Find("Tutorial");
    }

    public void Upgrade()
    {
        var upgradeObject = upgradeScript.upgradeObject;
        var upgradeCost = upgradeScript.upgradeCost;

        if (balanceScript.GetBalance() < upgradeCost)
            return;
        else
        {
            if (tutorial.activeSelf == true)
                tutorial.GetComponent<Tutorial>().UpgradePress();

            balanceScript.AdjustBalance(-upgradeCost);

            Vector3 pos = previousObject.transform.position;
            Quaternion rotation = previousObject.transform.rotation;

            var amenityScript = previousObject.GetComponent<Amenity>();
            GameObject pathCollider = amenityScript.PathCollider;
            GameObject[] amenitySlots = amenityScript.amenitySlots;
            amenityScript.PathUnset();
            Destroy(previousObject);

            upgradeObject = Instantiate(upgradeObject);
            upgradeObject.transform.position = pos;
            upgradeObject.transform.rotation = rotation;

            amenityScript = upgradeObject.GetComponent<Amenity>();
            amenityScript.PathCollider = pathCollider;
            amenityScript.PathSetup();
            amenityScript.amenitySlots = amenitySlots;

            // Update the amenity script for each capybara that's currently interacting with the upgraded amenity
            for(int i = 0; i < amenitySlots.Length; i++)
            {
                if(amenitySlots[i] != null)
                {
                    var amenityInteractionScript = amenitySlots[i].GetComponent<AmenityInteraction>();
                    amenityInteractionScript.amenity = amenityScript;
                }
            }

            uiManagerScript.closeUpgradeWindow();
        }
    }
}
