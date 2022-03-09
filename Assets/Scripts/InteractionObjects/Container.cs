using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Container : MonoBehaviour
{
    public int id;
    public string container_name;
    public bool isUnlocked;
    public int invSize = 4;
    public GameObject[] slots;
    public TextMeshProUGUI ingredientLine;
    public SpawnController spawner;

    private ItemUI[] inventory;
    private int selectedItem = -1;

    private void Awake()
    {
        inventory = new ItemUI[invSize];
        transform.parent.Find("EnterField").gameObject.SetActive(isUnlocked);
    }

    public void OnItemSelected(int slotNum)
    {
        ItemUI uiItem = inventory[slotNum];

        if (uiItem != null)
        {
            // When selected already - deselect 
            if (selectedItem == slotNum)
            {
                selectedItem = -1;
                uiItem.SetSelected(false);
                ingredientLine.text = "";
            }
            // When other is selected - deselect other and select new
            else if (selectedItem > -1)
            {
                inventory[selectedItem].SetSelected(false);
                selectedItem = slotNum;
                uiItem.SetSelected(true);
                ingredientLine.text = uiItem.item_name;
            }
            // When no item is selected - select
            else
            {
                selectedItem = slotNum;
                uiItem.SetSelected(true);
                ingredientLine.text = uiItem.item_name;
            }
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.TryGetComponent<PotionWorld>(out PotionWorld potionWorld))
        {

            TryToPutItem(potionWorld);
        }
        else
        {
            TryToPutItem(other.gameObject.GetComponent<ItemWorld>());
        }
    }

    public void TryToPutItem(ItemWorld item, int slot = -1)
    {
        if (item != null && isUnlocked)
        {
            int freeSlot = slot == -1 ? GetFreeSlot() : slot;
            if (item.CompareTag("Item") && freeSlot != -1)
            {
                PotionWorld pWorld = item as PotionWorld;
                if (pWorld != null)
                {
                    inventory[freeSlot] = spawner.SpawnItem<PotionUI>(pWorld.id, slots[freeSlot]);
                    (inventory[freeSlot] as PotionUI).potionData = new Potion(pWorld.potionData);

                    try
                    {
                        DataController.labContainers[id].items[freeSlot].id = pWorld.id;
                        DataController.labContainers[id].items[freeSlot].potionData = new Potion(pWorld.potionData);
                    }
                    catch {}
                }
                else
                {
                    inventory[freeSlot] = spawner.SpawnItem<ItemUI>(item.id, slots[freeSlot]);

                    try 
                    {
                        DataController.labContainers[id].items[freeSlot].id = item.id;
                    }
                    catch {}
                }
                item.Destroy();
            }
        }
    }

    int GetFreeSlot()
    {
        for (int i = 0; i < invSize; i++)
        {
            if (inventory[i] == null)
            {
                return i;
            }
        }
        return -1;
    }

    public ItemUI GetSelectedItem()
    {
        if (selectedItem > -1)
        {
            return inventory[selectedItem];
        }
        return null;
    }

    public int GetSelectedItemSlot() {
        return selectedItem;
    }

    public void ResetSelection()
    {
        if (selectedItem > -1)
        {
            inventory[selectedItem].SetSelected(false);
            ingredientLine.text = "";
        }
        selectedItem = -1;
    }

    public void Clear()
    {
        ResetSelection();
        inventory = new ItemUI[invSize];
        foreach (GameObject slot in slots)
        {
            foreach (Transform child in slot.transform)
            {
                Destroy(child.gameObject);
            }
        }
    }
}
