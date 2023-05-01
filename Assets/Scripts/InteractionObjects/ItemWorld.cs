using System.Collections;
using UnityEngine;
using UnityEngine.Localization.Settings;

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
                    StartCoroutine(MoveToSmoothly(owner.transform.position + new Vector3(0f, 2.5f + _vOffset, 0f))); // Center
                    break;
                case 1:
                    StartCoroutine(MoveToSmoothly(owner.transform.position + new Vector3(1f, 2.5f + _vOffset, -1f))); // Right
                    break;
                case 2:
                    StartCoroutine(MoveToSmoothly(owner.transform.position + new Vector3(-1f, 2.5f + _vOffset, 1f))); // Left
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

    private IEnumerator MoveToSmoothly(Vector3 destination)
    {
        float timer = 0;
        while (timer < logic.data.itemLerpTimer && isPickedUp)
        {
            timer += Time.fixedDeltaTime;
            gameObject.transform.position = Vector3.Slerp(
                gameObject.transform.position,
                destination,
                timer / logic.data.itemLerpTimer);
            yield return new WaitForFixedUpdate();
        }
    }

    private void Update()
    {
        anim.SetInteger("Slot", slot);
        anim.SetBool("IsPickedUp", isPickedUp); // This is bad, I know... However, I want to finish this fucking game!!!
    }

    public void SetPickedUp(bool pickedUp, int newSlot = -1, GameObject newOwner = null, float offset = 0f)
    {
        isPickedUp = pickedUp;
        SetPhysicsActive(!pickedUp);
        owner = newOwner;
        slot = newSlot;
        _vOffset = pickedUp ? offset : 0f;
        if (owner != null)
        {
            transform.position = new Vector3(0f, _vOffset, 0f) + owner.transform.position;
        }
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
                int coinNum = ((CoinWorld)this).Count;
                id = "picked_coin".Hash();
                DataController.genData.coins += coinNum;
                UIController.SpawnSideLine("coins_picked", new object[] { coinNum }, 3f);
                Destroy();
            }
            else if (slot != -1 && id != "picked_coin".Hash())
            {
                SetPickedUp(true, slot, other.gameObject, 1.5f);
                LogicController.PickedItems[slot] = this;
                UIController.SpawnSideLine($"item_picked", new object[] 
                { 
                    IsPotion() 
                        ? (this as PotionWorld).potionData 
                        : LocalizationSettings.StringDatabase.GetLocalizedString("Ingredients", item_name) 
                }, 3f);
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
