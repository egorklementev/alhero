using System.Collections;
using UnityEngine;

public class ItemWorld : AbstractItem
{
    public LogicController logic;

    private bool isPickedUp = false;
    private GameObject owner = null;
    private int slot = -1; // Where in player's inventory this item is stored [0-3]

    private Animator anim;
    private Rigidbody body;
    private CapsuleCollider clldr;
    private float _vOffset = 0f;

    protected virtual void OnEnable()
    {
        anim = GetComponentInChildren<Animator>();
        body = GetComponent<Rigidbody>();
        clldr = GetComponent<CapsuleCollider>();
    }

    private void FixedUpdate()
    {
        if (isPickedUp && owner != null)
        {
            switch (slot)
            {
                case 0:
                    // Center
                    StartCoroutine(LerpMove(owner.transform.position + new Vector3(0f, 2.5f + _vOffset, 0f), .1f));
                    break;
                case 1:
                    // Right
                    StartCoroutine(LerpMove(owner.transform.position + new Vector3(1f, 2.5f + _vOffset, -1f), .1f));
                    break;
                case 2:
                    // Left
                    StartCoroutine(LerpMove(owner.transform.position + new Vector3(-1f, 2.5f + _vOffset, 1f), .1f));
                    break;
                default:
                    break;
            }
        }

        if (!isPickedUp && owner == null && transform.position.y < -100f)
        {
            Destroy();
        }
    }

    private IEnumerator LerpMove(Vector3 target, float duration)
    {
        float time = 0;
        Vector3 startPosition = transform.position;
        while (time < duration)
        {
            transform.position = Vector3.Slerp(startPosition, target, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        transform.position = target;
    }

    private void Update()
    {
        anim.SetBool("IsPickedUp", isPickedUp); // This is bad, I know... However, I want to finish this fucking game!!!
        anim.SetInteger("Slot", slot);
    }

    public void SetPickedUp(bool pickedUp, int newSlot = -1, GameObject newOwner = null, float offset = 0f)
    {
        isPickedUp = pickedUp;
        SetPhysicsActive(!pickedUp);
        owner = newOwner;
        slot = newSlot;
        _vOffset = pickedUp ? offset : 0f;
    }

    public void SetSlot(int newSlot)
    {
        slot = newSlot;
    }

    public Rigidbody GetBody()
    {
        return body;
    }

    public void Destroy()
    {
        anim.SetBool("Destroy", true);
    }

    public void SetPhysicsActive(bool isActive)
    {
        body.useGravity = isActive;
        body.isKinematic = !isActive;
        clldr.enabled = isActive;
    }

    private void OnCollisionEnter(Collision other)
    {
        int slot = LogicController.GetFreeInvSlot();
        if (other.gameObject.CompareTag("Player"))
        {
            if (id == "coin".Hash())
            {
                id = "picked_coin".Hash();
                DataController.genData.coins += ((CoinWorld)this).Count;
                Destroy();
            }
            else if (slot != -1 && id != "picked_coin".Hash())
            {
                SetPickedUp(true, slot, other.gameObject, 1.5f);
                LogicController.PickedItems[slot] = this;
            }
        }
        else if (
            other.gameObject.TryGetComponent<ItemOwnerAI>(out var ai) && 
            ai.canTakeItems &&
            !ai.HasItem() && DataController.IsIngredient(id)
            )
        {
            SetPickedUp(true, 0, other.gameObject, ai.vOffset);
            ai.SetItem(this);
        }
    }
}
