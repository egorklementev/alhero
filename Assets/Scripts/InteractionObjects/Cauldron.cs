using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cauldron : MonoBehaviour
{
    public SpawnController spawner;
    public GameObject itemsGroup;

    [Space(10f)]
    public GameObject[] particles;
    public Animator spoonAnim;
    public ParticleSystem finishBubbles;

    [Space(10f)]
    public UIController ui;

    private Material waterMat;
    private float cooldown = 0f;
    private int cooldownMistakes = 0;
    private List<int> inventory; // Current recipe

    private void Start()
    {
        inventory = new List<int>();
        inventory.AddRange(DataController.genData.cauldronInventory);

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

    private void OnDestroy()
    {
        // Save inventory before exiting
        DataController.genData.cauldronInventory = new int[inventory.Count];
        inventory.CopyTo(DataController.genData.cauldronInventory);
    }

    public void ClearInventory()
    {
        StartCoroutine(FadeWaterColor(Color.cyan, Color.white, 1f));
        inventory.Clear();
        SetRecipeCooking(false);
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.CompareTag("Item"))
        {
            ItemWorld worldItem = other.gameObject.GetComponent<ItemWorld>();
            int id = worldItem.id;
            inventory.Add(id);
            DataController.AddHistoryIngredient(id); // Anyway
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
                    Debug.LogError($"[Cauldron.OnCollisionExit] No ingredient with name \"{id}\" !!!");
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
                                        (transform.parent.transform.position - new Vector3(4f, -2f, -4f)),
                                        Quaternion.identity,
                                        itemsGroup
                                        );

                                    DataController.AddHistoryIngredient(temp.GetID());
                                }
                                else
                                {
                                    // Spawn item
                                    int bottleShape = Random.Range(1, DataController.bootleShapesNumber + 1);
                                    PotionWorld potion = spawner.SpawnItem<PotionWorld>(
                                        ("potion_" + bottleShape).Hash(),
                                        (transform.parent.transform.position - new Vector3(4f, -2f, -4f)),
                                        Quaternion.identity,
                                        itemsGroup
                                        );

                                    // Transfer all necessary data
                                    Potion potionData = potion.potionData;
                                    potionData.recipe_id = potentialRecipe.GetID();
                                    potionData.bottle_shape = bottleShape;
                                    potionData.ingredients = new int[inventory.Count];
                                    potionData.titleIDs = new int[Random.Range(0, 3) + 1];
                                    for (int i = 0; i < potionData.titleIDs.Length; i++)
                                    {
                                        // TODO: think about it
                                        potionData.titleIDs[i] = Random.Range(0, inventory.Count);
                                    }
                                    inventory.CopyTo(potionData.ingredients);

                                    // Generated potion ID
                                    int newPotionID = potionData.GetID();
                                    string newPotionName = potionData.GenerateNameDebug();
                                    
                                    potion.id = newPotionID;
                                    potion.item_name = newPotionName;
                                    potion.UpdateColor();

                                    // Create generated versions for spawner
                                    PotionWorld potionCopy = spawner.SpawnItem<PotionWorld>(
                                        ("potion_" + bottleShape).Hash(),
                                        new Vector3(0f, 100f, 0f),
                                        Quaternion.identity,
                                        spawner.gameObject);
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
                                        spawner.gameObject);
                                    uiPotionCopy.id = newPotionID;
                                    uiPotionCopy.item_name = newPotionName;
                                    uiPotionCopy.potionData = new Potion(potionData);
                                    uiPotionCopy.gameObject.SetActive(false);
                                    spawner.absItems.Add(uiPotionCopy);

                                    // Add a new ingredient
                                    //int ingrNum = potionData.ingredients.Length;
                                    DataController.AddNewIngredient(
                                        newPotionID, 
                                        newPotionName, 
                                        AverageCooldown(potionData.ingredients), 
                                        RandomBreakChance(potionData.ingredients), 
                                        RandomR(potionData.ingredients), 
                                        RandomG(potionData.ingredients), 
                                        RandomB(potionData.ingredients), 
                                        AverageAlpha(potionData.ingredients), 
                                        potionData);

                                    // Unlock potion recipe when it is cooked for the first time 
                                    DataController.recipes[potentialRecipe.GetID()].is_unlocked = true;

                                    DataController.GenerateRandomRecipe();

                                    DataController.AddHistoryIngredient(newPotionID);
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
                            Debug.Log("[Cauldron]: Waiting for the next ingredient...");
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
        //int currentIngredientIndex = inventory.Count - 1;
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
                if (mistakes <= rec.mistakes_allowed)
                {
                    return rec;
                }
            }
        }
        return null;
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

    private void DestroyCurrentRecipe(bool success = false)
    {
        float delay = success ? 6f : 3f;
        Color color = success ? new Color(.3089f, .1244f, .5667f) : Color.black;

        finishBubbles.Stop();
        ParticleSystem.MainModule settings = finishBubbles.main;
        settings.startColor = new ParticleSystem.MinMaxGradient(color);
        settings.duration = delay;
        finishBubbles.Play();

        StartCoroutine(FadeWaterColor(color, Color.white, delay));
        SetRecipeCooking(false);
        inventory.Clear();
        cooldown = 0f;
        cooldownMistakes = 0;

        if (!success)
        {
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
}
