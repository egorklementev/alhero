public class HeroDeathAI : SomeAI
{
    public override void PrepareAction()
    {
        HeroMoveController.uiTookControl = true;
        DataController.genData.coins = 0;
    }

    public override void Act()
    {
    }
}