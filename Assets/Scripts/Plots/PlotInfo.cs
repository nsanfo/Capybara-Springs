using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlotInfo : MonoBehaviour
{
    public double price = 0;
    public int xLocation = 0, yLocation = 0;
    public bool purchased = false;

    public void ChangeMaterial(Material material)
    {
        GetComponent<Renderer>().material = material;
    }
}
