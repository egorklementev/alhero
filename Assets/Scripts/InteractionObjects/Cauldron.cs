using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Cauldron : MonoBehaviour
{
    public SpawnController spawner;

    [Space(10f)]
    public GameObject[] particles;
    public Animator spoonAnim;
    public GameObject finishBubbles;
    public AudioSource cookedSound;
    public AudioSource failedSound;
    public Transform cookedPotionAnchor;
    public List<int> inventory; // Current recipe

    [SerializeField] private Color regularColor;
    [SerializeField] private Color successColor;

    [Space(10f)]
    public UIController ui;

    private Material waterMat;
    private float cooldown = 0f;
    private int cooldownMistakes = 0;

    private void Start()
    {
        inventory = new List<int>();
        inventory.AddRange(DataController.genData.cauldronInventory);
        $"Adding inventory: {string.Join(", ", inventory)}".Log(this, "Start()");

        if (inventory.Count > 0)
        {
            SetRecipeCooking(true);
        }

        waterMat = GetComponentInChildren<Renderer>().materials[1];
    }

    private void Update()
    {
        if (cooldown > 0f)
        {
            cooldown -= Time.deltaTime;
        }
    }

    public void ClearInventory()
    {
        $"Clearing own inventory: {string.Join(", ", inventory)}".Log(this, "ClearInventory()");
        $"Clearing data inventory: {string.Join(", ", DataController.genData.cauldronInventory)}".Log(this, "ClearInventory()");
        // StartCoroutine(FadeWaterColor(Color.cyan, Color.white, 1f));
        DataController.genData.cauldronInventory = new int[] { };
        inventory.Clear();
        SetRecipeCooking(false);
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.CompareTag("Item"))
        {
            ItemWorld worldItem = other.gameObject.GetComponent<ItemWorld>();
            int id = worldItem.id;

            if (Array.Exists(DataController.genData.notIngredients, nId => nId == id))
            {
                return; // Do not count non-ingredients
            }

            inventory.Add(id);
            DataController.AddHistoryIngredient(id); // Anyway
            DataController.genData.ingsUsed++;

            if (cooldown > 0f)
            {
                cooldownMistakes++;
                cooldown = 0f;
            }

            Recipe potentialRecipe = InventoryHasPotential();
            if (potentialRecipe != null)
            {
                SetRecipeCooking(true); // Or continue

                Ingredient curIng = DataController.ingredients[id];
                if (curIng == null)
                {
                    $"No ingredient with name \"{id}\"!!!".Warn(this);
                    DestroyCurrentRecipe();
                }
                else
                {
                    cooldown = curIng.cooldown;

                    // In case we are 'lucky' - just break current recipe
                    if (Random.Range(0f, 1f) < curIng.breakChance)
                    {
                        DestroyCurrentRecipe();
                    }
                    else
                    {
                        // If a potion is ready
                        if (potentialRecipe.ingredient_seq.Length == inventory.Count)
                        {
                            if (IsInventoryValid(potentialRecipe))
                            {
                                if (potentialRecipe.is_unlocked)
                                {
                                    // Just spawn item
                                    Potion temp = new Potion();
                                    temp.recipe_id = potentialRecipe.GetID();
                                    int bottleShape = Random.Range(1, DataController.bootleShapesNumber + 1);
                                    PotionWorld potion = spawner.SpawnItem<PotionWorld>(
                                        temp.GetID(),
                                        cookedPotionAnchor.position,
                                        Quaternion.identity);

                                    DataController.AddHistoryIngredient(temp.GetID());
                                    DataController.UpdateTotalScore(100);
                                }
                                else
                                {
                                    // Unlock ingredient if it is correct
                                    if (curIng.id == potentialRecipe.ingredient_seq[inventory.Count - 1])
                                    {
                                        potentialRecipe.ingredient_known[inventory.Count - 1] = true;
                                    }

                                    // Spawn item
                                    int bottleShape = Random.Range(1, DataController.bootleShapesNumber + 1);
                                    PotionWorld potion = spawner.SpawnItem<PotionWorld>(
                                        ("potion_" + bottleShape).Hash(),
                                        cookedPotionAnchor.position,
                                        Quaternion.identity);

                                    // Transfer all necessary data
                                    Potion potionData = potion.potionData;
                                    potionData.recipe_id = potentialRecipe.GetID();
                                    potionData.bottle_shape = bottleShape;
                                    potionData.ingredients = new int[inventory.Count];
                                    potionData.titleIDs = new int[3];
                                    for (int i = 0; i < potionData.titleIDs.Length; i++)
                                    {
                                        potionData.titleIDs[i] = Random.Range(0, inventory.Count);
                                    }
                                    inventory.CopyTo(potionData.ingredients);

                                    // Generated potion ID
                                    int newPotionID = potionData.GetID();
                                    int genIdAttempts = 0;
                                    while (DataController.ingredients.ContainsKey(newPotionID) && genIdAttempts < 32)
                                    {
                                        newPotionID = $"{newPotionID}".Hash(); // Workaround to have unique ID
                                        genIdAttempts++;
                                    }

                                    if (genIdAttempts >= 32)
                                    {
                                        newPotionID = Random.Range(0, int.MaxValue); // Workaround #2 to have unique ID
                                    }

                                    string newPotionName = potionData.GenerateNameDebug();

                                    potion.id = newPotionID;
                                    potion.item_name = newPotionName;
                                    potion.UpdateColor();

                                    // Create generated versions for spawner
                                    PotionWorld potionCopy = spawner.SpawnItem<PotionWorld>(
                                        ("potion_" + bottleShape).Hash(),
                                        new Vector3(0f, 100f, 0f),
                                        Quaternion.identity,
                                        spawner.gameObject.transform);
                                    potionCopy.id = newPotionID;
                                    potionCopy.item_name = newPotionName;
                                    potionCopy.potionData = new Potion(potionData);
                                    potionCopy.SetPhysicsActive(false);
                                    potionCopy.gameObject.SetActive(false);
                                    spawner.absItems.Add(potionCopy);

                                    PotionUI uiPotionCopy = spawner.SpawnItem<PotionUI>(
                                        ("potion_" + bottleShape).Hash(),
                                        new Vector3(0f, 300f, 0f),
                                        Quaternion.identity,
                                        spawner.gameObject.transform);
                                    uiPotionCopy.id = newPotionID;
                                    uiPotionCopy.item_name = newPotionName;
                                    uiPotionCopy.potionData = new Potion(potionData);
                                    uiPotionCopy.gameObject.SetActive(false);
                                    spawner.absItems.Add(uiPotionCopy);

                                    // Add a new ingredient
                                    DataController.AddNewIngredient(
                                        newPotionID,
                                        newPotionName,
                                        AverageCooldown(potionData.ingredients),
                                        RandomBreakChance(potionData.ingredients),
                                        AverageRarity(potionData.ingredients) / 2, // To balance potions
                                        RandomR(potionData.ingredients),
                                        RandomG(potionData.ingredients),
                                        RandomB(potionData.ingredients),
                                        AverageAlpha(potionData.ingredients),
                                        "none",
                                        potionData).hasBeenDiscovered = true;

                                    DataController.genData.potionsCooked++;
                                    DataController.recipes[potentialRecipe.GetID()].is_unlocked = true;
                                    DataController.UpdateTotalScore(500);

                                    if (potentialRecipe.id == DataController.genData.winningRecipeId)
                                    {
                                        spawner.logic.FinishTheGame();
                                    }

                                    if (DataController.genData.potionsCooked % (DataController.maximumRecipes / 14) == 0)
                                    {
                                        UIController.SpawnSideLine(("new_pigeon_available", new object[] {}));
                                        DataController.genData.maxPigeons++;
                                    }

                                    if (DataController.genData.potionsCooked == DataController.maximumRecipes)
                                    {
                                        // Generate final potion
                                        var winRecipe = DataController.CreateNewRecipe(DataController.GenerateRandomRecipe(100f));
                                        DataController.genData.winningRecipeId = winRecipe.id;
                                        DataController.genData.potionsCooked++;

                                        UIController.SpawnSideLine("final_potion", new object[] {}, 10f);
                                    }
                                    else if (DataController.genData.potionsCooked < DataController.maximumRecipes)
                                    {
                                        DataController.CreateNewRecipe(DataController.GenerateRandomRecipe(
                                            4f + (DataController.genData.potionsCooked 
                                                / (float)(DataController.maximumRecipes)) * 46f));
                                    }

                                    DataController.AddHistoryIngredient(newPotionID);

                                    cookedSound.pitch = 1f;
                                    cookedSound.Play();
                                    ui.UpdateLabLabels();
                                }

                                DestroyCurrentRecipe(true);
                            }
                            else
                            {
                                DestroyCurrentRecipe();
                            }
                        }
                        else
                        {
                            GameObject fb = Instantiate(finishBubbles, transform);
                            ParticleSystem fbPS = fb.GetComponent<ParticleSystem>();
                            ParticleSystem.MainModule settings = fbPS.main;
                            settings.startColor = new ParticleSystem.MinMaxGradient(Color.white);
                            settings.duration = 2f;
                            fbPS.Play();

                            // Unlock ingredient if it is correct
                            if (curIng.id == potentialRecipe.ingredient_seq[inventory.Count - 1])
                            {
                                potentialRecipe.ingredient_known[inventory.Count - 1] = true;
                            }

                            cookedSound.pitch = .5f + inventory.Count * .025f;
                            cookedSound.Play();

                            "Waiting for the next ingredient...".Log(this);
                        }
                    }
                }
            }
            else
            {
                DestroyCurrentRecipe();
            }
            worldItem.Destroy();
        }
    }

    private Recipe InventoryHasPotential()
    {
        Recipe potential = null;
        int minMistakes = int.MaxValue;
        foreach (Recipe rec in DataController.recipes.Values)
        {
            if (inventory.Count <= rec.ingredient_seq.Length)
            {
                int mistakes = cooldownMistakes;
                for (int i = 0; i < inventory.Count; i++)
                {
                    if (!inventory[i].Equals(rec.ingredient_seq[i]))
                    {
                        mistakes++;
                    }
                }

                if (mistakes <= rec.mistakes_allowed && mistakes < minMistakes)
                {
                    potential = rec;
                    minMistakes = mistakes;
                }
            }
        }

        return potential;
    }

    private bool IsInventoryValid(Recipe recipe)
    {
        int mistakes = 0;
        for (int i = 0; i < recipe.ingredient_seq.Length; i++)
        {
            if (!inventory[i].Equals(recipe.ingredient_seq[i]))
            {
                mistakes++;
            }
        }
        return mistakes <= recipe.mistakes_allowed;
    }

    private void SetRecipeCooking(bool isCooking)
    {
        foreach (GameObject part in particles)
        {
            part.SetActive(isCooking);
        }
        spoonAnim.SetBool("IsCooking", isCooking);
    }

    private void DestroyCurrentRecipe(bool success = false, bool bad_ingredient = false)
    {
        float delay = success ? 6f : 3f;
        Color color = success ? successColor : Color.black;

        GameObject fb = Instantiate(finishBubbles, transform);
        ParticleSystem fbPS = fb.GetComponent<ParticleSystem>();
        ParticleSystem.MainModule settings = fbPS.main;
        settings.startColor = new ParticleSystem.MinMaxGradient(color);
        settings.duration = delay;
        fbPS.Play();

        StartCoroutine(FadeWaterColor(color, regularColor, delay));
        SetRecipeCooking(false);
        inventory.Clear();
        cooldown = 0f;
        cooldownMistakes = 0;

        if (!success)
        {
            failedSound.Play();
            DataController.genData.potionsFailed++;

            if (bad_ingredient)
            {
                UIController.SpawnSideLine(("cauldron_ingredient_break", new object[] {}));
            }
            else
            {
                UIController.SpawnSideLine(("cauldron_bad_hero", new object[] {}));
            }

            DataController.AddHistoryIngredient(0); // Fail
        }
        DataController.AddHistoryEntry(new HistoryEntry()); // Update potion cooking history
    }

    IEnumerator FadeWaterColor(Color from, Color to, float delay)
    {
        float elapsed = 0f;
        while (elapsed < delay) // 3 seconds
        {
            elapsed += Time.deltaTime;
            waterMat.SetColor("_Color", Color.Lerp(from, to, elapsed / delay));
            yield return null;
        }
    }

    private float AverageCooldown(int[] ingredients)
    {
        float cldwn = 0f;
        foreach (int id in ingredients)
        {
            cldwn += DataController.ingredients[id].cooldown;
        }
        return cldwn / ingredients.Length;
    }

    private float RandomBreakChance(int[] ingredients)
    {
        float chance = 0f;
        int randNum = Random.Range(1, ingredients.Length);
        int temp = randNum;
        List<int> ings = new List<int>(ingredients);
        while (randNum > 0)
        {
            int toRemove = Random.Range(1, ings.Count);
            chance += DataController.ingredients[ings[toRemove]].breakChance;
            ings.RemoveAt(toRemove);
            randNum--;
        }
        return chance / temp;
    }

    private float RandomR(int[] ingredients)
    {
        float color = 0f;
        int randNum = Random.Range(1, ingredients.Length);
        int temp = randNum;
        List<int> ings = new List<int>(ingredients);
        while (randNum > 0)
        {
            int toRemove = Random.Range(1, ings.Count);
            color += DataController.ingredients[ings[toRemove]].color_r;
            ings.RemoveAt(toRemove);
            randNum--;
        }
        return color / temp;
    }

    private float RandomG(int[] ingredients)
    {
        float color = 0f;
        int randNum = Random.Range(1, ingredients.Length);
        int temp = randNum;
        List<int> ings = new List<int>(ingredients);
        while (randNum > 0)
        {
            int toRemove = Random.Range(1, ings.Count);
            color += DataController.ingredients[ings[toRemove]].color_g;
            ings.RemoveAt(toRemove);
            randNum--;
        }
        return color / temp;
    }

    private float RandomB(int[] ingredients)
    {
        float color = 0f;
        int randNum = Random.Range(1, ingredients.Length);
        int temp = randNum;
        List<int> ings = new List<int>(ingredients);
        while (randNum > 0)
        {
            int toRemove = Random.Range(1, ings.Count);
            color += DataController.ingredients[ings[toRemove]].color_b;
            ings.RemoveAt(toRemove);
            randNum--;
        }
        return color / temp;
    }

    private float AverageAlpha(int[] ingredients)
    {
        float alpha = 0f;
        foreach (int id in ingredients)
        {
            alpha += DataController.ingredients[id].color_a;
        }
        return alpha / ingredients.Length;
    }

    private int AverageRarity(int[] ingredients)
    {
        float rarity = 0f;
        foreach (int id in ingredients)
        {
            rarity += DataController.ingredients[id].rarity;
        }

        return (int)(rarity / ingredients.Length);
    }
}
