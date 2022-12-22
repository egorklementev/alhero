using UnityEngine;
using UnityEngine.Events;

public class Trap : MonoBehaviour 
{
    public Animator anim = null;
    public UnityEvent onActivated;

    [Space(15)]
    public LogicController logic;

    private bool _activated = false;

    private void OnTriggerEnter(Collider other) 
    {
        if (other.TryGetComponent<AIManager>(out AIManager ai))
        {
            if (!_activated)
            {
                onActivated.Invoke();

                if (anim != null)
                    anim.SetBool("Activated", true);

                ai.Transition("Death");
                _activated = true;
            }
        }
    }    
}