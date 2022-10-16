public class ContainerEnterField : EnterField
{
    protected override void InteractionLogic()
    {
        UIController.TriggerRightPanel();
        UIController.ActivateUIGroup(groupToActivate);
        LogicController.curContainer = transform.parent.gameObject.GetComponentInChildren<Container>();
    }
}