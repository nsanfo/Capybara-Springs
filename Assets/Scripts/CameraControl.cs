using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public GameObject tmp;
    float horizontalInput;
    float forwardInput;
    public float scrollInput;
    public float cameraSpeed;
    float maxZoom = 0.6f;
    float minZoom = 5f;
    float modifier;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        tmp.transform.position = transform.position;
        tmp.transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, transform.localEulerAngles.z);
        horizontalInput = Input.GetAxis("Horizontal");
        forwardInput = Input.GetAxis("Vertical");
        scrollInput = Input.GetAxis("Mouse ScrollWheel");
        modifier = -0.1025f * Mathf.Pow((transform.position.y - 5), 2) + 2.583f;
        transform.Translate(((Vector3.right * Time.deltaTime * horizontalInput * cameraSpeed) / 0.6f) * modifier);
        transform.Translate(((Vector3.forward * Time.deltaTime * forwardInput * cameraSpeed) / 0.6f) * modifier, tmp.transform);
        tmp.transform.rotation = transform.rotation;
        tmp.transform.Translate(Vector3.forward * Time.deltaTime * scrollInput * 100);
        if (tmp.transform.position.y >= maxZoom && tmp.transform.position.y <= minZoom)
            transform.Translate(Vector3.forward * Time.deltaTime * scrollInput * 100);
    }
}
