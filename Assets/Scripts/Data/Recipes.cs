using System.Collections.Generic;

[System.Serializable]
public class Recipes
{
    public Recipe[] list;

    public Recipes(int capacity)
    {
        list = new Recipe[capacity];
    }

    public Dictionary<int, Recipe> GetDict()
    {
        Dictionary<int, Recipe> dict = new Dictionary<int, Recipe>();
        foreach (Recipe r in list)
        {
            dict.Add(r.id, r);
        }
        return dict;
    }
}