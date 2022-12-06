using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DetailsName : MonoBehaviour
{
    public TextMeshProUGUI detailsWindowName;
    public CapybaraInfo info;

    // Start is called before the first frame update
    void Start()
    {
        detailsWindowName.text = info.capyName;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
