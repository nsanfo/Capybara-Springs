using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonBuildType : MonoBehaviour
{
    [Header("Plot Buyer")]
    public PlotBuyer plotBuyer;

    public BuildType buildType;
    public bool popout;
    private BuildToggleHandler toggleHandler;

    void Start()
    {
        toggleHandler = transform.parent.GetComponent<BuildToggleHandler>();
    }

    public void UpdateBuildToggle()
    {
        toggleHandler.UpdateBuildingType(this);
    }
}
