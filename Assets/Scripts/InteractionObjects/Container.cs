using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Container : MonoBehaviour
{
    public string id;
    public bool isUnlocked;
    public int invSize = 4;
    public GameObject[] slots;
    public TextMeshProUGUI ingredientLine;

    private GameObject[] inventory;
    private int selectedItem = -1;

    private void Start()
    {
        inventory = new GameObject[invSize];
        transform.parent.Find("EnterField").gameObject.SetActive(isUnlocked);
    }

    public void OnItemSelected(int slotNum)
    {
        GameObject item = inventory[slotNum];

        if (item != null)
        {
            // When selected already - deselect 
            if (selectedItem == slotNum)
            {
                selectedItem = -1;
                item.GetComponent<Animator>().SetBool("IsSelected", false);
                ingredientLine.text = "";
            }
            // When other is selected - deselect other and select new
            else if (selectedItem > -1)
            {
                inventory[selectedItem].GetComponent<Animator>().SetBool("IsSelected", false);
                selectedItem = slotNum;
                item.GetComponent<Animator>().SetBool("IsSelected", true);
                ingredientLine.text = item.GetComponent<ItemUI>().worldItem.GetComponent<ItemWorld>().itemID;
            }
            // When no item is selected - select
            else
            {
                selectedItem = slotNum;
                item.GetComponent<Animator>().SetBool("IsSelected", true);
                ingredientLine.text = item.GetComponent<ItemUI>().worldItem.GetComponent<ItemWorld>().itemID;
            }
        }
    }

    private void OnCollisionExit(Collision other)
    {
        TryToPutItem(other.gameObject);
    }

    public void TryToPutItem(GameObject item, int slot = -1)
    {
        if (item != null && isUnlocked)
        {
            int freeSlot = slot == -1 ? GetFreeSlot() : slot;
            if (item.CompareTag("Item") && freeSlot != -1)
            {
                item.GetComponentInChildren<Animator>().SetBool("Destroy", true);
                PotionWorld pWorld = item.GetComponent<PotionWorld>();
                if (pWorld != null)
                {
                    GameObject uiItem = pWorld.uiVersion;

                    // Set potion data
                    PotionUI pUI = uiItem.GetComponent<PotionUI>();
                    pUI.potionData = new Potion(pWorld.potionData);

                    inventory[freeSlot] = Instantiate(uiItem, slots[freeSlot].transform);
                    inventory[freeSlot].GetComponent<Renderer>().materials[2].SetColor("_Color", pWorld.GetColor());

                    DataController.containers[id].items[freeSlot].itemID = pWorld.itemID;
                    DataController.containers[id].items[freeSlot].potionData = new Potion(pWorld.potionData);
                }
                else
                {
                    ItemWorld itemScript = item.GetComponent<ItemWorld>();
                    inventory[freeSlot] = Instantiate(itemScript.uiVersion, slots[freeSlot].transform);

                    DataController.containers[id].items[freeSlot].itemID = itemScript.itemID;
                }
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

    public GameObject GetSelectedItem()
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
            inventory[selectedItem].GetComponent<Animator>().SetBool("IsSelected", false);
            ingredientLine.text = "";
        }
        selectedItem = -1;
    }
}
