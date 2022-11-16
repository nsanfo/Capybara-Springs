using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNode : MonoBehaviour
{
    public void InitializeAnimator(RuntimeAnimatorController animatorController)
    {
        gameObject.AddComponent<Animator>();
        gameObject.GetComponent<Animator>().runtimeAnimatorController = animatorController;
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

    public void SnapNode()
    {
        gameObject.GetComponent<Animator>().Play("SnapPathNode");
    }

    public void UnsnapNode()
    {
        gameObject.GetComponent<Animator>().Play("UnsnapPathNode");
    }

    public void SetInactive()
    {
        gameObject.SetActive(false);
    }

    public GameObject GetNodeGameObject()
    {
        return gameObject;
    }

    public Vector3 GetNodePosition()
    {
        return gameObject.transform.position;
    }
}
