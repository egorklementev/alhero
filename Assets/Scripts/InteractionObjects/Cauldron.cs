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
            string itemID = other.gameObject.GetComponent<ItemWorld>().item_id;
            inventory.Add(itemID);
            Recipe potentialRecipe = RecipeHasPotential();
            if (potentialRecipe != null && cooldown <= 0f)
            {
                SetRecipeCooking(true); // Or continue

                Ingredient curIng = DataController.ingredients.Find(ing => ing.id.Equals(itemID));
                cooldown = curIng.cooldown;

                // In case we are 'lucky' - just break current recipe
                if (Random.Range(0f, 1f) <= curIng.breakChance)
                {
                    DestroyCurrentRecipe();
                }
                else
                {
                    // If a potion is ready
                    if (potentialRecipe.ingredientSeq.Length == inventory.Count)
                    {
                        // Count mistakes, adjust potion
                        Debug.Log("Congratulations!");
                        spawner.SpawnItem("potion_1", (transform.parent.transform.position - new Vector3(4f, -2f, -4f)), Quaternion.identity, itemsGroup);
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
            int mistakes = rec.mistakesAllowed;
            for (int i = 0; i < inventory.Count; i++)
            {
                if (!inventory[i].Equals(rec.ingredientSeq[i]))
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
    }
}
