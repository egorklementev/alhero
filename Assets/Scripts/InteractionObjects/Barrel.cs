using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barrel : MonoBehaviour
{
    public int invSize = 4;
    public GameObject[] slots;

    private GameObject[] inventory;
    private int selectedItem = -1;

    private void Awake()
    {
        inventory = new GameObject[invSize];
    }

    private void Update()
    {
        // UI item tap
        foreach (Touch touch in Input.touches)
        {
            if (touch.phase == TouchPhase.Ended)
            {
                Ray raycast = Camera.main.ScreenPointToRay(touch.position);
                RaycastHit raycastHit;
                if (Physics.Raycast(raycast, out raycastHit))
                {
                    if (raycastHit.collider.CompareTag("ItemUI"))
                    {
                        for (int i = 0; i < invSize; i++)
                        {
                            GameObject item = raycastHit.collider.gameObject;
                            if (item.transform.parent.gameObject == slots[i])
                            {
                                // When selected already - deselect 
                                if (selectedItem == i)
                                {
                                    selectedItem = -1;
                                    item.GetComponent<Animator>().SetBool("IsSelected", false);
                                }
                                // When other is selected - deselect other and select new
                                else if (selectedItem > -1)
                                {
                                    slots[selectedItem].GetComponentInChildren<Animator>().SetBool("IsSelected", false);
                                    selectedItem = i;
                                    item.GetComponent<Animator>().SetBool("IsSelected", true);
                                }
                                // When no item is selected - select
                                else
                                {
                                    selectedItem = i;
                                    item.GetComponent<Animator>().SetBool("IsSelected", true);
                                }
                            }
                        }
                    }
                }
            }
            break;
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        int freeSlot = GetFreeSlot();
        if (other.gameObject.CompareTag("Item") && freeSlot != -1)
        {
            other.gameObject.GetComponentInChildren<Animator>().SetBool("Destroy", true);
            inventory[freeSlot] = other.gameObject;
            GameObject uiItem = other.gameObject.GetComponent<ItemWorld>().uiVersion;
            Instantiate(uiItem, slots[freeSlot].transform);
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
}
