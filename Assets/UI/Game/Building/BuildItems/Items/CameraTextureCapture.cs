using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraTextureCapture : MonoBehaviour
{
    private Camera _camera;
    private readonly int width = 256, height = 256;

    void Start()
    {
        // Update camera
        _camera = transform.Find("Camera").GetComponent<Camera>();
        _camera.enabled = false;
        _camera.aspect = 1;
    }

    public Texture2D RetrieveTexture()
    {
        _camera = transform.Find("Camera").GetComponent<Camera>();
        _camera.enabled = false;
        _camera.aspect = 1;

        // Instantiate vars
        Rect region = new Rect(0, 0, Screen.width, Screen.height);
        RenderTexture renderTexture = new RenderTexture(width, height, 24);
        Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false);

        // Render texture
        _camera.targetTexture = renderTexture;
        _camera.Render();

        // Render to Texture2D
        RenderTexture.active = renderTexture;
        texture.ReadPixels(region, 0, 0);
        texture.Apply();

        // Clean up
        _camera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(renderTexture);

        return texture;
    }
}
