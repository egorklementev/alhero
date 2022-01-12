[System.Serializable]
public class Potion
{
    public string recipe_id;
    public int bottle_shape;
    public string[] ingredients;

    public float color_r;
    public float color_g;
    public float color_b;
    public float color_a;

    public Potion(Potion other)
    {
        recipe_id = other.recipe_id;
        bottle_shape = other.bottle_shape;
        ingredients = new string[other.ingredients.Length];
        other.ingredients.CopyTo(ingredients, 0);
        color_r = other.color_r;
        color_g = other.color_g;
        color_b = other.color_b;
        color_a = other.color_a;
    }

    /// We need this to generate new recipes with other potions
    public string GetID()
    {
        string newPotionName = "potion("; 
        foreach (string ing in DataController.recipes.Find(recipe => recipe.GetID().Equals(recipe_id)).ingredient_seq)
        {
            newPotionName += ing + "_";
        }
        return newPotionName.Substring(0, newPotionName.Length - 1) + ")";
    }
}
