using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class PlotManager : MonoBehaviour
{
    private bool[,] ownedPlots = new bool[5, 5];

    public Material plotMaterial, unsoldMaterial;

    void Start()
    {
        // Look for existing buildable terrain to populate the 2D plot array
        GameObject buildableTerrain = GameObject.Find("PlotManager/TerrainHolder/Buildable");
        if (buildableTerrain == null) return;

        foreach (Transform child in buildableTerrain.transform)
        {
            if (!child.TryGetComponent<PlotInfo>(out var currentPlot)) continue;

            UpdateMatrix(currentPlot);
        }

        GenerateUnsoldTerrain();

        DebugMatrix();
    }

    private void ExpandMatrix(int x, int y)
    {
        // Create new array with expanded size
        bool[,] newMatrix = new bool[x, y];

        // Copy original array variables to new array
        for (int i = 0; i < ownedPlots.GetLength(0); i++)
        {
            for (int j = 0; j < ownedPlots.GetLength(1); j++)
            {
                newMatrix[i, j] = ownedPlots[i, j];
            }
        }

        ownedPlots = newMatrix;
    }
    private void UpdateMatrix(PlotInfo plot)
    {
        // Set current matrix lengths
        int xLength = ownedPlots.GetLength(1), yLength = ownedPlots.GetLength(0);

        // Check x length
        if (plot.xLocation + 3 > ownedPlots.GetLength(1))
        {
            xLength = plot.xLocation + 3;
        }

        // Check y length
        if (plot.yLocation + 3 > ownedPlots.GetLength(0))
        {
            yLength = plot.yLocation + 3;
        }

        // Check to expand matrix
        if (xLength > ownedPlots.GetLength(1) || yLength > ownedPlots.GetLength(0))
        {
            ExpandMatrix(yLength, xLength);
        }

        // Set the added plot to true
        ownedPlots[plot.yLocation, plot.xLocation] = true;
    }

    public void AddPlot(PlotInfo plot)
    {
        UpdateMatrix(plot);
        UpdateTerrain();
    }

    private void UpdateTerrain()
    {

    }

    private void GenerateUnsoldTerrain()
    {
        GameObject nonBuildableHolder = GameObject.Find("PlotManager/TerrainHolder/NonBuildable");
        if (nonBuildableHolder == null) return;

        for (int i = 0; i < ownedPlots.GetLength(0); i++)
        {
            for (int j = 0; j < ownedPlots.GetLength(1); j++)
            {
                if (!ownedPlots[i, j])
                {
                    GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
                    plane.transform.SetParent(nonBuildableHolder.transform);
                    plane.transform.position = new Vector3(-15 + (10 * i), 0, -15 + (10 * j));
                    plane.GetComponent<Renderer>().material = unsoldMaterial;
                }
            }
        }
    }

    private void DebugMatrix()
    {
        StringBuilder debug = new StringBuilder();
        for (int i = 0; i < ownedPlots.GetLength(0); i++)
        {
            for (int j = 0; j < ownedPlots.GetLength(1); j++)
            {
                debug.Append(ownedPlots[i, j]);
                debug.Append(' ');
            }

            debug.AppendLine();
        }

        Debug.Log(debug.ToString());
    }
}