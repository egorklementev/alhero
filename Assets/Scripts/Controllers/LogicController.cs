using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogicController : MonoBehaviour
{
    public GameObject player;

    [Space(15f)]
    public GameObject recipeBook;
    public RecipeBook recipeBookScript;
    public Transform checkpoints;

    [Space(15f)]
    public SpawnController spawner;
    public UIController ui;
    public DataController data;
    public MapGenerator mapGen;

    private static int playerInvSize = 3; // Inverntory size
    private static bool newGameStarted = false;
    private static string checkpointToSpawn = "Initial";

    public static Container curContainer { get; set; } = null;
    public static ItemWorld[] PickedItems { get; set; } = new ItemWorld[playerInvSize];

    private static int[] _pickedItemsIDs = new int[playerInvSize];

    private void OnEnable() 
    {
        TryTeleportPlayer(checkpointToSpawn);
        checkpointToSpawn = "none";
        Vector3[] itemPos = new Vector3[] 
        { 
            Vector3.up * 1.5f + Vector3.left * 2f, 
            Vector3.up * 1.5f + Vector3.left * 2f + Vector3.back * 2f, 
            Vector3.up * 1.5f + Vector3.back * 2f, 
        };
        for (int i = 0; i < _pickedItemsIDs.Length; i++)
        {
            StartCoroutine(
                DelayedItemSpawn(_pickedItemsIDs[i], player.transform.position + itemPos[i])
                );
            _pickedItemsIDs[i] = 0; // Reset picked items
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
                try
                {
                    DataController.labContainers[b.id].items[b.GetSelectedItemSlot()].id = 0;
                    DataController.labContainers[b.id].items[b.GetSelectedItemSlot()].potionData = new Potion();
                }
                catch {}

                b.ResetSelection();

                selectedItem.Destroy();

                PotionUI uiPotion = selectedItem as PotionUI;
                if (uiPotion != null)
                {
                    PotionWorld newPotion = spawner.SpawnItem<PotionWorld>(uiPotion.id, spawner.itemsGroup);

                    newPotion.potionData = new Potion(uiPotion.potionData); // Perform potion data copy
                    int newPotionID = newPotion.potionData.GetID();
                    newPotion.name = newPotionID.ToString();
                    newPotion.SetPickedUp(true, slot, player);
                    PickedItems[slot] = newPotion;

                }
                else
                {
                    ItemWorld newWorldItem = spawner.SpawnItem<ItemWorld>(selectedItem.id, spawner.itemsGroup);
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
            spawner.SpawnItem<ItemWorld>(name.Hash(), pos, Quaternion.identity, spawner.itemsGroup);
            pos -= new Vector3(0f, 0f, 3f);
        }
    }

    public void StartNewGame()
    {
        foreach (Transform obj in spawner.itemsGroup)
        {
            Destroy(obj.gameObject);
        }

        spawner.ClearContainers();
        data.StartNewGame();
        player.transform.position = Vector3.zero;
        // TODO: play some player animation or something

        newGameStarted = true;
    }

    public AIManager GetClosestEntity(AIManager ai)
    {
        List<AIManager> lst = new List<AIManager>();
        foreach (Transform t in spawner.entitiesGroup.transform)
        {
            if (t.TryGetComponent<AIManager>(out AIManager otherAi) && !otherAi.Equals(ai))
            {
                lst.Add(otherAi);
            }
        }
        lst.Sort(
            delegate (AIManager ai1, AIManager ai2) 
            {
                float d1 = (ai1.transform.position - ai.transform.position).sqrMagnitude;
                float d2 = (ai2.transform.position - ai.transform.position).sqrMagnitude;
                return d1 > d2 ? 1 : -1;
            }
        );
        return lst.Count > 0 ? lst[0] : null;
    }

    public ItemWorld GetClosestItem(AIManager ai)
    {
        List<ItemWorld> lst = new List<ItemWorld>();
        foreach (Transform t in spawner.itemsGroup)
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

    public void SetSpawnCheckpoint(string checkpoint)
    {
        checkpointToSpawn = checkpoint;
    }

    public void ChangeScene(string newScene)
    {
        for (int i = 0; i < PickedItems.Length; i++)
        {
            if (PickedItems[i] != null)
            {
                _pickedItemsIDs[i] = PickedItems[i].id;
            }
        }
        data.Autosave(); // Save all stuff before loading a new scene
        ui.StartSceneFade(newScene, 1f);
    }

    public void ResetPlayerPosition()
    {
        player.transform.position = Vector3.zero;
    }

    public void SetRandomPlayerPosition()
    {
        if (mapGen != null)
        {
            player.transform.position = mapGen.GetRandomBlockSpawnLocation();
        }
    }

    public Vector3 GetHeroPosition()
    {
        return player.transform.position;
    }

    /// Enable/Disable agents' AI 
    public void SwitchAI()
    {
        foreach (Transform agent in spawner.entitiesGroup)
        {
            AIManager ai = agent.GetComponent<AIManager>();
            ai.enabled = !ai.enabled;
        }
    }

    public IEnumerator DelayedItemSpawn(int id, Vector3 pos, float time = 1f)
    {
        yield return new WaitForSeconds(time);
        spawner.SpawnItem<ItemWorld>(id, pos, Quaternion.identity, spawner.itemsGroup);
    }

    public void TryTeleportPlayer(string checkpoint)
    {
        try 
        {
            player.transform.position = checkpoints.Find(checkpoint).position;
        }
        catch
        {
            $"No checkpoint \"{checkpoint}\" for player to be spawned found.".Log(this);
        }
    }
}
