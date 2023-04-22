using UnityEngine;

public class ItemOwnerAI : SomeAI
{
    public bool canTakeItems = true;
    public float vOffset = 0f;
    public float force = 3f;

    [Range(0f, 360f)]
    public float angleOffset = 45f;

    private ItemWorld _item = null;
    private Mode _mode = Mode.THROW_ITEM;

    public override void PrepareAction()
    {
        _mode = _item == null ? Mode.FIND_SOME_ITEM : Mode.THROW_ITEM;
        switch (_mode)
        {
            case Mode.FIND_SOME_ITEM:
                ItemWorld item = _aiManager.logic.GetClosestItem(_aiManager);
                if (item != null && !item.Equals(_item))
                {
                    WalkingAI wai = _aiManager.GetAI<WalkingAI>();
                    wai.SetNextState("Idle");
                    wai.SetDestination(item.transform.position);
                }
                break;
            case Mode.THROW_ITEM:
                _item.SetPickedUp(false);
                _item.GetBody().AddRelativeForce(
                    (transform.forward + new Vector3(0f, 1f, 0f)) * force, ForceMode.Impulse
                    );
                _item = null;
                break;
            default:
                break;
        }
    }

    public override void Act() 
    {
        switch (_mode)
        {
            case Mode.FIND_SOME_ITEM:
                _aiManager.Transition("Walking");
                break;
            case Mode.THROW_ITEM:
                _aiManager.Transition("Idle");
                break;
            default:
                break;
        }
    }

    public void SetItem(ItemWorld item)
    {
        _item = item;
    }

    public ItemWorld GetItem()
    {
        return _item;
    }

    public bool HasItem()
    {
        return _item != null; 
    }

    public void SetMode(Mode mode)
    {
        _mode = mode;
    }

    public void DropItemsIfAny() {
        SetMode(Mode.THROW_ITEM);
        PrepareAction();
    }

    public enum Mode 
    {
        FIND_SOME_ITEM,
        THROW_ITEM
    }
}