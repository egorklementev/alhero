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


    private float cooldown = 0f;
    private int cooldownMistakes = 0;
    private List<string> inventory; // Current recipe

    private void Awake()
    {
        inventory = new List<string>();
    }

    private void Update()
    {
        if (cooldown > 0f)
        {
            cooldown -= Time.deltaTime;
        }
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

                Ingredient curIng = DataController.ingredients.Find(ing => ing.id.Equals(itemID));
                if (curIng == null)
                {
                    Debug.LogError("No ingredient with name: " + itemID);
                }
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
                        potionData.recipe_id = potentialRecipe.GetID();
                        potionData.bottle_shape = bottleShape;
                        potionData.ingredients = new string[inventory.Count];
                        inventory.CopyTo(potionData.ingredients);

                        // Generated potion ID
                        string newPotionName = potionData.GetID();
                        potion.name = newPotionName;
                        potionScript.itemID = newPotionName;

                        // Potion Color
                        Color potionColor = potionScript.GetColor();
                        potionData.color_r = potionColor.r;
                        potionData.color_g = potionColor.g;
                        potionData.color_b = potionColor.b;
                        potionData.color_a = potionColor.a;
                        potion.GetComponentInChildren<Renderer>().materials[2].SetColor("_Color", potionColor);

                        // Add as a new ingredient in any case
                        spawner.items.Add(potion);

                        //DataController.AddNewIngredient(newPotionName, 0f, 0f, 0f, .5f, 0f, .1f); // Should be randomized
                        //DataController.CreateNewRecipe(0, newPotionName, "salt", "horseshoe"); // Should be generated somehow

                        DestroyCurrentRecipe();
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
        foreach (Recipe rec in DataController.recipes)
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

    private void DestroyCurrentRecipe()
    {
        SetRecipeCooking(false);
        inventory.Clear();
        cooldown = 0f;
        cooldownMistakes = 0;
    }
}
