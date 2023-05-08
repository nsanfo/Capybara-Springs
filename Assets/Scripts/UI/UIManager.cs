using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEditor;
using TMPro;

public class UIManager : MonoBehaviour
{
    GraphicRaycaster m_Raycaster;
    PointerEventData m_PointerEventData;
    EventSystem m_EventSystem;

    public GameObject detailsWindow;
    GameObject detailsInstance;
    GameObject lastSelected;
    SelectionIndicator selectionScript;

    public GameObject upgradeWindow;
    GameObject upgradeInstance;

    enum AmenityValues { hunger, comfort, fun }

    bool capybaraSelected = false, amenitySelected = false;

    public void Start()
    {
        var canvas = GameObject.Find("Canvas");
        m_Raycaster = canvas.GetComponent<GraphicRaycaster>();
        var eventSystem = GameObject.Find("EventSystem");
        m_EventSystem = eventSystem.GetComponent<EventSystem>();
    }

    public void SetCDetailsWindow(GameObject target)
    {
        var info = target.GetComponent<CapybaraInfo>();

        var canvas = GameObject.Find("Canvas");
        detailsInstance = Instantiate(detailsWindow, canvas.transform);
        var faceScript = detailsInstance.GetComponent<WindowFace>();
        faceScript.info = info;
        var pos = detailsInstance.GetComponent<WindowPosition>();
        pos.cam = GameObject.Find("Main Camera");
        pos.target = target;

        var arrowScript = detailsInstance.GetComponent<DetailsArrows>();
        arrowScript.info = info;

        var name = detailsInstance.transform.GetChild(1);
        var nameScript = name.GetComponent<DetailsName>();
        nameScript.info = info;

        var happinessBar = detailsInstance.transform.GetChild(6);
        var happinessScript = happinessBar.GetComponent<HappinessBar>();
        happinessScript.info = info;

        var hungerBar = detailsInstance.transform.GetChild(7);
        var hungerScript = hungerBar.GetComponent<HungerBar>();
        hungerScript.info = info;

        var comfortBar = detailsInstance.transform.GetChild(8);
        var comfortScript = comfortBar.GetComponent<ComfortBar>();
        comfortScript.info = info;

        var funBar = detailsInstance.transform.GetChild(9);
        var funScript = funBar.GetComponent<FunBar>();
        funScript.info = info;
    }

    public void SetAmenityWindow(GameObject target)
    {
        var canvas = GameObject.Find("Canvas");
        upgradeInstance = Instantiate(upgradeWindow, canvas.transform);
        var amenityScript = target.GetComponent<Amenity>();
        var amenityUpgradeScript = amenityScript.GetComponent<AmenityUpgrade>();
        GameObject upgradeButton = upgradeInstance.transform.GetChild(9).gameObject;
        GameObject previousLevel = upgradeInstance.transform.GetChild(3).gameObject;
        GameObject nextLevel = upgradeInstance.transform.GetChild(4).gameObject;
        GameObject cost = upgradeInstance.transform.GetChild(10).gameObject;
        var pos = upgradeInstance.GetComponent<WindowPosition>();
        pos.cam = GameObject.Find("Main Camera");
        pos.target = target;

        if (amenityUpgradeScript == null)
        {
            GameObject maxLevelMessage = upgradeInstance.transform.GetChild(8).gameObject;
            GameObject panel1 = upgradeInstance.transform.GetChild(0).gameObject;

            panel1.SetActive(true);
            maxLevelMessage.SetActive(true);
            previousLevel.SetActive(false);
            nextLevel.SetActive(false);
            upgradeButton.SetActive(false);
            cost.SetActive(false);
            return;
        }

        var upgradeAmenityScript = amenityUpgradeScript.upgradeObject.GetComponent<Amenity>();
        GameObject hungerLine = upgradeInstance.transform.GetChild(5).gameObject;
        GameObject comfortLine = upgradeInstance.transform.GetChild(6).gameObject;
        GameObject funLine = upgradeInstance.transform.GetChild(7).gameObject;

        var upgradeButtonScript = upgradeButton.GetComponent<UpgradeButton>();
        upgradeButtonScript.previousObject = target;
        upgradeButtonScript.upgradeScript = amenityUpgradeScript;

        var previousLevelText = previousLevel.GetComponent<TextMeshProUGUI>();
        previousLevelText.text = "Level " + amenityScript.level;

        var nextLevelText = nextLevel.GetComponent<TextMeshProUGUI>();
        nextLevelText.text = "Level " + upgradeAmenityScript.level;

        var costText = cost.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        costText.text = amenityUpgradeScript.upgradeCost.ToString();

        List<AmenityValues> amenityValues = new List<AmenityValues>();
        if (amenityScript.hungerFill > 0 || upgradeAmenityScript.hungerFill > 0)
        {
            amenityValues.Add(AmenityValues.hunger);
        }
        if (amenityScript.comfortFill > 0 || upgradeAmenityScript.comfortFill > 0)
        {
            amenityValues.Add(AmenityValues.comfort);
        }
        if (amenityScript.funFill > 0 || upgradeAmenityScript.funFill > 0)
        {
            amenityValues.Add(AmenityValues.fun);
        }

        if (amenityValues.Count == 1)
        {
            GameObject panel1 = upgradeInstance.transform.GetChild(0).gameObject;
            panel1.SetActive(true);
        }
        else if (amenityValues.Count == 2)
        {
            GameObject panel2 = upgradeInstance.transform.GetChild(1).gameObject;
            panel2.SetActive(true);
            Vector3 posTmp = cost.GetComponent<RectTransform>().localPosition;
            cost.GetComponent<RectTransform>().localPosition = new Vector3(posTmp.x, -65.94339f, posTmp.z);
        }
        else
        {
            GameObject panel3 = upgradeInstance.transform.GetChild(2).gameObject;
            panel3.SetActive(true);
            Vector3 posTmp = cost.GetComponent<RectTransform>().localPosition;
            cost.GetComponent<RectTransform>().localPosition = new Vector3(posTmp.x, -93.26341f, posTmp.z);
        }

        //

        float posY = 13.77666f;

        for (int i = 0; i < amenityValues.Count; i++)
        {
            if (amenityValues[i] == AmenityValues.hunger)
            {
                hungerLine.SetActive(true);
                Vector3 posTmp = hungerLine.GetComponent<RectTransform>().localPosition;
                hungerLine.GetComponent<RectTransform>().localPosition = new Vector3(posTmp.x, posY, posTmp.z);

                Transform prevHungerValue = hungerLine.transform.GetChild(0).GetChild(1);
                var prevHungerText = prevHungerValue.GetComponent<TextMeshProUGUI>();
                prevHungerText.text = amenityScript.hungerFill.ToString();

                Transform nextHungerValue = hungerLine.transform.GetChild(2).GetChild(1);
                var nextHungerText = nextHungerValue.GetComponent<TextMeshProUGUI>();
                nextHungerText.text = upgradeAmenityScript.hungerFill.ToString();

                if (amenityScript.hungerFill < upgradeAmenityScript.hungerFill)
                {
                    prevHungerText.color = new Color32(255, 0, 0, 255);
                    nextHungerText.color = new Color32(98, 176, 182, 255);
                }
            }
            else if (amenityValues[i] == AmenityValues.comfort)
            {
                comfortLine.SetActive(true);
                Vector3 posTmp = comfortLine.GetComponent<RectTransform>().localPosition;
                comfortLine.GetComponent<RectTransform>().localPosition = new Vector3(posTmp.x, posY, posTmp.z);

                Transform prevComfortValue = comfortLine.transform.GetChild(0).GetChild(1);
                var prevComfortText = prevComfortValue.GetComponent<TextMeshProUGUI>();
                prevComfortText.text = amenityScript.comfortFill.ToString();

                Transform nextComfortValue = comfortLine.transform.GetChild(2).GetChild(1);
                var nextComfortText = nextComfortValue.GetComponent<TextMeshProUGUI>();
                nextComfortText.text = upgradeAmenityScript.comfortFill.ToString();

                if (amenityScript.comfortFill < upgradeAmenityScript.comfortFill)
                {
                    prevComfortText.color = new Color32(255, 0, 0, 255);
                    nextComfortText.color = new Color32(98, 176, 182, 255);
                }
            }
            else if (amenityValues[i] == AmenityValues.fun)
            {
                funLine.SetActive(true);
                Vector3 posTmp = funLine.GetComponent<RectTransform>().localPosition;
                funLine.GetComponent<RectTransform>().localPosition = new Vector3(posTmp.x, posY, posTmp.z);

                Transform prevFunValue = funLine.transform.GetChild(0).GetChild(1);
                var prevFunText = prevFunValue.GetComponent<TextMeshProUGUI>();
                prevFunText.text = amenityScript.funFill.ToString();

                Transform nextFunValue = funLine.transform.GetChild(2).GetChild(1);
                var nextFunText = nextFunValue.GetComponent<TextMeshProUGUI>();
                nextFunText.text = upgradeAmenityScript.funFill.ToString();

                if (amenityScript.funFill < upgradeAmenityScript.funFill)
                {
                    prevFunText.color = new Color32(255, 0, 0, 255);
                    nextFunText.color = new Color32(98, 176, 182, 255);
                }
            }
            posY -= 27.32002f;
        }
    }

    public void closeUpgradeWindow()
    {
        Destroy(upgradeInstance);
        amenitySelected = false;
    }

    void LateUpdate()
    {
        if (capybaraSelected && lastSelected == null)
            Destroy(detailsInstance);
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            Physics.Raycast(ray, out hit);

            m_PointerEventData = new PointerEventData(m_EventSystem);
            m_PointerEventData.position = Input.mousePosition;
            List<RaycastResult> results = new List<RaycastResult>();
            m_Raycaster.Raycast(m_PointerEventData, results);

            if (results.Count > 0)
                return;

            if (capybaraSelected)
            {
                Destroy(detailsInstance);

                if (lastSelected != null)
                    selectionScript.deactivateSelection();

                capybaraSelected = false;
            }
            else if (amenitySelected)
            {
                Destroy(upgradeInstance);
                amenitySelected = false;
            }

            if (Physics.Raycast(ray, out hit) && hit.transform.gameObject.name.StartsWith("BodyCollision"))
            {
                SetCDetailsWindow(hit.transform.parent.gameObject);
                lastSelected = hit.transform.gameObject;
                selectionScript = hit.transform.parent.gameObject.GetComponent<SelectionIndicator>();
                selectionScript.activateSelection();
                capybaraSelected = true;
            }
            else if(Physics.Raycast(ray, out hit) && hit.transform.gameObject.tag == "Amenity")
            {
                SetAmenityWindow(hit.transform.gameObject);
                lastSelected = hit.transform.gameObject;
                amenitySelected = true;
            }
        }
    }
}
