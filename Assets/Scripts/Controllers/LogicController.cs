using UnityEngine;

public class LogicController : MonoBehaviour
{
    public GameObject player;
    public GameObject itemsGroup;
    public GameObject containers;

    [Space(15f)]
    public SpawnController spawner;

    private static int playerInvSize = 3; // Inverntory size

    public static GameObject currentBarrel { get; set; } = null;
    public static ItemWorld[] PickedItems { get; set; } = new ItemWorld[playerInvSize];

    private void Start()
    {
        // spawner.SpawnContainer(1, new Vector3(-2f, -.5f, 2f), Quaternion.Euler(0f, 0f, 0f), staticObjsGroup);

        // Load containers with previously stored items
        foreach (Transform contTransform in containers.transform)
        {
            Container contScript = contTransform.gameObject.GetComponentInChildren<Container>();
            ContainerItems itemsToLoad = DataController.containers[contScript.id];
            if (itemsToLoad != null)
            {
                for (int i = 0; i < itemsToLoad.items.Length; i++)
                {
                    string itemToPutID = itemsToLoad.items[i].itemID;
                    if (itemToPutID.StartsWith("potion"))
                    {
                        Potion potionData = itemsToLoad.items[i].potionData;
                        GameObject potion = spawner.SpawnItem("potion_" + potionData.bottle_shape, new Vector3(0f, 100f, 0f), Quaternion.identity, itemsGroup);
                        PotionWorld potionScript = potion.GetComponent<PotionWorld>();
                        potionScript.potionData = potionData;
                        string newPotionName = potionData.GetID();
                        potion.name = newPotionName;
                        potionScript.itemID = newPotionName;
                        potion.GetComponentInChildren<Renderer>().materials[2].SetColor("_Color", potionScript.GetColor());
                        contScript.TryToPutItem(potion, i);
                    }
                    else
                    {
                        contScript.TryToPutItem(spawner.SpawnItem(itemToPutID, new Vector3(0f, 100f, 0f), Quaternion.identity, itemsGroup), i);
                    }
                }
            }
        }
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
        if (currentBarrel != null && slot != -1)
        {
            Container b = currentBarrel.GetComponentInChildren<Container>();
            GameObject selected = b.GetSelectedItem();
            if (selected != null)
            {
                selected.GetComponent<Animator>().SetBool("Destroy", true);

                DataController.containers[b.id].items[b.GetSelectedItemSlot()].itemID = "";
                DataController.containers[b.id].items[b.GetSelectedItemSlot()].potionData = new Potion();

                PotionUI pUI = selected.GetComponent<PotionUI>();
                if (pUI != null)
                {
                    GameObject newPotion = Instantiate(selected.GetComponent<ItemUI>().worldItem, itemsGroup.transform);

                    PotionWorld newPotionScript = newPotion.GetComponent<PotionWorld>();
                    newPotionScript.potionData = new Potion(pUI.potionData); // Perform potion data copy

                    string newPotionID = newPotionScript.potionData.GetID();
                    newPotion.name = newPotionID;
                    newPotionScript.itemID = newPotionID;

                    newPotion.GetComponentInChildren<Renderer>().materials[2].SetColor("_Color", newPotionScript.GetColor());

                    newPotionScript.SetPickedUp(true, slot, player);
                    PickedItems[slot] = newPotionScript;

                    b.ResetSelection();
                }
                else
                {
                    GameObject newWorldItem = Instantiate(selected.GetComponent<ItemUI>().worldItem, itemsGroup.transform);
                    ItemWorld newItem = newWorldItem.GetComponent<ItemWorld>();
                    newItem.SetPickedUp(true, slot, player);
                    PickedItems[slot] = newItem;
                    b.ResetSelection();
                }
            }
        }
    }

    public void SelectItemInBarrel(int slot)
    {
        if (currentBarrel != null)
        {
            currentBarrel.GetComponentInChildren<Container>().OnItemSelected(slot);
        }
    }

    public void SpawnIngredientsDebug()
    {
        string[] items = new string[]
        {
            "flower", "horseshoe", "meat", "salt", "wine"
        };
        Vector3 start = new Vector3(11f, 1f, 1.5f * items.Length / 2f);
        foreach (string id in items)
        {
            spawner.SpawnItem(id, start, Quaternion.identity, itemsGroup);
            start -= new Vector3(0f, 0f, 3f);
        }
    }
}
