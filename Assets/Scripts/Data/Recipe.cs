[System.Serializable]
public class Recipe
{
    public string[] ingredient_seq;
    public int mistakes_allowed;
    public bool is_unlocked;

    public Recipe(int mistakesAllowed, params string[] ingredients)
    {
        ingredient_seq = ingredients;
        mistakes_allowed = mistakesAllowed;
        is_unlocked = false;
    }

    public static string GetID(string[] ingredients)
    {
        string id = "recipe(";
        foreach (string i in ingredients)
        {
            id += i + "_";
        }
        return id.Substring(0, id.Length - 1) + ")"; // Remove last '_'
    }
}
