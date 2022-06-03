using UnityEngine;

public class Trap : MonoBehaviour 
{
    public Animator anim;

    private bool _activated = false;

    private void OnTriggerEnter(Collider other) 
    {
        if (other.TryGetComponent<AIManager>(out AIManager ai))
        {
            if (!_activated)
            {
                anim.SetBool("Activated", true);
                ai.Transition("Death");
                _activated = true;
            }
        }
    }    
}