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


    private Material waterMat;
    private float cooldown = 0f;
    private int cooldownMistakes = 0;
    private List<string> inventory; // Current recipe

    private void Start()
    {
        inventory = new List<string>();
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
        DataController.genData.cauldronInventory = new string[inventory.Count];
        inventory.CopyTo(DataController.genData.cauldronInventory);
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.CompareTag("Item"))
        {
            string itemID = other.gameObject.GetComponent<ItemWorld>().itemID;
            inventory.Add(itemID);
            if (cooldown > 0f)
            {
                cooldownMistakes++;
            }
            Recipe potentialRecipe = RecipeHasPotential();
            if (potentialRecipe != null)
            {
                SetRecipeCooking(true); // Or continue

                Ingredient curIng = DataController.ingredients[itemID];
                if (curIng == null)
                {
                    Debug.LogError("No ingredient with name: " + itemID + "!");
                    DestroyCurrentRecipe();
                }
                else
                {
                    cooldown = curIng.cooldown;

                    // In case we are 'lucky' - just break current recipe
                    if (Random.Range(0f, 1f) <= curIng.breakChance)
                    {
                        DestroyCurrentRecipe();
                    }
                    else
                    {
                        // If a potion is ready
                        if (potentialRecipe.ingredient_seq.Length == inventory.Count)
                        {
                            // Spawn item
                            int bottleShape = Random.Range(1, DataController.bootleShapesNumber + 1);
                            GameObject potion = spawner.SpawnItem(
                                "potion_" + bottleShape,
                                (transform.parent.transform.position - new Vector3(4f, -2f, -4f)),
                                Quaternion.identity,
                                itemsGroup
                                );

                            // Transfer all necessary data
                            PotionWorld potionScript = potion.GetComponentInChildren<PotionWorld>();
                            Potion potionData = potionScript.potionData;
                            potionData.recipe_id = Recipe.GetID(potentialRecipe.ingredient_seq);
                            potionData.bottle_shape = bottleShape;
                            potionData.ingredients = new string[inventory.Count];
                            inventory.CopyTo(potionData.ingredients);

                            // Generated potion ID
                            string newPotionName = potionData.GetID();
                            potion.name = newPotionName;
                            potionScript.itemID = newPotionName;

                            // Potion Color
                            Color potionColor = PotionWorld.GetColor(potionData);
                            potionData.color_r = potionColor.r;
                            potionData.color_g = potionColor.g;
                            potionData.color_b = potionColor.b;
                            potionData.color_a = potionColor.a;
                            potion.GetComponentInChildren<Renderer>().materials[2].SetColor("_Color", potionColor);

                            // Add as a new ingredient in any case
                            GameObject potionCopy = Instantiate(
                                potion,
                                new Vector3(0f, 100f, 0f),
                                Quaternion.identity,
                                spawner.gameObject.transform
                                );
                            potionCopy.GetComponent<Rigidbody>().useGravity = false;
                            potionCopy.GetComponent<BoxCollider>().enabled = false; // ATTENTION: May be bad, dunno
                            spawner.items.Add(potionCopy);
                            int ingrNum = potionData.ingredients.Length;
                            float ptnCooldown = AverageCooldown(potionData.ingredients);
                            float ptnBreakChance = RandomBreakChance(potionData.ingredients);
                            float ptnR = RandomR(potionData.ingredients);
                            float ptnG = RandomG(potionData.ingredients);
                            float ptnB = RandomB(potionData.ingredients);
                            float ptnA = AverageAlpha(potionData.ingredients);
                            DataController.AddNewIngredient(newPotionName, ptnCooldown, ptnBreakChance, ptnR, ptnG, ptnB, ptnA, potionData);

                            //DataController.CreateNewRecipe(0, newPotionName, "salt", "horseshoe"); // Should be generated somehow

                            DestroyCurrentRecipe(true);
                        }
                    }
                }
            }
            else
            {
                DestroyCurrentRecipe();
            }
            other.gameObject.GetComponentInChildren<Animator>().SetBool("Destroy", true);
        }
    }

    private Recipe RecipeHasPotential()
    {
        int currentIngredientIndex = inventory.Count - 1;
        foreach (Recipe rec in DataController.recipes.Values)
        {
            int mistakes = rec.mistakes_allowed - cooldownMistakes;
            for (int i = 0; i < inventory.Count; i++)
            {
                if (!inventory[i].Equals(rec.ingredient_seq[i]))
                {
                    mistakes--;
                }
            }
            if (mistakes >= 0)
            {
                return rec;
            }
        }
        return null;
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
        Color color = success ? Color.magenta : Color.black;

        ParticleSystem finishBubbles = transform.Find("cauldron").Find("FinishBubbles").gameObject.GetComponent<ParticleSystem>();
        ParticleSystem.MainModule settings = finishBubbles.main;
        settings.startColor = new ParticleSystem.MinMaxGradient(color);
        settings.duration = delay;
        finishBubbles.Play();

        StartCoroutine(FadeWaterColor(color, Color.white, delay));
        SetRecipeCooking(false);
        inventory.Clear();
        cooldown = 0f;
        cooldownMistakes = 0;
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

    private float AverageCooldown(string[] ingredients)
    {
        float cldwn = 0f;
        foreach (string id in ingredients)
        {
            cldwn += DataController.ingredients[id].cooldown;
        }
        return cldwn / ingredients.Length;
    }

    private float RandomBreakChance(string[] ingredients)
    {
        float chance = 0f;
        int randNum = Random.Range(0, ingredients.Length);
        int temp = randNum;
        List<string> ings = new List<string>(ingredients);
        while (randNum > 0)
        {
            int toRemove = Random.Range(0, ings.Count);
            chance += DataController.ingredients[ings[toRemove]].breakChance;
            ings.RemoveAt(toRemove);
            randNum--;
        }
        return chance / temp;
    }

    private float RandomR(string[] ingredients)
    {
        float color = 0f;
        int randNum = Random.Range(0, ingredients.Length);
        int temp = randNum;
        List<string> ings = new List<string>(ingredients);
        while (randNum > 0)
        {
            int toRemove = Random.Range(0, ings.Count);
            color += DataController.ingredients[ings[toRemove]].color_r;
            ings.RemoveAt(toRemove);
            randNum--;
        }
        return color / temp;
    }

    private float RandomG(string[] ingredients)
    {
        float color = 0f;
        int randNum = Random.Range(0, ingredients.Length);
        int temp = randNum;
        List<string> ings = new List<string>(ingredients);
        while (randNum > 0)
        {
            int toRemove = Random.Range(0, ings.Count);
            color += DataController.ingredients[ings[toRemove]].color_g;
            ings.RemoveAt(toRemove);
            randNum--;
        }
        return color / temp;
    }

    private float RandomB(string[] ingredients)
    {
        float color = 0f;
        int randNum = Random.Range(0, ingredients.Length);
        int temp = randNum;
        List<string> ings = new List<string>(ingredients);
        while (randNum > 0)
        {
            int toRemove = Random.Range(0, ings.Count);
            color += DataController.ingredients[ings[toRemove]].color_b;
            ings.RemoveAt(toRemove);
            randNum--;
        }
        return color / temp;
    }

    private float AverageAlpha(string[] ingredients)
    {
        float alpha = 0f;
        foreach (string id in ingredients)
        {
            alpha += DataController.ingredients[id].color_a;
        }
        return alpha / ingredients.Length;
    }
}
