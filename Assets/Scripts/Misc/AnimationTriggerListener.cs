using UnityEngine;
using UnityEngine.Events;

public class AnimationTriggerListener : MonoBehaviour {
    
    [SerializeField] private UnityEvent onAnimationTrigger;

    public void TriggerEvents() 
    {
        onAnimationTrigger?.Invoke();
    }
}