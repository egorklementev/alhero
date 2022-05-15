using System.Collections;
using UnityEngine;
using TMPro;

public class RaccoonAI : SomeAI
{
    public TextMeshProUGUI potionDesc;
    public Transform potionSlot;
    public Transform barCounterAnchor;
    [Space(20f)]
    public LogicController logic;

    private float _itemCheckTimer = 1f;
    private ItemOwnerAI _itemOwnAI;
    private MagpieAI _magAI;
    private WalkingAI _walkAI;

    public override void PrepareAction() 
    {
        _itemOwnAI = _aiManager.GetAI<ItemOwnerAI>();
        _magAI = _aiManager.GetAI<MagpieAI>();
        _walkAI = _aiManager.GetAI<WalkingAI>();

        if (!logic.spawner.DoesItemExist(DataController.genData.raccoonRequestedItem))
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
                    StartCoroutine(TurnOnStaringAI(10f));
                    _aiManager.Transition("Walking");
                    UpdateFields();
                }
                else
                {
                    transform.LookAt(logic.player.transform);
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
        int rand = Random.Range(0, DataController.ingredients.Count);
        foreach (Ingredient i in DataController.ingredients.Values)
        {
            rand--;
            if (rand < 0)
            {
                DataController.genData.raccoonRequestedItem = i.id;
                break;
            }
        }
    }

    public void UpdateFields() 
    {
        foreach (Transform t in potionSlot)
        {
            Destroy(t.gameObject);
        }
        ItemUI item = logic.spawner.SpawnItem<ItemUI>(DataController.genData.raccoonRequestedItem, potionSlot);
        Instantiate(
            item,
            Vector3.zero, 
            Quaternion.identity,
            potionSlot
        );
        potionDesc.text = item.item_name;
    }

    private IEnumerator TurnOnStaringAI(float sec)
    {
        yield return new WaitForSeconds(sec);
        _magAI.enabled = true;
    }
}