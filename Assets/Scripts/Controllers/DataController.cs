using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

public class DataController : MonoBehaviour
{
    public General debugGeneral;

    [Space(20f)]
    public string debugIngID;
    public Ingredient debugIngredient;

    [Space(20f)]
    public string debugRecID;
    public Recipe debugRecipe;

    [Space(20f)]
    public string debugContainerID;
    public ContainerItems debugContainerItems;

    [Space(20f)]
    public float autosaveTimer = 60f;
    public bool debugSaving = false;

    public static General genData;
    public static Dictionary<string, Recipe> recipes;
    public static Dictionary<string, Ingredient> ingredients;
    public static Dictionary<string, ContainerItems> containers;

    public const int bootleShapesNumber = 2;

    private float currentTimer = 0f;

    void Awake()
    {
        genData = LoadDataFile<General>("General", "general.json");
        ingredients = LoadGameData<Ingredient>(genData.ingredientIDs, "Ingredients", "ingredient");
        recipes = LoadGameData<Recipe>(genData.recipeIDs, "Recipes", "recipe");
        containers = LoadGameData<ContainerItems>(genData.labContainerIDs, "LabContainers", "container");
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

    private void OnDestroy()
    {
        Autosave();
    }

    public void Autosave()
    {
        if (debugSaving)
        {
            Debug.Log("[DataController.Autosave] Abort autosaving.");
        }
        else
        {
            Debug.Log("[DataController.Autosave] Autosaving...");
            SaveToDataFile<General>(genData, "General", "general.json");
            SaveGameData<Ingredient>(ingredients, "Ingredients", "ingredient");
            SaveGameData<Recipe>(recipes, "Recipes", "recipe");
            SaveGameData<ContainerItems>(containers, "LabContainers", "container");
        }
    }

    public static void AddNewIngredient(
        string id,
        float cooldown,
        float breakChance,
        float r, float g, float b, float a,
        Potion potionData = null)
    {
        try
        {
            ingredients.Add(id, new Ingredient(id, cooldown, breakChance, r, g, b, a, potionData));
            string[] temp = new string[genData.ingredientIDs.Length + 1];
            genData.ingredientIDs.CopyTo(temp, 0);
            temp[genData.ingredientIDs.Length] = id;
            genData.ingredientIDs = temp;
        }
        catch (ArgumentException)
        {
            Debug.LogWarning("[DataController.AddNewIngredient] Ingredient with ID \"" + id + "\" already exists!");
        }
    }

    public static void CreateNewRecipe(int mistakesAllowed, params string[] ingredients)
    {
        string id = Recipe.GetID(ingredients);
        recipes.Add(id, new Recipe(mistakesAllowed, ingredients));
        string[] temp = new string[genData.recipeIDs.Length + 1];
        genData.recipeIDs.CopyTo(temp, 0);
        temp[genData.recipeIDs.Length] = id;
        genData.recipeIDs = temp;
    }

    private Dictionary<string, T> LoadGameData<T>(string[] ids, string folder, string prefix)
    {
        Dictionary<string, T> dict = new Dictionary<string, T>();
        foreach (string id in ids)
        {
            T data = LoadDataFile<T>(folder, prefix + "(" + id + ").json");
            dict.Add(id, data);
        }
        Debug.Log("[DataController.LoadGameData]: Loaded " + ids.Length + " [" + typeof(T).ToString() + "] game data files.");
        return dict;
    }

    private void SaveGameData<T>(Dictionary<string, T> dict, string folder, string prefix)
    {
        foreach (var entry in dict)
        {
            SaveToDataFile<T>(entry.Value, folder, prefix + "(" + entry.Key + ").json");
        }
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
                        Debug.LogWarning("[DataController.LoadDataFile] Unable to read data file \"" + absPath + "\"!");
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
                            Debug.LogError("[DataController.LoadDataFile] What a fuck!?");
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

    public void SaveGameDataDebruh()
    {
        Debug.Log("[DataController] Debug saving...");

        SaveToDataFile<General>(debugGeneral, "General", "general.json");
        SaveToDataFile<Ingredient>(debugIngredient, "Ingredients", "ingredient(" + debugIngID + ").json");
        SaveToDataFile<Recipe>(debugRecipe, "Recipes", "recipe(" + debugRecID + ").json");
        SaveToDataFile<ContainerItems>(debugContainerItems, "LabContainers", "container(" + debugContainerID + ").json");
    }
}
