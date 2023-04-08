using System;
using UnityEngine;

public class ItemUI : AbstractItem
{
    public Vector3 uiRotation; 
    private Animator animator;

    private Action _onEnable;

    protected virtual void OnEnable() {
        transform.localRotation = Quaternion.Euler(uiRotation);
        foreach (Material m in GetComponent<MeshRenderer>().materials)
        {
            m.renderQueue = 3002;
        }
        animator = GetComponent<Animator>();

        _onEnable?.Invoke();
    }

    public void SetSelected(bool isSelected)
    {
        LazyAnimatorCall(() => animator.SetBool("IsSelected", isSelected));
    }

    public void SetSmall()
    {
        LazyAnimatorCall(() => animator.SetBool("IsSmall", true));
    }

    public void PlayUnableAnimation()
    {
        animator.SetTrigger("Unable");
    }

    public void Destroy()
    {
        animator.SetBool("Destroy", true);
    }

    private void LazyAnimatorCall(Action action)
    {
        if (animator == null)
        {
            _onEnable = action;
        }
        else
        {
            action.Invoke();
        }
    }
}
