using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public GameObject capybara;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        yield return new WaitForSeconds(0.1f);
        var newCapy = GameObject.Instantiate(capybara);
        newCapy.transform.position = new Vector3(0, 0, 0);
        var capyInfo = newCapy.GetComponent<CapybaraInfo>();
        capyInfo.capyName = "Gort";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
