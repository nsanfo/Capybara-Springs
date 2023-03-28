using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public GameObject capybara;

    public GameObject entrancePath;
    private Vector3 entranceForward;

    enum States { center, right, left };
    States states;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        entrancePath = GameObject.Find("EntrancePath");
        var entranceScript = entrancePath.GetComponent<Path>();
        entranceForward = entranceScript.spacedPoints[0] - entranceScript.spacedPoints[1];
        yield return new WaitForSeconds(0.01f);
        StartCoroutine(SpawnCapybara());
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator SpawnCapybara()
    {
        var newCapy = GameObject.Instantiate(capybara);
        var capyInfo = newCapy.GetComponent<CapybaraInfo>();
        var capyAI = newCapy.GetComponent<CapyAI>();
        float pathDistance = 0;
        capyInfo.capyName = CapyNames.GetRandomName();
        newCapy.transform.position = new Vector3(0, 0, 0);
        newCapy.transform.Rotate(Vector3.up, 45);
        states = States.center;

        while (true)
        {
            yield return 0;
            if (capyAI.BodyCollisions > 0)
            {
                if (states == States.center)
                {
                    states = States.right;
                    newCapy.transform.Translate(Vector3.right * 0.13f);
                    pathDistance += 0.13f;
                }
                else if (states == States.right)
                {
                    if (pathDistance < 0.26)
                    {
                        newCapy.transform.Translate(Vector3.right * 0.13f);
                        pathDistance += 0.13f;
                    }
                    else
                    {
                        states = States.left;
                        newCapy.transform.position = new Vector3(0, 0, 0);
                        newCapy.transform.Translate(Vector3.left * 0.13f);
                        pathDistance = -0.13f;
                    }
                }
                else if (states == States.left)
                {
                    if (pathDistance > -0.26)
                    {
                        newCapy.transform.Translate(Vector3.left * 0.13f);
                        pathDistance -= 0.13f;
                    }
                    else
                    {
                        states = States.center;
                        newCapy.transform.position = new Vector3(0, 0, 0);
                        pathDistance = 0;
                    }
                }
            }
            else
                break;
        }
    }
}
