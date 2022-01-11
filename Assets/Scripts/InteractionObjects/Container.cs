using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Container : MonoBehaviour
{
    public int invSize = 4;
    public GameObject[] slots;
    public TextMeshProUGUI ingredientLine;

    private GameObject[] inventory;
    private int selectedItem = -1;

    private void Awake()
    {
        inventory = new GameObject[invSize];
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
                ingredientLine.text = item.GetComponent<ItemUI>().worldItem.GetComponent<ItemWorld>().item_id;
            }
            // When no item is selected - select
            else
            {
                selectedItem = slotNum;
                item.GetComponent<Animator>().SetBool("IsSelected", true);
                ingredientLine.text = item.GetComponent<ItemUI>().worldItem.GetComponent<ItemWorld>().item_id;
            }
        }
    }

    private void OnCollisionExit(Collision other)
    {
        TryToTakeItem(other);
    }

    private void TryToTakeItem(Collision other)
    {
        int freeSlot = GetFreeSlot();
        if (other.gameObject.CompareTag("Item") && freeSlot != -1)
        {
            other.gameObject.GetComponentInChildren<Animator>().SetBool("Destroy", true);
            GameObject uiItem = other.gameObject.GetComponent<ItemWorld>().uiVersion;
            inventory[freeSlot] = Instantiate(uiItem, slots[freeSlot].transform);
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
