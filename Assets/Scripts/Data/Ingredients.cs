using System.Collections.Generic;

[System.Serializable]
public class Ingredients
{
    public Ingredient[] list;

    public Ingredients(int capacity)
    {
        list = new Ingredient[capacity];
    }

    public Dictionary<int, Ingredient> GetDict()
    {
        Dictionary<int, Ingredient> dict = new Dictionary<int, Ingredient>();
        foreach (Ingredient i in list)
        {
            dict.Add(i.id, i);
        }
        return dict;
    }
}