using UnityEngine;
using UnityEngine.UI;

public class EntityEnterField : EnterField
{
    protected override void InteractionLogic()
    {
        UIController.SpawnEntityPanel(groupToActivate);
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
        UIController.TryToDespawnEntityPanel(groupToActivate);
    }
}