public class HeroDeathAI : SomeAI
{
    public override void PrepareAction()
    {
        UIController.HideActiveGroups();
        HeroMoveController.uiTookControl = true;
    }

    public override void Act()
    {
    }
}