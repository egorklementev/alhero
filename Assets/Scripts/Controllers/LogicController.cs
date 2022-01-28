using UnityEngine;

public class LogicController : MonoBehaviour
{
    public GameObject player;

    [Space(15f)]
    public GameObject itemsGroup;

    [Space(15f)]
    public GameObject recipeBook;
    public RecipeBook recipeBookScript;

    [Space(15f)]
    public SpawnController spawner;

    private static int playerInvSize = 3; // Inverntory size

    public static Container curContainer { get; set; } = null;
    public static ItemWorld[] PickedItems { get; set; } = new ItemWorld[playerInvSize];

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
                DataController.labContainers[b.id].items[b.GetSelectedItemSlot()].id = 0;
                DataController.labContainers[b.id].items[b.GetSelectedItemSlot()].potionData = new Potion();

                b.ResetSelection();

                selectedItem.Destroy();

                PotionUI uiPotion = selectedItem as PotionUI;
                if (uiPotion != null)
                {
                    PotionWorld newPotion = spawner.SpawnItem<PotionWorld>(uiPotion.id, itemsGroup);

                    newPotion.potionData = new Potion(uiPotion.potionData); // Perform potion data copy
                    int newPotionID = newPotion.potionData.GetID();
                    newPotion.name = newPotionID.ToString();
                    newPotion.name = newPotionID.ToString();
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
        foreach (string name in items)
        {
            spawner.SpawnItem<ItemWorld>(name.Hash(), pos, Quaternion.identity, itemsGroup);
            pos -= new Vector3(0f, 0f, 3f);
        }
    }
}
