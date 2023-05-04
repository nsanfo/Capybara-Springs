using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;

public class Balance : MonoBehaviour
{
    [Header("Information Handler")]
    public InformationHandler informationHandler;

    [Header("Balance")]
    public double balance = 100000;

    // Start is called before the first frame update
    void Start()
    {
        informationHandler.UpdateUIBalance(balance);
    }

    public double GetBalance()
    {
        return balance;
    }

    public void AdjustBalance(double amount)
    {
        balance += amount;
        informationHandler.UpdateUIBalance(balance);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
