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

    /// Returns a value ranging from 0 to x 
    /// (where x is the complexity parameter)
    /// that shows recipe's complexity.
    public float GetComplexity()
    {
        float ingN = ingredient_seq.Length / 2f;
        float rarityAccum = 0f;
        float hiddenAccum = 7f;
        float stableAccum = 0f;
        float potionAccum = 0f;
        float mistakeAccum = -mistakes_allowed;

        for (int i = 0; i < ingredient_seq.Length; i++)
        {
            int id = ingredient_seq[i];
            if (DataController.ingredients[id].isPotion)
            {
                var potionComplexity = 
                    DataController.recipes[DataController.ingredients[id].potionData.recipe_id]
                    .GetComplexity() / 1.5f;
                potionAccum += potionComplexity;
            }
            else
            {
                rarityAccum += 3f / ((DataController.ingredients[id].rarity / 25f) + 1f);
            }

            stableAccum += 1f * (DataController.ingredients[id].breakChance / .03f);

            hiddenAccum *= ingredient_known[i] ? 1f : (i + 1); // Incomplete factorial
        }

        return ingN + rarityAccum + hiddenAccum + stableAccum + potionAccum + mistakeAccum;
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

    public bool IsIngredientKnown(int ingIndex)
    {
        return ingredient_known[ingIndex];
    }
}
