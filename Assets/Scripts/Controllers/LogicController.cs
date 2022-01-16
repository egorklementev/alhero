using UnityEngine;

public class LogicController : MonoBehaviour
{
    public GameObject player;

    [Space(15f)]
    public GameObject itemsGroup;
    public GameObject containers;

    [Space(15f)]
    public GameObject recipeBook;
    public RecipeBook recipeBookScript;

    [Space(15f)]
    public SpawnController spawner;

    private static int playerInvSize = 3; // Inverntory size

    public static Container curContainer { get; set; } = null;
    public static ItemWorld[] PickedItems { get; set; } = new ItemWorld[playerInvSize];

    private void Start()
    {
        // spawner.SpawnContainer(1, new Vector3(-2f, -.5f, 2f), Quaternion.identity, staticObjsGroup);

        LoadGeneratedPotions();

        // Load containers with previously stored items
        foreach (Transform contTransform in containers.transform)
        {
            Container contScript = contTransform.gameObject.GetComponentInChildren<Container>();
            if (!DataController.containers.ContainsKey(contScript.id))
            {
                Debug.LogWarning($"[LogicController.Start] No container with ID \"{contScript.id}\"!");
            }
            else
            {
                ContainerItems itemsToLoad = DataController.containers[contScript.id];
                if (itemsToLoad != null)
                {
                    for (int i = 0; i < itemsToLoad.items.Length; i++)
                    {
                        string itemToPutID = itemsToLoad.items[i].id;
                        if (itemToPutID.StartsWith("potion"))
                        {
                            Potion potionData = itemsToLoad.items[i].potionData;
                            PotionWorld potion = spawner.SpawnItem<PotionWorld>(
                                "potion_" + potionData.bottle_shape,
                                new Vector3(0f, 100f, 0f),
                                Quaternion.identity,
                                itemsGroup);
                            potion.potionData = potionData;
                            string newPotionName = potionData.GetID();
                            potion.name = newPotionName;
                            potion.id = newPotionName;
                            contScript.TryToPutItem(potion, i);
                        }
                        else
                        {
                            contScript.TryToPutItem(
                                spawner.SpawnItem<ItemWorld>(
                                    itemToPutID, 
                                    new Vector3(0f, 100f, 0f), 
                                    Quaternion.identity, 
                                    itemsGroup),
                                 i);
                        }
                    }
                }
            }
        }
    }

    private void LoadGeneratedPotions()
    {
        // We have to add generated potion items to the spawner
        foreach (Ingredient i in DataController.ingredients.Values)
        {
            if (spawner.absItems.Find(item => item.id.Equals(i.id)) == null)
            {
                // World version
                PotionWorld worldPotion = spawner.SpawnItem<PotionWorld>(
                    "potion_" + i.potionData.bottle_shape,
                    new Vector3(0f, 100f, 0f),
                    Quaternion.identity,
                    spawner.gameObject
                    );
                worldPotion.potionData = new Potion(i.potionData);
                string genPotionID = i.potionData.GetID();
                worldPotion.name = genPotionID;
                worldPotion.id = genPotionID;
                worldPotion.SetPhysicsActive(false);
                spawner.absItems.Add(worldPotion);

                // UI version
                PotionUI uiPotion = spawner.SpawnItem<PotionUI>(
                    "potion_" + i.potionData.bottle_shape,
                    new Vector3(0f, 300f, 0f),
                    Quaternion.identity,
                    spawner.gameObject
                );
                uiPotion.potionData = new Potion(i.potionData);
                uiPotion.name = genPotionID;
                uiPotion.id = genPotionID;
                spawner.absItems.Add(uiPotion);
            }
        }
    }

    private void FixedUpdate()
    {
        recipeBookScript.SetDistanceToPlayer(
            Vector3.Distance(recipeBook.transform.position, player.transform.position)
        );
    }

    public static int GetFreeInvSlot()
    {
        for (int i = 0; i < playerInvSize; i++)
        {
            if (PickedItems[i] == null) return i;
        }
        return -1;
    }

    public void RotateItems()
    {
        for (int i = 0; i < playerInvSize; i++)
        {
            if (PickedItems[i] != null)
            {
                PickedItems[i].SetSlot((i + 1) % playerInvSize);
            }
        }
        ItemWorld temp = PickedItems[0];
        PickedItems[0] = PickedItems[playerInvSize - 1];
        for (int i = playerInvSize - 1; i > 1; i--)
        {
            PickedItems[i] = PickedItems[i - 1];
        }
        PickedItems[1] = temp;
    }

    public void TakeItemFromContainer()
    {
        int slot = GetFreeInvSlot();
        if (curContainer != null && slot != -1)
        {
            Container b = curContainer;
            ItemUI selectedItem = b.GetSelectedItem();
            if (selectedItem != null)
            {
                DataController.containers[b.id].items[b.GetSelectedItemSlot()].id = "";
                DataController.containers[b.id].items[b.GetSelectedItemSlot()].potionData = new Potion();

                b.ResetSelection();

                selectedItem.Destroy();

                PotionUI uiPotion = selectedItem as PotionUI;
                if (uiPotion != null)
                {
                    PotionWorld newPotion = spawner.SpawnItem<PotionWorld>(uiPotion.id, itemsGroup);
                    
                    newPotion.potionData = new Potion(uiPotion.potionData); // Perform potion data copy
                    string newPotionID = newPotion.potionData.GetID();
                    newPotion.name = newPotionID;
                    newPotion.id = newPotionID;
                    newPotion.SetPickedUp(true, slot, player);
                    PickedItems[slot] = newPotion;

                }
                else
                {
                    ItemWorld newWorldItem = spawner.SpawnItem<ItemWorld>(selectedItem.id, itemsGroup);
                    newWorldItem.SetPickedUp(true, slot, player);
                    PickedItems[slot] = newWorldItem;
                }
            }
        }
    }

    public void SelectItemInBarrel(int slot)
    {
        if (curContainer != null)
        {
            curContainer.OnItemSelected(slot);
        }
    }

    public void SpawnIngredientsDebug()
    {
        string[] items = new string[]
        {
            "flower", "horseshoe", "meat", "salt", "wine"
        };
        Vector3 pos = new Vector3(11f, 1f, 1.5f * items.Length / 2f);
        foreach (string id in items)
        {
            spawner.SpawnItem<ItemWorld>(id, pos, Quaternion.identity, itemsGroup);
            pos -= new Vector3(0f, 0f, 3f);
        }
    }
}
