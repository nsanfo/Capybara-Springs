using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNode : MonoBehaviour
{
    private bool isSelected = false;

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
        if (!isSelected)
        {
            gameObject.GetComponent<Animator>().Play("HidePathNode");
        }
    }

    public void SnapNode()
    {
        gameObject.GetComponent<Animator>().Play("SnapPathNode");
    }

    public void UnsnapNode()
    {
        gameObject.GetComponent<Animator>().Play("UnsnapPathNode");
    }

    public void SetOff()
    {
        gameObject.SetActive(false);
    }

    public void SetSelected(bool selection)
    {
        isSelected = selection;
    }
}
