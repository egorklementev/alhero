using UnityEngine;

public class EntityEnterField : EnterField
{
    protected override void InteractionLogic()
    {
        UIController.SpawnEntityPanel(groupToActivate);
    }

    protected override void OnTriggerStay(Collider other)
    {
        base.OnTriggerStay(other);         
        if (other.gameObject.CompareTag("Player") && !UIController.ActiveGroup.Equals("none"))
        {
            UIController.TryToDespawnEntityPanel(groupToActivate);
        }
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
        if (other.gameObject.CompareTag("Player"))
        {
            UIController.TryToDespawnEntityPanel(groupToActivate);
        }
    }
}