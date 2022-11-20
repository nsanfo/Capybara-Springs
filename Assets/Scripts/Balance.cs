using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;

public class Balance : MonoBehaviour
{
    public double balance = 100000;
    public TextMeshProUGUI balanceText;

    // Start is called before the first frame update
    void Start()
    {
        balanceText.text = balance.ToString("C2");
    }

    public double GetBalance()
    {
        return balance;
    }

    public void AdjustBalance(double amount)
    {
        balance += amount;
        balanceText.text = balance.ToString("C2");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
