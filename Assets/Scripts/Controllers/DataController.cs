using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

public class DataController : MonoBehaviour
{
    public General debugGeneral;

    [Space(20f)]
    public Ingredient[] dIng;

    [Space(20f)]
    public Recipe[] dRec;

    [Space(20f)]
    public string[] dContIDs;
    public ContainerItems[] dCont;

    [Space(20f)]
    public float autosaveTimer = 60f;
    public bool debugMode = false;
    public GameObject[] debugButtons;

    public static General genData;
    public static Queue<HistoryEntry> history;
    public static Dictionary<int, Recipe> recipes;
    public static Dictionary<int, Ingredient> ingredients;
    public static Dictionary<int, ContainerItems> containers;

    public const int bootleShapesNumber = 4;

    private float currentTimer = 0f;

    void Awake()
    {
        genData = LoadDataFile<General>("General", "general.json");

        ingredients = LoadGameData<Ingredient>(genData.ingredientIDs, "Ingredients", "ingredient");
        recipes = LoadGameData<Recipe>(genData.recipeIDs, "Recipes", "recipe");
        containers = LoadGameData<ContainerItems>(genData.labContainerIDs, "LabContainers", "container");

        History historyData = LoadDataFile<History>("History", "history.json");
        history = new Queue<HistoryEntry>();
        foreach (HistoryEntry he in historyData.list)
        {
            history.Enqueue(he);
        }
    }

    void Update()
    {
        currentTimer -= Time.deltaTime;
        if (currentTimer < 0f)
        {
            currentTimer = autosaveTimer;
            Autosave();
        }
        foreach (GameObject btn in debugButtons)
        {
            btn.SetActive(debugMode);
        }
    }

    private void OnDestroy()
    {
        Autosave();
    }

    public void Autosave()
    {
        if (debugMode)
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

            History historyData = new History(history.Count);
            int index = 0;
            foreach (HistoryEntry he in history)
            {
                historyData.list[index++] = he;
            }
            SaveToDataFile<History>(historyData, "History", "history.json");
        }
    }

    public static void AddNewIngredient(
        int id,
        string ing_name,
        float cooldown,
        float breakChance,
        float r, float g, float b, float a,
        Potion potionData = null)
    {
        try
        {
            ingredients.Add(id, new Ingredient(id, ing_name, cooldown, breakChance, r, g, b, a, potionData));
            int[] temp = new int[genData.ingredientIDs.Length + 1];
            genData.ingredientIDs.CopyTo(temp, 0);
            temp[genData.ingredientIDs.Length] = id;
            genData.ingredientIDs = temp;
            Debug.Log($"[DataController.AddNewIngredient] New ingredient added: \"{id}\" (\"{ing_name}\")");
        }
        catch (ArgumentException)
        {
            Debug.LogWarning($"[DataController.AddNewIngredient] Ingredient with ID \"{id}\" (\"{ing_name}\") already exists!");
        }
    }

    public static void CreateNewRecipe(int mistakesAllowed, params int[] ingredients)
    {
        Recipe newRecipe = new Recipe(mistakesAllowed, ingredients);
        int id = newRecipe.GetID();
        try
        {
            recipes.Add(id, new Recipe(mistakesAllowed, ingredients));
            int[] temp = new int[genData.recipeIDs.Length + 1];
            genData.recipeIDs.CopyTo(temp, 0);
            temp[genData.recipeIDs.Length] = id;
            genData.recipeIDs = temp;
            Debug.Log($"[DataController.CreateNewRecipe]: New recipe created: {newRecipe.GetID()}");
        }
        catch (ArgumentException)
        {
            Debug.LogWarning($"[DataController.CreateNewRecipe] Recipe with ID \"{id}\" already exists!");
        }
    }

    public static void CreateEmptyInventoryItems(int id, int size)
    {
        ContainerItems ci = new ContainerItems();
        ci.items = new ContainerItem[size];
        try
        {
            containers.Add(id, ci);
            int[] temp = new int[genData.labContainerIDs.Length + 1];
            genData.labContainerIDs.CopyTo(temp, 0);
            temp[genData.labContainerIDs.Length] = id;
            genData.labContainerIDs = temp;
            Debug.Log($"[DataController.CreateInventoryItems]: New ContainerItems object created: {id}");
        }
        catch (ArgumentException)
        {
            Debug.LogWarning($"[DataController.CreateEmptyInventoryItems] Container with ID \"{id}\" already exists!");
        }
    }

    public static void AddHistoryEntry(HistoryEntry he)
    {
        if (history.Count >= 10)
        {
            history.Dequeue();
        }
        history.Enqueue(he);
    }

    public static void AddHistoryIngredient(int id)
    {
        if (history.Count == 0)
        {
            AddHistoryEntry(new HistoryEntry());
        }
        int[] cur = history.ToArray()[history.Count - 1].ingredients;
        int len = cur == null ? 0 : cur.Length;
        int[] temp = new int[len + 1];
        cur?.CopyTo(temp, 0);
        temp[len] = id;
        history.ToArray()[history.Count - 1].ingredients = temp;
        Debug.Log($"[DataController.AddHistoryIngredient]: Ingredient \"{id}\" added.");
    }

    public void AddIngredientsDebug()
    {
        foreach (Ingredient i in dIng)
        {
            AddNewIngredient(
                i.ing_name.Hash(),
                i.ing_name,
                i.cooldown,
                i.breakChance,
                i.color_r, i.color_g, i.color_b, i.color_a
            );
        }
    }

    public void AddRecipesDebug()
    {
        foreach (Recipe r in dRec)
        {
            CreateNewRecipe(r.mistakes_allowed, r.ingredient_seq);
        }
    }

    public void AddContainersDebug()
    {
        for (int i = 0; i < dContIDs.Length; i++)
        {
            string name = dContIDs[i];
            CreateEmptyInventoryItems(name.Hash(), dCont[i].items.Length);
        }
    }

    private Dictionary<int, T> LoadGameData<T>(int[] ids, string folder, string prefix)
    {
        Dictionary<int, T> dict = new Dictionary<int, T>();
        foreach (int id in ids)
        {
            T data = LoadDataFile<T>(folder, prefix + "(" + id + ").json");
            dict.Add(id, data);
        }
        Debug.Log($"[DataController.LoadGameData]: Loaded {ids.Length} [{typeof(T).ToString()}] game data files.");
        return dict;
    }

    private void SaveGameData<T>(Dictionary<int, T> dict, string folder, string prefix)
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
                        Debug.LogWarning($"[DataController.LoadDataFile] Unable to read data file \"{absPath}\"!");
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
        string jsonData = JsonUtility.ToJson(objToSave, true);
        File.WriteAllText(Path.Combine(Application.streamingAssetsPath, Path.Combine(path)), jsonData);
    }
}
