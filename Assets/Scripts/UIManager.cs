using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class UIManager : MonoBehaviour
{
    public GameObject detailsWindow;
    GameObject detailsInstance;
    GameObject lastSelected;
    SelectionIndicator selectionScript;

    public void setCDetailsWindow(GameObject target)
    {
        var canvas = GameObject.Find("Canvas");
        detailsInstance = Instantiate(detailsWindow, canvas.transform);
        var pos = detailsInstance.GetComponent<DetailsWindowPosition>();
        pos.cam = GameObject.Find("Main Camera");
        pos.target = target;

        var name = detailsInstance.transform.GetChild(1);
        var nameScript = name.GetComponent<DetailsName>();
        nameScript.info = target.GetComponent<CapybaraInfo>();

        var happinessBar = detailsInstance.transform.GetChild(6);
        var happinessScript = happinessBar.GetComponent<HappinessBar>();
        happinessScript.info = target.GetComponent<CapybaraInfo>();

        var hungerBar = detailsInstance.transform.GetChild(7);
        var hungerScript = hungerBar.GetComponent<HungerBar>();
        hungerScript.info = target.GetComponent<CapybaraInfo>();

        var comfortBar = detailsInstance.transform.GetChild(8);
        var comfortScript = comfortBar.GetComponent<ComfortBar>();
        comfortScript.info = target.GetComponent<CapybaraInfo>();

        var funBar = detailsInstance.transform.GetChild(9);
        var funScript = funBar.GetComponent<FunBar>();
        funScript.info = target.GetComponent<CapybaraInfo>();
    }

    void LateUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            Destroy(detailsInstance);

            if (lastSelected != null)
                selectionScript.deactivateSelection();

            if (Physics.Raycast(ray, out hit) && hit.transform.gameObject.name.StartsWith("Capybara"))
            {
                setCDetailsWindow(hit.transform.gameObject);
                lastSelected = hit.transform.gameObject;
                selectionScript = hit.transform.gameObject.GetComponent<SelectionIndicator>();
                selectionScript.activateSelection();
            }
        }
    }
}
