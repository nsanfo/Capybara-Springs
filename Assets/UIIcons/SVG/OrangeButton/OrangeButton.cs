using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OrangeButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Sprite selectedSprite;
    private Sprite originalSprite;

    private bool selected = false;
    Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        originalSprite = GetComponent<Image>().sprite;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (selected)
        {
            animator.Play("OrangeSelectedExpand");
            animator.SetBool("Hovered", true);
        }
        else
        {
            animator.Play("OrangeExpand");
            animator.SetBool("Hovered", true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        animator.SetBool("Hovered", false);
    }

    public void Clicked()
    {
        selected = !selected;
        animator.SetBool("Selected", selected);

        if (selected)
        {
            GetComponent<Image>().sprite = selectedSprite;
        }
        else
        {
            GetComponent<Image>().sprite = originalSprite;
        }
    }
}
