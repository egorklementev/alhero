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

    /// Range is from 0 to 2L where L is the number of ingredients. L: [2, inf]
    /// 0 - easiest recipe, 2L - hardest
    /// 0 means all ingredients are common, 2L means all ingredients are hidden & unique
    public float GetComplexity()
    {
        const float unknownCost = 1.5f;
        const float rarityCost = 1000f;
        const float mistakesCost = 1f;

        int ingNum = ingredient_seq.Length;

        float rarityAccum = 0f;
        float unknownAccum = 0f;
        float mistakesAccum = mistakesCost * mistakes_allowed;
        for (int i = 0; i < ingNum; i++)
        {
            var ingId = ingredient_seq[i];
            rarityAccum += DataController.ingredients[ingId].rarity;
            unknownAccum += ingredient_known[i] ? 0f : unknownCost;
        }

        // Nasty one
        return (1f - rarityAccum / (ingNum * rarityCost)) * ingNum + unknownAccum - mistakesAccum;
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
