using System.Collections;
using UnityEngine;
using TMPro;
using System.Linq;
using UnityEngine.Localization.Settings;

public class RaccoonAI : SomeAI
{
    public TextMeshProUGUI potionDesc;
    public TextMeshProUGUI rewardText;
    public Transform potionSlot;
    public Transform barCounterAnchor;

    private float _itemCheckTimer = 1f;
    private ItemOwnerAI _itemOwnAI;
    private MagpieAI _magAI;
    private WalkingAI _walkAI;
    private bool _isTransportingItem = false;

    public override void PrepareAction() 
    {
        _itemOwnAI = _aiManager.GetAI<ItemOwnerAI>();
        _magAI = _aiManager.GetAI<MagpieAI>();
        _walkAI = _aiManager.GetAI<WalkingAI>();

        if (!_aiManager.logic.spawner.DoesItemExist(DataController.genData.raccoonRequestedItem))
        {
            GenerateNewRequestItem();
        }    
        UpdateFields();
    }

    public override void Act() 
    {
        _aiManager.Transition("Idle");
    }

    private void FixedUpdate() 
    {
        _itemCheckTimer -= Time.fixedDeltaTime;     
        if (_itemCheckTimer < 0f)
        {
            _itemCheckTimer = 1f;
            
            if(_itemOwnAI.HasItem() && !_isTransportingItem) 
            {
                ItemWorld item = _itemOwnAI.GetItem();
                if (item.id == DataController.genData.raccoonRequestedItem)
                {
                    "Spawning reward...".Log(this);
                    _walkAI.SetDestination(barCounterAnchor.position);
                    _walkAI.SetNextState("ItemOwner");
                    _walkAI.SetOnArrivalAction(() =>
                    {
                        _isTransportingItem = false;
                        _magAI.enabled = true;
                    });
                    _isTransportingItem = true;
                    _magAI.enabled = false;

                    var rewardItem = _aiManager.logic.spawner.SpawnItem<ItemWorld>(
                        DataController.genData.raccoonRewardItem,
                        transform.position + new Vector3(-1.5f, 2f, -1.5f),
                        Quaternion.identity) ;
                    if (rewardItem is CoinWorld coin)
                    {
                        coin.Count = CalcualteReward(item);
                    }

                    GenerateNewRequestItem();
                    _aiManager.Transition("Walking");
                    UpdateFields();
                }
                else
                {
                    transform.LookAt(_aiManager.logic.player.transform);
                    _magAI.enabled = false;
                    StartCoroutine(TurnOnStaringAI(3f));
                    _aiManager.Transition("UnhappyRaccoon");
                }
                _itemOwnAI.SetItem(item);
            }     
        }
    }

    public void GenerateNewRequestItem()
    {
        DataController.UpdateRaccoonRequestItemAndReward();
    }

    public void UpdateFields() 
    {
        foreach (Transform t in potionSlot)
        {
            Destroy(t.gameObject);
        }

        ItemUI item = _aiManager.logic.spawner.SpawnItem<ItemUI>(DataController.genData.raccoonRequestedItem, potionSlot);
        Instantiate(
            item,
            Vector3.zero, 
            Quaternion.identity,
            potionSlot
        );

        if (item.IsPotion())
        {
            (item as PotionUI).potionData.LocalizePotion(potionDesc);
        }
        else
        {
            item.item_name.Localize("Ingredients", potionDesc);
        }

        if (DataController.genData.raccoonRewardItem == "coin".Hash())
        {
            "raccoon_reward_coins".Localize("Entities", rewardText, $"{CalcualteReward(item)}");
        }
        else
        {
            string itemName = _aiManager.logic.spawner.absItems
                .Where(item => item.id == DataController.genData.raccoonRewardItem)
                .ElementAt(0).item_name;
            string itemNameLoc = LocalizationSettings.StringDatabase.GetLocalizedString("Ingredients", itemName);
            "raccoon_reward".Localize("Entities", rewardText, itemNameLoc);
        }

    }

    private int CalcualteReward(AbstractItem item)
    {
       return 10 - (int)(DataController.ingredients[item.id].rarity / 100f);
    }

    private IEnumerator TurnOnStaringAI(float sec)
    {
        yield return new WaitForSeconds(sec);
        _magAI.enabled = true;
    }
}