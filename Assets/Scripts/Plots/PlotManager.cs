using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[RequireComponent(typeof(PlotOutsideHandler))]
public class PlotManager : MonoBehaviour
{
    private PlotInfo[,] plots = new PlotInfo[5, 5];

    public Material plotMaterial, unsoldMaterial;
    public GameObject plotPurchaseSprite;

    public GameObject buildableHolder, nonBuildableHolder;
    public bool debugMatrix = false;

    private PlotOutsideHandler edgeHandler;

    void Start()
    {
        // Housekeeping
        GameObject terrainHolder = GameObject.Find("PlotManager/TerrainHolder");
        if (terrainHolder == null)
        {
            terrainHolder = new GameObject("TerrainHolder");
            terrainHolder.transform.parent = transform;
        }

        GameObject spriteHolder = GameObject.Find("PlotManager/SpriteHolder");
        if (spriteHolder == null)
        {
            spriteHolder = new GameObject("SpriteHolder");
            spriteHolder.transform.parent = transform;
        }

        buildableHolder = GameObject.Find("PlotManager/TerrainHolder/Buildable");
        if (buildableHolder == null)
        {
            buildableHolder = new GameObject("Buildable");
            buildableHolder.transform.parent = terrainHolder.transform;
        }

        nonBuildableHolder = GameObject.Find("PlotManager/TerrainHolder/NonBuildable");
        if (nonBuildableHolder == null)
        {
            nonBuildableHolder = new GameObject("NonBuildable");
            nonBuildableHolder.transform.parent = terrainHolder.transform;
        }

        foreach (Transform child in buildableHolder.transform)
        {
            if (!child.TryGetComponent<PlotInfo>(out var currentPlot)) continue;

            UpdateMatrix(currentPlot, true);
        }

        edgeHandler = GetComponent<PlotOutsideHandler>();

        UpdateTerrain();
        edgeHandler.GenerateOutsideTiles(plots);
        UpdateCameraBounds();
        DebugMatrix();
    }

    public PlotInfo[,] GetPlotMatrix()
    {
        return plots;
    }

    private void ExpandMatrix(int x, int y, bool initialization)
    {
        // Create new array with expanded size
        PlotInfo[,] newMatrix = new PlotInfo[x, y];

        // Copy original array variables to new array
        for (int i = 0; i < plots.GetLength(0); i++)
        {
            for (int j = 0; j < plots.GetLength(1); j++)
            {
                newMatrix[i, j] = plots[i, j];
            }
        }

        plots = newMatrix;
    }

    private void UpdateMatrix(PlotInfo plot, bool initialization)
    {
        // Set current matrix lengths
        int xLength = plots.GetLength(1), yLength = plots.GetLength(0);

        // Check x length
        if (plot.xLocation + 3 > plots.GetLength(1))
        {
            xLength = plot.xLocation + 3;
        }

        // Check y length
        if (plot.yLocation + 3 > plots.GetLength(0))
        {
            yLength = plot.yLocation + 3;
        }

        // Check to expand matrix
        if (xLength > plots.GetLength(1) || yLength > plots.GetLength(0))
        {
            ExpandMatrix(yLength, xLength, initialization);
        }

        // Set the added plot
        plots[plot.yLocation, plot.xLocation] = plot;
    }

    public void AddPlot(PlotInfo plot)
    {
        UpdateMatrix(plot, false);
        UpdateTerrain();
        edgeHandler.GenerateOutsideTiles(plots);
        UpdateCameraBounds();
        DebugMatrix();
    }

    private void UpdateTerrain()
    {
        // Update new cells with information
        for (int i = 0; i < plots.GetLength(0); i++)
        {
            for (int j = 0; j < plots.GetLength(1); j++)
            {
                if (!plots[i, j])
                {
                    GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
                    plane.transform.SetParent(nonBuildableHolder.transform);
                    plane.transform.position = new Vector3(-12f + (8 * i), 0, -12f + (8 * j));
                    plane.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                    plane.GetComponent<Renderer>().material = unsoldMaterial;
                    plane.name = "Purchasable[" + j + "," + i + "]";

                    PlotInfo plotInfo = plane.AddComponent<PlotInfo>();
                    plotInfo.xLocation = j;
                    plotInfo.yLocation = i;

                    // Calculate price
                    float multiplier = 0f;
                    if (j > 6 || i > 6)
                    {
                        multiplier = 20 + Mathf.Pow(j, 2) + Mathf.Pow(i, 2);
                    }
                    else if (j > 5 || i > 5)
                    {
                        multiplier = 20;
                    }
                    else if (j > 4 || i > 4)
                    {
                        multiplier = 10;
                    }
                    else if (j > 3 || i > 3)
                    {
                        multiplier = 5;
                    }

                    plotInfo.price = 10000 + (500 * multiplier);

                    plots[i, j] = plotInfo;
                }
            }
        }
    }

    public void UpdateCameraBounds()
    {
        float upBound = 2, leftBound = 2, rightBound = 0, downBound = 0;

        // Get basic bounds
        for (int i = 0; i < plots.GetLength(0); i++)
        {
            for (int j = 0; j < plots.GetLength(1); j++)
            {
                if (plots[i, j].xLocation > rightBound)
                {
                    rightBound = plots[i, j].xLocation;
                }

                if (plots[i, j].yLocation > downBound)
                {
                    downBound = plots[i, j].yLocation;
                }
            }
        }

        upBound = -16 + (upBound * 8) + 0.5f;
        rightBound = (rightBound - 3) * 8 - 0.5f;
        downBound = (downBound - 3) * 8 + 1.2f;
        leftBound = -16 + (leftBound * 8) + 0.5f;

        GameObject camera = GameObject.Find("Camera/Main Camera");
        if (camera == null) return;

        CameraControl cameraControl = camera.GetComponent<CameraControl>();
        if (cameraControl == null) return;

        cameraControl.cameraBound = (upBound, rightBound, downBound, leftBound);
    }

    private void DebugMatrix()
    {
        if (!debugMatrix) return;

        StringBuilder debug = new StringBuilder();
        for (int i = 0; i < plots.GetLength(0); i++)
        {
            for (int j = 0; j < plots.GetLength(1); j++)
            {
                debug.Append(plots[i, j].purchased);
                debug.Append(' ');
            }

            debug.AppendLine();
        }

        Debug.Log(debug.ToString());
    }
}
