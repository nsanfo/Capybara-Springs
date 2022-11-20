using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blueprint : MonoBehaviour
{
    public GameObject concrete;
    public double cost;

    public GameObject GetConcrete()
    {
        return concrete;
    }

    public double GetCost()
    {
        return cost;
    }
}
