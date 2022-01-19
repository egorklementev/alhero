using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecipeBook : MonoBehaviour
{
    public float distanceThreshold = 2f; // Affects opening/closing of the book
    public ParticleSystem enterField;
    public GameObject scrollContent;
    public TextMeshProUGUI secondTitle;
    public GameObject[] buttons;

    [Space(15f)]
    public GameObject bookItem;
    public GameObject ingredientItem;
    public GameObject recipeIngItem;
    public GameObject recipeSignText;
    public GameObject recipeDescText;

    [Space(15f)]
    public int maxPages = 3;

    [Space(20f)]
    public SpawnController spawner;

    private static int currentPage = 1;
    private static int currentRecipe = 0;

    private string[] secondTitles;
    private float distanceToPlayer = 100f;
    private Animator bookAnim;

    private void Start()
    {
        bookAnim = GetComponentInChildren<Animator>();
        secondTitles = new string[]
        {
            "Ingredients", "Recipes", "News"
        };
        //ChangePage(0);
    }

    private void Update()
    {
        bookAnim.SetBool("IsOpened", distanceToPlayer < distanceThreshold);
        if (distanceToPlayer < distanceThreshold * 2)
        {
            ParticleSystem.EmissionModule emission = enterField.emission;
            emission.enabled = true;
        }
        else
        {
            ParticleSystem.EmissionModule emission = enterField.emission;
            emission.enabled = false;
        }
    }

    public void SetDistanceToPlayer(float distance)
    {
        distanceToPlayer = distance;
    }

    public void ChangePage(int direction) // -1, 0, or 1
    {
        currentPage += direction;
        if (currentPage == 0)
        {
            currentPage = maxPages;
        }
        else if (currentPage == maxPages + 1)
        {
            currentPage = 1;
        }

        if (direction == 0)
        {
            buttons[0].SetActive(true);
            buttons[1].SetActive(true);
            buttons[2].SetActive(false);
            buttons[3].SetActive(false);
        }

        secondTitle.text = secondTitles[currentPage - 1];

        // Clear previous content
        foreach (Transform t in scrollContent.transform)
        {
            Destroy(t.gameObject);
        }

        int index;
        switch (currentPage)
        {
            case 1: // Ingredients
                index = 0;
                foreach (Ingredient ing in DataController.ingredients.Values)
                {
                    if (ing.hasBeenDiscovered)
                    {
                        Transform ingEntry = Instantiate(bookItem, scrollContent.transform).transform;
                        ItemUI uiItem = spawner.absItems.Find(item => item.id == ing.id && item is ItemUI) as ItemUI;
                        ItemUI uiItemCopy;
                        PotionUI uiPotion = uiItem as PotionUI;
                        Transform slot = ingEntry.Find("Slot");
                        if (uiPotion != null)
                        {
                            uiItemCopy = spawner.SpawnItem<PotionUI>(uiPotion.id, slot);
                        }
                        else
                        {
                            uiItemCopy = spawner.SpawnItem<ItemUI>(uiItem.id, slot);
                        }
                        int ing_id = uiItemCopy.id;
                        slot.GetComponent<Button>().onClick.AddListener(delegate { OnIngredientClicked(ing_id); });
                        uiItemCopy.SetSmall();
                        ingEntry.transform.Find("Title").gameObject.GetComponent<TextMeshProUGUI>().text = uiItem.item_name;
                        index++;
                    }
                }
                break;

            case 2: // Recipes
                index = 0;
                foreach (Recipe rec in DataController.recipes.Values)
                {
                    if (rec.is_unlocked)
                    {
                        Transform recEntry = Instantiate(bookItem, scrollContent.transform).transform;
                        PotionUI uiPotion = spawner.absItems.Find(item => item is PotionUI && (item as PotionUI).potionData.recipe_id == rec.GetID()) as PotionUI;
                        PotionUI uiPotionCopy;
                        Transform slot = recEntry.Find("Slot");
                        uiPotionCopy = spawner.SpawnItem<PotionUI>(uiPotion.id, slot);
                        int rec_id = rec.GetID();
                        slot.GetComponent<Button>().onClick.AddListener(delegate { OnRecipeClicked(rec_id); });
                        uiPotionCopy.SetSmall();
                        recEntry.transform.Find("Title").GetComponent<TextMeshProUGUI>().text = uiPotion.item_name;
                        index++;
                    }
                }
                break;
            default:
                break;
        }
    }

    public void OnIngredientClicked(int id, bool fromRecipe = false)
    {
        // Clear previous content
        foreach (Transform t in scrollContent.transform)
        {
            Destroy(t.gameObject);
        }

        // Hide paging buttons
        buttons[0].SetActive(false);
        buttons[1].SetActive(false);
        buttons[2].SetActive(!fromRecipe);
        buttons[3].SetActive(fromRecipe);

        Transform ingDescEntry = Instantiate(ingredientItem, scrollContent.transform).transform;
        ItemUI uiItem = spawner.absItems.Find(item => item.id == id && item is ItemUI) as ItemUI;
        ItemUI uiItemCopy;
        PotionUI uiPotion = uiItem as PotionUI;
        Transform slot = ingDescEntry.Find("Slot");
        if (uiPotion != null)
        {
            uiItemCopy = spawner.SpawnItem<PotionUI>(uiPotion.id, slot);
        }
        else
        {
            uiItemCopy = spawner.SpawnItem<ItemUI>(uiItem.id, slot);
        }
        uiItemCopy.SetSelected(true);
        ingDescEntry.Find("Title").gameObject.GetComponent<TextMeshProUGUI>().text = uiItem.item_name;

        Ingredient ing = DataController.ingredients[uiItem.id];
        ingDescEntry.Find("Description").gameObject.GetComponent<TextMeshProUGUI>().text =
            $"Cooldown: {ing.cooldown:F1} seconds" + 
            $"{Environment.NewLine}{Environment.NewLine}" + 
            $"Chance to break a potion: {(ing.breakChance * 100f):F1} %";
    }

    public void OnRecipeClicked(int id)
    {
        currentRecipe = id;
        Recipe rec = DataController.recipes[id];

        // Clear previous content
        foreach (Transform t in scrollContent.transform)
        {
            Destroy(t.gameObject);
        }

        // Hide paging buttons
        buttons[0].SetActive(false);
        buttons[1].SetActive(false);
        buttons[2].SetActive(true);
        buttons[3].SetActive(false);

        float successChance = 1f;
        Transform lastText = null;
        foreach (int ing_id in rec.ingredient_seq)
        {
            Ingredient i = DataController.ingredients[ing_id];
            Transform ing = Instantiate(recipeIngItem, scrollContent.transform).transform;
            Transform slot = ing.Find("Slot");
            slot.GetComponent<Button>().onClick.AddListener(delegate { OnIngredientClicked(ing_id, true); });
            ItemUI uiItem = spawner.SpawnItem<ItemUI>(ing_id, slot);
            uiItem.SetSmall();
            lastText = Instantiate(recipeSignText, scrollContent.transform).transform;
            lastText.GetComponent<TextMeshProUGUI>().text = "+";
            successChance *= 1f - i.breakChance;
        }
        if (lastText != null)
        {
            lastText.GetComponent<TextMeshProUGUI>().text = "||";
        }
        Transform result = Instantiate(recipeIngItem, scrollContent.transform).transform;
        Transform resSlot = result.Find("Slot");
        Potion temp = new Potion(); // Kostiyl - uvazhayu, prikolno
        temp.recipe_id = rec.GetID();
        int potion_id = temp.GetID();
        resSlot.GetComponent<Button>().onClick.AddListener(delegate { OnIngredientClicked(potion_id, true); });
        PotionUI uiPotion = spawner.SpawnItem<PotionUI>(potion_id, resSlot);
        uiPotion.SetSmall();

        // Additional recipe info
        float failureChance = 1f - successChance;
        lastText = Instantiate(recipeDescText, scrollContent.transform).transform;
        lastText.GetComponent<TextMeshProUGUI>().text = $"Mistakes allowed: {rec.mistakes_allowed}{Environment.NewLine}{Environment.NewLine}" + 
            $"Chance of failure: {failureChance:F1} %";
    }
    
    public void OnRecipeBack()
    {
        OnRecipeClicked(currentRecipe);
    }
}
