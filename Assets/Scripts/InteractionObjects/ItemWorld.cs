using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemWorld : MonoBehaviour
{
    public GameObject uiVersion;

    private bool isPickedUp = false;
    private GameObject owner = null;
    private int slot = -1; // Where in player's inventory this item is stored [0-3]

    private Animator anim;
    private Rigidbody body;
    private BoxCollider bCollider;

    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        body = GetComponent<Rigidbody>();
        bCollider = GetComponent<BoxCollider>();
    }

    private void FixedUpdate()
    {
        if (isPickedUp && owner != null)
        {
            switch (slot) {
                case 0:
                    transform.position = owner.transform.position + new Vector3(0f, 2.5f, 0f); // Center
                    break;
                case 1:
                    transform.position = owner.transform.position + new Vector3(1f, 2.5f, -1f); // Right
                    break;
                case 2:
                    transform.position = owner.transform.position + new Vector3(-1f, 2.5f, 1f); // Left
                    break;
                default:
                    break;
            }
        }
    }

    private void Update()
    {
        anim.SetBool("IsPickedUp", isPickedUp);
        anim.SetInteger("Slot", slot);
    }

    public void SetPickedUp(bool pickedUp, int newSlot = -1, GameObject newOwner = null)
    {
        isPickedUp = pickedUp;
        body.useGravity = !isPickedUp;
        body.isKinematic = isPickedUp;
        bCollider.enabled = !isPickedUp;
        owner = newOwner;
        slot = newSlot;
    }

    public void SetSlot(int newSlot) {
        slot = newSlot;
    }

    public Rigidbody GetBody()
    {
        return body;
    }

    private void OnCollisionEnter(Collision other)
    {
        int slot = LogicController.GetFreeInvSlot();
        if (other.gameObject.CompareTag("Player") && slot != -1)
        {
            SetPickedUp(true, slot, other.gameObject);
            LogicController.PickedItems[slot] = this;
        }
    }

}
