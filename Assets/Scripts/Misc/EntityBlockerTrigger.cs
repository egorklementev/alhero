using UnityEngine;

public class EntityBlockerTrigger : MonoBehaviour 
{
    [SerializeField] private float pushForce = 2f;
    [SerializeField] private string[] entitiesToAllow;

    private void OnTriggerEnter(Collider other) 
    {
        PushBack(other);
    }

    private void OnTriggerStay(Collider other) 
    {
        PushBack(other);
    }

    private void PushBack(Collider other)
    {
        if (other.TryGetComponent<AIManager>(out var ai))
        {
            foreach (var ent in entitiesToAllow)
            {
                if (ai.gameObject.name.Contains(ent))
                {
                    // do nothing
                    return;
                }
            }

            // force back
            if (ai.gameObject.TryGetComponent<Rigidbody>(out var body))
            {
                body.AddForce(-1f * pushForce * body.velocity.normalized, ForceMode.Impulse);
            }
        }
    }
}