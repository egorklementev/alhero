using UnityEngine;

[System.Serializable]
public class Potion
{
    public int recipe_id;
    public int bottle_shape;
    public int[] ingredients;
    public int[] titleIDs;

    public Potion() {}

    public Potion(Potion other)
    {
        recipe_id = other.recipe_id;
        bottle_shape = other.bottle_shape;
        ingredients = new int[other.ingredients.Length];
        other.ingredients.CopyTo(ingredients, 0);
        titleIDs = new int[other.titleIDs.Length];
        other.titleIDs.CopyTo(titleIDs, 0);
    }

    /// We need this to generate new recipes with other potions
    public int GetID()
    {
        Recipe r = DataController.recipes[recipe_id];
        if (r == null)
        {
            Debug.LogError($"[Potion.GetID] No recipe found with ID \"{recipe_id}\"!");
        }
        return r.GetID();
    }

    public string GenerateNameDebug()
    {
        string name = "potion(";
        foreach (int title_id in titleIDs)
        {
            name += DataController.ingredients[ingredients[title_id]].ing_name + "_";
        }
        return name.Substring(0, name.Length- 1) + ")";
    }

    public Color GetColor()
    {
        // Based on ingredients & mistakes
        float r = 0f;
        float g = 0f;
        float b = 0f;
        float a = 0f;
        foreach (int ingID in ingredients)
        {
            Ingredient ing = DataController.ingredients[ingID];
            if (ing == null)
            {
                Debug.LogError($"[Potion.GetColor] No ingredient found with ID \"{ingID}\"!");
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
