using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class PlotBuyer : MonoBehaviour
{
    [Header("Mouse On UI")]
    public MouseOnUI mouse;

    private Vector3 originalPos = Vector3.positiveInfinity, plotPos = Vector3.positiveInfinity;
    private Quaternion originalRot = Quaternion.identity, plotRot = Quaternion.identity;
    private Quaternion plotViewAngle = Quaternion.Euler(new Vector3(90, -90, 0));

    private Vector3 targetPos = new Vector3(4, 15, 4);
    private Quaternion targetRot;

    private float duration = 0.75f, elapsedTime;
    public bool cameraAnimation = false;
    private bool zoom = true;

    // Mouse raycast
    public MouseRaycast mouseRaycast = new();

    // Building variables
    private BuildingModes buildingModes;

    // Camera
    private GameObject cameraObject;

    // Sprite
    private PurchasablePlotSprite previousPurchaseSprite;
    private GameObject spriteHolder;

    // Plot manager
    PlotManager plotManager;

    AudioSource buildSFX;
    AudioSource chainsawSound;

    void Start()
    {
        GameObject managerObject = GameObject.Find("PlotManager");
        if (managerObject != null)
        {
            plotManager = managerObject.GetComponent<PlotManager>();
        }

        BuildingUpgrade buildingScript = gameObject.GetComponent<BuildingUpgrade>();

        // Get building modes from building script
        buildingModes = buildingScript.buildingModes;

        // Get raycast from building script
        mouseRaycast = buildingScript.mouseRaycast;

        GameObject camera = GameObject.Find("Camera/Main Camera");
        if (camera != null)
        {
            cameraObject = camera;
        }

        GameObject spriteHolder = GameObject.Find("PlotManager/SpriteHolder");
        if (spriteHolder != null)
        {
            this.spriteHolder = spriteHolder;
        }

        var UISounds = GameObject.Find("UISounds");
        buildSFX = UISounds.transform.GetChild(2).GetComponent<AudioSource>();
        chainsawSound = UISounds.transform.GetChild(5).GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        HandleAnimation();

        if (!buildingModes.enablePlots) return;

        // Handle raycast to plot purchase sprites
        bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hitInfo);
        if (hit && hitInfo.transform.CompareTag("PlotPurchase"))
        {
            if (previousPurchaseSprite == null)
            {
                previousPurchaseSprite = hitInfo.collider.gameObject.GetComponent<PurchasablePlotSprite>();
            }

            // Update material
            if (previousPurchaseSprite != null)
            {
                previousPurchaseSprite.UpdateMaterial();
            }

            // Check for left-click
            if (Input.GetMouseButtonDown(0))
            {
                if (mouse.overUI) return;

                if (!hitInfo.transform.gameObject.TryGetComponent<PurchasablePlotSprite>(out var purchaseScript)) return;
                
                PurchasePlot(purchaseScript);
            }
        }
        else
        {
            // Reset material when no longer raycasting to sprite
            if (previousPurchaseSprite != null)
            {
                previousPurchaseSprite.ResetMaterial();
                previousPurchaseSprite = null;
            }
        }
    }

    private void PurchasePlot(PurchasablePlotSprite purchaseScript)
    {
        if (purchaseScript.Purchasable())
        {
            // Update plot info and update the matrix
            PlotInfo plotInfo = plotManager.GetPlotMatrix()[purchaseScript.yLocation, purchaseScript.xLocation];
            plotInfo.purchased = true;
            plotInfo.name = "Terrain[" + plotInfo.xLocation + "," + plotInfo.yLocation + "]";
            plotInfo.tag = "Terrain";
            plotManager.AddPlot(plotInfo);

            // Update material and parent for plot
            plotInfo.ChangeMaterial(plotManager.plotMaterial);
            plotInfo.transform.SetParent(plotManager.buildableHolder.transform);

            // Remove all outside deco in plot
            foreach (Transform child in plotInfo.gameObject.transform)
            {
                Destroy(child.gameObject);
            }

            buildSFX.Play();
            chainsawSound.Play();

            // Delete purchasing sprite
            GameObject scriptParent = purchaseScript.gameObject.transform.parent.gameObject;
            if (scriptParent != null)
            {
                Destroy(scriptParent);
            }

            // Adjust balance
            GameObject stats = GameObject.Find("Stats");
            Balance balance = null;
            if (stats != null)
            {
                balance = stats.GetComponent<Balance>();
            }

            if (balance != null)
            {
                balance.AdjustBalance(plotInfo.price * -1);
            }

            // Adjust gameplay state
            GameplayState gameplayState = stats.GetComponent<GameplayState>();
            gameplayState.AdjustPlotsPurchased();
            gameplayState.AdjustMoneySpent(plotInfo.price);

            // Update plot sprites
            UpdatePurchaseSprites();
        }
        else
        {
            // Cannot purchase 
        }
    }

    private void UpdatePurchaseSprites()
    {
        RemovePurchaseSprites();

        // Generate new sprites for plots left over
        List<PlotInfo> purchasablePlots = GetPurchasablePlots();
        foreach (PlotInfo plotInfo in purchasablePlots)
        {
            GeneratePurchaseSprite(plotInfo);
        }
    }

    public void InstantiatePurchaseSprites()
    {
        List<PlotInfo> purchasablePlots = GetPurchasablePlots();
        foreach (PlotInfo plotInfo in purchasablePlots)
        {
            GeneratePurchaseSprite(plotInfo);
        }
    }

    private void GeneratePurchaseSprite(PlotInfo plotInfo)
    {
        GameObject plotSprite = Instantiate(plotManager.plotPurchaseSprite);
        plotSprite.transform.parent = spriteHolder.transform;
        plotSprite.transform.localPosition = new Vector3(-12f + (8 * plotInfo.yLocation), 3, -12f + (8 * plotInfo.xLocation));
        plotSprite.name = "PurchaseSprite[" + plotInfo.xLocation + "," + plotInfo.yLocation + ")";

        PurchasablePlotSprite purchasePlot = plotSprite.transform.Find("PurchasingSprite").GetComponent<PurchasablePlotSprite>();
        if (purchasePlot != null)
        {
            purchasePlot.SetPrice(plotInfo.price);
            purchasePlot.xLocation = plotInfo.xLocation;
            purchasePlot.yLocation = plotInfo.yLocation;
        }
    }

    public void RemovePurchaseSprites()
    {
        foreach (Transform child in spriteHolder.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private List<PlotInfo> GetPurchasablePlots()
    {
        List<PlotInfo> purchasablePlots = new();
        PlotInfo[,] plots = plotManager.GetPlotMatrix();

        for (int i = 2; i < plots.GetLength(0) - 1; i++)
        {
            for (int j = 2; j < plots.GetLength(1) - 1; j++)
            {
                // If a purchasable plot is next to a purchased plot, add to array
                if (!plots[i, j].purchased)
                {
                    // Check for adjacency
                    if (CheckAdjacentPurchased(plots, plots[i, j]))
                    {
                        purchasablePlots.Add(plots[i, j]);
                    }
                }
            }
        }

        return purchasablePlots;
    }

    private bool CheckAdjacentPurchased(PlotInfo[,] plots, PlotInfo targetPlot)
    {
        PlotInfo up = plots[targetPlot.yLocation, targetPlot.xLocation - 1];
        if (up.purchased) return true;

        PlotInfo right = plots[targetPlot.yLocation + 1, targetPlot.xLocation];
        if (right.purchased) return true;

        PlotInfo down = plots[targetPlot.yLocation, targetPlot.xLocation + 1];
        if (down.purchased) return true;

        PlotInfo left = plots[targetPlot.yLocation - 1, targetPlot.xLocation];
        if (left.purchased) return true;

        return false;
    }

    private void HandleAnimation()
    {
        if (!cameraAnimation) return;

        if (originalPos == Vector3.positiveInfinity || originalRot == Quaternion.identity)
        {
            cameraAnimation = false;
            return;
        }

        elapsedTime += Time.deltaTime;
        float percentageComplete = elapsedTime / duration;

        cameraObject.transform.position = Vector3.Lerp(originalPos, targetPos, Mathf.SmoothStep(0, 1, percentageComplete));
        cameraObject.transform.rotation = Quaternion.Lerp(originalRot, targetRot, Mathf.SmoothStep(0, 1, percentageComplete));
        if (percentageComplete >= 1)
        {
            cameraAnimation = false;
            elapsedTime = 0;
        }
    }

    public void CameraZoomOut()
    {
        if (cameraObject.TryGetComponent<CameraControl>(out var cameraControl))
        {
            if (cameraControl.plotCamera)
            {
                return;
            }

            cameraControl.plotCamera = true;
        }

        targetPos = new Vector3(cameraObject.transform.position.x, 15, cameraObject.transform.position.z);
        targetRot = plotViewAngle;
        originalPos = cameraObject.transform.position;
        originalRot = cameraObject.transform.rotation;
        elapsedTime = 0;
        cameraAnimation = true;
    }

    public void CameraZoomBack()
    {
        
        if (cameraObject.TryGetComponent<CameraControl>(out var cameraControl))
        {
            if (!cameraControl.plotCamera)
            {
                return;
            }

            cameraControl.plotCamera = false;
        }

        targetPos = originalPos;
        targetRot = originalRot;
        originalPos = cameraObject.transform.position;
        originalRot = cameraObject.transform.rotation;
        elapsedTime = 0;
        cameraAnimation = true;
    }
}
