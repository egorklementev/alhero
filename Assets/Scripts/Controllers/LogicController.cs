using System.Collections.Generic;
using UnityEngine;

public class LogicController : MonoBehaviour
{
    public GameObject player;

    [Space(15f)]
    public GameObject worldItemsGroup;
    public GameObject entitiesGroup;

    [Space(15f)]
    public GameObject recipeBook;
    public RecipeBook recipeBookScript;

    [Space(15f)]
    public SpawnController spawner;
    public UIController ui;
    public DataController data;

    private static int playerInvSize = 3; // Inverntory size
    private static bool newGameStarted = false;

    public static Container curContainer { get; set; } = null;
    public static ItemWorld[] PickedItems { get; set; } = new ItemWorld[playerInvSize];

    private void OnEnable() 
    {
        if (newGameStarted)
        {
            #region DEBUG
            newGameStarted = false;
            string[] items = new string[]
            {
                "flower", "horseshoe", "meat", "salt", "wine"
            };
            Vector3[] pos = new Vector3[]
            {
                new Vector3(-68f, 0f, 34f),
                new Vector3(-52f, 0f, 38f),
                new Vector3(-73f, 0f, 52f),
                new Vector3(2f, 0f, 62f),
                new Vector3(2f, 0f, 15f),
            };
            int index = 0;
            foreach (string name in items)
            {
                spawner.SpawnItem<ItemWorld>(name.Hash(), pos[index++], Quaternion.identity, worldItemsGroup);
            }
            #endregion
        }
    }

    private void FixedUpdate()
    {
        recipeBookScript?.SetDistanceToPlayer(
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

    public void SwitchItems()
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
                    PotionWorld newPotion = spawner.SpawnItem<PotionWorld>(uiPotion.id, worldItemsGroup);

                    newPotion.potionData = new Potion(uiPotion.potionData); // Perform potion data copy
                    int newPotionID = newPotion.potionData.GetID();
                    newPotion.name = newPotionID.ToString();
                    newPotion.name = newPotionID.ToString();
                    newPotion.SetPickedUp(true, slot, player);
                    PickedItems[slot] = newPotion;

                }
                else
                {
                    ItemWorld newWorldItem = spawner.SpawnItem<ItemWorld>(selectedItem.id, worldItemsGroup);
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
        Vector3 pos = new Vector3(2f, 1f, 2f * items.Length / 2f);
        pos = player.transform.position + pos;
        foreach (string name in items)
        {
            spawner.SpawnItem<ItemWorld>(name.Hash(), pos, Quaternion.identity, worldItemsGroup);
            pos -= new Vector3(0f, 0f, 3f);
        }
    }

    public void StartNewGame()
    {
        foreach (Transform obj in worldItemsGroup.transform)
        {
            Destroy(obj.gameObject);
        }

        spawner.ClearLabContainers();
        data.StartNewGame();
        player.transform.position = Vector3.zero;
        // TODO: play some player animation or something

        newGameStarted = true;
    }

    public BaseAI GetClosestEntity(BaseAI ai)
    {
        List<BaseAI> lst = new List<BaseAI>();
        foreach (Transform t in entitiesGroup.transform)
        {
            if (t.TryGetComponent<BaseAI>(out BaseAI otherAi) && !otherAi.Equals(ai))
            {
                lst.Add(otherAi);
            }
        }
        lst.Sort(
            delegate (BaseAI ai1, BaseAI ai2) 
            {
                float d1 = (ai1.transform.position - ai.transform.position).sqrMagnitude;
                float d2 = (ai2.transform.position - ai.transform.position).sqrMagnitude;
                return d1 > d2 ? 1 : -1;
            }
        );
        return lst.Count > 0 ? lst[0] : null;
    }

    public ItemWorld GetClosestItem(BaseAI ai)
    {
        List<ItemWorld> lst = new List<ItemWorld>();
        foreach (Transform t in worldItemsGroup.transform)
        {
            if (t.TryGetComponent<ItemWorld>(out ItemWorld item))
            {
                lst.Add(item);
            }
        }
        lst.Sort(
            delegate (ItemWorld i1, ItemWorld i2)
            {
                float d1 = (i1.transform.position - ai.transform.position).sqrMagnitude;
                float d2 = (i2.transform.position - ai.transform.position).sqrMagnitude;
                return d1 > d2 ? 1 : -1;
            }
        );
        return lst.Count > 0 ? lst[0] : null;
    }

    public void ChangeScene(string newScene)
    {
        ui.StartSceneFade(newScene, 1f);
    }

    public void ResetPlayerPosition()
    {
        player.transform.position = Vector3.zero;
    }
}
