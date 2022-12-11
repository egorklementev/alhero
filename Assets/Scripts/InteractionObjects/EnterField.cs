using UnityEngine;

public abstract class EnterField : MonoBehaviour
{
    public string groupToActivate = "none";
    public float enterTime = 1f;

    protected float currentTimer = 1f;
    protected bool isActivated = false;

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            currentTimer = enterTime;
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            isActivated = false;
        }
    }

    protected virtual void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (currentTimer > 0f)
            {
                currentTimer -= Time.deltaTime;
            }
            else if (!isActivated)
            {
                isActivated = true;
                InteractionLogic();
            }
        }
    }

    protected abstract void InteractionLogic();
}
