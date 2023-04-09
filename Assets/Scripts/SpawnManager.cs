using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public GameObject capybara;

    public GameObject entrancePath;
    private Vector3 entranceForward;
    private float elapsedSpawnTime = 0;
    private float spawnLength = 5.0f;

    GameObject stats;
    private GameplayState gameplayStateScript;
    private CapybaraHandler capybaraHandlerScript;

    enum States { center, right, left };
    States states;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        stats = GameObject.Find("Stats");
        gameplayStateScript = stats.GetComponent<GameplayState>();

        capybaraHandlerScript = gameObject.GetComponent<CapybaraHandler>();

        entrancePath = GameObject.Find("EntrancePath");
        var entranceScript = entrancePath.GetComponent<Path>();
        entranceForward = entranceScript.spacedPoints[0] - entranceScript.spacedPoints[1];
        yield return new WaitForSeconds(0.01f);
    }

    // Update is called once per frame
    void Update()
    {
        elapsedSpawnTime += Time.deltaTime;

        if (elapsedSpawnTime > spawnLength)
        {
            if (capybaraHandlerScript.CapybaraCount() == 0)
            {
                StartCoroutine(SpawnCapybara());
                spawnLength = Random.Range(6, 13);
            }
            else
            {
                float weight = GetSpawnWeight() * 11;
                float random = Random.Range(0, 100);
                if (random < weight)
                {
                    StartCoroutine(SpawnCapybara());
                    spawnLength = Random.Range(6, 13);
                }
                else
                {
                    spawnLength = Random.Range(2, 5);
                }
            }

            elapsedSpawnTime = 0;
        }
    }

    float GetSpawnWeight()
    {
        if (gameplayStateScript.currentCapacity < 1)
        {
            return 0f;
        }

        // Weigh happiness
        float averageHappiness = 50;
        if (capybaraHandlerScript != null)
        {
            averageHappiness = capybaraHandlerScript.AverageHappiness();
        }

        float happinessCapacity = gameplayStateScript.currentCapacity;

        if (capybaraHandlerScript.CapybaraCount() >= happinessCapacity)
        {
            happinessCapacity = Mathf.RoundToInt(Mathf.Pow(happinessCapacity, 0.95f));
        }
        else
        {
            if (averageHappiness > 50)
            {
                happinessCapacity = Mathf.RoundToInt(Mathf.Pow((happinessCapacity + 0.5f), 1.025f));
            }
            else
            {
                happinessCapacity = Mathf.RoundToInt(Mathf.Pow((happinessCapacity - 0.5f), 0.975f));
            }
        }

        var currentCount = capybaraHandlerScript.CapybaraCount();
        if (currentCount == 0)
        {
            currentCount = 1;
        }

        return happinessCapacity / currentCount;
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

        if (capybaraHandlerScript != null)
        {
            capybaraHandlerScript.AddCapybara(newCapy);
        }

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
