using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class DecorBuilder : MonoBehaviour
{
    [Header("Decor Blueprints")]
		public GameObject toriiBlueprint;

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

		public void toriiObjectSelect()
    {
        if (blueprint != null && blueprint.name.StartsWith("Torii"))
        {
            Destroy(blueprint);
        }
        else if (blueprint != null)
        {
            Destroy(blueprint);
            blueprint = Instantiate(toriiBlueprint);
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
            blueprint = Instantiate(toriiBlueprint);
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
                        var newDecor = Instantiate(blueprintScript.GetConcrete());
                        newDecor.transform.position = new Vector3(hitInfo.point.x, hitInfo.point.y, hitInfo.point.z);
                        newDecor.transform.eulerAngles = new Vector3(newDecor.transform.eulerAngles.x, angle, newDecor.transform.eulerAngles.z);
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