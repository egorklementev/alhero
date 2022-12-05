using System.Collections;
using UnityEngine;
using TMPro;

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
            
            if(_itemOwnAI.HasItem()) 
            {
                ItemWorld item = _itemOwnAI.GetItem();
                if (item.id == DataController.genData.raccoonRequestedItem)
                {
                    _walkAI.SetDestination(barCounterAnchor.position);
                    _walkAI.SetNextState("ItemOwner");
                    GenerateNewRequestItem();
                    _magAI.enabled = false;
                    StartCoroutine(TurnOnStaringAI(20f));
                    _aiManager.Transition("Walking");
                    UpdateFields();
                    _aiManager.logic.spawner.SpawnItem<CoinWorld>(
                        "coin".Hash(), 
                        transform.position + new Vector3(-1.5f, 2f, -1.5f),
                        Quaternion.identity)
                        .Count = CalcualteReward(item);
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
        DataController.UpdateRaccoonRequestItem();
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
        potionDesc.text = item.item_name;
        rewardText.text = $"Reward: {CalcualteReward(item)}";
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