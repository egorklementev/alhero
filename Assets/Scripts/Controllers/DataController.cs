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
    public LabContainerItems[] dCont;

    [Space(20f)]
    public float autosaveTimer = 60f;
    public bool debugMode = false;
    public GameObject[] debugButtons;

    public static General genData;
    public static Queue<HistoryEntry> history;
    public static Dictionary<int, Recipe> recipes;
    public static Dictionary<int, Ingredient> ingredients;
    public static Dictionary<int, LabContainerItems> labContainers;

    public const int bootleShapesNumber = 4;

    private float currentTimer = 0f;

    void Awake()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            string[] folders = new string[] { "General", "Ingredients", "Recipes", "LabContainers", "History" };
            string[] files = new string[] { "general.json", "ingredients.json", "recipes.json", "lab_containers.json", "history.json" };
            for (int i = 0; i < folders.Length; i++)
            {
                SetupAndroidDataFile(folders[i], files[i]);
            }
        }

        genData = LoadDataFile<General>("General", "general.json");

        ingredients = LoadDataFile<Ingredients>("Ingredients", "ingredients.json").GetDict();
        recipes = LoadDataFile<Recipes>("Recipes", "recipes.json").GetDict();
        labContainers = LoadDataFile<LabContainers>("LabContainers", "lab_containers.json").GetDict();

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

            SaveGameData<Ingredient>(ingredients, "Ingredients", "ingredients.json");
            SaveGameData<Recipe>(recipes, "Recipes", "recipes.json");
            SaveGameData<LabContainerItems>(labContainers, "LabContainers", "lab_containers.json");

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
            Debug.Log($"[DataController.CreateNewRecipe]: New recipe created: {newRecipe.GetID()}");
        }
        catch (ArgumentException)
        {
            Debug.LogWarning($"[DataController.CreateNewRecipe] Recipe with ID \"{id}\" already exists!");
        }
    }

    public static void CreateEmptyInventoryItems(int id, int size)
    {
        LabContainerItems ci = new LabContainerItems();
        ci.id = id;
        ci.items = new LabContainerItem[size];
        try
        {
            labContainers.Add(id, ci);
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

    private void SaveGameData<T>(Dictionary<int, T> dict, string folder, string file) where T : GameDataEntry
    {
        GameData<T> gameData = new GameData<T>(dict.Count);
        int index = 0;
        foreach (var entry in dict)
        {
            gameData.list[index++] = entry.Value;
        }
        SaveToDataFile<GameData<T>>(gameData, folder, file);
    }

    public T LoadDataFile<T>(params string[] path)
    {
        string jsonData;
        string absPath;
        switch (Application.platform)
        {
            case RuntimePlatform.WindowsEditor:
                absPath = Path.Combine(Application.streamingAssetsPath, Path.Combine(path));
                break;
            case RuntimePlatform.Android:
                absPath = Path.Combine(Application.persistentDataPath, Path.Combine(path));
                break;
            default:
                absPath = Path.Combine(Application.streamingAssetsPath, Path.Combine(path));
                break;
        }
        try
        {
            if (File.Exists(absPath))
            {
                jsonData = File.ReadAllText(absPath);
            }
            else
            {
                Debug.LogWarning($"[DataController.LoadDataFile] Unable to read data file \"{absPath}\"!");
                return default(T);
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
        string absPath;
        switch (Application.platform)
        {
            case RuntimePlatform.WindowsEditor:
                absPath = Path.Combine(Application.streamingAssetsPath, Path.Combine(path));
                break;
            case RuntimePlatform.Android:
                absPath = Path.Combine(Application.persistentDataPath, Path.Combine(path));
                break;
            default:
                absPath = Path.Combine(Application.streamingAssetsPath, Path.Combine(path));
                break;
        }
        try
        {
            File.WriteAllText(absPath, jsonData);
        }
        catch (System.Exception)
        {
            throw;
        }
    }

    private void SetupAndroidDataFile(string folder, string file)
    {
        // Create "general.json" file
        string persPath = Path.Combine(Application.persistentDataPath, folder);
        string streamPath = Path.Combine(Application.streamingAssetsPath, folder, file);
        if (!Directory.Exists(persPath))
        {
            Directory.CreateDirectory(persPath);

            UnityWebRequest request = UnityWebRequest.Get(streamPath);
            request.SendWebRequest();
            while (!request.isDone)
            {
                if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.DataProcessingError)
                {
                    Debug.LogError("[DataController.SetupAndroidGenFile] What a fuck!?");
                    break;
                }
            }
            string jsonData = request.downloadHandler.text;
            File.WriteAllText(Path.Combine(persPath, file), jsonData);
        }
    }
}
