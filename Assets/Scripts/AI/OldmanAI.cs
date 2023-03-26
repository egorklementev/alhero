using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class OldmanAI : SomeAI
{
    public const byte ITEMS_TO_SELL = 2;

    public int SelectedItem = -1;

    private ItemOwnerAI _itemOwnAI;

    private Button _buyButton;

    private List<Transform> _itemSlots = new List<Transform>();
    private List<TextMeshProUGUI> _itemNames = new List<TextMeshProUGUI>();
    private List<TextMeshProUGUI> _itemCosts = new List<TextMeshProUGUI>();

    public override void PrepareAction()
    {
        _itemOwnAI = _aiManager.GetAI<ItemOwnerAI>();

        // This is fucked up, I agree...
        // But we have to publish this goddamn game!!!
        Transform oldmanUIGroup =_aiManager.logic.ui.UIGroups
            .Find(group => group.name.Equals("group_oldman")).transform;
        _buyButton = FindRecursively(oldmanUIGroup, "BuyButton").GetComponent<Button>();

        Transform[] slots = new Transform[]
        { 
            FindRecursively(oldmanUIGroup, "Slot1"),
            FindRecursively(oldmanUIGroup, "Slot2"),
        };

        foreach (var slot in slots)
        {
            _itemSlots.Add(
                FindRecursively(slot, "SlotImage")
            );
            _itemNames.Add(
                FindRecursively(slot, "ItemTitle").GetComponent<TextMeshProUGUI>()
            );
            _itemCosts.Add(
                FindRecursively(slot, "CostTitle").GetComponent<TextMeshProUGUI>()
            );
        }

        if (!_aiManager.logic.spawner.DoesItemExist(DataController.genData.oldmanItemsForSale[0]))
        {
            DataController.UpdateOldmanItems();
        }

        _buyButton.interactable = false;
        _buyButton.onClick.AddListener(() => TryToSellItem(_itemSlots, _buyButton));

        int i = 0;
        foreach (Transform slot in _itemSlots)
        {
            int indexCopy = i;
            List<Transform> copySlots = new List<Transform>(_itemSlots);
            slot.GetComponent<Button>().onClick.AddListener(() => SelectItem(indexCopy, copySlots, _buyButton));
            i++;
        }

        UpdateFields();
    }

    public override void Act()
    {
        _aiManager.Transition("Idle");
    }

    public void TryToSellItem(List<Transform> slots, Button btn)
    {
        if (SelectedItem >= 0 && DataController.ChangeCoins(-GetCost(SelectedItem)))
        {
            int itemId = slots[SelectedItem].GetComponentInChildren<ItemUI>().id;

            Transform itemToSellTransform = _aiManager.logic.spawner
                .SpawnItem<ItemWorld>(
                    itemId,
                    // Somewhere far away to not trigger collision script
                    gameObject.transform.position + Vector3.up * (_itemOwnAI.vOffset + 2f),
                    Quaternion.identity)
                .transform;

            ItemWorld itemToSell = itemToSellTransform.GetComponent<ItemWorld>();
            itemToSell.SetPickedUp(true, 0, gameObject, _itemOwnAI.vOffset);

            _itemOwnAI.SetMode(ItemOwnerAI.Mode.THROW_ITEM);
            _itemOwnAI.SetItem(itemToSell);

            // Temporarily disable staring AI
            _aiManager.GetAI<MagpieAI>().enabled = false;
            StartCoroutine(TurnOnStaringAI(3f));

            _aiManager.Transition("ItemOwner");

            // Reset both items
            DataController.UpdateOldmanItems();
            UpdateFields();
        }
    }

    public void SelectItem(int index, List<Transform> slots, Button btn)
    {
        if (SelectedItem != index && SelectedItem >= 0)
        {
            slots[SelectedItem].GetComponentInChildren<ItemUI>().SetSelected(false);
        }

        SelectedItem = SelectedItem == index ? -1 : index;

        if (SelectedItem >= 0)
        {
            ItemUI item = slots[SelectedItem].GetComponentInChildren<ItemUI>();
            btn.interactable = DataController.HasSufficientCoins(GetCost(SelectedItem));
            item.SetSelected(true);
        }
        else
        {
            btn.interactable = false;
            slots[index].GetComponentInChildren<ItemUI>().SetSelected(false);
        }
    }

    private void UpdateFields()
    {
        for (int i = 0; i < ITEMS_TO_SELL; i++)
        {
            foreach (Transform child in _itemSlots[i])
            {
                Destroy(child.gameObject);
            }

            int itemId = DataController.genData.oldmanItemsForSale[i];
            _itemCosts[i].text = GetCost(i).ToString();
            _itemNames[i].text = DataController.ingredients[itemId].ing_name;

            _aiManager.logic.spawner.SpawnItem<ItemUI>(
                itemId, _itemSlots[i]
            );
        }
    }

    private int GetCost(int itemIndex)
    {
        return 10 - (int)(
            DataController.ingredients[DataController.genData.oldmanItemsForSale[itemIndex]].rarity / 100f);
    }

    private Transform FindRecursively(Transform @object, string name)
    {
        Transform childToFind = null;
        foreach (Transform child in @object)
        {
            if (child.name.Equals(name))
                return child;
                
            childToFind = FindRecursively(child, name);

            if (childToFind != null)
            {
                return childToFind;
            }
        }

        return childToFind;
    }

    private IEnumerator TurnOnStaringAI(float sec)
    {
        yield return new WaitForSeconds(sec);
        _aiManager.GetAI<MagpieAI>().enabled = true;
    }
}
