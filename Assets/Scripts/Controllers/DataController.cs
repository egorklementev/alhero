using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

public class DataController : MonoBehaviour
{
    public List<Recipe> debugRecipes;
    public List<Ingredient> debugIngredients;

    public static List<Recipe> recipes;
    public static List<Ingredient> ingredients;

    private float autosaveTimer = 60f;
    private float currentTimer = 0f;

    void Start()
    {
        ingredients = LoadIngredients();
        recipes = LoadRecipes();
    }

    void Update()
    {
        currentTimer -= Time.deltaTime;
        if (currentTimer < 0f)
        {
            currentTimer = autosaveTimer;
            Autosave();
        }
    }

    void Autosave()
    {
        Debug.Log("Autosaving...");
        // TODO: Some savings...
    }

    private List<Ingredient> LoadIngredients()
    {
        int index = 0;
        Ingredient i = LoadDataFile<Ingredient>("Ingredients", "ingredient_0.json");
        List<Ingredient> list = new List<Ingredient>();
        while (i != null)
        {
            list.Add(i);
            i = LoadDataFile<Ingredient>("Ingredients", "ingredient_" + (++index) + ".json");
        }
        Debug.Log("[DataController]: Loaded " + index + " ingredients.");
        return list;
    }

    private List<Recipe> LoadRecipes()
    {
        int index = 0;
        Recipe i = LoadDataFile<Recipe>("Recipes", "recipe_0.json");
        List<Recipe> list = new List<Recipe>();
        while (i != null)
        {
            list.Add(i);
            i = LoadDataFile<Recipe>("Recipes", "recipe_" + (++index) + ".json");
        }
        Debug.Log("[DataController]: Loaded " + index + " recipes.");
        return list;
    }

    public T LoadDataFile<T>(params string[] path)
    {
        string jsonData;
        string absPath = Path.Combine(Application.streamingAssetsPath, Path.Combine(path));
        try
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                    if (File.Exists(absPath))
                    {
                        jsonData = File.ReadAllText(absPath);
                    }
                    else
                    {
                        return default(T);
                    }
                    break;
                case RuntimePlatform.Android:
                    UnityWebRequest request = UnityWebRequest.Get(absPath);
                    request.SendWebRequest();
                    while (!request.isDone)
                    {
                        if (request.result == UnityWebRequest.Result.ConnectionError || 
                        request.result == UnityWebRequest.Result.DataProcessingError)
                        {
                            Debug.LogError("What a fuck!?");
                            break;
                        }
                    }
                    jsonData = request.downloadHandler.text;
                    break;
                default:
                    jsonData = File.ReadAllText(absPath);
                    break;
            }
        }
        catch (System.Exception)
        {
            throw;
        }
        T dataObj = JsonUtility.FromJson<T>(jsonData);
        return dataObj;
    }

    public void SaveToDataFile<T>(T objToSave, params string[] path)
    {
        string jsonData = JsonUtility.ToJson(objToSave);
        File.WriteAllText(Path.Combine(Application.streamingAssetsPath, Path.Combine(path)), jsonData);
    }

    public void SaveIngredientsDebug()
    {
        int index = 0;
        foreach (Ingredient i in debugIngredients)
        {
            SaveToDataFile<Ingredient>(i, "Ingredients", "ingredient_" + (index++) + ".json");
        }
    }

    public void SaveRecipesDebug()
    {
        int index = 0;
        foreach (Recipe r in debugRecipes)
        {
            SaveToDataFile<Recipe>(r, "Recipes", "recipe_" + (index++) + ".json");
        }
    }
}
