using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public GameObject tmp;
    Ray ray;
    Plane ground;
    Vector3 intersection;
    public float cameraSpeed = 1.5f, maxZoom = 0.6f, minZoom = 5f;
    float horizontalInput, forwardInput, scrollInput, mouseInput, modifier;

    // Start is called before the first frame update
    void Start()
    {
        ground = new Plane(Vector3.up, new Vector3(0, 0, 0));

    }

    void KeyMove()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        forwardInput = Input.GetAxis("Vertical");

        transform.Translate(((Vector3.right * Time.deltaTime * horizontalInput * cameraSpeed) / 0.6f) * modifier);
        transform.Translate(((Vector3.forward * Time.deltaTime * forwardInput * cameraSpeed) / 0.6f) * modifier, tmp.transform);
    }

    void EdgeMove()
    {
        horizontalInput = (Input.mousePosition.x - (Screen.width / 2)) / (Screen.width / 2);
        forwardInput = (Input.mousePosition.y - (Screen.height / 2)) / (Screen.height / 2);

        transform.Translate(((Vector3.right * Time.deltaTime * horizontalInput * cameraSpeed) / 0.6f) * modifier);
        transform.Translate(((Vector3.forward * Time.deltaTime * forwardInput * cameraSpeed) / 0.6f) * modifier, tmp.transform);
    }

    void Zoom()
    {
        scrollInput = Input.GetAxis("Mouse ScrollWheel");
        tmp.transform.rotation = transform.rotation;
        tmp.transform.Translate(Vector3.forward * Time.deltaTime * scrollInput * 100);

        if (tmp.transform.position.y >= maxZoom && tmp.transform.position.y <= minZoom)
            transform.Translate(Vector3.forward * Time.deltaTime * scrollInput * 100);
    }

    void Rotate()
    {
        mouseInput = Input.GetAxis("Mouse X");
        float enter = 0.0f;
        ray = new Ray(transform.position, transform.forward);

        if (ground.Raycast(ray, out enter))
        {
            intersection = ray.GetPoint(enter);
            transform.RotateAround(intersection, Vector3.up, mouseInput * 12);
        }
    }

    // Update is called once per frame
    void Update()
    {
        tmp.transform.position = transform.position;
        tmp.transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, transform.localEulerAngles.z);
        modifier = -0.1025f * Mathf.Pow((transform.position.y - 5), 2) + 2.583f;

        KeyMove();

        if (Input.mousePosition.y >= Screen.height * 0.995 || Input.mousePosition.y <= Screen.height * 0.005 || Input.mousePosition.x >= Screen.width * 0.995 || Input.mousePosition.x <= Screen.width * 0.005)
        {
            EdgeMove();
        }

        Zoom();

        if (Input.GetMouseButton(2))
        {
            Rotate();
        }
    }
}