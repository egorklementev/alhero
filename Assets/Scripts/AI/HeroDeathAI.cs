public class HeroDeathAI : SomeAI
{
    public override void PrepareAction()
    {
        HeroMoveController.uiTookControl = true;
    }

    public override void Act()
    {
    }
}