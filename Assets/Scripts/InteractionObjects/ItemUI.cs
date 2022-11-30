using UnityEngine;

public class ItemUI : AbstractItem
{
    public Vector3 uiRotation; 
    private Animator animator;

    protected virtual void OnEnable() {
        transform.localRotation = Quaternion.Euler(uiRotation);
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

    public void PlayUnableAnimation()
    {
        animator.SetTrigger("Unable");
    }

    public void Destroy()
    {
        animator.SetBool("Destroy", true);
    }
}
