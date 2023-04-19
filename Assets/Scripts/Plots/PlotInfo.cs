using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlotInfo : MonoBehaviour
{
    private double price = 0;
    public int xLocation = 0, yLocation = 0;

    public double GetPrice()
    {
        return price;
    }

    public void SetPrice(double price)
    {
        this.price = price;
    }
}
