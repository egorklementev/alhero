using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnterFieldInteraction : MonoBehaviour
{
    public string groupToActivate = "none";
    public float enterTime = 1f;

    private float currentTimer = 1f;
    private bool isActivated = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            currentTimer = enterTime;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            currentTimer -= Time.deltaTime;
            if (currentTimer < 0f && !isActivated)
            {
                isActivated = true;
                UIController.TriggerRightPanel();
                UIController.ActivateUIGroup(groupToActivate);
                LogicController.currentBarrel = transform.parent.gameObject;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            isActivated = false;
        }
    }
}
