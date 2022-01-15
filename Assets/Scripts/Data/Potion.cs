using UnityEngine;

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

    public Potion() {}

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
        string newPotionName = "potion_" + bottle_shape + "(";
        Recipe r = DataController.recipes[recipe_id];
        if (r == null)
        {
            Debug.LogError("[Potion.GetID] No recipe found with ID \"" + recipe_id + "\"!");
        }
        foreach (string ing in r.ingredient_seq)
        {
            newPotionName += ing + "_";
        }
        return newPotionName.Substring(0, newPotionName.Length - 1) + ")";
    }

    public Color GetColor()
    {
        // Based on ingredients & mistakes
        float r = 0f;
        float g = 0f;
        float b = 0f;
        float a = 0f;
        foreach (string ingID in ingredients)
        {
            Ingredient ing = DataController.ingredients[ingID];
            if (ing == null)
            {
                Debug.LogError("[Potion.GetColor] No ingredient found with ID \"" + ingID + "\"!");
            }
            r += ing.color_r;
            g += ing.color_g;
            b += ing.color_b;
            a += ing.color_a;
        }
        r = r > 1f ? 1f : (r < 0f ? 0f : r);
        g = g > 1f ? 1f : (g < 0f ? 0f : g);
        b = b > 1f ? 1f : (b < 0f ? 0f : b);
        a = a > 1f ? 1f : (a < .25f ? .25f : a);

        return new Color(r, g, b, a);
    }
}
