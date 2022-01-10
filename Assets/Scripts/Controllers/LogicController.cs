using UnityEngine;

public class LogicController : MonoBehaviour
{
    private static int invSize = 3; // Inverntory size

    public static GameObject currentBarrel { get; set; } = null;
    public static ItemWorld[] PickedItems { get; set; } = new ItemWorld[invSize];

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
}
