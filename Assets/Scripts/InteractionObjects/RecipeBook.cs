using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RecipeBook : MonoBehaviour
{
    public float distanceThreshold = 2f; // Affects opening/closing of the book
    public ParticleSystem enterField;
    public GameObject scrollContent;
    public TextMeshProUGUI secondTitle;

    [Space(15f)]
    public GameObject bookItem;

    [Space(15f)]
    public int maxPages = 3;

    [Space(20f)]
    public SpawnController spawner;

    private static int currentPage = 1;

    private string[] secondTitles;
    private float distanceToPlayer = 100f;
    private Animator bookAnim;

    private void Start()
    {
        bookAnim = GetComponentInChildren<Animator>();
        secondTitles = new string[]
        {
            "Ingredients", "Recipes", "History"
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

    public void ChangePage(int direction) // -1 or 1
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

        secondTitle.text = secondTitles[currentPage - 1];

        // Clear previous content
        foreach (Transform t in scrollContent.transform)
        {
            Destroy(t.gameObject);
        }

        switch (currentPage)
        {
            case 1: // Ingredients
                foreach (Ingredient ing in DataController.ingredients.Values)
                {
                    if (ing.hasBeenDiscovered)
                    {
                        GameObject ingEntry = Instantiate(bookItem, scrollContent.transform);
                        {
                            ItemUI uiItem = spawner.absItems.Find(item => item.id.Equals(ing.id) && item is ItemUI) as ItemUI;
                            PotionUI uiPotion = uiItem as PotionUI;
                            if (uiPotion != null)
                            {
                                PotionUI uiPotionCopy = spawner.SpawnItem<PotionUI>(uiPotion.id, ingEntry.transform.Find("Slot"));
                            }
                            else
                            {
                                spawner.SpawnItem<ItemUI>(uiItem.id, ingEntry.transform.Find("Slot"));
                            }
                            ingEntry.transform.Find("Title").gameObject.GetComponent<TextMeshProUGUI>().text = uiItem.id;
                        }
                    }
                }
                break;

            default:
                break;
        }
    }
}
