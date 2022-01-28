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
