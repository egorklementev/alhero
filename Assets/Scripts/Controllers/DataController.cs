using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using Random = UnityEngine.Random;
using SysRandom = System.Random;
using System.Linq;

public class DataController : MonoBehaviour
{
    public string locationName;
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
    public float itemLerpTimer = .1f;

    [Space(20f)]
    public SpawnController spawner;
    public LogicController logic;

    public static General genData;
    public static Queue<HistoryEntry> history;
    public static Dictionary<int, Recipe> recipes;
    public static Dictionary<int, Ingredient> ingredients;
    public static Dictionary<int, LabContainerItems> labContainers;
    public static int newSeed = int.MaxValue;
    public static SysRandom random = new SysRandom();


    public const int bootleShapesNumber = 4;

    private float autoSaveTimer = 0f;

    private static List<int> currentHistoryIngs = new List<int>();
    private static List<int> _notIngredientsList = new List<int>();

    void Awake()
    {
        Application.targetFrameRate = 60;
        random = new SysRandom();

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

        foreach (var ing in ingredients)
        {
            if (ing.Value.location == locationName)
            {
                ing.Value.hasBeenDiscovered = true;
            }
        }

        foreach (int id in genData.cauldronInventory)
        {
            currentHistoryIngs.Add(id);
        }

        _notIngredientsList = new List<int>(genData.notIngredients);

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
            "Abort autosaving.".Log(this);
        }
        else
        {
            "Autosaving...".Log(this);
            
            genData.notIngredients = _notIngredientsList.ToArray();
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
        string location,
        Potion potionData = null)
    {
        Ingredient ing = new Ingredient(id, ing_name, cooldown, breakChance, rarity, r, g, b, a, location, potionData);
        try
        {
            ingredients.Add(id, ing);
            $"New ingredient added: \"{id}\" (\"{ing_name}\")".Log();
        }
        catch (ArgumentException)
        {
            $"Ingredient with ID \"{id}\" (\"{ing_name}\") already exists!".Warn();
        }
        return ing;
    }

    public static Recipe CreateNewRecipe(Recipe rec)
    {
        return CreateNewRecipe(rec.mistakes_allowed, rec.ingredient_seq, rec.ingredient_known);
    }

    public static Recipe CreateNewRecipe(int mistakesAllowed, int[] ingredients, bool[] isKnown)
    {
        Recipe newRecipe = new Recipe(mistakesAllowed, ingredients, isKnown);
        int id = newRecipe.GetID();
        try
        {
            recipes.Add(id, new Recipe(mistakesAllowed, ingredients, isKnown));
            $"New recipe created: {newRecipe.GetID()}".Log();
        }
        catch (ArgumentException)
        {
            $"Recipe with ID \"{id}\" already exists!".Warn();
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
            $"New ContainerItems object created: {id}".Log();
        }
        catch (ArgumentException)
        {
            $"Container with ID \"{id}\" already exists!".Warn();
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
        $"Ingredient \"{id}\" added.".Log();
    }

    public static Ingredient GetWeightedIngredientFromList(List<int> ingIDs, System.Random rand = null)
    {
        int ingID = 0;
        int sum = 0;
        foreach (int ingHash in ingIDs)
        {
            sum += DataController.ingredients[ingHash].rarity;
        }
        int dice = rand == null ? Random.Range(0, sum) : rand.Next(0, sum);
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

    public static int GetWeightedItemFromList(List<int> ids, List<float> probs, System.Random rand = null)
    {
        float dice = rand == null ? Random.value + .001f : (float)rand.NextDouble() + .001f;
        float accum = 0f;
        int index = -1;
        while (dice > accum && index + 1 < probs.Count)
        {
            accum += probs[++index];
        }

        return ids[index];
    }

    public static Recipe GenerateRandomRecipe(float estimateComplexity, int ingNum = 0, int trials = 1024)
    {
        int maxIngredients = ingNum == 0 ? (int)(estimateComplexity * 2f) : ingNum;
        ingNum = Random.Range(2, maxIngredients + 1);
        int mistakes = Random.Range(0, Mathf.FloorToInt(.333f * ingNum) + 1);
        int[] ings = new int[ingNum];
        List<int> ingIDs = ingredients.Where(ing => ing.Value.hasBeenDiscovered).Select(ing => ing.Key).ToList();

        for (int i = 0; i < ingNum; i++)
        {
            ings[i] = ingIDs[Random.Range(0, ingIDs.Count)];
        }

        bool[] isIngKnown = new bool[ingNum];
        List<int> randomIndices = new List<int>(ingNum);
        for (int i = 0; i < ingNum; i++)
        {
            randomIndices[i] = i;
        } 

        float isKnownAccum = Random.value;
        while (randomIndices.Count > 0)
        {
            int randIndexIndex = Random.Range(0, randomIndices.Count);
            int randIndex = randomIndices[randIndexIndex];
            randomIndices.RemoveAt(randIndexIndex);

            if (!ingredients[ings[randIndex]].isPotion)
            {
                isIngKnown[randIndex] = Random.value < .5f * isKnownAccum;
                isKnownAccum *= .5f;
            }
            else
            {
                isIngKnown[randIndex] = true;
            }
        }

        Recipe rec = new Recipe(mistakes, ings, isIngKnown);
        if (rec.GetComplexity() > estimateComplexity || HasOverlaps(rec))
        {
            if (trials > 0)
            {
                rec = GenerateRandomRecipe(estimateComplexity, ingNum, trials - 1);
            }
        }

        $"Trials for recipe generation: {1024 - trials}".Log();
        return rec;
    }

    private static bool HasOverlaps(Recipe rec)
    {
        foreach (Recipe r in recipes.Values) 
        {
            int recLen = r.ingredient_seq.Length;
            int potRecLen = rec.ingredient_seq.Length;
            int index = 0;
            while (
                index < recLen && 
                index < potRecLen && 
                r.ingredient_seq[index] == rec.ingredient_seq[index]
                )
            {
                index++;
            }
            if (potRecLen == index && potRecLen <= recLen)
            {
                return true;
            }
        }
        return false;
    }

    public static void UpdateRaccoonRequestItem()
    {
        genData.raccoonRequestedItem = GetWeightedIngredientFromList(
            ingredients.Where(ing => ing.Value.hasBeenDiscovered).Select(ing => ing.Key).ToList()).id;
    }

    public static void UpdateOldmanItems()
    {
        for (int i = 0; i < OldmanAI.ITEMS_TO_SELL; i++)
        {
            genData.oldmanItemsForSale[i] = GetWeightedIngredientFromList(
                ingredients.Where(ing => ing.Value.hasBeenDiscovered).Select(ing => ing.Key).ToList()).id;
        }
    }

    public static bool IsIngredient(int id)
    {
        return !Array.Exists(genData.notIngredients, i => i == id);
    }

    public static bool ChangeCoins(int number)
    {
        if (!HasSufficientCoins(number))
            return false;

        genData.coins += number;
        return true;
    }

    public static bool HasSufficientCoins(int number)
    {
        if (genData.coins - number < 0)
            return false;

        return true;
    }

    public static List<int> GetIngredientsByLocation(AbstractItem.ItemLocation location)
    {
        return ingredients
            .Where(ing => ing.Value.location == location.ToString())
            .Select(ing => ing.Key)
            .ToList();
    }

    public void StartNewGame()
    {
        Environment.NewLine.Log(this);
        "Starting a new game...".Log(this);
        Environment.NewLine.Log(this);
        
        // In case of user set seed
        if (newSeed != int.MaxValue)
        {
            genData.seed = newSeed;
            newSeed = 0;
            $"New game seed is {genData.seed}.".Log(this);
        }

        // The very new game
        if (genData.seed == 0)
        {
            genData.seed = Random.Range(int.MinValue, int.MaxValue);
            $"New game started. The seed is {genData.seed}.".Log(this);
        }
        Random.InitState(genData.seed);

        genData.locationGenerations = 0;
        genData.raccoonRequestedItem = 0;
        genData.oldmanItemsForSale = new int[2];
        genData.coins = 0;
        genData.maxPigeons = 3;
        genData.potionsCooked = 0;

        history.Clear();

        foreach (LabContainerItems lci in labContainers.Values)
        {
            int size = lci.items.Length;
            lci.items = new LabContainerItem[size];
            lci.isUnlocked = false;
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
                if (IsIngredient(item.id))
                {
                    AddNewIngredient(
                        item.item_name.Hash(),
                        item.item_name,
                        Random.Range(0f, 6f),
                        Random.value * .03f,
                        Random.Range(0, 1000),
                        2f * (Random.value - .5f),
                        2f * (Random.value - .5f),
                        2f * (Random.value - .5f),
                        2f * (Random.value - .5f),
                        item.location.ToString()
                    );
                } 
            }
        }

        // Make forest items discovered, so that the DataController can produce recipes 
        foreach (int id in GetIngredientsByLocation(AbstractItem.ItemLocation.FOREST))
        {
            ingredients[id].hasBeenDiscovered = true;
        }

        // Add initial recipe
        recipes.Clear();
        CreateNewRecipe(GenerateRandomRecipe(1f, 3));

        // Unconditional autosave
        Autosave();
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
                i.color_r, i.color_g, i.color_b, i.color_a,
                i.location
            );
        }
    }

    public void AddRecipesDebug()
    {
        foreach (Recipe r in dRec)
        {
            CreateNewRecipe(r.mistakes_allowed, r.ingredient_seq, r.ingredient_known);
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
                $"Unable to read data file \"{absPath}\"!".Warn(this);
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
                    "What a fuck!?".Err(this);
                    break;
                }
            }
            string jsonData = request.downloadHandler.text;
            File.WriteAllText(Path.Combine(persPath, file), jsonData);
        }
    }
}
