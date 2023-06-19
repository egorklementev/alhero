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

    [SerializeField] private RaccoonReward[] raccoonRewards;

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
    public Cauldron cauldron;
    public SpawnController spawner;
    public LogicController logic;

    public static General genData;
    public static Queue<HistoryEntry> history;
    public static Dictionary<int, Recipe> recipes;
    public static Dictionary<int, Ingredient> ingredients;
    public static Dictionary<int, LabContainerItems> labContainers;
    public static int newSeed = int.MaxValue;
    public static SysRandom random = new SysRandom();
    public static List<int> currentHistoryIngs = new List<int>();

    public const int bootleShapesNumber = 4;
    public const int maximumRecipes = 100;

    private static string[] dataFolders = new string[] { "General", "Ingredients", "Recipes", "LabContainers", "History" };
    private static string[] dataFiles = new string[] { "general.json", "ingredients.json", "recipes.json", "lab_containers.json", "history.json" };

    private float autoSaveTimer = 0f;

    private static List<int> _notIngredientsList = new List<int>();
    private static RaccoonReward[] rewards = new RaccoonReward[4];
    private static int locGensBeforBuyItemsUpdate = 5;

    void Awake()
    {
        Application.targetFrameRate = 60;
        random = new SysRandom();

        for (int i = 0; i < dataFolders.Length; i++)
        {
            SetupDataFile(dataFolders[i], dataFiles[i]);
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

        currentHistoryIngs.Clear();
        $"Adding current hist ings: {string.Join(", ", genData.cauldronInventory)}".Log(this, "Awake()");
        foreach (int id in genData.cauldronInventory)
        {
            currentHistoryIngs.Add(id);
        }

        _notIngredientsList = new List<int>(genData.notIngredients);

        if (raccoonRewards != null) 
        {
            Array.Copy(raccoonRewards, rewards, raccoonRewards.Length);
        }

        if (genData.seed == 0)
        {
            logic.StartNewGame();
        }

        if (locationName != "lab")
        {
            genData.locationGenerations++;
            UpdateTotalScore(-250);
        }

        if (locGensBeforBuyItemsUpdate-- <= 0)
        {
            // UpdateOldmanItems();
            UpdateRaccoonRequestItemAndReward();
            locGensBeforBuyItemsUpdate = 5;
        }

        Random.InitState(genData.seed + genData.locationGenerations);
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

            if (cauldron != null)
            {
                $"Saving own cauldron inventory: {string.Join(", ", cauldron.inventory)}".Log(this, "OnDestroy()");
                DataController.genData.cauldronInventory = new int[cauldron.inventory.Count];
                cauldron.inventory.CopyTo(DataController.genData.cauldronInventory);
                $"Saving data cauldron inventory: {string.Join(", ", DataController.genData.cauldronInventory)}".Log(this, "OnDestroy()");
            }
            
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
        if (history.Count >= 9)
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

    public static Recipe GenerateRandomRecipe(float estimateComplexity, int maxTrials = 128)
    {
        int trials = 0;
        Recipe closestRecipe = null;
        Recipe rec = GetRandomRecipe(estimateComplexity);
        float complexityDiff = Math.Abs(rec.GetComplexity() - estimateComplexity);
        while ((complexityDiff > .1f * estimateComplexity || HasOverlaps(rec)) && trials++ < maxTrials)
        {
            rec = GetRandomRecipe(estimateComplexity);
            complexityDiff = Math.Abs(rec.GetComplexity() - estimateComplexity);

            if ((closestRecipe == null) || (complexityDiff < Math.Abs(closestRecipe.GetComplexity() - estimateComplexity)))
            {
                // $"Complexity of new recipe: ({rec.GetComplexity()}).".Log();
                closestRecipe = rec;
            }
        }

        if (trials >= maxTrials)
        {
            // "Selecting closest recipe.".Log();
            rec = closestRecipe;
        }

        $"Trials for recipe generation: {trials}".Log();
        $"Recipe final complexity: {rec.GetComplexity()}, estimated: {estimateComplexity}".Log();

        return rec;
    }

    private static Recipe GetRandomRecipe(float estimateComplexity) 
    {
        // Every 4 potions coocked increase maximum number of ingredients in a recipe
        int ingNum = Random.Range(3, 4 + genData.potionsCooked / 10);
        int mistakes = Random.Range(0, Mathf.FloorToInt(.666f * ingNum) + 1);
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
            randomIndices.Add(i);
        } 

        float isKnownAccum = .2f * Random.value;
        while (randomIndices.Count > 0)
        {
            int randIndexIndex = Random.Range(0, randomIndices.Count);
            int randIndex = randomIndices[randIndexIndex];
            randomIndices.RemoveAt(randIndexIndex);

            if (!ingredients[ings[randIndex]].isPotion)
            {
                isIngKnown[randIndex] = Random.value > isKnownAccum;
            }
            else
            {
                isIngKnown[randIndex] = true;
            }

            isKnownAccum *= .7f;
        }

        return new Recipe(mistakes, ings, isIngKnown);
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

    public static void UpdateRaccoonRequestItemAndReward()
    {
        var lst = ingredients.Where(ing => ing.Value.hasBeenDiscovered).Select(ing => ing.Key).ToList();
        genData.raccoonRequestedItem = lst[Random.Range(0, lst.Count)];

        float totalWeight = rewards.Sum(r => r.chance_weight);
        float[] chances = rewards.Select(r => r.chance_weight / totalWeight).ToArray();
        Array.Sort(chances);
        float currentChance = chances[0];
        float dice = Random.value;
        // $"Raccon reward dice: {dice}".Log();
        int index = 0;
        while (dice >= currentChance && index < chances.Length - 1)
        {
            currentChance += chances[index + 1];
            index++;
        }

        var randomReward = rewards[index];
        genData.raccoonRewardItem = randomReward.item_id.Hash();
        genData.locGensBeforBuyItemsUpdate = 5;
    }

    public static void UpdateOldmanItems()
    {
        for (int i = 0; i < OldmanAI.ITEMS_TO_SELL; i++)
        {
            var lst = ingredients.Where(ing => ing.Value.hasBeenDiscovered).Select(ing => ing.Key).ToList();
            var randId = Random.Range(0, lst.Count);
            genData.oldmanItemsForSale[i] = lst[randId];
            lst.Remove(randId);
            
        }
    }

    public static void UpdateTotalScore(int value)
    {
        if (genData.totalScore + value < 0)
        {
            genData.totalScore = 0;
        }
        else
        {
            genData.totalScore += value;
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

    public static string GetIngredientName(int id)
    {
        if (ingredients[id].isPotion)
        {
            // Nasty one
            var ingLen = ingredients[id].potionData.ingredients.Length;
            return GetIngredientName(ingredients[id].potionData.ingredients[Math.Abs(ingredients[id].ing_name.Hash() % ingLen)]);
        }
        else
        {
            return ingredients[id].ing_name;
        }
    }

    public void StartNewGame()
    {
        Environment.NewLine.Log(this);
        "Starting a new game...".Log(this);
        Environment.NewLine.Log(this);

        // Remove all data files
        for (int i = 0; i < dataFolders.Length; i++)
        {
            var folder = Path.Combine(Application.persistentDataPath, dataFolders[i]);
            var file = Path.Combine(Application.persistentDataPath, dataFiles[i]);
            Directory.Delete(folder, true);
            SetupDataFile(folder, file);
        }
        
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

        // Initial game values
        genData.locationGenerations = 0;
        genData.raccoonRequestedItem = 0;
        genData.raccoonRewardItem = 0;
        genData.oldmanItemsForSale = new int[2];
        genData.coins = 0;
        genData.maxPigeons = 3;
        genData.potionsCooked = 0;
        genData.locGensBeforBuyItemsUpdate = 5;
        // ---
        genData.totalScore = 0;
        genData.deaths = 0;
        genData.containersUnlocked = 0;
        genData.ingsUsed = 0;
        genData.itemsBought = 0;
        genData.itemsBrought = 0;
        genData.moneyCollected = 0;
        genData.moneyEarned = 0;
        genData.moneySpent = 0;
        genData.potionsFailed = 0;

        history.Clear();

        int cont_index = 0;
        foreach (LabContainerItems lci in labContainers.Values)
        {
            int size = lci.items.Length;
            lci.items = new LabContainerItem[size];
            lci.isUnlocked = cont_index++ == 0;
        }

        // Randomize ingredient values
        // The stuff below basically means removal of all generated potions
        ingredients.Clear();
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
                        (int) (417.0 * Math.Log((Random.Range(0, 1000) / 100.0) + 1.0)),
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
        // foreach (int id in ingredients.Keys)
        foreach (int id in GetIngredientsByLocation(AbstractItem.ItemLocation.FOREST))
        {
            ingredients[id].hasBeenDiscovered = true;
        }

        // Add initial recipe
        recipes.Clear();
        for (int i = 0; i < 3; i++) 
        {
            CreateNewRecipe(GenerateRandomRecipe(4f));
        }

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

    [Serializable]
    private struct RaccoonReward
    {
        public string item_id;
        public int chance_weight;
        public object data;
    }
}
