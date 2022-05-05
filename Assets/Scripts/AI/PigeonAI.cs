using UnityEngine;

public class PigeonAI : SomeAI 
{
    public Rigidbody body;
    public float FlyDuration;
    [Range(0f, 500f)]
    public float FlyForce = 10f;

    private float _flyTimer;
    private ItemWorld _item;

    public override void PrepareAction() 
    {
        _flyTimer = FlyDuration;
    }

    public override void Act()
    {
        _flyTimer -= Time.fixedDeltaTime;
        if (_flyTimer < 0f)
        {
            Destroy(gameObject);
            if (_item != null)
            {
                Destroy(_item.gameObject);
            }
        }
        body.AddForce((Vector3.up + .1f * (Vector3.left + Vector3.back)) * FlyForce, ForceMode.Force);
        transform.LookAt(transform.position + new Vector3(-10f, 0f, -10f));
    }

    private void OnCollisionExit(Collision other) 
    {
        if (other.collider.CompareTag("Item"))
        {
            _aiManager.Transition("Flying");    
            ItemWorld item = other.collider.GetComponent<ItemWorld>();
            item.SetPickedUp(true, 0, gameObject);
            LogicController.ItemsToSpawnInTheLab.Add(item.id);
            _item = item;
        }
    }
}