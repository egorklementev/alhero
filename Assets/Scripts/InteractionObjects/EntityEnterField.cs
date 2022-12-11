using UnityEngine;
using UnityEngine.UI;

public class EntityEnterField : EnterField
{
    protected override void InteractionLogic()
    {
        UIController.SpawnEntityPanel(groupToActivate);
    }

    protected override void OnTriggerStay(Collider other)
    {
        base.OnTriggerStay(other);         
        if (!UIController.ActiveGroup.Equals("none"))
        {
            UIController.TryToDespawnEntityPanel(groupToActivate);
        }
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
        UIController.TryToDespawnEntityPanel(groupToActivate);
    }
}