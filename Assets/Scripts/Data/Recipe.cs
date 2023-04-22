using System;
using System.Linq;

[System.Serializable]
public class Recipe : GameDataEntry
{
    public int[] ingredient_seq;
    public bool[] ingredient_known;
    public int mistakes_allowed;
    public bool is_unlocked;

    public Recipe(int mistakesAllowed, int[] ids, bool[] isKnown)
    {
        ingredient_seq = ids;
        ingredient_known = isKnown;
        mistakes_allowed = mistakesAllowed;
        is_unlocked = false;
        id = GetID();
    }

    /// Range is from 0 to L where L is the number of ingredients L: [2, inf]
    /// 0 - easiest recipe, L - hardest
    public float GetComplexity()
    {
        const int unknownCost = 2000;
        const float rarityCost = 1000f;

        int accum = unknownCost * ingredient_seq.Length;
        for (int i = 0; i < ingredient_seq.Length; i++)
        {
            var ingId = ingredient_seq[i];
            accum += DataController.ingredients[ingId].rarity;
            accum -= ingredient_known[i] ? 0 : unknownCost;
        }
        return (1f - (float) accum / ((unknownCost + rarityCost) * ingredient_seq.Length)) * ingredient_seq.Length;
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

    public bool IsIngredientKnown(int ingId)
    {
        return ingredient_known[Array.IndexOf(ingredient_seq, ingId)];
    }
}
