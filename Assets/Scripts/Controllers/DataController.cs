using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using Random = UnityEngine.Random;

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
    public float autosaveDelay = 60f;
    public bool debugMode = false;
    public GameObject[] debugButtons;

    [Space(20f)]
    public SpawnController spawner;
    public LogicController logic;

    public static General genData;
    public static Queue<HistoryEntry> history;
    public static Dictionary<int, Recipe> recipes;
    public static Dictionary<int, Ingredient> ingredients;
    public static Dictionary<int, LabContainerItems> labContainers;
    public static int newSeed = 0;


    public const int bootleShapesNumber = 4;

    private float autoSaveTimer = 0f;

    private static List<int> currentHistoryIngs = new List<int>();

    void Awake()
    {
        Application.targetFrameRate = 60;

        string[] folders = new string[] { "General", "Ingredients", "Recipes", "LabContainers", "History" };
        string[] files = new string[] { "general.json", "ingredients.json", "recipes.json", "lab_containers.json", "history.json" };
        for (int i = 0; i < folders.Length; i++)
        {
            SetupDataFile(folders[i], files[i]);
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

        foreach (int id in genData.cauldronInventory)
        {
            currentHistoryIngs.Add(id);
        }

        if (genData.seed == 0)
        {
            logic.StartNewGame();
        }
        Random.InitState(genData.seed + genData.locationGenerations++);
    }

    void Update()
    {
        autoSaveTimer -= Time.deltaTime;
        if (autoSaveTimer < 0f)
        {
            autoSaveTimer = autosaveDelay;
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

    public static Ingredient AddNewIngredient(
        int id,
        string ing_name,
        float cooldown,
        float breakChance,
        int rarity,
        float r, float g, float b, float a,
        Potion potionData = null)
    {
        Ingredient ing = new Ingredient(id, ing_name, cooldown, breakChance, rarity, r, g, b, a, potionData);
        try
        {
            ingredients.Add(id, ing);
            Debug.Log($"[DataController.AddNewIngredient] New ingredient added: \"{id}\" (\"{ing_name}\")");
        }
        catch (ArgumentException)
        {
            Debug.LogWarning($"[DataController.AddNewIngredient] Ingredient with ID \"{id}\" (\"{ing_name}\") already exists!");
        }
        return ing;
    }

    public static Recipe CreateNewRecipe(int mistakesAllowed, params int[] ingredients)
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
        return newRecipe;
    }

    public static LabContainerItems CreateEmptyInventoryItems(int id, int size)
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
        return ci;
    }

    public static void AddHistoryEntry(HistoryEntry he)
    {
        if (history.Count >= 10)
        {
            history.Dequeue();
        }
        he.ingredients = new int[currentHistoryIngs.Count];
        currentHistoryIngs.CopyTo(he.ingredients);
        history.Enqueue(he);
        currentHistoryIngs.Clear();
    }

    public static void AddHistoryIngredient(int id)
    {
        currentHistoryIngs.Add(id);
        Debug.Log($"[DataController.AddHistoryIngredient]: Ingredient \"{id}\" added.");
    }

    public void StartNewGame()
    {
        Debug.Log(Environment.NewLine);
        Debug.Log("Starting a new game...");
        Debug.Log(Environment.NewLine);
        
        // In case of user set seed
        if (newSeed != 0)
        {
            genData.seed = newSeed;
            newSeed = 0;
            Debug.Log($"[DataController.StartNewGame]: New game seed is {genData.seed}.");
        }

        // The very new game
        if (genData.seed == 0)
        {
            genData.seed = Random.Range(int.MinValue, int.MaxValue);
            Debug.Log($"[DataController.StartNewGame]: New game started. The seed is {genData.seed}.");
        }
        Random.InitState(genData.seed);

        genData.locationGenerations = 0;

        history.Clear();

        foreach (LabContainerItems lci in labContainers.Values)
        {
            int size = lci.items.Length;
            lci.items = new LabContainerItem[size];
        }

        // Randomize ingredient values
        ingredients.Clear();
        // The stuff below basically means removal of all generated potions
        spawner.absItems.RemoveAll(item => item.IsPotion() && (
            (item as PotionUI == null ? (item as PotionWorld).potionData.recipe_id != 0 : (item as PotionUI).potionData.recipe_id != 0)
        ));
        foreach (AbstractItem item in spawner.absItems)
        {
            if (item as PotionWorld == null && item as PotionUI == null)
            {
                AddNewIngredient(
                    item.item_name.Hash(),
                    item.item_name,
                    Random.Range(0f, 6f),
                    Random.value * .15f, // TODO:
                    Random.Range(1, 1000),
                    2f * (Random.value - .5f),
                    2f * (Random.value - .5f),
                    2f * (Random.value - .5f),
                    2f * (Random.value - .5f)
                );
            }
        }

        // Add initial recipe
        recipes.Clear();
        GenerateRandomRecipe(3);

        // Unconditional autosave
        Autosave();
    }

    public static Ingredient GetWeightedIngredientFromList(List<int> ingIDs)
    {
        int ingID = 0;
        int sum = 0;
        foreach (int ingHash in ingIDs)
        {
            sum += DataController.ingredients[ingHash].rarity;
        }
        int dice = Random.Range(0, sum);
        int increment = 0;
        foreach (int ingHash in ingIDs)
        {
            increment += DataController.ingredients[ingHash].rarity;
            if (increment > dice)
            {
                ingID = ingHash;
                break;
            }
        }
        return ingredients[ingID];
    }

    public static Recipe GenerateRandomRecipe(int ingNum = 0)
    {
        int maxIngredients = ingNum == 0 ? ingredients.Count : ingNum;
        int mistakes = Random.Range(0, Mathf.FloorToInt(.333f * ingNum) + 1);
        ingNum = Random.Range(2, maxIngredients + 1);
        int[] ings = new int[ingNum];
        List<int> ingIDs = new List<int>(ingredients.Keys);
        for (int i = 0; i < ingNum; i++)
        {
            ings[i] = ingIDs[Random.Range(0, ingredients.Count)];
        }
        Recipe rec = new Recipe(mistakes, ings);
        CreateNewRecipe(rec.mistakes_allowed, rec.ingredient_seq);
        return rec;
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
                i.rarity,
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
        string absPath = Path.Combine(Application.persistentDataPath, Path.Combine(path));
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
        string absPath = Path.Combine(Application.persistentDataPath, Path.Combine(path));
        try
        {
            File.WriteAllText(absPath, jsonData);
        }
        catch (System.Exception)
        {
            throw;
        }
    }

    /// Creates a copy of data file located in Streaming Assets inside persistent data storage
    private void SetupDataFile(string folder, string file)
    {
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
                    Debug.LogError("[DataController.SetupDataFile] What a fuck!?");
                    break;
                }
            }
            string jsonData = request.downloadHandler.text;
            File.WriteAllText(Path.Combine(persPath, file), jsonData);
        }
    }
}
