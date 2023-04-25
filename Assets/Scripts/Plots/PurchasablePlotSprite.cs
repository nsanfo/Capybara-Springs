using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PurchasablePlotSprite : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Color passive = new Color32(136, 216, 255, 255), unable = new Color32(241, 125, 69, 255), able = new Color32(125, 240, 87, 255);
    Balance balance;

    private double price;
    public int xLocation = 0, yLocation = 0;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        GameObject stats = GameObject.Find("Stats");
        if (stats == null) return;

        balance = stats.GetComponent<Balance>();
    }

    public void SetPrice(double price)
    {
        this.price = price;
        gameObject.transform.parent.Find("Text (TMP)").GetComponent<TextMeshPro>().text = "$" + price.ToString();
    }

    public void ResetMaterial()
    {
        if (spriteRenderer.color != passive)
        {
            spriteRenderer.color = passive;
        }
    }

    public void UpdateMaterial()
    {
        if (Purchasable())
        {
            if (spriteRenderer.color != able)
            {
                spriteRenderer.color = able;
            }
        } else
        {
            if (spriteRenderer.color != unable)
            {
                spriteRenderer.color = unable;
            }
        }
    }

    public bool Purchasable()
    {
        if (balance == null) return false;

        return balance.balance >= price;
    }
}
