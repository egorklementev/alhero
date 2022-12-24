using UnityEngine;

public class HeroDeathAI : SomeAI
{
    public override void PrepareAction()
    {
        UIController.HideActiveGroups();
        HeroMoveController.uiTookControl = true;

        foreach (ItemWorld item in LogicController.PickedItems)
        {
            var randomDirection = Random.onUnitSphere * 5f;
            randomDirection.y = Mathf.Abs(randomDirection.y);
            if (item != null)
            {
                item.SetPickedUp(false);
                item.GetBody().AddRelativeForce(randomDirection, ForceMode.Impulse);
            }
        }
    }

    public override void Act()
    {
    }
}