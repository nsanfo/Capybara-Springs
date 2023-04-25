using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.EventSystems;

public class DecorBuilder : MonoBehaviour
{
    [Header("Decor Blueprints")]
    public GameObject ToriiBlueprint;
    public GameObject LampBlueprint;
    public GameObject GreyLampBlueprint;
    public GameObject MossyLampBlueprint;


    [Header("Blueprint Material")]
    public Material blueprintMat;

    GameObject stats;
    GameObject blueprint;
    bool red = false, rotating = false;
    Vector2 originalMousePos;

    AudioSource buildSFX;
    AudioSource click2;
    AudioSource errorSound;

    // Start is called before the first frame update
    void Start()
    {
        stats = GameObject.Find("Stats");
        var UISounds = GameObject.Find("UISounds");
        click2 = UISounds.transform.GetChild(1).GetComponent<AudioSource>();
        buildSFX = UISounds.transform.GetChild(2).GetComponent<AudioSource>();
        errorSound = UISounds.transform.GetChild(3).GetComponent<AudioSource>();
    }

    public void ObjectSelect(GameObject go, string s)
    {
        click2.Play();
        if (blueprint != null && blueprint.name.StartsWith(s))
        {
            Destroy(blueprint);
        }
        else if (blueprint != null)
        {
            Destroy(blueprint);
            blueprint = Instantiate(go);
            if (red)
            {
                var redColor = new Color(1f, 0f, 0f, 0.27f);
                blueprintMat.SetColor("_BaseColor", redColor);
            }
            else
            {

                var blueColor = new Color(0f, 0.69f, 0.98f, 0.27f);
                blueprintMat.SetColor("_BaseColor", blueColor);
            }
        }
        else if (blueprint == null)
        {
            blueprint = Instantiate(go);
            if (red)
            {
                var redColor = new Color(1f, 0f, 0f, 0.27f);
                blueprintMat.SetColor("_BaseColor", redColor);
            }
            else
            {
                var blueColor = new Color(0f, 0.69f, 0.98f, 0.27f);
                blueprintMat.SetColor("_BaseColor", blueColor);
            }
        }
    }

    public void ToriiObjectSelect()
    {
        ObjectSelect(ToriiBlueprint, "Torii");
    }

    public void LampObjectSelect()
    {
        ObjectSelect(LampBlueprint, "Lamp");
    }

    public void GreyLampObjectSelect()
    {
        ObjectSelect(GreyLampBlueprint, "StoneLampGrey");
    }

    public void MossyLampObjectSelect()
    {
        ObjectSelect(MossyLampBlueprint, "StoneLampMossy");
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (blueprint != null)
        {
            var blueprintScript = blueprint.GetComponent<DecorBlueprint>();
            var balanceScript = stats.GetComponent<Balance>();
            var balance = balanceScript.GetBalance();
            var cost = blueprintScript.cost;
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

                if ((balance - cost < 0 || blueprintScript.buildCollisions > 0)  && !red)
                {
                    var redColor = new Color(1f, 0f, 0f, 0.27f);
                    blueprintMat.SetColor("_BaseColor", redColor);
                    red = true;
                }

                else if (balance - cost >= 0 && blueprintScript.buildCollisions == 0)
                {
                    if (red)
                    {
                        var blueColor = new Color(0f, 0.69f, 0.98f, 0.27f);
                        blueprintMat.SetColor("_BaseColor", blueColor);
                        red = false;
                    }
                    if (Input.GetMouseButtonDown(0))
                    {
                        buildSFX.Play();
                        var angle = blueprint.transform.eulerAngles.y;
                        var newDecor = Instantiate(blueprintScript.GetConcrete());
                        newDecor.transform.position = new Vector3(hitInfo.point.x, hitInfo.point.y, hitInfo.point.z);
                        newDecor.transform.eulerAngles = new Vector3(newDecor.transform.eulerAngles.x, angle, newDecor.transform.eulerAngles.z);
                        Destroy(blueprint);
                        balanceScript.AdjustBalance(cost * -1);
                    }
                }

                if (red && EventSystem.current.IsPointerOverGameObject() == false && Input.GetMouseButtonDown(0))
                    errorSound.Play();

                if (Input.GetMouseButtonDown(1))
                {
                    Destroy(blueprint);
                }
            }
        }
    }
}