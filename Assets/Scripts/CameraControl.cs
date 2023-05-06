using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [Header("Mouse On UI")]
    public MouseOnUI mouse;

    public GameObject tmp;
    Ray ray;
    Plane ground;
    Vector3 intersection;
    public float cameraSpeed = 1.5f, zoomSpeed = 100f, maxZoom = 0.6f, minZoom = 5f;
    float horizontalInput, forwardInput, scrollInput, mouseInput, modifier;

    public bool plotCamera = false;
    public (float, float, float, float) cameraBound;

    // Start is called before the first frame update
    void Start()
    {
        cameraBound = (0.5f, 7.5f, 7.5f, 0.5f);
        ground = new Plane(Vector3.up, new Vector3(0, 0, 0));
    }

    void KeyMove()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        forwardInput = Input.GetAxis("Vertical");

        if (plotCamera)
        {
            horizontalInput *= -1;
            forwardInput *= -1;
        }

        transform.Translate(((Vector3.right * Time.deltaTime * horizontalInput * cameraSpeed) / 0.6f) * modifier);
        transform.Translate(((Vector3.forward * Time.deltaTime * forwardInput * cameraSpeed) / 0.6f) * modifier, tmp.transform);
    
        // Horizontal bound
        if (transform.position.z > cameraBound.Item2)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, cameraBound.Item2);
        }
        else if (transform.position.z < cameraBound.Item4)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, cameraBound.Item4);
        }

        // Vertical bound
        if (transform.position.x < cameraBound.Item1)
        {
            transform.position = new Vector3(cameraBound.Item1, transform.position.y, transform.position.z);
        }
        else if (transform.position.x > cameraBound.Item3)
        {
            transform.position = new Vector3(cameraBound.Item3, transform.position.y, transform.position.z);
        }
    }

    void EdgeMove()
    {
        if (Input.mousePosition.x > Screen.width)
            horizontalInput = 1;
        else if (Input.mousePosition.x < 0)
            horizontalInput = -1;
        else
            horizontalInput = (Input.mousePosition.x - (Screen.width / 2)) / (Screen.width / 2);
        if (Input.mousePosition.y > Screen.height)
            forwardInput = 1;
        else if (Input.mousePosition.y < 0)
            forwardInput = -1;
        else
            forwardInput = (Input.mousePosition.y - (Screen.height / 2)) / (Screen.height / 2);

        transform.Translate(((Vector3.right * Time.deltaTime * horizontalInput * cameraSpeed) / 0.6f) * modifier);
        transform.Translate(((Vector3.forward * Time.deltaTime * forwardInput * cameraSpeed) / 0.6f) * modifier, tmp.transform);
    }

    void Zoom()
    {
        if (mouse.overUI) return;

        scrollInput = Input.GetAxis("Mouse ScrollWheel");
        tmp.transform.rotation = transform.rotation;
        tmp.transform.Translate(Vector3.forward * Time.deltaTime * scrollInput * zoomSpeed);

        if (tmp.transform.position.y >= maxZoom && tmp.transform.position.y <= minZoom)
            transform.Translate(Vector3.forward * Time.deltaTime * scrollInput * zoomSpeed);
    }

    void Rotate()
    {
        if (plotCamera) return;

        mouseInput = Input.GetAxis("Mouse X");
        float enter = 0.0f;
        ray = new Ray(transform.position, transform.forward);

        if (ground.Raycast(ray, out enter))
        {
            intersection = ray.GetPoint(enter);
            transform.RotateAround(intersection, Vector3.up, mouseInput * Time.deltaTime * 1000);
        }
    }

    // Update is called once per frame
    void Update()
    {
        tmp.transform.position = transform.position;
        tmp.transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, transform.localEulerAngles.z);
        modifier = -0.1025f * Mathf.Pow((transform.position.y - 5), 2) + 2.583f;

        KeyMove();

        /*
        if (Input.mousePosition.y >= Screen.height * 0.995 || Input.mousePosition.y <= Screen.height * 0.005 || Input.mousePosition.x >= Screen.width * 0.995 || Input.mousePosition.x <= Screen.width * 0.005)
        {
            EdgeMove();
        }
        */

        Zoom();

        if (Input.GetMouseButton(2))
        {
            Rotate();
        }
    }
}