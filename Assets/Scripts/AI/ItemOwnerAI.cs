using UnityEngine;

public class ItemOwnerAI : SomeAI
{
    private ItemWorld _item = null;

    public override void PrepareAction()
    {
        if (_item == null)
        {
            ItemWorld item = _aiManager.logic.GetClosestItem(_aiManager);
            if (item != null && !item.Equals(_item))
            {
                WalkingAI wai = _aiManager.GetAI<WalkingAI>();
                wai.SetNextState("Idle");
                wai.SetDestination(item.transform.position);
            }
        }
        else
        {
            float force = 3f;
            float rot = transform.rotation.y;
            _item.SetPickedUp(false);
            _item.GetBody().AddRelativeForce(
                new Vector3(force * Mathf.Cos(rot), force, .25f * force * Mathf.Sin(rot)), ForceMode.Impulse
                );
            _item = null;
        }
    }

    public override void Act() 
    {
        if (_item == null)
        {
            _aiManager.Transition("Walking");
        }
        else
        {
            _aiManager.Transition("Idle");
        }
    }

    public void SetItem(ItemWorld item)
    {
        _item = item;
    }

    public bool HasItem()
    {
        return _item != null; 
    }
}