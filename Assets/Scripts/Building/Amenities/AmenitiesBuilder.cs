using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
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
    bool red = false;
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

    public void FreePlace(Vector3 hitVector)
    {
        blueprint.transform.position = hitVector;
        if (!red)
        {
            var redColor = new Color(1f, 0f, 0f, 0.27f);
            blueprintMat.SetColor("_Color", redColor);
            red = true;
        }
    }

    public void SnapPlace(Vector3 hitVector)
    {
        var blueprintScript = blueprint.GetComponent<AmenityBlueprint>();
        var balanceScript = stats.GetComponent<Balance>();
        var balance = balanceScript.GetBalance();
        var cost = blueprintScript.cost;
        var pathTuple = blueprintScript.FindClosestCollider(hitVector);
        var closestForward = pathTuple.Item1;
        var closestPathCollider = pathTuple.Item2;
        var closestPosition = closestPathCollider.transform.position;
        var rightSide = pathTuple.Item3;
        var shortestDistance = pathTuple.Item4;
        var pathRight = Vector3.Cross(closestForward.normalized, Vector3.up);
        var pathLeft = -pathRight;
        var entranceDistance = blueprintScript.entranceDistance + 0.495f;

        if (shortestDistance > blueprintScript.snapDistance)
        {
            blueprintScript.ClearPathCollision();
            FreePlace(hitVector);
        }

        if (rightSide)
        {
            blueprint.transform.rotation = Quaternion.LookRotation(pathLeft);
            blueprint.transform.position = closestPosition + Vector3.Scale(pathRight, new Vector3(entranceDistance, entranceDistance, entranceDistance));
        }
        else
        {
            blueprint.transform.rotation = Quaternion.LookRotation(pathRight);
            blueprint.transform.position = closestPosition + Vector3.Scale(pathLeft, new Vector3(entranceDistance, entranceDistance, entranceDistance));
        }

        if ((balance - cost < 0 || blueprintScript.GetBuildCollisions() != 0) && !red)
        {
            var redColor = new Color(1f, 0f, 0f, 0.27f);
            blueprintMat.SetColor("_Color", redColor);
            red = true;
        }

        else if (balance - cost >= 0 && blueprintScript.GetBuildCollisions() == 0)
        {
            if (red)
            {
                var blueColor = new Color(0f, 0.69f, 0.98f, 0.27f);
                blueprintMat.SetColor("_Color", blueColor);
                red = false;
            }
            if (Input.GetMouseButtonDown(0) && EventSystem.current.IsPointerOverGameObject() == false)
            {
                var angle = blueprint.transform.eulerAngles.y;
                var newAmenity = Instantiate(blueprintScript.GetConcrete());
                newAmenity.transform.position = new Vector3(blueprint.transform.position.x, blueprint.transform.position.y, blueprint.transform.position.z);
                newAmenity.transform.eulerAngles = new Vector3(newAmenity.transform.eulerAngles.x, angle, newAmenity.transform.eulerAngles.z);
                var amenityScript = newAmenity.GetComponent<Amenity>();
                amenityScript.PathCollider = closestPathCollider;
                amenityScript.PathSetup();
                amenityScript.SlotSetup();
                Destroy(blueprint);
                balanceScript.AdjustBalance(cost * -1);
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            Destroy(blueprint);
        }
    }

    void LateUpdate()
    {
        if(blueprint != null)
        {
            var blueprintScript = blueprint.GetComponent<AmenityBlueprint>();
            RaycastHit hitInfo = new RaycastHit();
            bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);
            var hitVector = new Vector3(hitInfo.point.x, hitInfo.point.y, hitInfo.point.z);

            if (hit && hitInfo.transform.tag == "Terrain" && blueprintScript.GetPathCollisionCount() == 0)
            {
                FreePlace(hitVector);
            }
            else if (hit && hitInfo.transform.tag == "Terrain")
            {
                SnapPlace(hitVector);
            }
        }
    }
}
