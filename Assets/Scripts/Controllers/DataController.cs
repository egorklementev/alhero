using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

public class DataController : MonoBehaviour
{
    public General debugGeneral;
    public List<Recipe> debugRecipes;
    public List<Ingredient> debugIngredients;
    public string debugContainerID;
    public ContainerItems debugContainerItems;

    public static General genData;
    public static List<Recipe> recipes;
    public static List<Ingredient> ingredients;
    public static Dictionary<string, ContainerItems> containers;

    public const int bootleShapesNumber = 2;

    private float autosaveTimer = 60f;
    private float currentTimer = 0f;

    void Awake()
    {
        genData = LoadDataFile<General>("general.json");
        ingredients = LoadIngredients();
        recipes = LoadRecipes();
        containers = LoadContainers();
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
    
    private void OnDestroy() {
        Autosave();
    }

    public void Autosave()
    {
        Debug.Log("Autosaving...");

        SaveToDataFile<General>(genData, "general.json");
        SaveIngredients();
        SaveRecipes();
        SaveContainers();
    }

    public static void AddNewIngredient(
        string id,
        float cooldown,
        float breakChance,
        float r,
        float g,
        float b,
        float a)
    {
        ingredients.Add(new Ingredient(id, cooldown, breakChance, r, g, b, a));

        /*
        Debug.Log("---");
        Debug.Log("Ingredients: ");
        foreach (Ingredient i in ingredients)
        {
            Debug.Log(i.id);
        }
        Debug.Log("---");
        */
    }

    public static void CreateNewRecipe(int mistakesAllowed, params string[] ingredients)
    {
        recipes.Add(new Recipe(mistakesAllowed, ingredients));

        /*
        Debug.Log("---");
        Debug.Log("Recipes: ");
        foreach (Recipe r in recipes)
        {
            Debug.Log(r.GetID());
        }
        Debug.Log("---");
        */
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

    private Dictionary<string, ContainerItems> LoadContainers()
    {
        Dictionary<string, ContainerItems> dict = new Dictionary<string, ContainerItems>();
        for (int i = 0; i < genData.labContainerIDs.Length; i++)
        {
            string containerID = genData.labContainerIDs[i];
            ContainerItems contItems = LoadDataFile<ContainerItems>("LabContainers", "container_" + containerID + ".json");
            dict.Add(containerID, contItems);
        }
        Debug.Log("[DataController]: Loaded " + genData.labContainerIDs.Length + " containers.");
        return dict;
    }

    private void SaveIngredients()
    {
        for (int i = 0; i < ingredients.Count; i++)
        {
            SaveToDataFile<Ingredient>(ingredients[i], "Ingredients", "ingredient_" + i + ".json");
        }
    }

    private void SaveRecipes()
    {
        for (int i = 0; i < recipes.Count; i++)
        {
            SaveToDataFile<Recipe>(recipes[i], "Recipes", "recipe_" + i + ".json");
        }
    }

    private void SaveContainers()
    {
        foreach (var contItems in containers)
        {
            SaveToDataFile<ContainerItems>(contItems.Value, "LabContainers", "container_" + contItems.Key + ".json");
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
                        Debug.LogWarning("Unable to read data file: " + absPath);
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

    public void SaveGameDataDebruh()
    {
        Debug.Log("Debug saving...");

        int index = 0;
        SaveToDataFile<General>(debugGeneral, "general.json");
        foreach (Ingredient i in debugIngredients)
        {
            SaveToDataFile<Ingredient>(i, "Ingredients", "ingredient_" + (index++) + ".json");
        }
        index = 0;
        foreach (Recipe r in debugRecipes)
        {
            SaveToDataFile<Recipe>(r, "Recipes", "recipe_" + (index++) + ".json");
        }
        //SaveToDataFile<ContainerItems>(debugContainerItems, "LabContainers", "container_" + debugContainerID + ".json");
    }
}
