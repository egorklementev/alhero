using UnityEngine;

public class PotionWorld : ItemWorld
{
    public Potion potionData;

    public static Color GetColor(Potion pData)
    {
        // Based on ingredients & mistakes
        float r = 0f;
        float g = 0f;
        float b = 0f;
        float a = 0f;
        foreach (string ingID in pData.ingredients)
        {
            Ingredient ing = DataController.ingredients[ingID];
            if (ing == null)
            {
                Debug.LogError("No ingredient found with ID: " + ingID + "!");
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
