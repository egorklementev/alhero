using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpawnController : MonoBehaviour
{

    [Header("Items")]
    [Space(5f)]
    public List<AbstractItem> absItems;

    [Header("Containers")]
    [Space(5f)]
    public GameObject sidePanel; // For corresponding UI panels
    public GameObject[] containerUIGroups;
    public GameObject[] containers;

    [Space(15f)]
    public LogicController logic;
    public UIController ui;

    private static int containersSpawned = 0; // Increase me if there are already prefab containers in a scene

    public GameObject SpawnContainer(int containerID, Vector3 pos, Quaternion rot, GameObject owner)
    {
        containersSpawned++;

        GameObject container = Instantiate(containers[containerID], pos, rot, owner.transform);
        GameObject uiGroup = Instantiate(containerUIGroups[containerID], sidePanel.transform);
        uiGroup.name += "_" + containersSpawned;
        ui.uiGroups.Add(uiGroup);
        uiGroup.SetActive(false);

        // Enable slots logic
        int slotIndex = 0;
        GameObject slots = uiGroup.transform.Find("Slots").gameObject;
        foreach (Transform slot in slots.transform)
        {
            if (slot.gameObject.TryGetComponent<Button>(out Button btn))
            {
                int i = slotIndex; // Wierd flex but ok
                btn.onClick.AddListener(delegate { logic.SelectItemInBarrel(i); });
                slotIndex++;
            }
        }
        GameObject ingredientTitle = slots.transform.Find("IngredientTitle").gameObject;

        // Enable 'take' button logic
        GameObject takeBtn = uiGroup.transform.Find("TakeButton").gameObject;
        takeBtn.GetComponent<Button>().onClick.AddListener(logic.TakeItemFromContainer);

        // Connect enter field to UI
        container.GetComponentInChildren<EnterFieldInteraction>().groupToActivate = uiGroup.name;

        // Connect barrel to slots
        Container contScript = container.GetComponentInChildren<Container>();
        contScript.slots = new GameObject[contScript.invSize];
        for (int i = 0; i < contScript.invSize; i++)
        {
            contScript.slots[i] = slots.transform.Find("slot" + i).gameObject;
        }
        contScript.ingredientLine = ingredientTitle.GetComponent<TextMeshProUGUI>();
        contScript.isUnlocked = true;
        contScript.id = "spawned_" + containersSpawned.ToString();

        return container;
    }

    public T SpawnItem<T>(string id, GameObject owner) where T : AbstractItem
    {
        return SpawnItem<T>(id, Vector3.zero, Quaternion.identity, owner.transform);
    }

    public T SpawnItem<T>(string id, Transform owner) where T : AbstractItem
    {
        return SpawnItem<T>(id, Vector3.zero, Quaternion.identity, owner);
    }

    public T SpawnItem<T>(string id, Vector3 pos, Quaternion rot, GameObject owner) where T : AbstractItem
    {
        return SpawnItem<T>(id, pos, rot, owner.transform);
    }

    public T SpawnItem<T>(string id, Vector3 pos, Quaternion rot, Transform owner) where T : AbstractItem
    {
        AbstractItem item = absItems.Find(i => i.id.Equals(id) && i is T);
        if (item != null)
        {
            T obj = Instantiate(item.gameObject, pos, rot, owner).GetComponent<AbstractItem>() as T;
            obj.gameObject.SetActive(true); // In any case, whynot
            if (obj is ItemWorld)
            {
                (obj as ItemWorld).SetPhysicsActive(true);
            }
            return obj;
        }
        Debug.LogError($"[SpawnController.SpawnItem] No item with ID \"{id}\"!!!");
        return null;
    }
}
