using System;
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
    public GameObject[] containersForSpawn;

    [Header("Portal")]
    public GameObject portal;

    [Header("Entities")]
    public GameObject[] entities;

    [Space(5f)]
    public Transform containersGroup;
    public Transform itemsGroup;
    public Transform entitiesGroup;

    [Space(15f)]
    public LogicController logic;
    public UIController ui;

    private static int containersSpawned = 0; // Increase me if there are already prefab containers in a scene

    private void Start()
    {
        UpdateItemIDs();
        LoadGeneratedPotions();
        LoadContainers();
    }

    private void UpdateItemIDs()
    {
        // Since we do not want to use hashes in prefabs, we need to precalculated them in the Start()
        foreach (AbstractItem item in absItems) {
            // $"{item.item_name} -> {item.item_name.Hash()}".Log(this);
            item.id = item.item_name.Hash();
        }
    }

    private void LoadGeneratedPotions()
    {
        // We have to add generated potion items to the spawner
        foreach (Ingredient i in DataController.ingredients.Values)
        {
            if (absItems.Find(item => item.id == i.id) == null)
            {
                // World version
                PotionWorld worldPotion = SpawnItem<PotionWorld>(
                    ("potion_" + i.potionData.bottle_shape).Hash(),
                    new Vector3(0f, 100f, 0f),
                    Quaternion.identity,
                    gameObject
                    );
                worldPotion.potionData = new Potion(i.potionData);
                int genPotionID = i.potionData.GetID();
                string genPotionName = i.potionData.GenerateNameDebug();
                worldPotion.id = genPotionID;
                worldPotion.item_name = genPotionName;
                worldPotion.SetPhysicsActive(false);
                worldPotion.gameObject.SetActive(false);
                absItems.Add(worldPotion);

                // UI version
                PotionUI uiPotion = SpawnItem<PotionUI>(
                    ("potion_" + i.potionData.bottle_shape).Hash(),
                    new Vector3(0f, 300f, 0f),
                    Quaternion.identity,
                    gameObject
                );
                uiPotion.potionData = new Potion(i.potionData);
                uiPotion.id = genPotionID;
                uiPotion.item_name = genPotionName;
                uiPotion.gameObject.SetActive(false);
                absItems.Add(uiPotion);
            }
        }
    }

    private void LoadContainers()
    {
        if (containersGroup != null)
        {
            // Load containers with previously stored items
            foreach (Transform contTransform in containersGroup.transform)
            {
                Container contScript = contTransform.gameObject.GetComponentInChildren<Container>();
                if (!DataController.labContainers.ContainsKey(contScript.id))
                {
                    $"No container with ID \"{contScript.id}\"!".Warn(this);
                }
                else
                {
                    LabContainerItems itemsToLoad = DataController.labContainers[contScript.id];
                    if (itemsToLoad != null)
                    {
                        for (int i = 0; i < itemsToLoad.items.Length; i++)
                        {
                            int itemToPutID = itemsToLoad.items[i].id;
                            if (itemToPutID != 0)
                            {
                                if (itemsToLoad.items[i].potionData.ingredients.Length > 0)
                                {
                                    Potion potionData = itemsToLoad.items[i].potionData;
                                    PotionWorld potion = SpawnItem<PotionWorld>(
                                        ("potion_" + potionData.bottle_shape).Hash(),
                                        new Vector3(0f, 100f, 0f),
                                        Quaternion.identity,
                                        itemsGroup);
                                    potion.potionData = potionData;
                                    int newPotionID = potionData.GetID();
                                    string newPotionName = potionData.GenerateNameDebug();
                                    potion.id = newPotionID;
                                    potion.item_name = newPotionName;
                                    contScript.TryToPutItem(potion, i);
                                }
                                else
                                {
                                    contScript.TryToPutItem(
                                        SpawnItem<ItemWorld>(
                                            itemToPutID,
                                            new Vector3(0f, 100f, 0f),
                                            Quaternion.identity,
                                            itemsGroup),
                                        i);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public Container SpawnContainer(int containerID, Vector3 pos, Quaternion rot, Transform owner)
    {
        containersSpawned++;

        GameObject container = Instantiate(containersForSpawn[containerID], pos, rot, owner);
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

        // Connect barrel to slots
        Container contScript = container.GetComponentInChildren<Container>();
        contScript.slots = new GameObject[contScript.invSize];
        for (int i = 0; i < contScript.invSize; i++)
        {
            contScript.slots[i] = slots.transform.Find("slot" + i).gameObject;
        }
        contScript.ingredientLine = ingredientTitle.GetComponent<TextMeshProUGUI>();
        contScript.isUnlocked = true;
        contScript.id = ("spawned_" + containersSpawned.ToString()).Hash();
        contScript.spawner = this;

        // Connect enter field to UI
        container.transform.Find("EnterField").gameObject.SetActive(true);
        container.GetComponentInChildren<EnterFieldInteraction>().groupToActivate = uiGroup.name;

        return contScript;
    }

    public T SpawnItem<T>(int id, GameObject owner) where T : AbstractItem
    {
        return SpawnItem<T>(id, Vector3.zero, Quaternion.identity, owner.transform);
    }

    public T SpawnItem<T>(int id, Transform owner) where T : AbstractItem
    {
        return SpawnItem<T>(id, Vector3.zero, Quaternion.identity, owner);
    }

    public T SpawnItem<T>(int id, Vector3 pos, Quaternion rot, GameObject owner) where T : AbstractItem
    {
        return SpawnItem<T>(id, pos, rot, owner.transform);
    }

    public T SpawnItem<T>(int id, Vector3 pos, Quaternion rot, Transform owner) where T : AbstractItem
    {
        AbstractItem item = absItems.Find(i => i.id == id && i is T);
        if (item != null)
        {
            T obj = Instantiate(item.gameObject, pos, rot, owner).GetComponent<AbstractItem>() as T;
            obj.gameObject.SetActive(true); // In any case, whynot
            if (obj is ItemWorld iw)
            {
                iw.SetPhysicsActive(true);
                iw.logic = logic;
            }
            return obj;
        }
        $"No item with ID \"{id}\"!!!".Warn(this);
        return null;
    }

    public bool DoesItemExist(int id)
    {
        return absItems.Find(i => i.id == id) != null;
    }

    public AIManager SpawnEntity(string entityName, Vector3 pos, Quaternion rot, Transform owner)
    {
        GameObject ent = Array.Find(entities, e => e.name.Equals(entityName));
        if (ent == null)
        {
            $"No entity with name ({entityName}) found!!!".Warn(this);
            return null;
        }
        else
        {
            return Instantiate(ent, pos, rot, owner).GetComponentInChildren<AIManager>();
        }
    }

    public void SpawnPortal(string sceneToLoad, string label, string checkpoint, Color color, Transform parent, Vector3 pos, Quaternion rot)
    {
        Portal p = Instantiate(portal, pos, rot, parent).GetComponent<Portal>();
        p.SceneToLoad = sceneToLoad;
        p.LabelToShow = label;
        p.Color = color;
        p.CheckpointToSpawn = checkpoint;
        p.logic = logic;
    }

    public void ClearContainers()
    {
        foreach (Transform container in containersGroup)
        {
            container.GetComponentInChildren<Container>().Clear();
        }
    }
}
