using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNode : MonoBehaviour
{
    void Start()
    {
        gameObject.SetActive(false);
    }

    public void InitializeAnimator(RuntimeAnimatorController animatorController)
    {
        gameObject.AddComponent<Animator>();
        gameObject.GetComponent<Animator>().runtimeAnimatorController = animatorController;
    }

    public void UpdatePosition(Vector3 position)
    {
        gameObject.transform.position = position;
    }

    public void ShowNode()
    {
        gameObject.SetActive(true);
        gameObject.GetComponent<Animator>().Play("ShowPathNode");
    }

    public void HideNode()
    {
        gameObject.GetComponent<Animator>().Play("HidePathNode");
    }

    public void SetOff()
    {
        gameObject.SetActive(false);
    }
}
