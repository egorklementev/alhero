using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemUI : AbstractItem
{
    private Animator animator;

    protected virtual void OnEnable() {
        foreach (Material m in GetComponent<MeshRenderer>().materials)
        {
            m.renderQueue = 3002;
        }
        animator = GetComponent<Animator>();
    }

    public void SetSelected(bool isSelected)
    {
        animator.SetBool("IsSelected", isSelected);
    }

    public void SetSmall()
    {
        animator.SetBool("IsSmall", true);
    }

    public void Destroy()
    {
        animator.SetBool("Destroy", true);
    }
}
