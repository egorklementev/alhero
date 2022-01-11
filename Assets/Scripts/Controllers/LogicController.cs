using UnityEngine;

public class LogicController : MonoBehaviour
{
    public GameObject player;
    public GameObject itemsGroup;
    public GameObject staticObjsGroup;

    [Space(15f)]
    public SpawnController spawner;

    private static int invSize = 3; // Inverntory size

    public static GameObject currentBarrel { get; set; } = null;
    public static ItemWorld[] PickedItems { get; set; } = new ItemWorld[invSize];

    private void Awake()
    {
        spawner.SpawnContainer(1, new Vector3(-2f, -.5f, 2f), Quaternion.Euler(0f, 0f, 0f), staticObjsGroup);
    }

    public static int GetFreeInvSlot()
    {
        for (int i = 0; i < invSize; i++)
        {
            if (PickedItems[i] == null) return i;
        }
        return -1;
    }

    public void RotateItems()
    {
        for (int i = 0; i < invSize; i++)
        {
            if (PickedItems[i] != null)
            {
                PickedItems[i].SetSlot((i + 1) % invSize);
            }
        }
        ItemWorld temp = PickedItems[0];
        PickedItems[0] = PickedItems[invSize - 1];
        for (int i = invSize - 1; i > 1; i--)
        {
            PickedItems[i] = PickedItems[i - 1];
        }
        PickedItems[1] = temp;
    }

    public void TakeItemFromBarrel()
    {
        int slot = GetFreeInvSlot();
        if (currentBarrel != null && slot != -1)
        {
            Container b = currentBarrel.GetComponentInChildren<Container>();
            GameObject selected = b.GetSelectedItem();
            if (selected != null)
            {
                selected.GetComponent<Animator>().SetBool("Destroy", true);
                GameObject newWorldItem = Instantiate(selected.GetComponent<ItemUI>().worldItem, itemsGroup.transform);
                ItemWorld newItem = newWorldItem.GetComponent<ItemWorld>();
                newItem.SetPickedUp(true, slot, player);
                PickedItems[slot] = newItem;
                b.ResetSelection();
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
}
