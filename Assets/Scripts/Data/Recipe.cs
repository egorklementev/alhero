[System.Serializable]
public class Recipe : GameDataEntry
{
    public int[] ingredient_seq;
    public int mistakes_allowed;
    public bool is_unlocked;

    public Recipe(int mistakesAllowed, params int[] ingredients)
    {
        ingredient_seq = ingredients;
        mistakes_allowed = mistakesAllowed;
        is_unlocked = false;
        id = GetID();
    }

    /// Range is from 0 to L where L is the number of ingredients L: [2, inf]
    public float GetComplexity()
    {
        int accum = 0;
        foreach (int i in ingredient_seq)
        {
            accum += DataController.ingredients[i].rarity;
        }
        return (1f - (float) accum / (1000f * ingredient_seq.Length)) * ingredient_seq.Length;
    }

    public int GetID()
    {
        string id = "";
        foreach (int i in ingredient_seq)
        {
            id += i.ToString() + "_";
        }
        return id.Hash();
    }
}
