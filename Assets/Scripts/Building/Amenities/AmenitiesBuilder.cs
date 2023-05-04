using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class AmenitiesBuilder : MonoBehaviour
{
    private int currentCap;

    [Header("Blueprint Material")]
    public Material blueprintMat;

    [Header("Item Select Toggles")]
    public ItemToggleHandler itemToggles;

    [Header("Mouse On UI")]
    public MouseOnUI mouse;

    Color redColor = new Color(1f, 0f, 0f, 0.27f);
    Color blueColor = new Color(0f, 0.69f, 0.98f, 0.27f);

    GameObject stats;
    Balance balanceScript;
    GameplayState gameplayStateScript;
    GameObject blueprint;
    bool red = false;
    Vector2 originalMousePos;

    AudioSource buildSFX;
    AudioSource click2;
    AudioSource errorSound;

    // Start is called before the first frame update
    void Start()
    {
        stats = GameObject.Find("Stats");
        balanceScript = stats.GetComponent<Balance>();
        gameplayStateScript = stats.GetComponent<GameplayState>();
        var UISounds = GameObject.Find("UISounds");
        click2 = UISounds.transform.GetChild(1).GetComponent<AudioSource>();
        buildSFX = UISounds.transform.GetChild(2).GetComponent<AudioSource>();
        errorSound = UISounds.transform.GetChild(3).GetComponent<AudioSource>();
    }

    public void BuildItem(GameObject blueprintPrefab)
    {
        if (blueprint != null && blueprint.name.Equals(blueprintPrefab.name))
        {
            Destroy(blueprint);
        }
        else if (blueprint != null)
        {
            Destroy(blueprint);
            blueprint = Instantiate(blueprintPrefab);
            blueprint.tag = "Blueprint";
            currentCap = blueprint.GetComponent<AmenityBlueprint>().concrete.GetComponent<Amenity>().numSlots;
            if (red)
                blueprintMat.SetColor("_BaseColor", redColor);
            else
                blueprintMat.SetColor("_BaseColor", blueColor);
        }
        else if (blueprint == null)
        {
            blueprint = Instantiate(blueprintPrefab);
            blueprint.tag = "Blueprint";
            currentCap = blueprint.GetComponent<AmenityBlueprint>().concrete.GetComponent<Amenity>().numSlots;
            if (red)
                blueprintMat.SetColor("_BaseColor", redColor);
            else
                blueprintMat.SetColor("_BaseColor", blueColor);
        }
        click2.Play();
    }

    public void FreePlace(Vector3 hitVector)
    {
        blueprint.transform.position = hitVector;
        if (!red)
        {
            blueprintMat.SetColor("_BaseColor", redColor);
            red = true;
        }
        if (Input.GetMouseButtonDown(0) && EventSystem.current.IsPointerOverGameObject() == false)
            errorSound.Play();
    }

    public void SnapPlace(Vector3 hitVector)
    {
        var blueprintScript = blueprint.GetComponent<AmenityBlueprint>();
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

        if ((balance - cost < 0 || blueprintScript.buildCollisions != 0) && !red)
        {
            blueprintMat.SetColor("_BaseColor", redColor);
            red = true;
        }

        else if (balance - cost >= 0 && blueprintScript.buildCollisions == 0)
        {
            if (red)
            {
                blueprintMat.SetColor("_BaseColor", blueColor);
                red = false;
            }
            if (Input.GetMouseButtonDown(0) && EventSystem.current.IsPointerOverGameObject() == false)
            {
                if (mouse.overUI) return;

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
                buildSFX.Play();
                itemToggles.AllTogglesOff();

                gameplayStateScript.AdjustCapacity(currentCap);
                currentCap = 0;
            }
        }

        if (Input.GetMouseButtonDown(0) && red && EventSystem.current.IsPointerOverGameObject() == false)
            errorSound.Play();

        if (Input.GetMouseButtonDown(1))
        {
            Destroy(blueprint);
            itemToggles.AllTogglesOff();
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

        if (Input.GetMouseButtonDown(1))
        {
            Destroy(blueprint);
            itemToggles.AllTogglesOff();
        }
    }
}
