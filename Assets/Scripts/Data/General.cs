[System.Serializable]
public class General
{
    public int seed = 0;
    public int locationGenerations = 0;
    public int potionsCooked = 0;
    public int[] cauldronInventory;
    public int raccoonRequestedItem = 0;
    public int raccoonRewardItem = 0;
    public int coins = 0;
    public int[] notIngredients;
    public int[] oldmanItemsForSale;
    public int maxPigeons = 0;
    public int winningRecipeId = 0;
    public int locGensBeforBuyItemsUpdate = 5;

    // ---

    public long totalScore = 0L;
    public long deaths = 0L;
    public long ingsUsed = 0L;
    public long potionsFailed = 0L;
    public long moneyCollected = 0L;
    public long moneyEarned = 0L;
    public long moneySpent = 0L;
    public long itemsBought = 0L;
    public long itemsBrought = 0L;
    public long containersUnlocked = 0L;
}
