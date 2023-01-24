using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class AmenitiesBuilder : MonoBehaviour
{
    [Header("Amenity Blueprints")]
    public GameObject smallOnsenBlueprint;
    public GameObject mediumOnsenBlueprint;
    public GameObject largeOnsenBlueprint;

    [Header("Blueprint Material")]
    public Material blueprintMat;

    GameObject stats;
    GameObject blueprint;
    bool red = false, rotating = false;
    Vector2 originalMousePos;
    
    // Start is called before the first frame update
    void Start()
    {
        stats = GameObject.Find("Stats");
    }

    public void SmallOnsenSelect()
    {
        if (blueprint != null && blueprint.name.StartsWith("SmallOnsen"))
        {
            Destroy(blueprint);
        }
        else if (blueprint != null)
        {
            Destroy(blueprint);
            blueprint = Instantiate(smallOnsenBlueprint);
            if (red)
            {
                var redColor = new Color(1f, 0f, 0f, 0.27f);
                blueprintMat.SetColor("_Color", redColor);
            }
            else
            {

                var blueColor = new Color(0f, 0.69f, 0.98f, 0.27f);
                blueprintMat.SetColor("_Color", blueColor);
            }
        }
        else if (blueprint == null)
        {
            blueprint = Instantiate(smallOnsenBlueprint);
            if (red)
            {
                var redColor = new Color(1f, 0f, 0f, 0.27f);
                blueprintMat.SetColor("_Color", redColor);
            }
            else
            {
                var blueColor = new Color(0f, 0.69f, 0.98f, 0.27f);
                blueprintMat.SetColor("_Color", blueColor);
            }
        }
    }

    public void MediumOnsenSelect()
    {
        if (blueprint != null && blueprint.name.StartsWith("MediumOnsen"))
        {
            Destroy(blueprint);
        }
        else if (blueprint != null)
        {
            Destroy(blueprint);
            blueprint = Instantiate(mediumOnsenBlueprint);
            if (red)
            {
                var redColor = new Color(1f, 0f, 0f, 0.27f);
                blueprintMat.SetColor("_Color", redColor);
            }
            else
            {

                var blueColor = new Color(0f, 0.69f, 0.98f, 0.27f);
                blueprintMat.SetColor("_Color", blueColor);
            }
        }
        else if (blueprint == null)
        {
            blueprint = Instantiate(mediumOnsenBlueprint);
            if (red)
            {
                var redColor = new Color(1f, 0f, 0f, 0.27f);
                blueprintMat.SetColor("_Color", redColor);
            }
            else
            {
                var blueColor = new Color(0f, 0.69f, 0.98f, 0.27f);
                blueprintMat.SetColor("_Color", blueColor);
            }
        }
    }

    public void LargeOnsenSelect()
    {
        if (blueprint != null && blueprint.name.StartsWith("LargeOnsen"))
        {
            Destroy(blueprint);
        }
        else if (blueprint != null)
        {
            Destroy(blueprint);
            blueprint = Instantiate(largeOnsenBlueprint);
            if (red)
            {
                var redColor = new Color(1f, 0f, 0f, 0.27f);
                blueprintMat.SetColor("_Color", redColor);
            }
            else
            {

                var blueColor = new Color(0f, 0.69f, 0.98f, 0.27f);
                blueprintMat.SetColor("_Color", blueColor);
            }
        }
        else if (blueprint == null)
        {
            blueprint = Instantiate(largeOnsenBlueprint);
            if (red)
            {
                var redColor = new Color(1f, 0f, 0f, 0.27f);
                blueprintMat.SetColor("_Color", redColor);
            }
            else
            {
                var blueColor = new Color(0f, 0.69f, 0.98f, 0.27f);
                blueprintMat.SetColor("_Color", blueColor);
            }
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if(blueprint != null)
        {
            var blueprintScript = blueprint.GetComponent<Blueprint>();
            var balanceScript = stats.GetComponent<Balance>();
            var balance = balanceScript.GetBalance();
            var cost = blueprintScript.GetCost();
            if (Input.GetKey("r"))
            {
                if (!rotating)
                {
                    rotating = true;
                    Cursor.visible = false;
                    originalMousePos = Mouse.current.position.ReadValue();
                }

                var mouseInput = Input.GetAxis("Mouse X");
                var ray = new Ray(transform.position, transform.forward);
                blueprint.transform.Rotate(Vector3.up, mouseInput * Time.deltaTime * 250);
            }
            else
            {
                if (rotating)
                {
                    rotating = false;
                    Cursor.visible = true;
                    Mouse.current.WarpCursorPosition(originalMousePos);
                    InputState.Change(Mouse.current.position, originalMousePos);
                }

                RaycastHit hitInfo = new RaycastHit();
                bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);

                if (hit && hitInfo.transform.tag == "Terrain")
                {
                    blueprint.transform.position = new Vector3(hitInfo.point.x, hitInfo.point.y, hitInfo.point.z);
                }

                if (balance - cost < 0 && !red)
                {
                    var redColor = new Color(1f, 0f, 0f, 0.27f);
                    blueprintMat.SetColor("_Color", redColor);
                    red = true;
                }

                else if (balance - cost >= 0)
                {
                    if(red)
                    {
                        var blueColor = new Color(0f, 0.69f, 0.98f, 0.27f);
                        blueprintMat.SetColor("_Color", blueColor);
                        red = false;
                    }
                    if (Input.GetMouseButtonDown(0))
                    {
                        var angle = blueprint.transform.eulerAngles.y;
                        var newAmenity = Instantiate(blueprintScript.GetConcrete());
                        newAmenity.transform.position = new Vector3(hitInfo.point.x, hitInfo.point.y, hitInfo.point.z);
                        newAmenity.transform.eulerAngles = new Vector3(newAmenity.transform.eulerAngles.x, angle, newAmenity.transform.eulerAngles.z);
                        Destroy(blueprint);
                        balanceScript.AdjustBalance(cost * -1);
                    }
                }

                if (Input.GetMouseButtonDown(1))
                {
                    Destroy(blueprint);
                }
            }
        }
    }
}