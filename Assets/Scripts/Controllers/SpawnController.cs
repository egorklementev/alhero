using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpawnController : MonoBehaviour
{

    [Header("Items")]
    [Space(5f)]
    public List<GameObject> items;

    [Header("Containers")]
    [Space(5f)]
    public GameObject sidePanel; // For corresponding UI panels
    public GameObject[] containerUIGroups;
    public GameObject[] containers;

    [Space(15f)]
    public LogicController logic;
    public UIController ui;

    private static int containersSpawned = 0;

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

        return container;
    }

    public GameObject SpawnItem(string itemID, Vector3 pos, Quaternion rot, GameObject owner)
    {
        return Instantiate(
            items.Find(i => i.GetComponent<ItemWorld>().itemID.Equals(itemID)),
            pos,
            rot,
            owner.transform
            );
    }
}
