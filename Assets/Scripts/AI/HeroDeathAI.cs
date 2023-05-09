using UnityEngine;

public class HeroDeathAI : SomeAI
{
    public override void PrepareAction()
    {
        UIController.HideActiveGroups();
        HeroMoveController.uiTookControl = true;
        DataController.genData.deaths++;
        DataController.UpdateTotalScore(-100);

        foreach (ItemWorld item in LogicController.PickedItems)
        {
            var randomDirection = Random.onUnitSphere * 3f;
            randomDirection.y = Mathf.Abs(randomDirection.y);
            if (item != null)
            {
                item.SetPickedUp(false, 0);
                item.GetBody().AddRelativeForce(randomDirection, ForceMode.Impulse);
            }
        }
    }

    public override void Act()
    {
    }
}